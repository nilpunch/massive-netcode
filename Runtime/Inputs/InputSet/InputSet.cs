using System;
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
			_localChangeTracker.ConfirmChangesUpTo(startTick);
			Serializer = serializer ?? new UnmanagedInputSerializer<T>();
			InputType = typeof(T);
			DataSize = Serializer.DataSize;
			FullInputSize = Serializer.FullInputSize;
		}

		public IInputSerializer<T> Serializer { get; }

		public Type InputType { get; }

		public int DataSize { get; }

		public int FullInputSize { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetInputs(int tick)
		{
			return _inputs[tick];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetApproved(int tick, int channel, T input)
		{
			PopulateUpTo(tick);

			_inputs[tick].SetApproved(channel, input);

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
		public void ClearPrediction(int startTick, int endTick)
		{
			if (startTick < _inputs.HeadIndex)
			{
				startTick = _inputs.HeadIndex;
			}

			if (endTick > _inputs.TailIndex - 1)
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
			ref var inputs = ref _inputs.Append();
			inputs.EnsureInitialized();
			inputs.Clear();
			_localChangeTracker.NotifyChange(0);
			_localChangeTracker.ConfirmChangesUpTo(startTick);
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
			_localChangeTracker.ConfirmChangesUpTo(tick);
		}

		public void Reevaluate()
		{
			for (var i = _localChangeTracker.EarliestChangedTick + 1; i < _inputs.TailIndex; i++)
			{
				_inputs[i].CopyAgedIfNotFreshFrom(_inputs[i - 1]);
			}

			_localChangeTracker.ConfirmChangesUpTo(_inputs.TailIndex);
		}

		public void ReadApproved(int tick, int channel, Stream stream)
		{
			PopulateUpTo(tick);

			_inputs[tick].SetApproved(channel, Serializer.Read(stream));
		}

		public void ReadFullInput(int tick, int channel, Stream stream)
		{
			PopulateUpTo(tick);

			_inputs[tick].SetFullInput(channel, Serializer.ReadFullInput(stream));
		}

		public void Write(int tick, int channel, Stream stream)
		{
			Serializer.Write(_inputs[tick].Get(channel).LastFreshInput, stream);
		}

		public void WriteFullInput(int tick, int channel, Stream stream)
		{
			Serializer.WriteFullInput(_inputs[tick].Inputs[channel], stream);
		}

		public void Skip(Stream stream)
		{
			Serializer.Read(stream);
		}

		public int GetUsedChannels(int tick)
		{
			return _inputs[tick].UsedChannels;
		}

		public int GetFreshInputsCount(int tick)
		{
			return _inputs[tick].FreshInputsCount();
		}

		public bool IsFresh(int tick, int channel)
		{
			return _inputs[tick].Inputs[channel].IsFresh;
		}

		public MaskEnumerator GetFreshInputs(int tick)
		{
			return _inputs[tick].GetFreshInputs();
		}
	}
}
