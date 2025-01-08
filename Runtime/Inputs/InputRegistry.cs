using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class InputRegistry : IInputPrediction
	{
		private readonly int _inputBufferSize;
		private readonly int _startTick;
		private readonly SetRegistry _setRegistry;
		private readonly FadeOutConfig _defaultFadeOutConfig;
		private readonly Dictionary<Type, FadeOutConfig> _customFadeOutConfigs = new Dictionary<Type, FadeOutConfig>();

		public int Global { get; } = 0;

		public InputRegistry(int inputBufferSize = 120, int startTick = 0, FadeOutConfig? defaultFadeOutConfig = default)
		{
			_inputBufferSize = inputBufferSize;
			_startTick = startTick;
			_defaultFadeOutConfig = defaultFadeOutConfig ?? new FadeOutConfig(30, 60);

			_setRegistry = new SetRegistry(new NormalSetFactory(pageSize: 1024));
		}

		public event Action<int> InputChanged;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetGlobalAt<T>(int tick)
		{
			return GetInputBuffer<T>(Global).GetPredicted(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetGlobalAt<T>(int tick, T input)
		{
			GetInputBuffer<T>(Global).SetActualInput(tick, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetAt<T>(int tick, int client)
		{
			return GetInputBuffer<T>(client).GetPredicted(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAt<T>(int tick, int client, T input)
		{
			GetInputBuffer<T>(client).SetActualInput(tick, input);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllInputs<T> GetAllAt<T>(int tick)
		{
			return new AllInputs<T>(GetAllInputs<T>(), tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		public void ChangeFadeOutConfig<T>(FadeOutConfig fadeOutConfig)
		{
			if (!FadeOutInput.IsImplementedFor(typeof(T)))
			{
				throw new Exception($"Type {typeof(T).GetGenericName()} does not implement IFadeOutInput.");
			}

			var allInputs = GetAllInputs<T>();
			foreach (var inputBuffer in allInputs.Data.AsSpan(allInputs.Count))
			{
				((IFadeOutInputBuffer)inputBuffer).FadeOutConfig = fadeOutConfig;
			}

			_customFadeOutConfigs[typeof(T)] = fadeOutConfig;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private DataSet<InputBuffer<T>> GetAllInputs<T>()
		{
			return (DataSet<InputBuffer<T>>)_setRegistry.Get<InputBuffer<T>>();
		}

		private InputBuffer<T> CreateInputBuffer<T>()
		{
			InputBuffer<T> inputBuffer;

			if (FadeOutInput.IsImplementedFor(typeof(T)))
			{
				inputBuffer = FadeOutInput.CreateInputBuffer<T>(_startTick, _inputBufferSize, _customFadeOutConfigs.GetValueOrDefault(typeof(T), _defaultFadeOutConfig));
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
