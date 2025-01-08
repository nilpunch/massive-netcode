using System;
using System.Linq;

namespace Massive.Netcode
{
	public static class FadeOutInput
	{
		public static bool IsImplementedFor(Type type)
		{
			return type.GetInterfaces()
				.Any(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IFadeOutInput<>));
		}

		public static InputBuffer<T> CreateInputBuffer<T>(int startTick, int bufferSize, FadeOutConfig fadeOutConfig)
		{
			return (InputBuffer<T>)ReflectionUtils.CreateGeneric(typeof(FadeOutInputBuffer<>), typeof(T), startTick, bufferSize, fadeOutConfig);
		}
	}
}
