using DuloGames.UI;
using fps.Item;
using UnityEngine;
using UnityEngine.UI;

namespace Windows
{
	public class InventoryWindow : UIWindow
	{
		#region COMPONENTS
		[SerializeField]
		private Text goldText;

		[SerializeField]
		private Transform slotGrid;
		#endregion
		
		public void SetGold(int amount)
		{
			goldText.text = amount.ToString();
		}

		public void AddItem(LootItem lootItem)
		{
			foreach (Transform slotTransform in slotGrid)
			{
				UIItemSlot itemSlot = slotTransform.GetComponent<UIItemSlot>();
				if (!itemSlot.IsAssigned())
				{
					UIItemInfo itemInfo = new UIItemInfo()
					{
						ID = lootItem.ID,
						Name = lootItem.Name,
						Description = lootItem.Name,
						Icon = lootItem.ItemSprite
					};
					itemSlot.Assign(itemInfo);
					break;
				}
			}
		}
	}
}