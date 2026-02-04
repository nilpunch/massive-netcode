namespace Massive.Netcode
{
	public interface IConnectionListener
	{
		void Start();
		void Stop();

		bool TryAccept(out Connection connection);
		void ReturnToPool(Connection connection);
	}
}
