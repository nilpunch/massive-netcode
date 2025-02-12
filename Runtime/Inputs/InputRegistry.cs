using System;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class InputRegistry : IInputBuffer
	{
		private readonly int _startTick;
		private readonly SetRegistry _setRegistry;
		private readonly FastList<IInputBuffer> _allInputs = new FastList<IInputBuffer>();

		public int Global { get; } = 0;

		public InputRegistry(int startTick = 0)
		{
			_startTick = startTick;

			_setRegistry = new SetRegistry(new NormalSetFactory(pageSize: 1024));
		}

		public event Action<int> InputChanged;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetGlobalAt<T>(int tick)
		{
			return GetInputBuffer<T>(Global).GetInput(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetGlobalAt<T>(int tick, T input)
		{
			GetInputBuffer<T>(Global).SetActualInput(tick, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Input<T> GetAt<T>(int tick, int client)
		{
			return GetInputBuffer<T>(client).GetInput(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAt<T>(int tick, int client, T input)
		{
			GetInputBuffer<T>(client).SetActualInput(tick, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetAllAt<T>(int tick)
		{
			return new AllInputs<T>(GetAllInputBuffers<T>(), tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllActualInputs<T> GetAllActualAt<T>(int tick)
		{
			return new AllActualInputs<T>(GetAllInputBuffers<T>(), tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public InputBuffer<T> GetInputBuffer<T>(int client)
		{
			var buffers = GetAllInputBuffers<T>();

			if (!buffers.IsAssigned(client))
			{
				buffers.Assign(client);
				ref var inputBuffer = ref buffers.Get(client);
				inputBuffer ??= CreateInputBuffer<T>();
				inputBuffer.Reset(_startTick);
				return inputBuffer;
			}
			else
			{
				return buffers.Get(client);
			}
		}

		public void PopulateInputsUpTo(int tick)
		{
			foreach (var input in _allInputs)
			{
				input.PopulateInputsUpTo(tick);
			}
		}

		public void ForgetInputsUpTo(int tick)
		{
			foreach (var input in _allInputs)
			{
				input.ForgetInputsUpTo(tick);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private DataSet<InputBuffer<T>> GetAllInputBuffers<T>()
		{
			return (DataSet<InputBuffer<T>>)_setRegistry.Get<InputBuffer<T>>();
		}

		private InputBuffer<T> CreateInputBuffer<T>()
		{
			var inputBuffer = new InputBuffer<T>(_startTick);

			inputBuffer.InputChanged += InputChanged;
			_allInputs.Add(inputBuffer);

			return inputBuffer;
		}
	}
}
