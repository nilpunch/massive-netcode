namespace Massive.Netcode
{
	public interface IConnectionListener
	{
		bool TryAccept(out Connection connection);
		void ReturnToPool(Connection connection);
	}
}
