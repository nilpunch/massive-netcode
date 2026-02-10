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

		public ulong[] FreshMask { get; private set; }

		public int FreshMaskLength => (UsedChannels + 63) >> 6;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureInitialized()
		{
			Inputs ??= Array.Empty<Input<T>>();
			FreshMask ??= Array.Empty<ulong>();
			InputsCapacity = Inputs.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> Get(int channel)
		{
			if (channel < 0 || channel >= UsedChannels)
			{
				return Input<T>.Stale;
			}

			return Inputs[channel];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetApproved(int channel, T input)
		{
			EnsureChannel(channel);

			Inputs[channel] = new Input<T>(input, 0, true);
			FreshMask[channel >> 6] |= 1UL << (channel & 63);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPrediction(int channel, T input)
		{
			EnsureChannel(channel);

			if (Inputs[channel].IsFresh && Inputs[channel].IsApproved)
			{
				throw new InvalidOperationException($"You are trying to override approved input at channel {channel}.");
			}

			Inputs[channel] = new Input<T>(input, 0, false);
			FreshMask[channel >> 6] |= 1UL << (channel & 63);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFullInput(int channel, Input<T> input)
		{
			EnsureChannel(channel);

			Inputs[channel] = input;
			if (input.IsFresh)
			{
				FreshMask[channel >> 6] |= 1UL << (channel & 63);
			}
			else
			{
				FreshMask[channel >> 6] &= ~(1UL << (channel & 63));
			}
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
			Array.Fill(Inputs, Input<T>.Stale);

			var freshMaskLength = FreshMaskLength;

			for (var i = 0; i < freshMaskLength; i++)
			{
				FreshMask[i] = 0;
			}

			UsedChannels = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearPrediction()
		{
			var staleInput = Input<T>.Stale;

			foreach (var channel in GetFreshInputs())
			{
				if (!Inputs[channel].IsApproved)
				{
					Inputs[channel] = staleInput;
					FreshMask[channel >> 6] &= ~(1UL << (channel & 63));
				}
			}
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
				Array.Fill(Inputs, Input<T>.Stale, InputsCapacity, capacity - InputsCapacity);
			}
			InputsCapacity = capacity;

			var maskLength = (InputsCapacity + 63) >> 6;
			if (maskLength > FreshMask.Length)
			{
				FreshMask = FreshMask.Resize(maskLength);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(AllInputs<T> other)
		{
			EnsureChannel(other.UsedChannels - 1);
			Array.Copy(other.Inputs, Inputs, other.UsedChannels);
			for (var i = 0; i < other.FreshMaskLength; i++)
			{
				FreshMask[i] = other.FreshMask[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyAgedFrom(AllInputs<T> other)
		{
			EnsureChannel(other.UsedChannels - 1);
			for (var i = 0; i < other.UsedChannels; i++)
			{
				Inputs[i] = other.Inputs[i].Aged();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyAgedIfNotFreshFrom(AllInputs<T> other)
		{
			EnsureChannel(other.UsedChannels - 1);
			for (var i = 0; i < other.UsedChannels; i++)
			{
				if (!Inputs[i].IsFresh)
				{
					Inputs[i] = other.Inputs[i].Aged();
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FreshInputsCount()
		{
			var count = 0;
			for (var i = 0; i < FreshMaskLength; i++)
			{
				count += MathUtils.PopCount(FreshMask[i]);
			}
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MaskEnumerator GetFreshInputs()
		{
			return new MaskEnumerator(FreshMask, FreshMaskLength);
		}
	}
}
