using System;

namespace Massive.Netcode
{
	public struct PlayerDisconnectedEvent : IEvent, IAuthoritive
	{
		public Guid PlayerGuid;
	}
}
