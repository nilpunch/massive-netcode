namespace Massive.Netcode
{
	public interface IInputProvider<T>
	{
		bool HasInput(int inputId);
		T GetInput(int inputId);
	}
	
	
}
