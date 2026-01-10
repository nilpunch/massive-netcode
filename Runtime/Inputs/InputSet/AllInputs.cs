using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public struct AllInputs<T> where T : IInput
	{
		public int UsedChannels { get; private set; }

		public Input<T>[] Inputs { get; private set; }

		public int InputsCapacity { get; private set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureInitialized()
		{
			Inputs ??= Array.Empty<Input<T>>();
			InputsCapacity = Inputs.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> Get(int channel)
		{
			if (channel < 0 || channel >= UsedChannels)
			{
				return Input<T>.Inactual;
			}

			return Inputs[channel];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActual(int channel, T input)
		{
			EnsureChannel(channel);

			Inputs[channel] = new Input<T>(input, 0);
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
			Array.Fill(Inputs, Input<T>.Inactual);
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
				Array.Fill(Inputs, Input<T>.Inactual, InputsCapacity, capacity - InputsCapacity);
			}
			InputsCapacity = capacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(AllInputs<T> other)
		{
			EnsureChannel(other.UsedChannels - 1);
			Array.Copy(other.Inputs, Inputs, other.UsedChannels);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyAgedFrom(AllInputs<T> other)
		{
			EnsureChannel(other.UsedChannels - 1);
			for (var i = 0; i < other.UsedChannels; ++i)
			{
				Inputs[i] = other.Inputs[i].Aged();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyAgedIfInactualFrom(AllInputs<T> other)
		{
			EnsureChannel(other.UsedChannels - 1);
			for (var i = 0; i < other.UsedChannels; ++i)
			{
				if (Inputs[i].TicksPassed != 0)
				{
					Inputs[i] = other.Inputs[i].Aged();
				}
			}
		}
	}
}
