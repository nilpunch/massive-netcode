using System.Threading.Tasks;

namespace Massive.Netcode.Samples
{
	public struct ClientConnection : IResetInput { public bool JustConnected; }

	public struct Player { public Entity Client; }

	public struct PlayerInput { public bool IsShooting; }

	public struct Session { public bool IsFinished; }

	public class ClientInputSample
	{
		private readonly Registry _simulation;
		private readonly ClientRegistry _clients;

		public ClientInputSample()
		{
			_simulation = new Registry();
			_clients = new ClientRegistry();
		}

		public async void Run()
		{
			int tick = 0;

			while (true)
			{
				if (_clients.GetGlobalInput<Session>(tick).IsFinished)
				{
					break;
				}

				UpdatePlayerCreation(tick);
				UpdateShootingLogic(tick);

				await Task.Yield();
			}
		}

		private void UpdatePlayerCreation(int tick)
		{
			var connections = _clients.DataSet<InputBuffer<ClientConnection>>();

			foreach (var client in _clients.View().Include<InputBuffer<ClientConnection>>())
			{
				if (connections.Get(client).GetInput(tick).JustConnected)
				{
					var player = _simulation.CreateEntity();
					_simulation.Assign(player, new Player() { Client = _clients.GetEntity(client) });
				}
			}
		}

		private void UpdateShootingLogic(int tick)
		{
			foreach (var player in _simulation.View().Include<Player>())
			{
				var playerInput = _clients.GetInput<PlayerInput>(_simulation.Get<Player>(player).Client, tick);
				
				if (playerInput.IsShooting)
				{
					// Perform shooting
				}
			}
		}
	}
}
