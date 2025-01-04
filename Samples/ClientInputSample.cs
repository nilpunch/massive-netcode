using System.Threading.Tasks;

namespace Massive.Netcode.Samples
{
	public struct SpawnInput : IResetInput { public bool Spawn; }

	public struct PlayerInput { public bool IsShooting; }

	public struct SessionInput { public bool IsFinished; }

	public struct Player { public int ClientId; }

	public class ClientInputSample
	{
		private readonly Registry _registry;
		private readonly SimulationInput _input;
		private readonly ClientInput _clientInput;
		private readonly Time _time;

		public ClientInputSample()
		{
			_registry = new Registry();
			_clientInput = new ClientInput();
			_time = new Time();
			_input = new SimulationInput(_clientInput, _time);
		}

		public void ConnectClient(int client, int spawnTick)
		{
			_clientInput.SetInput(client, spawnTick, new SpawnInput() { Spawn = true });
		}

		public void ApplyPlayerInput(int client, int tick, PlayerInput playerInput)
		{
			_clientInput.SetInput(client, tick, playerInput);
		}

		public void FinishSession(int finishAtTick)
		{
			_clientInput.SetGlobalInput(finishAtTick, new SessionInput() { IsFinished = true });
		}

		public async void Run()
		{
			while (true)
			{
				if (_input.GetGlobalInput<SessionInput>().IsFinished)
				{
					break;
				}

				UpdatePlayerSpawn();
				UpdateShootingLogic();

				await Task.Yield();

				_time.Tick += 1;
			}
		}

		private void UpdatePlayerSpawn()
		{
			var spawns = _clientInput.GetAllInputs<SpawnInput>();

			foreach (var client in spawns)
			{
				if (spawns.Get(client).GetInput(_time.Tick).Spawn)
				{
					_registry.CreateEntity(new Player() { ClientId = client });
				}
			}
		}

		private void UpdateShootingLogic()
		{
			_registry.View().ForEach((ref Player player) =>
			{
				var playerInput = _input.GetInput<PlayerInput>(player.ClientId);
	
				if (playerInput.IsShooting)
				{
					// Perform shooting
				}
			});
		}
	}
}
