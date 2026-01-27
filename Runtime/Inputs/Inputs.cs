using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class Inputs : IInputSet, ISimulation
	{
		private int _startTick;
		private readonly ChangeTracker _changeTracker;
		private readonly IPredictionReceiver _predictionReceiver;

		private IInputSet[] _eventsLookup = Array.Empty<IInputSet>();
		private IInputSet[] _inputsLookup = Array.Empty<IInputSet>();
		private IEventSerializer[] _eventsSerializerLookup = Array.Empty<IEventSerializer>();
		private IInputSerializer[] _inputsSerializerLookup = Array.Empty<IInputSerializer>();
		private int _lookupCapacity;

		public FastList<IInputSet> EventSets { get; } = new FastList<IInputSet>();
		public FastList<IInputSet> InputSets { get; } = new FastList<IInputSet>();

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
		public void SetActualEvent<T>(int localOrder, T data) where T : IEvent
		{
			SetActualEventAt(CurrentTick, localOrder, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActualEventAt<T>(int tick, int localOrder, T data) where T : IEvent
		{
			GetEventSet<T>().SetActual(tick, localOrder, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendActualEvent<T>(T data) where T : IEvent
		{
			AppendActualEventAt(CurrentTick, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendActualEventAt<T>(int tick, T data) where T : IEvent
		{
			GetEventSet<T>().AppendActual(tick, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendPredictionEvent<T>(T data) where T : IEvent
		{
			AppendPredictionEventAt(CurrentTick, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendPredictionEventAt<T>(int tick, T data) where T : IEvent
		{
			GetEventSet<T>().AppendPrediction(tick, data);
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

			var eventSet = new EventSet<T>(_changeTracker, _startTick, _predictionReceiver);
			_eventsLookup[info.Index] = eventSet;
			_eventsSerializerLookup[info.Index] = new UnmanagedEventSerializer<T>(eventSet);
			EventSets.Add(eventSet);

			return eventSet;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IInputSet GetEventSetReflected(Type inputType)
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

			var createMethod = typeof(Sets).GetMethod(nameof(GetEventSet));
			var genericMethod = createMethod?.MakeGenericMethod(inputType);
			return (IInputSet)genericMethod?.Invoke(this, new object[] { });
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEventSerializer GetEventSetSerializer<T>() where T : IEvent
		{
			var info = TypeId<InputKind, T>.Info;

			EnsureLookupByTypeAt(info.Index);
			var candidate = _eventsSerializerLookup[info.Index];

			if (candidate != null)
			{
				return candidate;
			}

			// Warmup serializer.
			GetEventSet<T>();

			return _eventsSerializerLookup[info.Index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEventSerializer GetEventSetSerializer(Type inputType)
		{
			// Warmup serializer.
			GetEventSetReflected(inputType);

			var info = TypeId<InputKind>.GetInfo(inputType);
			return _eventsSerializerLookup[info.Index];
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

			var inputSet = new InputSet<T>(_changeTracker, _startTick, _predictionReceiver);
			_inputsLookup[info.Index] = inputSet;
			_inputsSerializerLookup[info.Index] = new UnmanagedInputSerializer<T>(inputSet);
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

			var createMethod = typeof(Sets).GetMethod(nameof(GetInputSet));
			var genericMethod = createMethod?.MakeGenericMethod(inputType);
			return (IInputSet)genericMethod?.Invoke(this, new object[] { });
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IInputSerializer GetInputSetSerializer<T>() where T : IInput
		{
			var info = TypeId<InputKind, T>.Info;

			EnsureLookupByTypeAt(info.Index);
			var candidate = _inputsSerializerLookup[info.Index];

			if (candidate != null)
			{
				return candidate;
			}

			// Warmup serializer.
			GetInputSet<T>();

			return _inputsSerializerLookup[info.Index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IInputSerializer GetInputSetSerializer(Type inputType)
		{
			// Warmup serializer.
			GetInputSetReflected(inputType);

			var info = TypeId<InputKind>.GetInfo(inputType);
			return _inputsSerializerLookup[info.Index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureLookupByTypeAt(int index)
		{
			if (index >= _lookupCapacity)
			{
				_lookupCapacity = MathUtils.RoundUpToPowerOfTwo(index + 1);
				_eventsLookup = _eventsLookup.Resize(_lookupCapacity);
				_inputsLookup = _inputsLookup.Resize(_lookupCapacity);
				_eventsSerializerLookup = _eventsSerializerLookup.Resize(_lookupCapacity);
				_inputsSerializerLookup = _inputsSerializerLookup.Resize(_lookupCapacity);
			}
		}
	}

	internal struct InputKind
	{
	}
}
