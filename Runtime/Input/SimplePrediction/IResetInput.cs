using System;
using System.Linq;

namespace Massive.Netcode
{
	public interface IResetInput
	{
		public static bool IsImplementedFor(Type type)
		{
			return type.GetInterfaces().Any(@interface => @interface == typeof(IResetInput));
		}
	}
}
