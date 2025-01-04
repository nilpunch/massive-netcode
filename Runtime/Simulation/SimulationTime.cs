namespace Massive.Netcode
{
	public class SimulationTime : ISimulation
	{
		public int FPS = 60;
		public int Tick;

		public float ElapsedTime => Tick * DeltaTime;
		public float DeltaTime => 1f / FPS;

		public void Update(int tick)
		{
			Tick = tick;
		}
	}
}
