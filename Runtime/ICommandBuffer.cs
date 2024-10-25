namespace Massive.Netcode
{
	public interface ICommandBuffer
	{
		void PopulateCommandsUpTo(int frame);
	}
}