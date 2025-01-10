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
}
