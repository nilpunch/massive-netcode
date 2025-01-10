using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public static class InputExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TInput LastActual<TInput>(this Input<TInput> input)
		{
			return input.LastActualInput;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsActual<TInput>(this Input<TInput> input)
		{
			return input.TicksPassed == 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TInput Actual<TInput>(this Input<TInput> input, TInput fallback = default)
		{
			return input.TicksPassed == 0 ? input.LastActualInput : fallback;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TInput FadeOut<TInput>(this Input<TInput> input, in FadeOutConfig fadeOutConfig) where TInput : IFadeOutInput<TInput>
		{
			return input.LastActualInput.FadeOut(input.TicksPassed, fadeOutConfig);
		}
	}
}
