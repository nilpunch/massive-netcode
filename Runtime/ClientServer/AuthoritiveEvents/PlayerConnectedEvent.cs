using System;

namespace Massive.Netcode
{
	[Authoritive]
	public struct PlayerConnectedEvent : IEvent
	{
		public Guid PlayerGuid;
	}
}
