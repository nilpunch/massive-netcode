using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public readonly struct Event<T>
	{
		public readonly int Channel;
		public readonly T Data;

		public Event(int channel, T data)
		{
			Channel = channel;
			Data = data;
		}

		public void Deconstruct(out int channel, out T data)
		{
			channel = Channel;
			data = Data;
		}

		public class ChannelComparer : IComparer<Event<T>>
		{
			public static ChannelComparer Instance { get; } = new ChannelComparer();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int Compare(Event<T> x, Event<T> y)
			{
				return x.Channel.CompareTo(y.Channel);
			}
		}
	}
}
