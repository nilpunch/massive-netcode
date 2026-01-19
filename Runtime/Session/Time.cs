namespace Massive.Netcode
{
	public class Time : ISimulation
	{
		public int TickRate { get; }

		public int Tick { get; private set; }

		public Time(int tickRate)
		{
			TickRate = tickRate;
		}

		public float ElapsedTime => Tick * DeltaTime;
		public float DeltaTime => 1f / TickRate;

		public void Update(int tick)
		{
			Tick = tick;
		}
	}
}
