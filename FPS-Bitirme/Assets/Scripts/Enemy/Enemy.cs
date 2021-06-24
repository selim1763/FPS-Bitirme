using System.Collections.Generic;
using System.Linq;
using Animancer;
using fps.Item;
using MonsterLove.StateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace fps.Enemy
{
	public abstract class Enemy : MonoBehaviour
	{
		public enum States
		{
			IDLE = 0,
			PATROL,
			CHASE,
			ATTACK,
			DEAD
		}

		#region COMPONENTS
		protected NavMeshAgent navMeshAgent;
		protected AnimancerComponent animancer;
		protected PlayerController player;
		protected LootManager lootManager;
		protected StateMachine<States, StateDriverUnity> fsm;
		#endregion

		#region VARIABLES
		[SerializeField]
		protected int id;
		
		[SerializeField]
		protected bool debug = false;
		
		[Header("Hit")]
		[SerializeField]
		protected float hitEffectScale = 1.0f;
		
		[Header("Patrolling")]
		[SerializeField]
		protected float patrolIntervalMin;
		[SerializeField]
		protected float patrolIntervalMax;

		protected float patrolInterval;
		protected Vector3 basePoint;
		protected float patrolRadius;
		
		[Header("Chase & Attack")]
		[SerializeField]
		protected float chaseDistance;

		[SerializeField]
		protected float attackDistance;

		[SerializeField]
		protected float chaseSpeedMultiplier = 1.0f;
		
		protected float initialSpeed;

		[Header("Stats")]
		[SerializeField]
		protected int health;

		[Header("Loot")]
		[SerializeField]
		protected LootData lootData;
		#endregion

		#region PROPERTIES
		public int ID => id;

		public States State => fsm.State;

		public float HitEffectScale => hitEffectScale;
		#endregion

		protected virtual void Awake()
		{
			navMeshAgent = GetComponent<NavMeshAgent>();
			animancer = GetComponent<AnimancerComponent>();
			player = FindObjectOfType<PlayerController>();
			lootManager = FindObjectOfType<LootManager>();

			initialSpeed = navMeshAgent.speed;
		}

		public virtual void Initialize(Vector3 basePoint, float patrolRadius)
		{
			this.basePoint = basePoint;
			this.patrolRadius = patrolRadius;
		}

		protected virtual void Update()
		{
			fsm.Driver.Update.Invoke();
		}
		
		public void TakeHit(int damage)
		{
			if (fsm.State == States.DEAD)
			{
				return;
			}
			
			health -= damage;
			if (health <= 0)
			{
				OnDeath();
			}
			else
			{
				OnTakeHit(damage);
			}
		}

		protected virtual void OnTakeHit(int damage)
		{
			
		}

		protected virtual void OnDeath()
		{
			player.OnEnemyDeath(this);
		}

		protected void HandleItemDrop()
		{
			// drop item max to 3
			
			// for one look chance is %100
			// for second one look chance is %20
			// for third look chance is %7

			const int second_check_chance = 18;
			const int third_check_chance = 7;

			int dropItemCheckCount = 1;
			
			// max is exclusive so its 101
			int rndDropCountChance = Random.Range(1, 101);
			if (rndDropCountChance <= third_check_chance)
			{
				dropItemCheckCount = 3;
			}
			else if(rndDropCountChance <= second_check_chance)
			{
				dropItemCheckCount = 2;
			}

			for (int i = 0; i < dropItemCheckCount; i++)
			{
				LootItem lootItem = TryDropItem();
				if (lootItem != null)
				{
					LootObject lootObject = lootManager.DropLoot(lootItem);
					lootObject.transform.position = transform.position + new Vector3(1, 0, 1) * (i * 3);
				}
			}
		}

		public LootItem TryDropItem()
		{
			int rndItemDrop = Random.Range(1, 101);
			List<LootItem> possibleDropItems = lootData.Items.Where(x => rndItemDrop <= x.DropChange).ToList();

			if (possibleDropItems.Count > 0)
			{
				// select least drop chance
				int minDropChange = possibleDropItems.Min(x => x.DropChange);
				return possibleDropItems.FirstOrDefault(x => x.DropChange == minDropChange);
			}
			else
			{
				return null;
			}
		}
		
		protected float GetDistanceToPlayer()
		{
			return Vector3.Distance(transform.position, player.transform.position);
		}
		
		private void OnDrawGizmosSelected()
		{
			if (!debug)
			{
				return;
			}
			
			// draw ordering
			if (chaseDistance > attackDistance)
			{
				GizmoUtility.DrawGizmoDisk(transform, chaseDistance,  Color.green);
				GizmoUtility.DrawGizmoDisk(transform, attackDistance, Color.red);
			}
			else
			{
				GizmoUtility.DrawGizmoDisk(transform, attackDistance, Color.red);
				GizmoUtility.DrawGizmoDisk(transform, chaseDistance,  Color.green);
			}
		}

	}
}