using System.Threading.Tasks;
using Animancer;
using UnityEngine;

namespace fps.Guns
{
	public sealed class Shotgun : Gun
	{
		#region VARIABLES
		private bool hasReloadIntercepted;
		#endregion
		
		protected override void OnShooting()
		{
			muzzleEffect.Play(true);
		}

		public override void TryInterceptReloading()
		{
			hasReloadIntercepted = true;
			isReloading = false;
		}

		public override async Task ReloadAsync()
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
			hasReloadIntercepted = false;
			// --

			int sufficientAmmoCount = Mathf.Clamp(currentAmmoCount - requestedAmmoCount, 0, requestedAmmoCount);
			sufficientAmmoCount = Mathf.Clamp(currentAmmoCount - sufficientAmmoCount, 0, requestedAmmoCount);

			while (sufficientAmmoCount > 0 && !hasReloadIntercepted)
			{
				AnimancerState state = animancer.Play(reloadClip, 0.1f);
				await TaskUtility.WaitForSeconds(state.Length);

				if (!hasReloadIntercepted)
				{
					sufficientAmmoCount  -= 1;
					currentAmmoCount     -= 1;
					currentClipAmmoCount += 1;
				}
				
				uiManager.UpdateCurrentAmmoText(currentClipAmmoCount);
				uiManager.UpdateTotalAmmoText(currentAmmoCount);
			}
			
			// --
			isReloading = false;
		}
		
	}
}