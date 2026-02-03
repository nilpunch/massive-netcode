using System.Runtime.InteropServices;

namespace Massive.Netcode
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Event<T> where T : IEvent
	{
		public T Data;
		public int Channel;

		public Event(int channel, T data)
		{
			Data = data;
			Channel = channel;
		}

		public void Deconstruct(out int channel, out T data)
		{
			data = Data;
			channel = Channel;
		}
	}
}
