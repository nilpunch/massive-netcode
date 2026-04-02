using System;
using UnityEngine.Scripting;

namespace Massive.Netcode
{
	[Preserve]
	[Authoritive]
	public struct PlayerConnectedEvent : IEvent
	{
		public Guid PlayerGuid;
	}
}
