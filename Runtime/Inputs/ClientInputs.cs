using System;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class ClientInputs : IInputPrediction
	{
		private readonly int _inputBufferSize;
		private readonly int _startTick;
		private readonly SetRegistry _setRegistry;

		public int Global { get; }

		public ClientInputs(int inputBufferSize = 120, int startTick = 0)
		{
			_inputBufferSize = inputBufferSize;
			_startTick = startTick;

			_setRegistry = new SetRegistry(new NormalSetFactory(pageSize: 1024));

			Global = 0;
		}

		public event Action<int> InputChanged;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetGlobalAt<T>(int tick)
		{
			return GetInputBuffer<T>(Global).GetInput(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetGlobalAt<T>(int tick, T input)
		{
			GetInputBuffer<T>(Global).SetActualInput(tick, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetAt<T>(int tick, int client)
		{
			return GetInputBuffer<T>(client).GetInput(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAt<T>(int tick, int client, T input)
		{
			GetInputBuffer<T>(client).SetActualInput(tick, input);
		}

		public void ForgetClient(int client)
		{
			if (client == Global)
			{
				return;
			}

			foreach (var set in _setRegistry.All)
			{
				set.Unassign(client);
			}
		}

		public AllInputs<T> GetAllAt<T>(int tick)
		{
			return new AllInputs<T>(GetAllInputs<T>(), tick);
		}

		public InputBuffer<T> GetInputBuffer<T>(int client)
		{
			var buffers = GetAllInputs<T>();

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
			foreach (var set in _setRegistry.All)
			{
				if (set is IDataSet dataSet)
				{
					foreach (var client in set)
					{
						var inputPrediction = (IInputPrediction)dataSet.GetRaw(client);
						inputPrediction.PopulateInputsUpTo(tick);
					}
				}
			}
		}

		private DataSet<InputBuffer<T>> GetAllInputs<T>()
		{
			return (DataSet<InputBuffer<T>>)_setRegistry.Get<InputBuffer<T>>();
		}

		private InputBuffer<T> CreateInputBuffer<T>()
		{
			InputBuffer<T> inputBuffer;

			if (FadeOutInput.IsImplementedFor(typeof(T)))
			{
				inputBuffer = FadeOutInput.CreateInputBuffer<T>(_startTick, _inputBufferSize);
			}
			else if (IRepeatInput.IsImplementedFor(typeof(T)))
			{
				inputBuffer = new RepeatInputBuffer<T>(_startTick, _inputBufferSize);
			}
			else
			{
				inputBuffer = new ResetInputBuffer<T>(_startTick, _inputBufferSize);
			}

			inputBuffer.InputChanged += InputChanged;

			return inputBuffer;
		}
	}
}
