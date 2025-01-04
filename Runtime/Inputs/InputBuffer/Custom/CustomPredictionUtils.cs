using System;
using System.Linq;

namespace Massive.Netcode
{
	public static class CustomPredictionUtils
	{
		public static bool IsImplementedFor(Type type)
		{
			return type.GetInterfaces()
				.Any(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(ICustomInput<>));
		}

		public static InputBuffer<T> CreateCustomInputBuffer<T>(int startTick, int bufferSize)
		{
			return (InputBuffer<T>)ReflectionUtils.CreateGeneric(typeof(CustomInputBuffer<>), typeof(T), startTick, bufferSize);
		}
	}
}
