namespace Massive.Netcode
{
	public interface IInputReceiver
	{
		void SetInputAt<T>(int tick, int channel, T input);
		void SetInputsAt<T>(int tick, AllInputs<T> allInputs);
		void ApplyEventAt<T>(int tick, int channel, T data);
	}
}
