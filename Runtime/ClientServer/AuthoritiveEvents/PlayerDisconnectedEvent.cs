using System;

namespace Massive.Netcode
{
	[Authoritive]
	public struct PlayerDisconnectedEvent : IEvent
	{
		public Guid PlayerGuid;
	}
}
