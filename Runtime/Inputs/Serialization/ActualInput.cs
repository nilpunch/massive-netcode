using System.Runtime.InteropServices;

namespace Massive.Netcode
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ActualInput<T>
	{
		public T Input;
		public ushort Channel;
	}
}
