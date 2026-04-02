using System;
using UnityEngine.Scripting;

namespace Massive.Netcode
{
	[Preserve]
	[Authoritive]
	public struct PlayerDisconnectedEvent : IEvent
	{
		public Guid PlayerGuid;
	}
}
