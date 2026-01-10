using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public readonly struct Input<TInput> where TInput : IInput
	{
		public readonly TInput LastActualInput;
		public readonly int TicksPassed;

		public Input(TInput lastActualInput, int ticksPassed)
		{
			LastActualInput = lastActualInput;
			TicksPassed = ticksPassed;
		}

		public static readonly Input<TInput> Inactual = new Input<TInput>(Default<TInput>.Value, int.MaxValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<TInput> Aged()
		{
			var clampedNextTick = MathUtils.SaturationAdd(TicksPassed, 1);
			return new Input<TInput>(LastActualInput, clampedNextTick);
		}
	}
}
