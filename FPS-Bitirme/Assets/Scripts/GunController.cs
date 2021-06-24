using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultNamespace;
using fps.Guns;
using UnityEngine;

namespace fps
{
	public class GunController : MonoBehaviour
	{
		#region COMPONENTS
		[SerializeField]
		private List<Gun> Guns;

		private PlayerController player;
		private UIManager uiManager;
		#endregion

		#region VARIABLES
		private IGun currentGun;
		private bool isBusy;
		#endregion

		#region PROPERTIES
		public IGun CurrentGun => currentGun;

		public bool IsBusy => isBusy;
		#endregion

		private void Awake()
		{
			player = FindObjectOfType<PlayerController>();
			uiManager = FindObjectOfType<UIManager>();
			
			TrySwitchGun(GunType.PISTOL).Forget();
		}

		private void Update()
		{
			if (uiManager.HasAnyWindowOpen || player.IsInteractingWithNpc)
			{
				return;
			}
			
			if (!isBusy && !currentGun.IsBusy)
			{
				currentGun.SetCharacterStateAnimation(player.State);
			}
				
			if (Input.GetMouseButton(0))
			{
				TryShoot();
			}
			else if(currentGun.GunType == GunType.RIFLE && Input.GetMouseButton(0))
			{
				TryShoot();
			}

			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				TrySwitchGun(GunType.PISTOL).Forget();
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				TrySwitchGun(GunType.SHOTGUN).Forget();
			}			
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				TrySwitchGun(GunType.RIFLE).Forget();
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				currentGun.ReloadAsync().Forget();
			}
		}

		private void TryShoot()
		{
			if (currentGun.IsReloading)
			{
				currentGun.TryInterceptReloading();
			}
			
			bool canShoot = !isBusy && !currentGun.IsBusy && player.State != CharacterState.Running;
			if (canShoot)
			{
				currentGun.TryShoot();
			}
		}

		public async Task TrySwitchGun(int gunIndex)
		{
			GunType gunType = (GunType) gunIndex;
			await TrySwitchGun(gunType);
		}

		public async Task TrySwitchGun(GunType gunType)
		{
			if (currentGun != null && currentGun.IsReloading)
			{
				currentGun.TryInterceptReloading();
			}
				
			bool canSwitch = !isBusy && (!currentGun?.IsBusy ?? true) && player.State != CharacterState.Running;
			if (!canSwitch)
			{
				return;
			}
			
			IGun gun = GetGun(gunType);
			await SetCurrentGun(gun);
		}

		private async Task SetCurrentGun(IGun gun)
		{
			if (isBusy)
			{
				return;
			}
			
			isBusy = true;
			
			if (currentGun != null)
			{
				await currentGun.Hide();
				currentGun.SetActive(false);
			}
			
			currentGun = gun;
			currentGun.SetActive(true);
			uiManager.SetCrossHair(gun.CrossHairSprite);
			
			await currentGun.Show();
			currentGun.SetCharacterStateAnimation(player.State);

			isBusy = false;
		}
		
		public IGun GetGun(GunType GunType)
		{
			return Guns[(int) GunType];
		}
	}
	
}