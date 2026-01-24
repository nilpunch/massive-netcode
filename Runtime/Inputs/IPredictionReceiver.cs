namespace Massive.Netcode
{
	public interface IPredictionReceiver
	{
		void OnInputPredicted<T>(int tick, int channel, T input) where T : IInput;
		void OnEventPredicted<T>(int tick, int localOrder, T data) where T : IEvent;
	}
}
