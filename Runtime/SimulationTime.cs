namespace Massive.Netcode
{
	public class SimulationTime
	{
		public int FPS;
		public int Tick;

		public float Time => Tick * DeltaTime;
		public float DeltaTime => 1f / FPS;
	}
}
