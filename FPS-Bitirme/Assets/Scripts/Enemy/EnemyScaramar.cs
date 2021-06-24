using System.Threading.Tasks;
using Animancer;
using DefaultNamespace;
using DG.Tweening;
using MonsterLove.StateMachine;
using UnityEngine;

namespace fps.Enemy
{
	public sealed class EnemyScaramar : Enemy
	{
		#region COMPONENTS
		[Header("Animation Clips")]
		[SerializeField]
		private AnimationClip idleClip;
		
		[SerializeField]
		private AnimationClip walkClip;
		
		[SerializeField]
		private AnimationClip attackClip;

		[SerializeField]
		private AnimationClip getHitClip;

		[SerializeField]
		private AnimationClip deathClip;

		[Header("Particles")] 
		[SerializeField]
		private Transform projectileSpawnPoint;
		
		[SerializeField]
		private ProjectileObject spitProjectile;
		
		private ParticlePoolSystem particlePoolSystem;
		#endregion
		
		#region VARIABLES
		private float idleTimer;
		private Vector3 patrolDestination;
		#endregion

		protected override void Awake()
		{
			base.Awake();
			particlePoolSystem = FindObjectOfType<ParticlePoolSystem>();
			fsm = new StateMachine<States, StateDriverUnity>(this);
		}

		public override void Initialize(Vector3 basePoint, float patrolRadius)
		{
			base.Initialize(basePoint, patrolRadius);
			fsm.ChangeState(States.IDLE);
		}
		
		
		private void IDLE_Enter()
		{
			idleTimer      = 0.0f;
			patrolInterval = Random.Range(patrolIntervalMin, patrolIntervalMax);
			animancer.Play(idleClip, 0.1f);
		}
		
		private void IDLE_Update()
		{
			idleTimer += Time.deltaTime;
			if (idleTimer >= patrolInterval)
			{
				// Do patrol.
				fsm.ChangeState(States.PATROL);
			}
			else
			{
				if (!animancer.IsPlaying(idleClip))
				{
					animancer.Play(idleClip, 0.1f);
				}
			}
			
			// can chase to player ?
			float distance = GetDistanceToPlayer();
			if (distance <= chaseDistance)
			{
				fsm.ChangeState(States.CHASE);
			}
		}

		private void CHASE_Enter()
		{
			animancer.Play(walkClip, 0.1f);
			navMeshAgent.speed *= chaseSpeedMultiplier;
		}

		private void CHASE_Update()
		{
			if(!animancer.IsPlaying(walkClip))
			{
				animancer.Play(walkClip, 0.1f);
			}

			float distance = GetDistanceToPlayer();
			if (distance <= chaseDistance)
			{
				// attack distance.
				if (distance <= attackDistance)
				{
					fsm.ChangeState(States.ATTACK);
				}
				else
				{
					if (navMeshAgent.remainingDistance < 2.0f)
					{
						Vector3 diff = player.transform.position - transform.position;
						Vector3 center = transform.position + (diff / 2.0f);

						float radius = (distance * 0.8f) / 2.0f;
						Vector2 rndCirclePoint = VectorUtility.RandomPointInsideCircle(radius);
						Vector3 destination = center + new Vector3(rndCirclePoint.x, 0, rndCirclePoint.y);

						navMeshAgent.SetDestination(destination);
					}					
				}

			}
			else
			{
				fsm.ChangeState(States.PATROL);
			}
		}
		
		private void CHASE_Exit()
		{
			navMeshAgent.ResetPath();
			navMeshAgent.speed = initialSpeed;
		}

		
		private void ATTACK_Enter()
		{
			isAttacking = false;
			attackTimer = 0.0f;
		}


		private bool  isAttacking;
		private float attackTimer;
		
		private void ATTACK_Update()
		{
			float distance = GetDistanceToPlayer();
			if (!isAttacking && distance > attackDistance)
			{
				fsm.ChangeState(States.CHASE);
			}
			else
			{
				if (!isAttacking)
				{
					transform.DOLookAt(player.transform.position, 0.28f);
					attackTimer += Time.deltaTime;

					if (attackTimer >= 0.9f)
					{
						DoAttack();
					}
				}
				
			}
		}

		private void ATTACK_Exit()
		{
			navMeshAgent.ResetPath();
		}


		private async void DoAttack()
		{
			isAttacking = true;
			// --
			AnimancerState animancerState = animancer.Play(attackClip, 0.1f, FadeMode.FromStart);
			float animationLength = animancerState.Duration;

			await TaskUtility.WaitForSeconds(animationLength / 2.0f);

			
			ParticleObject projectile = particlePoolSystem.GetParticle(spitProjectile);
			projectile.transform.position         = projectileSpawnPoint.position;
			projectile.transform.localEulerAngles = Vector3.zero;
			projectile.transform.LookAt(player.transform);
			
			projectile.Play();
			
			
			await TaskUtility.WaitForSeconds(animationLength / 2.0f);
			
			
			// --
			attackTimer = 0.0f;
			isAttacking = false;
		}
		


		private void PATROL_Enter()
		{
			Vector2 rndCirclePoint = VectorUtility.RandomPointInsideCircle(patrolRadius);
			patrolDestination = basePoint + new Vector3(rndCirclePoint.x, 0, rndCirclePoint.y);
			navMeshAgent.SetDestination(patrolDestination);
			animancer.Play(walkClip, 0.1f);
		}

		private void PATROL_Update()
		{
			if (navMeshAgent.remainingDistance <= 1.5f)
			{
				//navMeshAgent.destination = transform.position;
				navMeshAgent.ResetPath();
				// set to idle.
				fsm.ChangeState(States.IDLE);
			}
			else
			{
				if(!animancer.IsPlaying(walkClip))
				{
					animancer.Play(walkClip, 0.1f);
				}
			}
			
			// can chase to player ?
			float distance = GetDistanceToPlayer();
			if (distance <= chaseDistance)
			{
				fsm.ChangeState(States.CHASE);
			}
		}

		private void PATROL_Exit()
		{
			navMeshAgent.ResetPath();
		}
		
		protected override void OnTakeHit(int damage)
		{
			animancer.Play(getHitClip, 0.07f, FadeMode.FromStart);
			chaseDistance *= 3;
		}
		
		
		protected override void OnDeath()
		{
			fsm.ChangeState(States.DEAD);
			base.OnDeath();
		}

		private async void DEAD_Enter()
		{
			Destroy(navMeshAgent);
			Destroy(GetComponent<BoxCollider>());
			await Task.Yield();
			
			animancer.Play(deathClip);
			
			transform.DOLocalRotate(new Vector3(5, -70, 90), 0.9f);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, Vector3.down, out hit, 15f))
			{
				await transform.DOMove(hit.point, 1.2f).AsyncWaitForCompletion();
			}
			
			HandleItemDrop();
		}
	}
}