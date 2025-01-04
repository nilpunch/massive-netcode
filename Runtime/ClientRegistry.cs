namespace Massive.Netcode
{
	public class ClientRegistry : Registry, IInputPrediction
	{
		private readonly int _inputBufferSize;
		private readonly int _startTick;

		public Entity Master { get; }

		public ClientRegistry(int inputBufferSize = 120, int startTick = 0, RegistryConfig registryConfig = null)
			: base(registryConfig ?? new RegistryConfig())
		{
			_inputBufferSize = inputBufferSize;
			_startTick = startTick;

			Master = this.CreateEntity();
		}

		public T GetGlobalInput<T>(int tick)
		{
			return GetInputBuffer<T>(Master.Id).GetInput(tick);
		}

		public void SetGlobalInput<T>(int tick, T input)
		{
			GetInputBuffer<T>(Master.Id).InsertInput(tick, input);
		}

		public T GetInput<T>(Entity client, int tick)
		{
			return GetInputBuffer<T>(client.Id).GetInput(tick);
		}

		public void SetInput<T>(Entity client, int tick, T input)
		{
			GetInputBuffer<T>(client.Id).InsertInput(tick, input);
		}

		public Entity CreateClient()
		{
			return this.CreateEntity();
		}

		public void DestroyClient(Entity client)
		{
			if (!this.IsAlive(client) || client == Master)
			{
				return;
			}

			this.Destroy(client);
		}

		private InputBuffer<T> GetInputBuffer<T>(int clientId)
		{
			var buffers = this.DataSet<InputBuffer<T>>();

			if (!buffers.IsAssigned(clientId))
			{
				buffers.Assign(clientId);
				ref var inputBuffer = ref buffers.Get(clientId);
				inputBuffer ??= CreateInputBuffer<T>();
				inputBuffer.Reset(_startTick);
				return inputBuffer;
			}
			else
			{
				return buffers.Get(clientId);
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

		public InputBuffer<T> CreateInputBuffer<T>()
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
