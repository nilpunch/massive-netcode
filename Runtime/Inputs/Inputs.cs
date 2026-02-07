using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class Inputs : IInputs, ISimulation
	{
		private int _startTick;
		private int _populateUpToTick;
		private readonly ChangeTracker _changeTracker;
		private readonly IPredictionReceiver _predictionReceiver;

		private IEventSet[] _eventsLookup = Array.Empty<IEventSet>();
		private IInputSet[] _inputsLookup = Array.Empty<IInputSet>();
		private int _lookupCapacity;

		public List<IEventSet> EventSets { get; } = new List<IEventSet>();
		public List<IInputSet> InputSets { get; } = new List<IInputSet>();

		private int CurrentTick { get; set; }

		public Inputs(ChangeTracker changeTracker, IPredictionReceiver predictionReceiver = null)
		{
			_changeTracker = changeTracker;
			_predictionReceiver = predictionReceiver;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActualInput<T>(int channel, T input) where T : IInput
		{
			SetActualInputAt(CurrentTick, channel, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActualInputAt<T>(int tick, int channel, T input) where T : IInput
		{
			GetInputSet<T>().SetActual(tick, channel, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPredictionInput<T>(int channel, T input) where T : IInput
		{
			SetPredictionInputAt(CurrentTick, channel, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPredictionInputAt<T>(int tick, int channel, T input) where T : IInput
		{
			GetInputSet<T>().SetPrediction(tick, channel, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetInput<T>(int channel) where T : IInput
		{
			return GetInputAt<T>(CurrentTick, channel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetInputAt<T>(int tick, int channel) where T : IInput
		{
			return GetInputSet<T>().GetInputs(tick).Get(channel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetAllInputs<T>() where T : IInput
		{
			return GetAllInputsAt<T>(CurrentTick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetAllInputsAt<T>(int tick) where T : IInput
		{
			return GetInputSet<T>().GetInputs(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FreshInputsEnumerable<T> GetFreshInputs<T>() where T : IInput
		{
			return GetFreshInputsAt<T>(CurrentTick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FreshInputsEnumerable<T> GetFreshInputsAt<T>(int tick) where T : IInput
		{
			return new FreshInputsEnumerable<T>(GetInputSet<T>().GetInputs(tick));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActualEvent<T>(int localOrder, int channel, T data) where T : IEvent
		{
			SetActualEventAt(CurrentTick, localOrder, channel, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActualEventAt<T>(int tick, int localOrder, int channel, T data) where T : IEvent
		{
			GetEventSet<T>().SetActual(tick, localOrder, channel, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendActualEvent<T>(int channel, T data) where T : IEvent
		{
			AppendActualEventAt(CurrentTick, channel, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendActualEventAt<T>(int tick, int channel, T data) where T : IEvent
		{
			GetEventSet<T>().AppendActual(tick, channel, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendPredictionEvent<T>(int channel, T data) where T : IEvent
		{
			AppendPredictionEventAt(CurrentTick, channel, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendPredictionEventAt<T>(int tick, int channel, T data) where T : IEvent
		{
			GetEventSet<T>().AppendPrediction(tick, channel, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEvents<T> GetAllEvents<T>() where T : IEvent
		{
			return GetAllEventsAt<T>(CurrentTick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEvents<T> GetAllEventsAt<T>(int tick) where T : IEvent
		{
			return GetEventSet<T>().GetAllEvents(tick);
		}

		void ISimulation.Update(int tick)
		{
			CurrentTick = tick;
		}

		public void Reset(int startTick)
		{
			_startTick = startTick;
			_populateUpToTick = startTick;
			CurrentTick = _startTick;

			for (var i = 0; i < InputSets.Count; i++)
			{
				InputSets[i].Reset(_startTick);
			}
			for (var i = 0; i < EventSets.Count; i++)
			{
				EventSets[i].Reset(_startTick);
			}
		}

		public void PopulateUpTo(int tick)
		{
			_populateUpToTick = tick;

			for (var i = 0; i < InputSets.Count; i++)
			{
				InputSets[i].PopulateUpTo(tick);
			}
			for (var i = 0; i < EventSets.Count; i++)
			{
				EventSets[i].PopulateUpTo(tick);
			}
		}

		public void DiscardUpTo(int tick)
		{
			_startTick = tick;

			for (var i = 0; i < InputSets.Count; i++)
			{
				InputSets[i].DiscardUpTo(tick);
			}
			for (var i = 0; i < EventSets.Count; i++)
			{
				EventSets[i].DiscardUpTo(tick);
			}
		}

		public void Reevaluate()
		{
			for (var i = 0; i < InputSets.Count; i++)
			{
				InputSets[i].Reevaluate();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EventSet<T> GetEventSet<T>() where T : IEvent
		{
			var info = TypeId<InputKind, T>.Info;

			EnsureLookupByTypeAt(info.Index);
			var candidate = _eventsLookup[info.Index];

			if (candidate != null)
			{
				return (EventSet<T>)candidate;
			}

			var eventSet = new EventSet<T>(_changeTracker, _startTick, _predictionReceiver, new UnmanagedEventSerializer<T>());
			eventSet.PopulateUpTo(_populateUpToTick);
			_eventsLookup[info.Index] = eventSet;
			EventSets.Add(eventSet);

			return eventSet;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEventSet GetEventSetReflected(Type inputType)
		{
			if (TypeId<InputKind>.TryGetInfo(inputType, out var info))
			{
				EnsureLookupByTypeAt(info.Index);
				var candidate = _eventsLookup[info.Index];

				if (candidate != null)
				{
					return candidate;
				}
			}

			var createMethod = typeof(Inputs).GetMethod(nameof(GetEventSet));
			var genericMethod = createMethod?.MakeGenericMethod(inputType);
			return (IEventSet)genericMethod?.Invoke(this, new object[] { });
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public InputSet<T> GetInputSet<T>() where T : IInput
		{
			var info = TypeId<InputKind, T>.Info;

			EnsureLookupByTypeAt(info.Index);
			var candidate = _inputsLookup[info.Index];

			if (candidate != null)
			{
				return (InputSet<T>)candidate;
			}

			var inputSet = new InputSet<T>(_changeTracker, _startTick, _predictionReceiver, new UnmanagedInputSerializer<T>());
			inputSet.PopulateUpTo(_populateUpToTick);
			_inputsLookup[info.Index] = inputSet;
			InputSets.Add(inputSet);

			return inputSet;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IInputSet GetInputSetReflected(Type inputType)
		{
			if (TypeId<InputKind>.TryGetInfo(inputType, out var info))
			{
				EnsureLookupByTypeAt(info.Index);
				var candidate = _inputsLookup[info.Index];

				if (candidate != null)
				{
					return candidate;
				}
			}

			var createMethod = typeof(Inputs).GetMethod(nameof(GetInputSet));
			var genericMethod = createMethod?.MakeGenericMethod(inputType);
			return (IInputSet)genericMethod?.Invoke(this, new object[] { });
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureLookupByTypeAt(int index)
		{
			if (index >= _lookupCapacity)
			{
				_lookupCapacity = MathUtils.RoundUpToPowerOfTwo(index + 1);
				_eventsLookup = _eventsLookup.Resize(_lookupCapacity);
				_inputsLookup = _inputsLookup.Resize(_lookupCapacity);
			}
		}
	}

	internal struct InputKind
	{
	}
}
