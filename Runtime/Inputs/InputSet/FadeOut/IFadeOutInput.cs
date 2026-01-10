namespace Massive.Netcode
{
	public interface IFadeOutInput<T> : IInput where T : IFadeOutInput<T>
	{
		T FadeOut(int ticksPassed, in FadeOutConfig config);
	}
}
