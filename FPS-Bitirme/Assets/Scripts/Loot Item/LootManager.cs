using UnityEngine;

namespace fps.Item
{
	public class LootManager : MonoBehaviour
	{
		#region COMPONENTS
		[SerializeField]
		private LootObject lootObjectPrefab;
		#endregion

		public LootObject DropLoot(LootItem lootItem)
		{
			LootObject lootObject = Instantiate(lootObjectPrefab);
			lootObject.Initialize(lootItem);

			return lootObject;
		}
	}
}