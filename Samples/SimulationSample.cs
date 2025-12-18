using System.Threading.Tasks;

namespace Massive.Netcode.Samples
{
	public struct Player { public int InputChannel; }

	public struct PlayerSpawnEvent { }
	public struct SessionFinishedEvent { }

	public struct PlayerShootingInput { public bool IsShooting; }

	public class SimulationSample
	{
		private Session Session { get; }

		public SimulationSample()
		{
			Session = new Session();

			Session.Systems
				.Instance(new SpawnPlayers(Session.Inputs))
				.Instance(new Shooting(Session.Inputs))
				.Build(Session.World);

			Session.Simulations.Add(new SystemsSimulation(Session.Systems));
		}

		// Modify inputs via RPC or any other source, in any order, at any time.
		public void ConnectClient(int inputChannel, int connectionTick)
		{
			Session.Inputs.ApplyEventAt(connectionTick, inputChannel, new PlayerSpawnEvent());
		}

		public void ApplyPlayerInput(int inputChannel, int tick, PlayerShootingInput playerInput)
		{
			Session.Inputs.SetAt(tick, inputChannel, playerInput);
		}

		public void FinishSession(int finishTick)
		{
			Session.Inputs.ApplyGlobalEventAt(finishTick, new SessionFinishedEvent());
		}

		public async void Run()
		{
			// The target tick must be synchronized with server.
			int targetTick = 0;

			while (true)
			{
				if (Session.Inputs.GetEvents<SessionFinishedEvent>().HasAny)
				{
					break;
				}

				// Automatic rollbacks and resimulation based on input changes.
				Session.Loop.FastForwardToTick(targetTick);

				targetTick += 1;

				await Task.Yield();
			}
		}
	}

	public class SystemsSimulation : ISimulation
	{
		public Systems Systems { get; }

		public SystemsSimulation(Systems systems)
		{
			Systems = systems;
		}

		public void Update(int tick)
		{
			Systems.Run<IUpdate>();
		}
	}

	public interface IUpdate : ISystemMethod<IUpdate>
	{
		void Update();

		void ISystemMethod<IUpdate>.Run() => Update();
	}

	public class SpawnPlayers : SystemBase, IUpdate
	{
		public Inputs Inputs { get; }

		public SpawnPlayers(Inputs inputs) => Inputs = inputs;

		public void Update()
		{
			foreach (var (channel, spawnEnvent) in Inputs.GetEvents<PlayerSpawnEvent>())
			{
				World.CreateEntity(new Player() { InputChannel = channel });
			}
		}
	}

	public class Shooting : SystemBase, IUpdate
	{
		public Inputs Inputs { get; }

		public Shooting(Inputs inputs) => Inputs = inputs;

		public void Update()
		{
			World.ForEach(this, static (ref Player player, Shooting system) =>
			{
				var playerInput = system.Inputs.Get<PlayerShootingInput>(player.InputChannel).LastActual();

				if (playerInput.IsShooting)
				{
					// Perform shooting.
				}
			});
		}
	}
}
