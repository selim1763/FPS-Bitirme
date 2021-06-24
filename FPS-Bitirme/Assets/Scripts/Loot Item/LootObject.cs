using UnityEngine;

namespace fps.Item
{
	public class LootObject : MonoBehaviour
	{
		#region COMPONENTS
		private LootItem lootItem;
		#endregion

		#region PROPERTIES
		public LootItem LootItem => lootItem;
		#endregion

		public void Initialize(LootItem lootItem)
		{
			this.lootItem = lootItem;
		}

		public void DoLoot()
		{
			Destroy(gameObject);
		}
	}
}