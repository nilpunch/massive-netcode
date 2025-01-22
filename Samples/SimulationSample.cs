using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Massive.Netcode.Samples
{
	public struct Player { public int ClientId; }

	public struct PlayerSpawnEvent { }

	public struct PlayerShootingInput { public bool IsShooting; }
	public struct SessionInput { public bool IsFinished; }

	public class SimulationSample
	{
		private readonly Session _session;

		public SimulationSample()
		{
			_session = new Session();

			_session.Simulations.Add(new SpawnPlayers(_session.Registry, _session.Inputs));
			_session.Simulations.Add(new Shooting(_session.Registry, _session.Inputs));
		}

		// Modify inputs via RPC or any other source, in any order, at any time.
		public void ConnectClient(int client, int connectionTick)
		{
			_session.Inputs.SetAt(connectionTick, client, new PlayerSpawnEvent());
		}

		public void ApplyPlayerInput(int client, int tick, PlayerShootingInput playerInput)
		{
			_session.Inputs.SetAt(tick, client, playerInput);
		}

		public void FinishSession(int finishTick)
		{
			_session.Inputs.SetGlobalAt(finishTick, new SessionInput() { IsFinished = true });
		}

		public async void Run()
		{
			// The target tick must be synchronized with server.
			int targetTick = 0;

			while (true)
			{
				if (_session.Inputs.GetGlobal<SessionInput>().LastActual().IsFinished)
				{
					break;
				}

				// Automatic rollbacks and resimulation based on input changes.
				_session.Loop.FastForwardToTick(targetTick);

				targetTick += 1;

				await Task.Yield();
			}
		}
	}

	public class SpawnPlayers : ISimulation
	{
		private readonly Registry _registry;
		private readonly Inputs _inputs;

		public SpawnPlayers(Registry registry, Inputs inputs)
		{
			_registry = registry;
			_inputs = inputs;
		}

		public void Update(int tick)
		{
			foreach (var (client, spawnEvent) in _inputs.GetAllActual<PlayerSpawnEvent>())
			{
				_registry.CreateEntity(new Player() { ClientId = client });
			}
		}
	}

	public class Shooting : ISimulation
	{
		private readonly Registry _registry;
		private readonly Inputs _inputs;

		public Shooting(Registry registry, Inputs inputs)
		{
			_registry = registry;
			_inputs = inputs;
		}

		public void Update(int tick)
		{
			_registry.View().ForEach((ref Player player) =>
			{
				var playerInput = _inputs.Get<PlayerShootingInput>(player.ClientId).LastActual();

				if (playerInput.IsShooting)
				{
					// Perform shooting.
				}
			});
		}
	}
}
