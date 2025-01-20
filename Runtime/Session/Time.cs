namespace Massive.Netcode
{
	public class Time : ISimulation
	{
		public int FPS { get; }

		public int Tick { get; private set; }

		public Time(int fps)
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
