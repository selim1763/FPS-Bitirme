using System;
using Windows;
using DG.Tweening;
using DuloGames.UI;
using fps.Item;
using fps.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
	public class UIManager : MonoBehaviour
	{
		#region EVENTS
		public Action<UIWindow> OnWindowOpened;
		public Action<UIWindow> OnWindowClosed;
		#endregion
		
		#region COMPONENTS
		[Header("Windows")]
		[SerializeField]
		private UIWindow inventoryWindow;

		[SerializeField]
		private QuestWindow questWindow;
		
		[SerializeField]
		private Image crossHairImage;
		
		[SerializeField]
		private TextMeshProUGUI lootObjectTMP;
		
		[Header("Ammo Hud")]
		[SerializeField]
		private CanvasGroup ammoHudCanvasGroup;
		[SerializeField]
		private TextMeshProUGUI currentAmmoText;
		[SerializeField]
		private TextMeshProUGUI totalAmmoText;

		[Header("Npc")]
		[SerializeField]
		private GameObject npcInteractionTextObject;
		#endregion

		#region PROPERTIES
		public bool HasAnyWindowOpen => inventoryWindow.IsOpen;
		#endregion

		private void Update()
		{
			HandleWindowRequests();
		}

		private void HandleWindowRequests()
		{
			if (Input.GetKeyDown(KeyCode.I))
			{
				inventoryWindow.Toggle();
				
				if (inventoryWindow.IsOpen)
				{
					OnWindowOpened?.Invoke(inventoryWindow);
				}
				else
				{
					OnWindowClosed?.Invoke(inventoryWindow);
				}
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (inventoryWindow.IsOpen)
				{
					inventoryWindow.Hide();
					OnWindowClosed?.Invoke(inventoryWindow);
				}

				if (questWindow.IsOpen)
				{
					questWindow.Hide();
				}
			}
		}

		public void ShowAmmoHud()
		{
			ammoHudCanvasGroup.DOFade(1, 0.4f);
		}

		public void HideAmmoHud()
		{
			ammoHudCanvasGroup.DOFade(0, 0.4f);
		}
		
		public void UpdateCurrentAmmoText(int currentAmmo)
		{
			currentAmmoText.text = currentAmmo.ToString();
		}

		public void UpdateTotalAmmoText(int totalAmmo)
		{
			totalAmmoText.text = totalAmmo.ToString();
		}

		public void SetCrossHair(Sprite crossHairSprite)
		{
			crossHairImage.sprite = crossHairSprite;
		}

		public void HideCrossHair()
		{
			crossHairImage.DOFade(0, 0.45f);
		}

		public void ShowCrossHair()
		{
			crossHairImage.DOFade(1, 0.45f);
		}

		public void FadeCrossHair(float value, float duration)
		{
			crossHairImage.DOFade(value, duration);
		}

		public void ShowNpcInteractionText()
		{
			npcInteractionTextObject.SetActive(true);
		}

		public void HideNpcInteractionText()
		{
			npcInteractionTextObject.SetActive(false);
		}

		public void ShowQuestPanel(QuestData questData)
		{
			questWindow.InitializeForQuestTake(questData);
			questWindow.Show();
		}

		public void ShowQuestCompletePanel(QuestData completedQuest)
		{
			questWindow.InitializeForQuestComplete(completedQuest);
			questWindow.Show();
		}

		public void HideQuestPanel()
		{
			questWindow.Hide();
		}

		public void OnEnterNpcInteraction()
		{
			HideNpcInteractionText();
			HideCrossHair();
			HideAmmoHud();
		}

		public void OnExitNpcInteraction()
		{
			ShowNpcInteractionText();
			ShowCrossHair();
			ShowAmmoHud();
		}

		public void OnLootObject(LootObject lootObject)
		{
			string text = $"<color=yellow>{lootObject.LootItem.Name}</color> has collected.";
			ShowGainObjectText(text);
		}

		public void ShowGainObjectText(string text)
		{
			if (lootObjectTMP.gameObject.activeInHierarchy)
			{
				lootObjectTMP.gameObject.SetActive(false);
			}

			lootObjectTMP.text = text;
			lootObjectTMP.gameObject.SetActive(true);
		}
	}
}