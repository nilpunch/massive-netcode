using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public struct AllInputs<T>
	{
		public struct StoredInput
		{
			public readonly T State;
			public readonly int Tick;

			public StoredInput(T state, int tick)
			{
				State = state;
				Tick = tick;
			}

			public static readonly StoredInput Inactual = new StoredInput(Default<T>.Value, -1);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsActualAt(int tick)
			{
				return Tick == tick;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Input<T> GetInputAt(int tick)
			{
				if (Tick < 0)
				{
					return Input<T>.Inactual;
				}

				var ticksPassed = tick - Tick;

				NegativeArgumentException.ThrowIfNegative(ticksPassed);

				return new Input<T>(State, ticksPassed);
			}
		}

		public int UsedChannels { get; private set; }

		public StoredInput[] Inputs { get; private set; }

		public int InputsCapacity { get; private set; }

		public int Tick { get; private set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureInitialized(int tick)
		{
			Inputs ??= Array.Empty<StoredInput>();
			InputsCapacity = Inputs.Length;
			Tick = tick;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Input<T> Get(int channel)
		{
			if (channel < 0 || channel >= UsedChannels)
			{
				return Input<T>.Inactual;
			}

			var input = Inputs[channel];

			if (input.Tick < 0)
			{
				return Input<T>.Inactual;
			}

			return new Input<T>(input.State, Tick - input.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActual(int channel, T input)
		{
			EnsureChannel(channel);

			Inputs[channel] = new StoredInput(input, Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureChannel(int channel)
		{
			// If channel already there, nothing to be done.
			if (channel < UsedChannels)
			{
				return;
			}

			EnsureInputForChannel(channel);

			UsedChannels = channel + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			Array.Fill(Inputs, StoredInput.Inactual);
			UsedChannels = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureInputForChannel(int channel)
		{
			if (channel >= InputsCapacity)
			{
				ResizeInputs(MathUtils.RoundUpToPowerOfTwo(channel + 1));
			}
		}

		/// <summary>
		/// Resizes the packed array to the specified capacity.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResizeInputs(int capacity)
		{
			Inputs = Inputs.Resize(capacity);
			if (capacity > InputsCapacity)
			{
				Array.Fill(Inputs, StoredInput.Inactual, InputsCapacity, capacity - InputsCapacity);
			}
			InputsCapacity = capacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(AllInputs<T> other)
		{
			EnsureChannel(other.UsedChannels - 1);
			Array.Copy(other.Inputs, Inputs, other.UsedChannels);
		}
	}
}
