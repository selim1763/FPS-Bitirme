using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

namespace fps.Enemy
{
	public class EnemyGroup : MonoBehaviour
	{
		#region COMPONENTS
		[SerializeField]
		private Enemy enemyPrefab;
		#endregion

		[Space(3)]

		#region VARIABLES
		[SerializeField, Range(0, 300)]
		private int maxEnemyCount = 10;

		[SerializeField]
		private float spawnRadius;

		[SerializeField]
		private float patrolRadius;

		private List<Enemy> activeEnemies;
		private Stack<Enemy> inactiveEnemies;
		#endregion

		private void Awake()
		{
			activeEnemies = new List<Enemy>();
			inactiveEnemies = new Stack<Enemy>();
			
			SpawnEnemy(maxEnemyCount);
		}

		private void SpawnEnemy(int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				int activeEnemyCount = activeEnemies.Count;
				if (activeEnemyCount >= maxEnemyCount)
				{
					return;
				}
				
				Vector2 rndCirclePoint = VectorUtility.RandomPointInsideCircle(spawnRadius);
				Vector3 basePoint = transform.position;
				Vector3 spawnPoint = basePoint + new Vector3(rndCirclePoint.x, 50f, rndCirclePoint.y);
				
				RaycastHit hit;
				if (Physics.Raycast(spawnPoint, Vector3.down, out hit, 300f))
				{
					spawnPoint = hit.point;
				}

				if (inactiveEnemies.Count > 0)
				{
				
				}
				else
				{
					Enemy enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity, transform);
					enemy.Initialize(basePoint, patrolRadius);
					activeEnemies.Add(enemy);
				}
			}

		}
		
		private void OnDrawGizmosSelected()
		{
			// draw ordering
			if (spawnRadius > patrolRadius)
			{
				GizmoUtility.DrawGizmoDisk(transform, spawnRadius,  Color.green);
				GizmoUtility.DrawGizmoDisk(transform, patrolRadius, Color.blue);
			}
			else
			{
				GizmoUtility.DrawGizmoDisk(transform, patrolRadius, Color.blue);
				GizmoUtility.DrawGizmoDisk(transform, spawnRadius,  Color.green);
			}
		}
		
	}
}