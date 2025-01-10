using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Massive.Netcode.Samples
{
	public struct Player { public int ClientId; }

	public struct PlayerSpawnEvent { public bool IsTriggered; }

	public struct PlayerShootingInput { public bool IsShooting; }
	public struct SessionInput { public bool IsFinished; }

	// Smooth input prediction for analog inputs.
	// Note: Floats are used here for simplicity. Replace them with deterministic types in real usage.
	public struct PlayerMovingInput : IFadeOutInput<PlayerMovingInput>
	{
		public Vector2 Direction;

		public PlayerMovingInput FadeOut(int ticksPassed, in FadeOutConfig config)
		{
			float fadeOutPercent = Math.Clamp((ticksPassed - config.StartDecayTick) / (float)config.DecayDurationTicks, 0f, 1f);
			return new PlayerMovingInput()
			{
				Direction = Direction * (1f - fadeOutPercent),
			};
		}
	}

	public class SimulationSample
	{
		private readonly Simulation _simulation;

		public SimulationSample()
		{
			_simulation = new Simulation();

			_simulation.Systems.Add(new SpawnPlayersSystem(_simulation.Registry));
			_simulation.Systems.Add(new ShootingSystem(_simulation.Registry));
		}

		// Modify inputs via RPC or any other source, in any order, at any time.
		public void ConnectClient(int client, int connectionTick)
		{
			_simulation.Input.SetAt(connectionTick, client, new PlayerSpawnEvent());
		}

		public void ApplyPlayerInput(int client, int tick, PlayerShootingInput playerInput)
		{
			_simulation.Input.SetAt(tick, client, playerInput);
		}

		public void FinishSession(int finishTick)
		{
			_simulation.Input.SetGlobalAt(finishTick, new SessionInput() { IsFinished = true });
		}

		public async void Run()
		{
			// The target tick must be synchronized with server.
			int targetTick = 0;

			while (true)
			{
				if (_simulation.Input.GetGlobal<SessionInput>().LastActual().IsFinished)
				{
					break;
				}

				// Automatic rollbacks and resimulation based on input changes.
				_simulation.Loop.FastForwardToTick(targetTick);

				targetTick += 1;

				await Task.Yield();
			}
		}
	}

	public class SpawnPlayersSystem : ISimulationSystem
	{
		private readonly Registry _registry;
		private readonly SimulationInput _input;

		public SpawnPlayersSystem(Registry registry)
		{
			_registry = registry;
			_input = registry.Service<SimulationInput>();
		}

		public void Update(int tick)
		{
			foreach (var (client, playerSpawnEvent) in _input.GetAll<PlayerSpawnEvent>())
			{
				if (playerSpawnEvent.Actual().IsTriggered)
				{
					_registry.CreateEntity(new Player() { ClientId = client });
				}
			}
		}
	}

	public class ShootingSystem : ISimulationSystem
	{
		private readonly Registry _registry;
		private readonly SimulationInput _input;

		public ShootingSystem(Registry registry)
		{
			_registry = registry;
			_input = registry.Service<SimulationInput>();
		}

		public void Update(int tick)
		{
			_registry.View().ForEach((ref Player player) =>
			{
				var playerInput = _input.Get<PlayerShootingInput>(player.ClientId).LastActual();

				if (playerInput.IsShooting)
				{
					// Perform shooting.
				}
			});
		}
	}
}
