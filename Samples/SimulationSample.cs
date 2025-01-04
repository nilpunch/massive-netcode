using System.Threading.Tasks;

namespace Massive.Netcode.Samples
{
	public struct PlayerSpawnInput : IResetInput { public bool NeedToSpawnPlayer; }

	public struct PlayerInput { public bool IsShooting; }

	public struct SessionInput { public bool IsFinished; }

	public struct Player { public int ClientId; }

	public class SimulationSample
	{
		private readonly Simulation _simulation;

		public SimulationSample()
		{
			_simulation = new Simulation();
		}

		public void ConnectClient(int client, int spawnTick)
		{
			_simulation.InputRegistry.SetInput(client, spawnTick, new PlayerSpawnInput() { NeedToSpawnPlayer = true });
		}

		public void ApplyPlayerInput(int client, int tick, PlayerInput playerInput)
		{
			_simulation.InputRegistry.SetInput(client, tick, playerInput);
		}

		public void FinishSession(int finishAtTick)
		{
			_simulation.InputRegistry.SetMasterInput(finishAtTick, new SessionInput() { IsFinished = true });
		}

		public async void Run()
		{
			int tick = 0;
			
			while (true)
			{
				if (_simulation.Inputs.GetMasterInput<SessionInput>().IsFinished)
				{
					break;
				}

				_simulation.Loop.FastForwardToTick(tick);

				await Task.Yield();
				tick += 1;
			}
		}

		private void UpdatePlayerSpawn()
		{
			var spawnInputs = _simulation.Inputs.GetAllInputs<PlayerSpawnInput>();
			var currentTick = _simulation.Time.Tick;

			foreach (var client in spawnInputs)
			{
				if (spawnInputs.Get(client).GetInput(currentTick).NeedToSpawnPlayer)
				{
					_simulation.Registry.CreateEntity(new Player() { ClientId = client });
				}
			}
		}

		private void UpdateShootingLogic()
		{
			_simulation.Registry.View().ForEach((ref Player player) =>
			{
				var playerInput = _simulation.Inputs.GetInput<PlayerInput>(player.ClientId);
	
				if (playerInput.IsShooting)
				{
					// Perform shooting
				}
			});
		}
	}
}
