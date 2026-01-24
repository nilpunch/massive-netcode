using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class Inputs : InputRegistry
	{
		private Time Time { get; }

		public Inputs(Time time, ChangeTracker changeTracker, int startTick = 0, IPredictionReceiver predictionReceiver = null)
			: base(changeTracker, startTick, predictionReceiver)
		{
			Time = time;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetGlobal<T>() where T : IInput
		{
			return GetGlobalAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> Get<T>(int channel) where T : IInput
		{
			return GetAt<T>(Time.Tick, channel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetAllInputs<T>() where T : IInput
		{
			return GetAllInputsAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FreshInputsEnumerable<T> GetAllActual<T>() where T : IInput
		{
			return GetAllActualAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEvents<T> GetEvents<T>() where T : IEvent
		{
			return GetEventsAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplyEvent<T>(int localOrder, T data) where T : IEvent
		{
			ApplyEventAt(Time.Tick, localOrder, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendEvent<T>(T data) where T : IEvent
		{
			AppendEventAt(Time.Tick, data);
		}
	}
}
