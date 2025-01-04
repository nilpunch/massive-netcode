namespace Massive.Netcode
{
	public class ClientInput : IInputPrediction
	{
		private readonly int _inputBufferSize;
		private readonly int _startTick;

		public SetRegistry SetRegistry { get; }

		public int Master { get; }

		public ClientInput(int inputBufferSize = 120, int startTick = 0, RegistryConfig registryConfig = null)
		{
			_inputBufferSize = inputBufferSize;
			_startTick = startTick;

			SetRegistry = new SetRegistry(new NormalSetFactory(registryConfig));

			Master = 0;
		}

		public T GetGlobalInput<T>(int tick)
		{
			return GetInputBuffer<T>(Master).GetInput(tick);
		}

		public void SetGlobalInput<T>(int tick, T input)
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

			foreach (var set in SetRegistry.All)
			{
				set.Unassign(client);
			}
		}

		public DataSet<InputBuffer<T>> GetAllInputs<T>()
		{
			return (DataSet<InputBuffer<T>>)SetRegistry.Get<InputBuffer<T>>();
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
			
			foreach (var set in SetRegistry.All)
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
			if (CustomPredictionUtils.IsImplementedFor(typeof(T)))
			{
				return CustomPredictionUtils.CreateCustomInputBuffer<T>(_startTick, _inputBufferSize);
			}

			if (IResetInput.IsImplementedFor(typeof(T)))
			{
				return new PredictionInputBuffer<T>(_startTick, _inputBufferSize, PredictionInputBuffer<T>.ResetPrediction);
			}

			return new PredictionInputBuffer<T>(_startTick, _inputBufferSize, PredictionInputBuffer<T>.RepeatPrediction);
		}
	}
}
