using System.Threading.Tasks;

namespace Massive.Netcode.Samples
{
	public struct ConnectionInput : IResetInput { public bool JustConnected; }

	public struct PlayerInput { public bool IsShooting; }

	public struct SessionInput { public bool IsFinished; }

	public struct Player { public Entity Client; }

	public class ClientInputSample
	{
		private readonly Registry _simulation;
		private readonly ClientRegistry _clients;

		public ClientInputSample()
		{
			_simulation = new Registry();
			_clients = new ClientRegistry();
		}

		public void ConnectClient(int playerSpawnTick)
		{
			var client = _clients.CreateClient();
			_clients.SetInput(client, playerSpawnTick, new ConnectionInput() { JustConnected = true });
		}

		public void ApplyPlayerInput(Entity client, int tick, PlayerInput playerInput)
		{
			_clients.SetInput(client, tick, playerInput);
		}

		public void FinishSession(int finishAtTick)
		{
			_clients.SetGlobalInput(finishAtTick, new SessionInput() { IsFinished = true });
		}

		public async void Run()
		{
			int tick = 0;

			while (true)
			{
				if (_clients.GetGlobalInput<SessionInput>(tick).IsFinished)
				{
					break;
				}

				UpdatePlayerSpawn(tick);
				UpdateShootingLogic(tick);

				await Task.Yield();

				tick += 1;
			}
		}

		private void UpdatePlayerSpawn(int tick)
		{
			var connections = _clients.DataSet<InputBuffer<ConnectionInput>>();

			foreach (var client in _clients.View().Include<InputBuffer<ConnectionInput>>())
			{
				if (connections.Get(client).GetInput(tick).JustConnected)
				{
					_simulation.CreateEntity(new Player() { Client = _clients.GetEntity(client) });
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
