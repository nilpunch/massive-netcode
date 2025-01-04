namespace Massive.Netcode
{
	public interface ICustomInput<T> where T : ICustomInput<T>
	{
		T Predict(int ticksPassed);

		private static void ReflectionSupportForAOT()
		{
			_ = new CustomInputBuffer<T>(0, 0);
		}
	}
}
