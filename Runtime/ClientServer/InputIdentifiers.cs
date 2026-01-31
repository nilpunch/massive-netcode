using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Massive.Netcode
{
	public class InputIdentifiers
	{
		private readonly int _startId;
		private int _usedIds;

		private readonly Dictionary<Type, int> _idsByInputs = new Dictionary<Type, int>();
		private readonly Dictionary<Type, int> _idsByEvents = new Dictionary<Type, int>();

		private readonly List<bool> _isEvent = new List<bool>();

		public List<Type> RegisteredTypes { get; } = new List<Type>();

		public InputIdentifiers(int startId)
		{
			_startId = startId;
			_usedIds = startId;
		}

		public void RegisterAutomatically(Assembly assembly)
		{
			var assemblyTypes = assembly.GetTypes();

			var inputTypes = assemblyTypes
				.Where(t => typeof(IInput).IsAssignableFrom(t))
				.OrderBy(type => type.GetFullGenericName())
				.ToArray();

			var eventTypes = assemblyTypes
				.Where(t => typeof(IEvent).IsAssignableFrom(t))
				.OrderBy(type => type.GetFullGenericName())
				.ToArray();

			foreach (var inputType in inputTypes)
			{
				RegisterInput(inputType);
			}

			foreach (var eventType in eventTypes)
			{
				RegisterEvent(eventType);
			}
		}

		public void RegisterAutomaticallyFromAllAssemblies()
		{
			var asemblies = AppDomain.CurrentDomain.GetAssemblies();

			var inputTypes = asemblies
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => typeof(IInput).IsAssignableFrom(t))
				.OrderBy(type => type.GetFullGenericName())
				.ToArray();

			var eventTypes = asemblies
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => typeof(IEvent).IsAssignableFrom(t))
				.OrderBy(type => type.GetFullGenericName())
				.ToArray();

			foreach (var inputType in inputTypes)
			{
				RegisterInput(inputType);
			}

			foreach (var eventType in eventTypes)
			{
				RegisterEvent(eventType);
			}
		}

		public void RegisterInput<T>()
		{
			RegisterInput(typeof(T));
		}

		public void RegisterEvent<T>()
		{
			RegisterEvent(typeof(T));
		}

		public bool IsRegistered(int inputId)
		{
			return inputId >= _startId && inputId < _usedIds;
		}

		public bool IsEvent(int inputId)
		{
			if (!IsRegistered(inputId))
			{
				throw new InvalidOperationException($"Input with id: {inputId} is not registered.");
			}

			return _isEvent[inputId];
		}

		public Type GetTypeById(int inputId)
		{
			if (!IsRegistered(inputId))
			{
				throw new InvalidOperationException($"Input with id: {inputId} is not registered.");
			}

			return RegisteredTypes[inputId];
		}

		private void RegisterEvent(Type type)
		{
			if (!_idsByEvents.TryAdd(type, _usedIds))
			{
				throw new Exception($"Duplicate event type registration. Type: {type.GetFullGenericName()}");
			}
			RegisteredTypes.Add(type);
			_isEvent.Add(true);
			_usedIds++;
		}

		private void RegisterInput(Type type)
		{
			if (!_idsByInputs.TryAdd(type, _usedIds))
			{
				throw new Exception($"Duplicate input type registration. Type: {type.GetFullGenericName()}");
			}
			RegisteredTypes.Add(type);
			_isEvent.Add(false);
			_usedIds++;
		}
	}
}
