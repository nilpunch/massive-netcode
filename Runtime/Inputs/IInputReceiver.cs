namespace Massive.Netcode
{
	public interface IInputReceiver
	{
		void SetInputAt<T>(int tick, int channel, T input);
		void SetInputsAt<T>(int tick, AllInputs<T> allInputs) where T : IInput;
		void ApplyEventAt<T>(int tick, int localOrder, T data);
		void ApplyEventsAt<T>(int tick, AllEvents<T> allEvents) where T : IEvent;
	}
}
