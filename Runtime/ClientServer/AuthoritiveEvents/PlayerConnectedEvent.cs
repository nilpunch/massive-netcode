using System;

namespace Massive.Netcode
{
	public struct PlayerConnectedEvent : IEvent, IAuthoritive
	{
		public Guid PlayerGuid;
	}
}
