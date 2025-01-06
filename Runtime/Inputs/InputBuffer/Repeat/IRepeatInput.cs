using System;
using System.Linq;

namespace Massive.Netcode
{
	public interface IRepeatInput
	{
		public static bool IsImplementedFor(Type type)
		{
			return type.GetInterfaces().Any(@interface => @interface == typeof(IRepeatInput));
		}
	}
}
