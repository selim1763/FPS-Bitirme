using System;
using DefaultNamespace;
using UnityEngine;

namespace fps.npc
{
	public class Npc : MonoBehaviour
	{
		#region COMPONENTS
		protected PlayerController player;
		protected UIManager        uiManager;
		#endregion

		protected virtual void Awake()
		{
			player = FindObjectOfType<PlayerController>();
			uiManager = FindObjectOfType<UIManager>();
		}

		protected float GetDistanceToPlayer()
		{
			return Vector3.Distance(transform.position, player.transform.position);
		}
	}
}