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

		public static InputBuffer<T> CreateCustomInputBuffer<T>(int startTick, int bufferSize)
		{
			return (InputBuffer<T>)ReflectionUtils.CreateGeneric(typeof(FadeOutInputBuffer<>), typeof(T), startTick, bufferSize);
		}
	}
}
