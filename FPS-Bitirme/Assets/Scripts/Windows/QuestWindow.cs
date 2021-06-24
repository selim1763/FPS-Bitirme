using DuloGames.UI;
using fps.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Windows
{
	public class QuestWindow : UIWindow
	{
		#region COMPONENTS
		[SerializeField]
		public GameObject questTakeContentPanel;
		[SerializeField]
		public GameObject questCompleteContentPanel;

		[Header("Take Panel")]
		[SerializeField]
		private Text headerText;
		[SerializeField]
		private Text contentText;
		[SerializeField]
		private Text objectiveText;
		[SerializeField]
		private Image questImage;

		[SerializeField]
		private Image rewardImage;
		[SerializeField]
		private TextMeshProUGUI rewardCoinText;
		
		[Header("Complete Panel")]
		[SerializeField]
		private Text completeHeaderText;
		[SerializeField]
		private Text completeContentText;
		[SerializeField]
		private Image completeRewardImage;
		[SerializeField]
		private TextMeshProUGUI completeRewardCoinText;
		#endregion

		public void InitializeForQuestTake(QuestData questData)
		{
			questTakeContentPanel.SetActive(true);
			questCompleteContentPanel.SetActive(false);
			
			headerText.text = questData.HeaderText;
			contentText.text = questData.Content;
			objectiveText.text = questData.ObjectiveText;
			questImage.sprite = questData.QuestSprite;
			rewardImage.sprite = questData.RewardSprite;
			rewardCoinText.text = questData.RewardCoin.ToString();
		}

		public void InitializeForQuestComplete(QuestData completedQuest)
		{
			questTakeContentPanel.SetActive(false);
			questCompleteContentPanel.SetActive(true);

			completeHeaderText.text = completedQuest.HeaderText;
			completeContentText.text = completedQuest.QuestCompletedText;
			completeRewardImage.sprite = completedQuest.RewardSprite;
			completeRewardCoinText.text = completedQuest.RewardCoin.ToString();
		}
	}
}