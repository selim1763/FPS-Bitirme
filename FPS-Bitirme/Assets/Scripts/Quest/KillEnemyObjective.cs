using System;
using UnityEngine;

namespace fps.Quest
{
	[CreateAssetMenu(menuName = "Create/Quest/Objectives/Kill Enemy Objective", fileName = "Kill Enemy Objective")]
	public class KillEnemyObjective : QuestObjective
	{
		#region VARIABLES
		[SerializeField]
		private int enemyID;

		[SerializeField]
		private int count;
		#endregion

		#region PROPERTIES
		public int EnemyID => enemyID;

		public int Count => count;
		#endregion
	}
}