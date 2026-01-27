namespace Massive.Netcode
{
	public interface IPredictionReceiver
	{
		void OnInputPredicted<T>(int tick, int channel) where T : IInput;
		void OnEventPredicted<T>(int tick, int localOrder) where T : IEvent;
	}
}
