namespace Massive.Netcode.Samples
{
	public struct Player
	{
	}

	public struct PlayerInput
	{
		public bool IsShooting;
	}

	public class InputReadSample
	{
		public void AddNewPlayer(int tick)
		{
			
		}
		
		public void Run()
		{
			var registry = new Registry();

			var playerRegistry = new ClientRegistry(120);

			
		}

		private void Update(Registry registry)
		{
			foreach (var player in registry.View().Include<Player>())
			{
				var input = registry.Get<PlayerInput>(player);

				if (input.IsShooting)
				{
					// Perform shooting
				}
			}
		}
	}
}
