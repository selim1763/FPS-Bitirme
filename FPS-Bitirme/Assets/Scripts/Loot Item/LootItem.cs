using System.Collections.Generic;
using UnityEngine;

namespace fps.Item
{
	[CreateAssetMenu(menuName = "Create/Loot/Loot Item", fileName = "Loot Item")]
	public class LootItem : ScriptableObject
	{
		#region COMPONENTS
		[SerializeField]
		private int id;
		[SerializeField]
		private string name;
		[SerializeField]
		private Sprite itemSprite;
		[SerializeField]
		private int dropChange;
		#endregion

		#region PROPERTIES
		public int ID => id;
		public string Name => name;
		public Sprite ItemSprite => itemSprite;
		public int DropChange => dropChange;
		#endregion
	}
	
}