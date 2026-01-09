namespace Massive.Netcode
{
	public static class NetSystemsExtensions
	{
		/// <summary>
		/// Builds systems and calls <see cref="IInject{TArg}.Inject"/> with <see cref="Session"/> and <see cref="Session.World"/> argument on all builded systems.
		/// </summary>
		public static Systems Build(this Systems systems, Session session)
		{
			return systems.Build()
				.Inject(session)
				.Inject(session.World);
		}
	}
}
