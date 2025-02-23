using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public struct Event<T>
	{
		public T Data;
		public readonly int Client;

		public Event(T data, int client)
		{
			Data = data;
			Client = client;
		}

		public class ClientComparer : IComparer<Event<T>>
		{
			public static ClientComparer Instance { get; } = new ClientComparer();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int Compare(Event<T> x, Event<T> y)
			{
				return x.Client.CompareTo(y.Client);
			}
		}
	}
}
