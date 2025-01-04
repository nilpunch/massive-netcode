namespace Massive.Netcode
{
	public class Time
	{
		public int FPS = 60;
		public int Tick;

		public float ElapsedTime => Tick * DeltaTime;
		public float DeltaTime => 1f / FPS;
	}
}
