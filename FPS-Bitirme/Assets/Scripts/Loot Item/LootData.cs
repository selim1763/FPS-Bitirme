using System.Collections.Generic;
using UnityEngine;

namespace fps.Item
{
	[CreateAssetMenu(menuName = "Create/Loot/Loot Data", fileName = "Loot Data")]
	public class LootData : ScriptableObject
	{
		#region COMPONENTS
		[SerializeField]
		private List<LootItem> items;
		#endregion
	
		#region PROPERTIES
		public List<LootItem> Items => items;
		#endregion
	}
}