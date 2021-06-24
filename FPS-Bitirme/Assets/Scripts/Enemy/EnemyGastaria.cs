using System;
using System.Linq;
using System.Threading.Tasks;
using Animancer;
using DefaultNamespace;
using DG.Tweening;
using MonsterLove.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace fps.Enemy
{
	public sealed class EnemyGastaria : Enemy
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
		private AnimationClip deathClip;
		#endregion

		#region VARIABLES
		[SerializeField]
		private int minAttackDamage;
		[SerializeField]
		private int maxAttackDamage;

		[SerializeField]
		private float CheckHitExtentMultiplier = 1.0f;
		
		private float idleTimer;
		private Vector3 patrolDestination;
		#endregion
		
		protected override void Awake()
		{
			base.Awake();
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
			attackTimer     = 0.0f;
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

					if (attackTimer >= 0.2f)
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

			await TaskUtility.WaitForSeconds(0.02f);
			
			AnimancerState animancerState = animancer.Play(attackClip, 0.1f, FadeMode.FromStart);
			animancerState.Speed = 1.5f;
			
			float animationLength = animancerState.Duration;

			Vector3 endPosition = transform.position + transform.forward * attackDistance;
			
			NavMeshHit hit;
			if (NavMesh.SamplePosition(endPosition, out hit, navMeshAgent.height * 5f, NavMesh.AllAreas))
			{
				endPosition = hit.position;
			}

			var tween = transform.DOMove(endPosition, animationLength);

			await TaskUtility.WaitForSeconds(animationLength / 2.0f);
			// --
			// check hit.

			Vector3 checkPosition = transform.position + Vector3.up * 2;

			Collider playerCollider = Physics.OverlapBox(checkPosition, Vector3.one / 2.0f * CheckHitExtentMultiplier)
			                                 .FirstOrDefault(collider => collider.CompareTag("Player"));
			if (playerCollider != null)
			{
				int rndDamage = Random.Range(minAttackDamage, maxAttackDamage);
				player.TakeHit(rndDamage);
				
				gameObject.SetIndicator();
			}
			
			// --
			await tween.AsyncWaitForCompletion();
			
			navMeshAgent.SetDestination(transform.position + transform.forward * attackDistance);
			animancer.Play(walkClip, 0.1f);

			await TaskUtility.WaitUntil(() => navMeshAgent.remainingDistance < 2.2f);
			
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
			chaseDistance *= 3;
		}

		protected override void OnDeath()
		{
			HandleItemDrop();
			fsm.ChangeState(States.DEAD);
			base.OnDeath();
		}

		private void DEAD_Enter()
		{
			animancer.Play(deathClip);
			Destroy(navMeshAgent, 0.5f);
		}
	}
}