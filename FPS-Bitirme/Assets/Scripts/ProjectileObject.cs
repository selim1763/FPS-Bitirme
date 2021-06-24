using System;
using System.Linq;
using fps;
using fps.Enemy;
using UnityEngine;

namespace DefaultNamespace
{
	public class ProjectileObject : ParticleObject
	{
		#region COMPONENTS
		[Header("Hit Particles")]
		[SerializeField]
		private ParticleObject hitParticle;

		private Rigidbody rb;
		#endregion

		#region VARIABLES
		[SerializeField]
		private float speed;

		[SerializeField]
		private int damage;
		#endregion

		private void Awake()
		{
			rb = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			rb.MovePosition(rb.position + transform.forward * speed);
		}

		public override async void Play()
		{
			particleSystem.Play(true);
		}

		private async void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.TryGetComponent(out EnemyScaramar _))
			{
				return;
			}
			
			// play hit particle.
			await TaskUtility.WaitForSeconds(0.08f);
			
			ParticleObject hitParticleObject = poolSystem.GetParticle(hitParticle);
			hitParticleObject.transform.position = transform.position;
			hitParticleObject.Play();


			Collider playerCollider = Physics.OverlapBox(transform.position, Vector3.one * 3)
			                                 .FirstOrDefault(collider => collider.CompareTag("Player"));
			if (playerCollider != null)
			{
				playerCollider.GetComponent<PlayerController>()
				              .TakeHit(damage);
			}
			
			poolSystem.PoolParticle(this);
		}

	}
}