﻿using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class SimulationInput : InputRegistry
	{
		public SimulationTime Time { get; }

		public SimulationInput(SimulationTime time, int inputBufferSize = 120, int startTick = 0)
			: base(inputBufferSize, startTick)
		{
			Time = time;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetGlobal<T>()
		{
			return GetGlobalAt<T>(Time.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get<T>(int client)
		{
			return GetAt<T>(Time.Tick, client);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetAll<T>()
		{
			return GetAllAt<T>(Time.Tick);
		}
	}
}