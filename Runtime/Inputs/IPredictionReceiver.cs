namespace Massive.Netcode
{
	public interface IPredictionReceiver
	{
		void OnInputPredicted(IInputSet inputSet, int tick, int channel);
		void OnEventPredicted(IEventSet eventSet, int tick, int localOrder);
	}
}
