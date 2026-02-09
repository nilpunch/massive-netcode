using System;

namespace Massive.Netcode
{
	/// <summary>
	/// Marks inputs/events that should only be sent from server to clients,
	/// never from clients to server.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class AuthoritiveAttribute : Attribute
	{
	}
}
