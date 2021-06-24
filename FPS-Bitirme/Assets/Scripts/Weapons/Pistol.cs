namespace fps.Guns
{
	public sealed class Pistol : Gun
	{
		protected override void OnShooting()
		{
			muzzleEffect.Play(true);
		}
	}
}