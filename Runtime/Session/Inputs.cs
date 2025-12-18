using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class Inputs : InputRegistry
	{
		private Time Time { get; }

		public Inputs(Time time, ChangeTracker changeTracker, int startTick = 0, IInputReceiver inputReceiver = null)
			: base(changeTracker, startTick, inputReceiver)
		{
			Time = time;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetGlobal<T>()
		{
			return GetGlobalAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> Get<T>(int channel)
		{
			return GetAt<T>(Time.Tick, channel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetAllInputs<T>()
		{
			return GetAllInputsAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllActualInputs<T> GetAllActual<T>()
		{
			return GetAllActualAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEvents<T> GetEvents<T>()
		{
			return GetEventsAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplyEvent<T>(int channel, T data)
		{
			ApplyEventAt(Time.Tick, channel, data);
		}
	}
}
