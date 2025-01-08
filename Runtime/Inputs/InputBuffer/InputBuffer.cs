using System;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public readonly struct Input<TInput>
	{
		public readonly TInput LastActualInput;
		public readonly int TicksPassed;

		public Input(TInput lastActualInput, int ticksPassed)
		{
			LastActualInput = lastActualInput;
			TicksPassed = ticksPassed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<TInput> PassTick()
		{
			return new Input<TInput>(LastActualInput, TicksPassed + 1);
		}
	}

	public class InputBuffer<TInput> : IInputPrediction
	{
		private readonly CyclicList<Input<TInput>> _inputs;

		public InputBuffer(int startTick, int bufferSize)
		{
			_inputs = new CyclicList<Input<TInput>>(bufferSize, startTick);

			_inputs.Append(new Input<TInput>());
		}

		public event Action<int> InputChanged;

		public void Reset(int startTick)
		{
			_inputs.Reset(startTick);
			_inputs.Append(new Input<TInput>());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<TInput> GetInput(int tick)
		{
			return _inputs[tick];
		}

		public void PopulateInputsUpTo(int tick)
		{
			while (_inputs.TailIndex <= tick)
			{
				_inputs.Append(_inputs[_inputs.TailIndex - 1].PassTick());
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActualInput(int tick, TInput input)
		{
			PopulateInputsUpTo(tick);

			_inputs[tick] = new Input<TInput>(input, 0);

			ReevaluateFrom(tick);

			InputChanged?.Invoke(tick);
		}

		public void SetManyActualInputs((int tick, TInput input)[] inputs)
		{
			var earlyestChangedTick = int.MaxValue;

			foreach (var (tick, input) in inputs)
			{
				PopulateInputsUpTo(tick);

				_inputs[tick] = new Input<TInput>(input, 0);
				earlyestChangedTick = Math.Min(earlyestChangedTick, tick);
			}

			ReevaluateFrom(earlyestChangedTick);

			InputChanged?.Invoke(earlyestChangedTick);
		}

		public void ReevaluateFrom(int tick)
		{
			for (int i = tick + 1; i < _inputs.TailIndex; i++)
			{
				if (_inputs[i].TicksPassed != 0)
				{
					_inputs[i] = _inputs[i - 1].PassTick();
				}
			}
		}
	}
}
