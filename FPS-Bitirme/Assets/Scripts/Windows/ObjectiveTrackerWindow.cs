using fps.Quest;
using UnityEngine;
using UnityEngine.UI;

namespace Windows
{
	public class ObjectiveTrackerWindow : MonoBehaviour
	{
		#region COMPONENTS
		[SerializeField]
		private Text questHeaderText;

		[SerializeField]
		private Text objectiveText;

		[SerializeField]
		private GameObject checkMarkerObject;
		#endregion

		public void SetDataForQuest(QuestData questData)
		{
			questHeaderText.text = questData.HeaderText;
			UpdateObjectiveText(0, questData.QuestObjective);
			checkMarkerObject.SetActive(false);
		}

		public void OnObjectiveProgress(int currentObjective, QuestObjective objective)
		{
			UpdateObjectiveText(currentObjective, objective);
		}

		public void OnObjectiveCompleted()
		{
			checkMarkerObject.SetActive(true);
		}
		
		private void UpdateObjectiveText(int currentObjectiveCount, QuestObjective objective)
		{
			string objectiveTextValue = string.Empty;
			if (objective is KillEnemyObjective killEnemyObjective)
			{
				objectiveTextValue = $"{currentObjectiveCount}/{killEnemyObjective.Count} ";
				
				string enemyName = killEnemyObjective.EnemyID switch
				                   {
					                   1 => "Gastaria",
					                   2 => "Scaramar",
					                   _ => string.Empty
				                   };

				objectiveTextValue += $"{enemyName} has killed.";
			}

			objectiveText.text = objectiveTextValue;
		}

		public void Show()
		{
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}