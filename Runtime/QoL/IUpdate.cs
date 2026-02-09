namespace Massive.Netcode
{
	public interface IUpdate : ISystemMethod<IUpdate>
	{
		void Update();

		void ISystemMethod<IUpdate>.Run() => Update();
	}
}
