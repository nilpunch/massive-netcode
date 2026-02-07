using System;

namespace Massive.Netcode
{
	public struct PlayerConnectedEvent : IEvent
	{
		public Guid PlayerGuid;
	}
}
