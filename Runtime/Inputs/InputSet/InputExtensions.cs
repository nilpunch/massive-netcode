using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public static class InputExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TInput LastFresh<TInput>(this Input<TInput> input) where TInput : IInput
		{
			return input.LastFreshInput;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TInput FreshOrDefault<TInput>(this Input<TInput> input) where TInput : IInput
		{
			return input.IsFresh ? input.LastFreshInput : Default<TInput>.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TInput FreshOrDefault<TInput>(this Input<TInput> input, TInput fallback) where TInput : IInput
		{
			return input.IsFresh ? input.LastFreshInput : fallback;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TInput FadeOut<TInput>(this Input<TInput> input, in FadeOutConfig fadeOutConfig)
			where TInput : IInput, IFadeOutInput<TInput>
		{
			return input.LastFreshInput.FadeOut(input.TicksPassed, fadeOutConfig);
		}
	}
}
