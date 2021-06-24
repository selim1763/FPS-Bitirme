using System;
using UnityEngine;

namespace fps
{
	public class ParticleObject : MonoBehaviour
	{
		#region COMPONENTS
		protected ParticlePoolSystem poolSystem;
		protected ParticleSystem  particleSystem;
		#endregion

		#region VARIABLES
		[SerializeField]
		protected int id;
		#endregion

		#region PROPERTIES
		public int ID => id;
		#endregion

		public virtual void Initialize(ParticlePoolSystem poolSystem)
		{
			this.poolSystem = poolSystem;
			particleSystem  = GetComponent<ParticleSystem>();
		}

		public virtual async void Play()
		{
			particleSystem.Play(true);
			
			await TaskUtility.WaitForSeconds(particleSystem.main.duration);
			poolSystem.PoolParticle(this);
		}

	}
}