using System;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public struct PredictionConfig
	{
		public int StartDecay;
		public int DecayDuration;
	}

	public abstract class InputBuffer<TInput> : IInputPrediction
	{
		private readonly CyclicList<Input> _inputs;

		private int _lastTickWithoutPrediction;

		protected InputBuffer(int startTick, int bufferSize)
		{
			_inputs = new CyclicList<Input>(bufferSize, startTick);

			_inputs.Append(Input.Actual(default));
			_lastTickWithoutPrediction = startTick;
		}

		public event Action<int> InputChanged;

		public void Reset(int startTick)
		{
			_inputs.Reset(startTick);
			_inputs.Append(Input.Actual(default));
			_lastTickWithoutPrediction = startTick;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TInput GetInput(int tick)
		{
			return _inputs[tick].Value;
		}

		public void PopulateInputsUpTo(int tick)
		{
			while (_inputs.TailIndex <= tick)
			{
				_inputs.Append(Input.Predicted(Predict(_inputs[_lastTickWithoutPrediction].Value, _inputs.TailIndex - _lastTickWithoutPrediction)));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActualInput(int tick, TInput input)
		{
			if (_inputs.TailIndex <= tick)
			{
				PopulateInputsUpTo(tick - 1);
				_inputs.Append(Input.Actual(input));
			}
			else
			{
				_inputs[tick] = Input.Actual(input);
			}

			_lastTickWithoutPrediction = Math.Max(_lastTickWithoutPrediction, tick);
			ReevaluateFrom(tick);

			InputChanged?.Invoke(tick);
		}

		public void SetManyActualInputs((int tick, TInput input)[] inputs)
		{
			var reevaluateFrame = int.MaxValue; 
			
			foreach (var (frame, input) in inputs)
			{
				_inputs[frame] = Input.Actual(input);
				_lastTickWithoutPrediction = Math.Max(_lastTickWithoutPrediction, frame);
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
				if (_inputs[tick].IsPredicted)
				{
					if (lastConfirmedTick != -1)
					{
						_inputs[tick].Value = Predict(_inputs[lastConfirmedTick].Value, tick - lastConfirmedTick);
					}
				}
				else
				{
					lastConfirmedTick = tick;
				}
			}
		}

		private struct Input
		{
			public TInput Value;
			public bool IsPredicted;

			private Input(TInput value, bool isPredicted)
			{
				Value = value;
				IsPredicted = isPredicted;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Input Actual(TInput input)
			{
				return new Input(input, false);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Input Predicted(TInput input)
			{
				return new Input(input, true);
			}
		}
	}
}
