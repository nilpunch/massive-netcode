using System;

namespace Massive.Netcode
{
	public class InputRegistry : IInputPrediction
	{
		private readonly int _inputBufferSize;
		private readonly int _startTick;
		private readonly SetRegistry _setRegistry;

		public int Master { get; }

		public InputRegistry(int inputBufferSize = 120, int startTick = 0, RegistryConfig registryConfig = null)
		{
			_inputBufferSize = inputBufferSize;
			_startTick = startTick;

			_setRegistry = new SetRegistry(new NormalSetFactory(registryConfig));

			Master = 0;
		}

		public event Action<int> InputChanged;

		public T GetMasterInput<T>(int tick)
		{
			return GetInputBuffer<T>(Master).GetInput(tick);
		}

		public void SetMasterInput<T>(int tick, T input)
		{
			GetInputBuffer<T>(Master).InsertInput(tick, input);
		}

		public T GetInput<T>(int client, int tick)
		{
			return GetInputBuffer<T>(client).GetInput(tick);
		}

		public void SetInput<T>(int client, int tick, T input)
		{
			GetInputBuffer<T>(client).InsertInput(tick, input);
		}

		public void ForgetClient(int client)
		{
			if (client == Master)
			{
				return;
			}

			foreach (var set in _setRegistry.All)
			{
				set.Unassign(client);
			}
		}

		public DataSet<InputBuffer<T>> GetAllInputs<T>()
		{
			return (DataSet<InputBuffer<T>>)_setRegistry.Get<InputBuffer<T>>();
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
			var inputPrediciton = typeof(IInputPrediction);
			
			foreach (var set in _setRegistry.All)
			{
				if (set is IDataSet dataSet && dataSet.Data.ElementType.IsAssignableFrom(inputPrediciton))
				{
					foreach (var client in set)
					{
						var inputPrediction = (IInputPrediction)dataSet.GetRaw(client);
						inputPrediction.PopulateInputsUpTo(tick);
					}
				}
			}
		}

		private InputBuffer<T> CreateInputBuffer<T>()
		{
			InputBuffer<T> inputBuffer;

			if (CustomPredictionUtils.IsImplementedFor(typeof(T)))
			{
				inputBuffer = CustomPredictionUtils.CreateCustomInputBuffer<T>(_startTick, _inputBufferSize);
			}
			else if (IResetInput.IsImplementedFor(typeof(T)))
			{
				inputBuffer = new PredictionInputBuffer<T>(_startTick, _inputBufferSize, PredictionInputBuffer<T>.ResetPrediction);
			}
			else
			{
				inputBuffer = new PredictionInputBuffer<T>(_startTick, _inputBufferSize, PredictionInputBuffer<T>.RepeatPrediction);
			}

			inputBuffer.InputChanged += InputChanged;

			return inputBuffer;
		}
	}
}
