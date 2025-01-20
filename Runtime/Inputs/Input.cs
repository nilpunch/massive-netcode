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

		public static Input<TInput> Inactual
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Input<TInput>(default, int.MaxValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<TInput> PassTick()
		{
			var clampedNextTick = MathUtils.SaturationAdd(TicksPassed, 1);
			return new Input<TInput>(LastActualInput, clampedNextTick);
		}
	}
}
