namespace Massive.Netcode
{
	public interface ILogger
	{
		void Log(string message);
		void Warn(string message);
		void Error(string message);
	}
}
