using System.Runtime.InteropServices;

namespace Massive.Netcode
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ActualEvent<T>
	{
		public T Event;
		public ushort LocalOrder;
	}
}
