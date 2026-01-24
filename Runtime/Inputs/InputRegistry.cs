using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class InputRegistry : IInputSet
	{
		private readonly ChangeTracker _changeTracker;
		private readonly int _startTick;
		private readonly IPredictionReceiver _predictionReceiver;
		private readonly GenericLookup<IInputSet> _eventsLookup = new GenericLookup<IInputSet>();
		private readonly GenericLookup<IInputSet> _inputsLookup = new GenericLookup<IInputSet>();
		private readonly FastList<IInputSet> _events = new FastList<IInputSet>();
		private readonly FastList<IInputSet> _inputs = new FastList<IInputSet>();

		public int Global { get; } = 0;

		public InputRegistry(ChangeTracker changeTracker, int startTick = 0, IPredictionReceiver predictionReceiver = null)
		{
			_changeTracker = changeTracker;
			_startTick = startTick;
			_predictionReceiver = predictionReceiver;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetGlobalAt<T>(int tick) where T : IInput
		{
			return GetInputSet<T>().GetInputs(tick).Get(Global);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetGlobalAt<T>(int tick, T input) where T : IInput
		{
			GetInputSet<T>().SetActual(tick, Global, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplyEventAt<T>(int tick, int localOrder, T data) where T : IEvent
		{
			GetEventSet<T>().SetActual(tick, localOrder, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendEventAt<T>(int tick, T data) where T : IEvent
		{
			GetEventSet<T>().AppendPrediction(tick, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetAt<T>(int tick, int channel) where T : IInput
		{
			return GetInputSet<T>().GetInputs(tick).Get(channel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAt<T>(int tick, int channel, T input) where T : IInput
		{
			GetInputSet<T>().SetActual(tick, channel, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetAllInputsAt<T>(int tick) where T : IInput
		{
			return GetInputSet<T>().GetInputs(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FreshInputsEnumerable<T> GetAllActualAt<T>(int tick) where T : IInput
		{
			return new FreshInputsEnumerable<T>(GetInputSet<T>().GetInputs(tick));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEvents<T> GetEventsAt<T>(int tick) where T : IEvent
		{
			return GetEventSet<T>().GetEvents(tick);
		}

		public void PopulateUpTo(int tick)
		{
			for (var i = 0; i < _inputs.Count; i++)
			{
				_inputs[i].PopulateUpTo(tick);
			}
			for (var i = 0; i < _events.Count; i++)
			{
				_events[i].PopulateUpTo(tick);
			}
		}

		public void DiscardUpTo(int tick)
		{
			for (var i = 0; i < _inputs.Count; i++)
			{
				_inputs[i].DiscardUpTo(tick);
			}
			for (var i = 0; i < _events.Count; i++)
			{
				_events[i].DiscardUpTo(tick);
			}
		}

		public void Reevaluate()
		{
			for (var i = 0; i < _inputs.Count; i++)
			{
				_inputs[i].Reevaluate();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EventSet<T> GetEventSet<T>() where T : IEvent
		{
			var eventSet = (EventSet<T>)_eventsLookup.Find<T>();

			if (eventSet == null)
			{
				eventSet = new EventSet<T>(_changeTracker, _startTick);
				_eventsLookup.Assign<T>(eventSet);
				_events.Add(eventSet);
			}

			return eventSet;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public InputSet<T> GetInputSet<T>() where T : IInput
		{
			var inputSet = (InputSet<T>)_inputsLookup.Find<T>();

			if (inputSet == null)
			{
				inputSet = new InputSet<T>(_changeTracker, _startTick, _predictionReceiver);
				_inputsLookup.Assign<T>(inputSet);
				_inputs.Add(inputSet);
			}

			return inputSet;
		}
	}
}
