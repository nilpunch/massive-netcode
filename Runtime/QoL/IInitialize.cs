namespace Massive.Netcode
{
	public interface IInitialize : ISystemMethod<IInitialize>
	{
		void Initialize();

		void ISystemMethod<IInitialize>.Run() => Initialize();
	}
}
