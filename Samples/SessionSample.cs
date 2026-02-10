using System.Threading.Tasks;

namespace Massive.Netcode.Samples
{
	public struct Player { public int InputChannel; }

	public struct PlayerSpawnEvent : IEvent { public int PlayerInputChannel; }

	public struct SessionFinishedEvent : IEvent { }

	public struct PlayerShootingInput : IInput { public bool IsShooting; }

	public class SessionSample
	{
		private Session Session { get; }

		public SessionSample()
		{
			Session = new Session();

			Session.Systems
				.New<SpawnPlayers>()
				.New<Shooting>()
				.Build(Session);

			Session.Simulations.Add(new SystemsSimulation(Session.Systems));
		}

		// Modify inputs via RPC or any other source, in any order, at any time.
		public void ConnectClient(int connectionTick, int order, int channel)
		{
			Session.Inputs.SetApprovedEventAt(connectionTick, order, channel, new PlayerSpawnEvent());
		}

		public void ApplyPlayerInput(int inputChannel, int tick, PlayerShootingInput playerInput)
		{
			Session.Inputs.SetApprovedInputAt(tick, inputChannel, playerInput);
		}

		public void FinishSession(int finishTick, int order, int channel)
		{
			Session.Inputs.SetApprovedEventAt(finishTick, order, channel, new SessionFinishedEvent());
		}

		public async void Run()
		{
			// The target tick must be synchronized with server.
			int targetTick = 0;

			while (true)
			{
				if (Session.Inputs.GetAllEvents<SessionFinishedEvent>().HasAny)
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

	public class SpawnPlayers : NetSystem, IUpdate
	{
		public void Update()
		{
			foreach (var spawnEnvent in Inputs.GetAllEvents<PlayerSpawnEvent>())
			{
				World.CreateEntity(new Player() { InputChannel = spawnEnvent.Data.PlayerInputChannel });
			}
		}
	}

	public class Shooting : NetSystem, IUpdate
	{
		public void Update()
		{
			World.ForEach(this, static (ref Player player, Shooting system) =>
			{
				var playerInput = system.Inputs.GetInput<PlayerShootingInput>(player.InputChannel).LastFresh();

				if (playerInput.IsShooting)
				{
					// Perform shooting.
				}
			});
		}
	}
}
