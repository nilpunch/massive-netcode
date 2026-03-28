namespace Massive.Netcode
{
	public class NullLogger : ILogger
	{
		public static readonly NullLogger Instance = new NullLogger();

		public void Log(string message)
		{
		}

		public void Warn(string message)
		{
		}

		public void Error(string message)
		{
		}
	}
}
