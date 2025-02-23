﻿using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.DivideByZeroChecks, false)]
	public sealed class InputSet<T> : IInput
	{
		private readonly ChangeTracker _localChangeTracker = new ChangeTracker();
		private readonly ChangeTracker _globalChangeTracker;
		private readonly CyclicList<AllInputs<T>> _inputs;

		public InputSet(ChangeTracker globalChangeTracker, int startTick)
		{
			_globalChangeTracker = globalChangeTracker;
			_inputs = new CyclicList<AllInputs<T>>(startTick);
			_inputs.Append().EnsureInit();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetInputs(int tick)
		{
			return _inputs[tick];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetInput(int tick, int client, T input)
		{
			PopulateInputsUpTo(tick);

			_inputs[tick].SetActual(client, input);

			_localChangeTracker.NotifyChange(tick);
			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetInputs(int tick, AllInputs<T> allInputs)
		{
			PopulateInputsUpTo(tick);

			_inputs[tick].CopyFrom(allInputs);

			_localChangeTracker.NotifyChange(tick);
			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PopulateInputsUpTo(int tick)
		{
			for (var i = _inputs.TailIndex; i <= tick; ++i)
			{
				ref var inputs = ref _inputs.Append();
				inputs.EnsureInit();
				inputs.CopyAged(_inputs[i - 1]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DiscardInputsUpTo(int tick)
		{
			_inputs.RemoveUpTo(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReevaluateInputs()
		{
			for (var i = _localChangeTracker.EarliestChangedTick + 1; i < _inputs.TailIndex; i++)
			{
				_inputs[i].CopyAgedIfInactual(_inputs[i - 1]);
			}
			_localChangeTracker.ConfirmChangesUpTo(_inputs.TailIndex);
		}
	}
}
