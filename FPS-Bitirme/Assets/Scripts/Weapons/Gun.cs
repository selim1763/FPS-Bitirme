using System;
using System.Threading.Tasks;
using Animancer;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

namespace fps.Guns
{
	public enum GunType
	{
		PISTOL = 0,
		SHOTGUN = 1,
		RIFLE = 2
	}

	public interface IGun
	{
		#region PROPERTIES
		public GunType GunType { get; }
		
		public Sprite CrossHairSprite { get; }
		
		public bool IsShooting { get; }
		
		public bool IsReloading { get; }
		
		public bool IsBusy { get; }
		#endregion
		
		public void TryShoot();

		public void TryInterceptReloading();

		public Task ReloadAsync();

		public Task Show();

		public Task Hide();

		public void SetCharacterStateAnimation(CharacterState characterState);

		public void SetActive(bool isActive);
	}

	public abstract partial class Gun
	{
		public struct Bullet
		{
			#region VARIABLES
			public Vector3 Center;
			public Vector3 Direction;
			#endregion
		}

		#region COMPONENTS
		[Header("Shoot Particles")]
		[SerializeField]
		protected ParticleObject enemyHitParticlePrefab;
		[SerializeField]
		protected ParticleObject groundHitParticlePrefab;

		[SerializeField]
		protected float hitParticleScaleMultiplier = 1.0f;

		private ParticlePoolSystem particlePoolSystem;
		#endregion
		
		#region VARIABLES
		[Header("Shoot Infos")]
		[SerializeField]
		protected Transform gunEndPoint;

		[SerializeField]
		protected float range = 2.0f;
		[SerializeField]
		protected float minRadius = 0.05f;
		[SerializeField]
		protected float maxRadius = 1.0f;

		private Bullet[] bulletReserve;

		[Header("Stats")]
		[SerializeField]
		protected int minBulletDamage;
		
		[SerializeField]
		protected int maxBulletDamage;
		#endregion

		
		
		private void CalculateBulletDispersion()
		{
			Vector3 startPoint = gunEndPoint.TransformPoint (new Vector3 (0, 0, 0));
			
			float delta = (2 * Mathf.PI) / bulletReserve.Length;
			float theta = 0;

			for (int i = 0; i < bulletReserve.Length; i++)
			{
				float radius = Random.Range(minRadius, maxRadius);
				
				float x = radius * Mathf.Cos (theta);
				float y = radius * Mathf.Sin (theta);
				float z = 0f;

				bulletReserve[i].Center = startPoint;
				bulletReserve[i].Direction = gunEndPoint.rotation * new Vector3(x, y, z);
				theta += delta;
			}
		}

		protected void Shoot()
		{
			CalculateBulletDispersion();
			
			for (int i = 0; i < shootAmmoCount; i++)
			{
				int selectedBulletIndex = Random.Range(0, bulletReserve.Length);
				Bullet selectedBullet = bulletReserve[selectedBulletIndex];

				RaycastHit hit;
				Vector3 dir = selectedBullet.Direction + gunEndPoint.forward * range;
				if (Physics.Raycast (selectedBullet.Center, dir, out hit, range))
				{
					string hitTag = hit.collider.tag;
					if (hitTag == "Enemy")
					{
						int hitDamage = Random.Range(minBulletDamage, maxBulletDamage);
						
						Enemy.Enemy enemy = hit.collider.gameObject.GetComponent<Enemy.Enemy>();
						enemy.TakeHit(hitDamage);
						
						ParticleObject hitParticle = particlePoolSystem.GetParticle(enemyHitParticlePrefab);
						hitParticle.transform.position = hit.point;
						hitParticle.transform.localScale = hitParticleScaleMultiplier * enemy.HitEffectScale * Vector3.one;
						
						hitParticle.Play();
						
					}
					else if (hitTag == "Ground")
					{
						ParticleObject hitParticle = particlePoolSystem.GetParticle(groundHitParticlePrefab);
						hitParticle.transform.position = hit.point;
						hitParticle.Play();
					}
					
				}	
				
			}
			
		}

		private int drawCounter = 0;

		private void DrawBulletLines()
		{
			drawCounter += 1;
			if (drawCounter % 15 == 0)
			{
				CalculateBulletDispersion();
			}
			
			foreach (Bullet bullet in bulletReserve)
			{
				Vector3 dir = bullet.Direction + (gunEndPoint.forward * range);
				Debug.DrawRay(bullet.Center, dir, Color.green);
			}
		}

	}
	
	public abstract partial class Gun : MonoBehaviour, IGun
	{
		#region COMPONENTS
		protected AnimancerComponent animancer;
		protected UIManager uiManager;
		#endregion

		#region VARIABLES
		[SerializeField]
		private GunType gunType;

		[SerializeField]
		private Sprite crossHairSprite;
		
		[Header("Max ammo")]
		[SerializeField]
		protected int clipCapacity;
		[SerializeField]
		protected int totalAmmoCapacity;

		[Header("Current ammo")]
		[SerializeField]
		protected int currentClipAmmoCount;
		[SerializeField]
		protected int currentAmmoCount;

		[SerializeField]
		protected int shootAmmoCount;

		[SerializeField]
		protected int shootAmmoDecreaseCount;
		
		
		[Header("Animation Clips")]
		[SerializeField]
		protected AnimationClip idleClip;
		
		[SerializeField]
		protected AnimationClip walkClip;		
		
		[SerializeField]
		protected AnimationClip runClip;
		
		[SerializeField]
		protected AnimationClip reloadClip;		
		
		[SerializeField]
		protected AnimationClip shootClip;
		
		[SerializeField]
		protected AnimationClip hideClip;
		
		[SerializeField]
		protected AnimationClip getClip;

		[Header("Effects")]
		[SerializeField]
		protected ParticleSystem muzzleEffect;
		
		protected bool isShooting;
		protected bool isReloading;
		#endregion
		
		#region PROPERTIES
		public GunType GunType => gunType;

		public Sprite CrossHairSprite => crossHairSprite;

		public bool IsShooting => isShooting;

		public bool IsReloading => isReloading;

		public bool IsBusy => isShooting || isReloading;
		#endregion

		public virtual void TryInterceptReloading()
		{
		}
		
		private void Awake()
		{
			uiManager = FindObjectOfType<UIManager>();
			animancer = GetComponent<AnimancerComponent>();
			particlePoolSystem = FindObjectOfType<ParticlePoolSystem>();

			int reserveCount = Mathf.Clamp(shootAmmoCount * 10, 15, 50);
			bulletReserve = new Bullet[reserveCount];
		}

		private void Update()
		{
			DrawBulletLines();
		}

		public virtual async void TryShoot()
		{
			if (isShooting || isReloading)
			{
				return;
			}

			bool canShoot = currentClipAmmoCount > 0;
			bool tryReload = !canShoot;
			
			if (canShoot)
			{
				isShooting = true;
				// --
				
				AnimancerState state = animancer.Play(shootClip, 0.1f, FadeMode.FromStart);
				
				Shoot();
				OnShooting();
				
				await TaskUtility.WaitForSeconds(state.Length - 0.05f);

				currentClipAmmoCount =  Mathf.Clamp(currentClipAmmoCount - shootAmmoDecreaseCount, 0, clipCapacity);
				uiManager.UpdateCurrentAmmoText(currentClipAmmoCount);
				if (currentClipAmmoCount == 0)
				{
					tryReload = true;
				}

				// --
				isShooting = false;
			}

			if (tryReload)
			{
				bool canReload = currentAmmoCount > 0;
				if (canReload)
				{
					await ReloadAsync();
				}
				else
				{
					// no ammo.
				}
			}
		}

		public virtual async Task ReloadAsync()
		{
			if (isReloading || isShooting)
			{
				return;
			}
			
			int requestedAmmoCount = clipCapacity - currentClipAmmoCount;
			if (requestedAmmoCount == 0)
			{
				return;
			}

			isReloading = true;
			// --

			int tempAmmoCount = currentAmmoCount;
			currentAmmoCount = Mathf.Clamp(currentAmmoCount - requestedAmmoCount, 0, totalAmmoCapacity);

			currentClipAmmoCount += tempAmmoCount - currentAmmoCount;
			
			AnimancerState state = animancer.Play(reloadClip, 0.1f, FadeMode.FromStart);
			await TaskUtility.WaitForSeconds(state.Length);
			
			uiManager.UpdateCurrentAmmoText(currentClipAmmoCount);
			uiManager.UpdateTotalAmmoText(currentAmmoCount);

			// --
			isReloading = false;
		}
		
		public async Task Show()
		{
			uiManager.UpdateCurrentAmmoText(currentClipAmmoCount);
			uiManager.UpdateTotalAmmoText(currentAmmoCount);
			uiManager.ShowAmmoHud();
			
			await PlayGetAnimationAsync();
		}

		public async Task Hide()
		{
			uiManager.UpdateCurrentAmmoText(currentClipAmmoCount);
			uiManager.UpdateTotalAmmoText(currentAmmoCount);
			uiManager.HideAmmoHud();
			
			await PlayHideAnimationAsync();
		}
		
		private async Task PlayHideAnimationAsync()
		{
			AnimancerState state = animancer.Play(hideClip, 0.1f, FadeMode.FromStart);
			uiManager.HideCrossHair();
			await TaskUtility.WaitForSeconds(state.Length);
		}
		
		private async Task PlayGetAnimationAsync()
		{
			AnimancerState state = animancer.Play(getClip, 0.1f, FadeMode.FromStart);
			uiManager.ShowCrossHair();
			await TaskUtility.WaitForSeconds(state.Length);
		}

		protected virtual void OnShooting()
		{
			
		}
		
		public void SetCharacterStateAnimation(CharacterState characterState)
		{
			AnimationClip animationClip;
			
			switch (characterState)
			{
				case CharacterState.Idle:
					animationClip = idleClip;
					break;
				case CharacterState.Walking:
					animationClip = walkClip;
					break;
				case CharacterState.Running:
					animationClip = runClip;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(characterState), characterState, null);
			}

			if (!animancer.IsPlaying(animationClip))
			{
				if (characterState == CharacterState.Running)
				{
					uiManager.FadeCrossHair(0.3f, 0.25f);
				}
				else if (animancer.IsPlaying(runClip))
				{
					uiManager.FadeCrossHair(1, 0.25f);
				}
				
				animancer.Play(animationClip, 0.2f);
			}
		}

		public void SetActive(bool isActive)
		{
			gameObject.SetActive(isActive);
		}
	}
}