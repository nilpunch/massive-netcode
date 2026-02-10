using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Massive.Netcode
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct Input<TInput> where TInput : IInput
	{
		public readonly TInput LastFreshInput;
		public readonly int TicksPassed;
		public readonly bool IsApproved;

		public Input(TInput lastFreshInput, int ticksPassed, bool isApproved)
		{
			LastFreshInput = lastFreshInput;
			TicksPassed = ticksPassed;
			IsApproved = isApproved;
		}

		public bool IsFresh
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => TicksPassed == 0;
		}

		public static readonly Input<TInput> Stale = new Input<TInput>(Default<TInput>.Value, int.MaxValue, false);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<TInput> Aged()
		{
			var clampedNextTick = MathUtils.SaturationAdd(TicksPassed, 1);
			return new Input<TInput>(LastFreshInput, clampedNextTick, IsApproved);
		}
	}
}
