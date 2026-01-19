namespace Massive.Netcode
{
	public interface IInputReceiver
	{
		void SetInputAt<T>(int tick, int channel, T input)  where T : IInput;
		void SetInputsAt<T>(int tick, AllInputs<T> allInputs) where T : IInput;
		void ApplyEventAt<T>(int tick, int localOrder, T data) where T : IEvent;
		void ApplyEventsAt<T>(int tick, AllEvents<T> allEvents) where T : IEvent;
	}
}
