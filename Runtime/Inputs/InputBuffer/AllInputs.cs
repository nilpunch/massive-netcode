using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.DivideByZeroChecks, false)]
	public struct AllInputs<T>
	{
		public int MaxClients { get; private set; }

		public Input<T>[] Inputs { get; private set; }

		public int InputsCapacity { get; private set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureInit()
		{
			Inputs ??= Array.Empty<Input<T>>();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> Get(int client)
		{
			if (client < 0 || client >= MaxClients)
			{
				return Input<T>.Inactual;
			}

			return Inputs[client];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActual(int client, T input)
		{
			Ensure(client);

			Inputs[client] = new Input<T>(input, 0);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Ensure(int client)
		{
			// If client already there, nothing to be done.
			if (client < MaxClients)
			{
				return;
			}

			EnsureInputAt(client);

			MaxClients = client + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			Array.Fill(Inputs, Input<T>.Inactual);
			MaxClients = 0;
		}

		/// <summary>
		/// Ensures the packed array has sufficient capacity for the specified index.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureInputAt(int index)
		{
			if (index >= InputsCapacity)
			{
				ResizeInputs(MathUtils.NextPowerOf2(index + 1));
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
			Ensure(other.MaxClients);
			Array.Copy(other.Inputs, Inputs, other.MaxClients);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyAged(AllInputs<T> other)
		{
			Ensure(other.MaxClients);
			for (var i = 0; i < other.MaxClients; ++i)
			{
				Inputs[i] = other.Inputs[i].Aged();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyAgedIfInactual(AllInputs<T> other)
		{
			Ensure(other.MaxClients);
			for (var i = 0; i < other.MaxClients; ++i)
			{
				var agedInput = other.Inputs[i].Aged();
				if (!Inputs[i].IsActual())
				{
					Inputs[i] = agedInput;
				}
			}
		}
	}
}
