using System.IO;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public sealed class InputSet<T> : IInputSet where T : IInput
	{
		private readonly ChangeTracker _localChangeTracker = new ChangeTracker();
		private readonly ChangeTracker _globalChangeTracker;
		private readonly IPredictionReceiver _predictionReceiver;
		private readonly CyclicList<AllInputs<T>> _inputs;

		public InputSet(ChangeTracker globalChangeTracker, int startTick, IPredictionReceiver predictionReceiver = null, IInputSerializer<T> serializer = null)
		{
			_globalChangeTracker = globalChangeTracker;
			_predictionReceiver = predictionReceiver;
			_inputs = new CyclicList<AllInputs<T>>(startTick);
			_inputs.Append().EnsureInitialized();
			Serializer = serializer ?? new UnmanagedInputSerializer<T>();
		}

		public IInputSerializer<T> Serializer { get; private set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetInputs(int tick)
		{
			return _inputs[tick];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActual(int tick, int channel, T input)
		{
			PopulateUpTo(tick);

			_inputs[tick].SetActual(channel, input);

			_localChangeTracker.NotifyChange(tick);
			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPrediction(int tick, int channel, T input)
		{
			PopulateUpTo(tick);

			_inputs[tick].SetPrediction(channel, input);

			_localChangeTracker.NotifyChange(tick);
			_globalChangeTracker.NotifyChange(tick);
			_predictionReceiver?.OnInputPredicted(this, tick, channel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetInputs(int tick, AllInputs<T> allInputs)
		{
			PopulateUpTo(tick);

			_inputs[tick].CopyFrom(allInputs);

			_localChangeTracker.NotifyChange(tick);
			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearPrediction(int tick)
		{
			ClearPrediction(tick, tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearPrediction(int startTick, int endTick)
		{
			PopulateUpTo(endTick);

			if (startTick < _inputs.HeadIndex)
			{
				startTick = _inputs.HeadIndex;
			}

			if (endTick >= _inputs.TailIndex)
			{
				endTick = _inputs.TailIndex - 1;
			}

			for (var currentTick = startTick; currentTick <= endTick; currentTick++)
			{
				_inputs[currentTick].ClearPrediction();
			}

			_localChangeTracker.NotifyChange(startTick);
			_globalChangeTracker.NotifyChange(startTick);
		}

		public void Reset(int startTick)
		{
			_inputs.Reset(startTick);
			_inputs.Append().EnsureInitialized();
		}

		public void PopulateUpTo(int tick)
		{
			for (var currentTick = _inputs.TailIndex; currentTick <= tick; currentTick++)
			{
				ref var inputs = ref _inputs.Append();
				inputs.EnsureInitialized();
				inputs.CopyAgedFrom(_inputs[currentTick - 1]);
			}
		}

		public void DiscardUpTo(int tick)
		{
			// Ensure there at least one input left in list, so we can populate from it.
			if (tick > _inputs.TailIndex - 1)
			{
				tick = _inputs.TailIndex - 1;
			}

			_inputs.RemoveUpTo(tick);
		}

		public void Reevaluate()
		{
			for (var i = _localChangeTracker.EarliestChangedTick + 1; i < _inputs.TailIndex; i++)
			{
				_inputs[i].CopyAgedIfNotFreshFrom(_inputs[i - 1]);
			}

			_localChangeTracker.ConfirmChangesUpTo(_inputs.TailIndex);
		}

		public void Read(Stream stream)
		{
			throw new System.NotImplementedException();
		}

		public void Write(int tick, int channel, Stream stream)
		{
			throw new System.NotImplementedException();
		}

		public void WriteAll(int tick, Stream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}
