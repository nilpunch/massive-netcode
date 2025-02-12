using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class Inputs : InputRegistry
	{
		private Time Time { get; }

		public Inputs(Time time, int startTick = 0)
			: base(startTick)
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
		public AllInputs<T> GetAll<T>()
		{
			return GetAllAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllActualInputs<T> GetAllActual<T>()
		{
			return GetAllActualAt<T>(Time.Tick);
		}
	}
}
