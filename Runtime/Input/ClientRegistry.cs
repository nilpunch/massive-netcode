namespace Massive.Netcode
{
	public class ClientRegistry : IInputPrediction
	{
		private readonly int _inputBufferSize;
		private readonly int _startTick;
		private readonly Registry _registry;

		public Entity Master { get; }

		public ClientRegistry(int inputBufferSize = 120, int startTick = 0, RegistryConfig registryConfig = null)
		{
			_inputBufferSize = inputBufferSize;
			_startTick = startTick;
			_registry = new Registry(registryConfig ?? new RegistryConfig());

			Master = _registry.CreateEntity();
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
			return _registry.CreateEntity();
		}

		public void DestroyClient(Entity client)
		{
			if (!_registry.IsAlive(client) || client == Master)
			{
				return;
			}

			_registry.Destroy(client);
		}

		private InputBuffer<T> GetInputBuffer<T>(int clientId)
		{
			var buffers = _registry.DataSet<InputBuffer<T>>();

			if (!buffers.IsAssigned(clientId))
			{
				buffers.Assign(clientId);
				ref var inputBuffer = ref buffers.Get(clientId);
				inputBuffer ??= new InputBuffer<T>(_startTick, _inputBufferSize);
				inputBuffer.ResetInputs(_startTick);
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
			
			foreach (var set in _registry.SetRegistry.All)
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
	}
}
