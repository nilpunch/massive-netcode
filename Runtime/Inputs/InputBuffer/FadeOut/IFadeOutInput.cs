namespace Massive.Netcode
{
	public interface IFadeOutInput<T> where T : IFadeOutInput<T>
	{
		T FadeOut(int ticksPassed);

		private static void ReflectionSupportForAOT()
		{
			_ = new FadeOutInputBuffer<T>(0, 0);
		}
	}
}
