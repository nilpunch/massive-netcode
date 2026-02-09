namespace Massive.Netcode
{
	public interface IFirstTick : ISystemMethod<IFirstTick>
	{
		void FirstTick();

		void ISystemMethod<IFirstTick>.Run() => FirstTick();
	}
}
