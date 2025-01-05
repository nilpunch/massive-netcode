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
			_simulation.Systems.Add(new SpawnPlayersSystem(_simulation));
			_simulation.Systems.Add(new ShootingSystem(_simulation));
		}

		public void ConnectClient(int client, int spawnTick)
		{
			_simulation.Inputs.SetInput(client, spawnTick, new PlayerSpawnInput() { NeedToSpawnPlayer = true });
		}

		public void ApplyPlayerInput(int client, int tick, PlayerInput playerInput)
		{
			_simulation.Inputs.SetInput(client, tick, playerInput);
		}

		public void FinishSession(int finishAtTick)
		{
			_simulation.Inputs.SetMasterInput(finishAtTick, new SessionInput() { IsFinished = true });
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
	}

	public class SpawnPlayersSystem : ISimulation
	{
		private readonly Simulation _simulation;

		public SpawnPlayersSystem(Simulation simulation)
		{
			_simulation = simulation;
		}

		public void Update(int tick)
		{
			var spawnInputs = _simulation.Inputs.GetAllInputs<PlayerSpawnInput>();

			foreach (var client in spawnInputs)
			{
				if (spawnInputs.Get(client).GetInput(tick).NeedToSpawnPlayer)
				{
					_simulation.Registry.CreateEntity(new Player() { ClientId = client });
				}
			}
		}
	}

	public class ShootingSystem : ISimulation
	{
		private readonly Simulation _simulation;

		public ShootingSystem(Simulation simulation)
		{
			_simulation = simulation;
		}

		public void Update(int tick)
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
