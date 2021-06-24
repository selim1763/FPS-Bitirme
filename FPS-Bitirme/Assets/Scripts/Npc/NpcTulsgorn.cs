using System;
using System.Collections.Generic;
using fps.Quest;
using UnityEngine;

namespace fps.npc
{
	public class NpcTulsgorn : Npc
	{
		#region COMPONENTS
		[SerializeField]
		private Camera interactionCamera;

		[SerializeField]
		private List<QuestData> quests;
		#endregion
		
		#region VARIABLES
		private int  currentQuestIndex;
		private bool hasQuestAccepted;
		
		private bool canInteract;
		private bool isInteracting;
		#endregion
		
		private void Update()
		{
			if (!isInteracting && canInteract && Input.GetKeyDown(KeyCode.E))
			{
				EnterInteraction();
			}

			if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
			{
				ExitInteraction();
			}
		}
		
		private void EnterInteraction()
		{
			if ((hasQuestAccepted && !player.HasObjectiveCompleted) || currentQuestIndex >= quests.Count)
			{
				return;
			}

			player.OnEnterNpcInteraction();
			uiManager.OnEnterNpcInteraction();
			interactionCamera.gameObject.SetActive(true);
			isInteracting = true;
			// --
			
			if (hasQuestAccepted && player.HasObjectiveCompleted)
			{
				OnQuestCompleted();
			}
			else
			{
				QuestData currentQuest = quests[currentQuestIndex];
				uiManager.ShowQuestPanel(currentQuest);
			}
		}

		private void ExitInteraction()
		{
			player.OnExitNpcInteraction();
			uiManager.OnExitNpcInteraction();
			interactionCamera.gameObject.SetActive(false);
			isInteracting = false;
			// --
			
			uiManager.HideQuestPanel();
		}
		
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				canInteract = true;
				uiManager.ShowNpcInteractionText();
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				canInteract = false;
				uiManager.HideNpcInteractionText();
			}
		}

		public void OnQuestAccepted()
		{
			ExitInteraction();
			hasQuestAccepted = true;
			
			QuestData acceptedQuest = quests[currentQuestIndex];
			player.OnQuestAccepted(acceptedQuest);
		}

		private void OnQuestCompleted()
		{
			QuestData completedQuest = quests[currentQuestIndex];
			uiManager.ShowQuestCompletePanel(completedQuest);
			player.OnQuestCompleted(completedQuest);

			currentQuestIndex += 1;
			hasQuestAccepted = false;
		}

		public void OnQuestRewardAccept()
		{
			uiManager.HideQuestPanel();
			ExitInteraction();
		}
	}
}