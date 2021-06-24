using System;
using System.Collections.Generic;
using UnityEngine;

namespace fps
{
	public class ParticlePoolSystem : MonoBehaviour
	{
		#region VARIABLES
		private Dictionary<int, Stack<ParticleObject>> inactiveParticles;
		#endregion

		private void Awake()
		{
			inactiveParticles = new Dictionary<int,  Stack<ParticleObject>>();
		}

		public ParticleObject GetParticle(ParticleObject particlePrefab)
		{
			int particleID = particlePrefab.ID;
			ParticleObject particle;

			if (!inactiveParticles.TryGetValue(particleID, out Stack<ParticleObject> particleStack))
			{
				particleStack = new Stack<ParticleObject>();
				inactiveParticles.Add(particleID, particleStack);
				
				particle = Instantiate(particlePrefab, transform);
				particle.Initialize(this);
			}
			else
			{
				if (particleStack.Count <= 0)
				{
					particle = Instantiate(particlePrefab, transform);
					particle.Initialize(this);
				}
				else
				{
					particle = particleStack.Pop();
				}
			}

			particle.gameObject.SetActive(true);
			return particle;
		}

		public void PoolParticle(ParticleObject particle)
		{
			inactiveParticles[particle.ID].Push(particle);
			particle.gameObject.SetActive(false);
		}
	}
}