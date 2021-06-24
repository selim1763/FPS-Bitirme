using UnityEngine;

namespace fps.Quest
{
	[CreateAssetMenu(menuName = "Create/Quest/Quest Data", fileName = "Quest Data")]
	public class QuestData : ScriptableObject
	{
		#region VARIABLES
		[SerializeField]
		private string headerText;
		[SerializeField]
		private string content;
		[SerializeField]
		private string objectiveText;
		[SerializeField]
		private Sprite questSprite;
		[SerializeField]
		private Sprite rewardSprite;
		[SerializeField]
		private int rewardCoin;
		[SerializeReference]
		private QuestObjective questObjective;
		[SerializeField]
		private string questCompletedText;
		#endregion

		#region PROPERTIES
		public string HeaderText => headerText;
		public string Content => content;
		public string ObjectiveText => objectiveText;
		public Sprite QuestSprite => questSprite;
		public Sprite RewardSprite => rewardSprite;
		public int RewardCoin => rewardCoin;
		public QuestObjective QuestObjective => questObjective;
		public string QuestCompletedText => questCompletedText;
		#endregion
	}
}