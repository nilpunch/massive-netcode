using System;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class InputRegistry : IInput
	{
		private readonly ChangeTracker _changeTracker;
		private readonly int _startTick;
		private readonly GenericLookup<object> _eventSets = new GenericLookup<object>();
		private readonly GenericLookup<object> _inputSets = new GenericLookup<object>();
		private readonly FastList<IInput> _allInputs = new FastList<IInput>();

		public int Global { get; } = 0;

		public InputRegistry(ChangeTracker changeTracker, int startTick = 0)
		{
			_changeTracker = changeTracker;
			_startTick = startTick;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetGlobalAt<T>(int tick)
		{
			return GetInputSet<T>().GetInputs(tick).Get(Global);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetGlobalAt<T>(int tick, T input)
		{
			GetInputSet<T>().SetInput(tick, Global, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplyGlobalEventAt<T>(int tick, T data)
		{
			GetEventSet<T>().ApplyEvent(tick, Global, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetAt<T>(int tick, int channel)
		{
			return GetInputSet<T>().GetInputs(tick).Get(channel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAt<T>(int tick, int channel, T input)
		{
			GetInputSet<T>().SetInput(tick, channel, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetAllInputsAt<T>(int tick)
		{
			return GetInputSet<T>().GetInputs(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllActualInputs<T> GetAllActualAt<T>(int tick)
		{
			return new AllActualInputs<T>(GetInputSet<T>().GetInputs(tick));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEvents<T> GetEventsAt<T>(int tick)
		{
			return GetEventSet<T>().GetEvents(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplyEventAt<T>(int tick, int channel, T data)
		{
			GetEventSet<T>().ApplyEvent(tick, channel, data);
		}

		public void PopulateUpTo(int tick)
		{
			for (var i = 0; i < _allInputs.Count; i++)
			{
				_allInputs[i].PopulateUpTo(tick);
			}
		}

		public void DiscardUpTo(int tick)
		{
			for (var i = 0; i < _allInputs.Count; i++)
			{
				_allInputs[i].DiscardUpTo(tick);
			}
		}

		public void Reevaluate()
		{
			for (var i = 0; i < _allInputs.Count; i++)
			{
				_allInputs[i].Reevaluate();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EventSet<T> GetEventSet<T>()
		{
			var eventSet = (EventSet<T>)_eventSets.Find<T>();

			if (eventSet == null)
			{
				eventSet = new EventSet<T>(_changeTracker, _startTick);
				_eventSets.Assign<T>(eventSet);
				_allInputs.Add(eventSet);
			}

			return eventSet;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public InputSet<T> GetInputSet<T>()
		{
			var inputSet = (InputSet<T>)_inputSets.Find<T>();

			if (inputSet == null)
			{
				inputSet = new InputSet<T>(_changeTracker, _startTick);
				_inputSets.Assign<T>(inputSet);
				_allInputs.Add(inputSet);
			}

			return inputSet;
		}
	}
}
