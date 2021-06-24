using Windows;
using UnityEngine;

namespace fps.Quest
{
    public class ObjectiveTracker : MonoBehaviour
    {
        #region COMPONENTS
        [SerializeField]
        private ObjectiveTrackerWindow objectiveTrackerWindow;
        #endregion

        #region VARIABLES
        private KillEnemyObjective activeObjective;
        private int objectiveProgressCount;
        private bool hasObjectiveCompleted;
        #endregion

        #region PROPERTIES
        public bool HasObjectiveCompleted => hasObjectiveCompleted;
        #endregion
        
        public void OnQuestAccepted(QuestData questData)
        {
            QuestObjective questObjective = questData.QuestObjective;
            if (questObjective is KillEnemyObjective killEnemyObjective)
            {
                activeObjective = killEnemyObjective;
            }

            hasObjectiveCompleted  = false;
            objectiveProgressCount = 0;
            
            objectiveTrackerWindow.SetDataForQuest(questData);
            objectiveTrackerWindow.Show();
        }

        public void OnEnemyDeath(Enemy.Enemy enemy)
        {
            if (activeObjective == null)
            {
                return;
            }

            if (enemy.ID == activeObjective.EnemyID)
            {
                objectiveProgressCount += 1;
                OnObjectiveProgress();

                if (objectiveProgressCount == activeObjective.Count)
                {
                    OnObjectiveCompleted();
                }
            }
        }
        
        private void OnObjectiveProgress()
        {
            objectiveTrackerWindow.OnObjectiveProgress(objectiveProgressCount, activeObjective);
        }
        
        private void OnObjectiveCompleted()
        {
            objectiveTrackerWindow.OnObjectiveCompleted();
            activeObjective = null;
            hasObjectiveCompleted = true;
        }

        public void OnQuestCompleted()
        {
            objectiveTrackerWindow.Hide();
        }
    }

}