using System;

namespace Massive.Netcode
{
	public struct PlayerDisconnectedEvent : IEvent
	{
		public Guid PlayerGuid;
	}
}
