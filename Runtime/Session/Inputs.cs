using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class Inputs : InputRegistry
	{
		private Time Time { get; }

		public Inputs(Time time, ChangeTracker changeTracker, int startTick = 0)
			: base(changeTracker, startTick)
		{
			Time = time;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetGlobal<T>()
		{
			return GetGlobalAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> Get<T>(int client)
		{
			return GetAt<T>(Time.Tick, client);
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
	}
}
