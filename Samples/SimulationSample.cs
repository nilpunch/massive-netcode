using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Massive.Netcode.Samples
{
	public struct Player { public int ClientId; }

	// Default inputs are reset during prediction, behaving like one-time events.
	public struct PlayerSpawnInput { public bool NeedToSpawnPlayer; }

	// Repeat inputs apply the latest actual input during prediction.
	public struct PlayerShootingInput : IRepeatInput { public bool IsShooting; }
	public struct SessionInput : IRepeatInput { public bool IsFinished; }

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

			_simulation.Systems.Add(new SpawnPlayersSystem(_simulation));
			_simulation.Systems.Add(new ShootingSystem(_simulation));
		}

		// Modify inputs via RPC or any other source, in any order, at any time.
		public void ConnectClient(int client, int connectionTick)
		{
			_simulation.Input.SetAt(connectionTick, client, new PlayerSpawnInput() { NeedToSpawnPlayer = true });
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
				if (_simulation.Input.GetGlobal<SessionInput>().IsFinished)
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

	public class SpawnPlayersSystem : SystemBase
	{
		public SpawnPlayersSystem(Simulation simulation) : base(simulation) { }

		public override void Update(int tick)
		{
			foreach (var (client, spawnInput) in Simulation.Input.GetAll<PlayerSpawnInput>())
			{
				if (spawnInput.NeedToSpawnPlayer)
				{
					Simulation.Registry.CreateEntity(new Player() { ClientId = client });
				}
			}
		}
	}

	public class ShootingSystem : SystemBase
	{
		public ShootingSystem(Simulation simulation) : base(simulation) { }

		public override void Update(int tick)
		{
			Simulation.Registry.View().ForEach((ref Player player) =>
			{
				var playerInput = Simulation.Input.Get<PlayerShootingInput>(player.ClientId);

				if (playerInput.IsShooting)
				{
					// Perform shooting.
				}
			});
		}
	}
}
