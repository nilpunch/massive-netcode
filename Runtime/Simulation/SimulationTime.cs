namespace Massive.Netcode
{
	public class SimulationTime : ISimulationSystem
	{
		public int FPS { get; }

		public int Tick { get; private set; }

		public SimulationTime(int fps)
		{
			FPS = fps;
		}

		public float ElapsedTime => Tick * DeltaTime;
		public float DeltaTime => 1f / FPS;

		public void Update(int tick)
		{
			Tick = tick;
		}
	}
}
