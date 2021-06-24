namespace fps.Guns
{
	public class Rifle : Gun
	{
		protected override void OnShooting()
		{
			muzzleEffect.Play(true);
		}
		
	}
}