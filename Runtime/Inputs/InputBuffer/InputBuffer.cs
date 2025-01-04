using System;

namespace Massive.Netcode
{
	public abstract class InputBuffer<TInput> : IInputPrediction
	{
		private readonly CyclicList<bool> _isPredicted;
		private readonly CyclicList<TInput> _inputs;

		private int _lastTickWithAnyInput;

		protected InputBuffer(int startTick, int bufferSize)
		{
			_inputs = new CyclicList<TInput>(bufferSize, startTick);
			_isPredicted = new CyclicList<bool>(bufferSize, startTick);

			_inputs.Append(default);
			_isPredicted.Append(false);
			_lastTickWithAnyInput = startTick;
		}

		public event Action<int> InputChanged;

		public void Reset(int startTick)
		{
			_inputs.Reset(startTick);
			_isPredicted.Reset(startTick);

			_inputs.Append(default);
			_isPredicted.Append(false);
			_lastTickWithAnyInput = startTick;
		}

		public TInput GetInput(int tick)
		{
			return _inputs[tick];
		}

		public void PopulateInputsUpTo(int tick)
		{
			while (_inputs.TailIndex - 1 < tick)
			{
				_inputs.Append(Predict(_inputs[_lastTickWithAnyInput], _inputs.TailIndex - _lastTickWithAnyInput));
				_isPredicted.Append(false);
			}
		}

		public void InsertInput(int tick, TInput input)
		{
			_inputs[tick] = input;
			_isPredicted[tick] = false;
			_lastTickWithAnyInput = Math.Max(_lastTickWithAnyInput, tick);
			ReevaluateFrom(tick);

			InputChanged?.Invoke(tick);
		}

		public void InsertManyInputs((int tick, TInput input)[] inputs)
		{
			var reevaluateFrame = int.MaxValue; 
			
			foreach (var (frame, input) in inputs)
			{
				_inputs[frame] = input;
				_isPredicted[frame] = false;
				_lastTickWithAnyInput = Math.Max(_lastTickWithAnyInput, frame);
				reevaluateFrame = Math.Min(reevaluateFrame, frame);
			}

			ReevaluateFrom(reevaluateFrame);

			InputChanged?.Invoke(reevaluateFrame);
		}

		protected abstract TInput Predict(TInput input, int ticksPassed);

		private void ReevaluateFrom(int tick)
		{
			int lastConfirmedTick = -1;
			for (int i = tick; i < _inputs.TailIndex; i++)
			{
				if (_isPredicted[tick])
				{
					if (lastConfirmedTick != -1)
					{
						_inputs[tick] = Predict(_inputs[lastConfirmedTick], tick - lastConfirmedTick);
					}
				}
				else
				{
					lastConfirmedTick = tick;
				}
			}
		}
	}
}
