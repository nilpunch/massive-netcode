using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class NetSystem<TState> : ISystem, IInject<Session> where TState : unmanaged
	{
		public Session Session { get; private set; }
		public int Id { get; private set; }
		public Allocator Allocator { get; private set; }

		public Inputs Inputs => Session.Inputs;

		public int Tick => Session.Loop.CurrentTick;

		public MassiveWorld World => Session.World;

		private Pointer<TState> StatePointer { get; set; }

		private TState InitialState { get; }

		public ref TState State
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref StatePointer.Value(Allocator);
		}

		public NetSystem(TState initialState = default)
		{
			InitialState = initialState;
		}

		void ISystem.Build(int id, Allocator allocator)
		{
			Id = id;
			Allocator = allocator;
			StatePointer = Allocator.AllocVar(InitialState);
		}

		void IInject<Session>.Inject(Session session)
		{
			Session = session;
		}
	}
}
