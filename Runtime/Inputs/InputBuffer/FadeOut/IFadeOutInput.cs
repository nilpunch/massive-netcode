namespace Massive.Netcode
{
	public interface IFadeOutInput<T> where T : IFadeOutInput<T>
	{
		T FadeOut(int ticksPassed, in FadeOutConfig config);
	}
}
