namespace Massive.Netcode
{
	public interface ICommandTimeline
	{
		void PopulateCommandsUpTo(int frame);
	}
}