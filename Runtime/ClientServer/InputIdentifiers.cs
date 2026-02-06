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
		private readonly List<Type> _registeredTypes = new List<Type>();

		public InputIdentifiers(int startId = (int)MessageType.Count)
		{
			_startId = startId;
			_usedIds = startId;
			for (var i = 0; i < startId; i++)
			{
				_isEvent.Add(false);
				_registeredTypes.Add(default);
			}
		}

		public void RegisterAutomatically(Assembly assembly)
		{
			var assemblyTypes = assembly.GetTypes();

			var inputTypes = assemblyTypes
				.Where(IsInputType)
				.OrderBy(type => type.GetFullGenericName())
				.ToArray();

			var eventTypes = assemblyTypes
				.Where(IsEventType)
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
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			var inputTypes = assemblies
				.SelectMany(assembly => assembly.GetTypes())
				.Where(IsInputType)
				.OrderBy(type => type.GetFullGenericName())
				.ToArray();

			var eventTypes = assemblies
				.SelectMany(assembly => assembly.GetTypes())
				.Where(IsEventType)
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

		private static bool IsInputType(Type type)
		{
			return typeof(IInput).IsAssignableFrom(type) && (type.IsValueType || type.IsClass) && !type.IsGenericTypeDefinition;
		}

		private static bool IsEventType(Type type)
		{
			return typeof(IEvent).IsAssignableFrom(type) && (type.IsValueType || type.IsClass) && !type.IsGenericTypeDefinition;
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

			return _registeredTypes[inputId];
		}

		public int GetEventId(Type type)
		{
			if (!_idsByEvents.TryGetValue(type, out var id))
			{
				throw new InvalidOperationException($"Event with type: {type.GetGenericName()} is not registered.");
			}

			return id;
		}

		public int GetInputId(Type type)
		{
			if (!_idsByInputs.TryGetValue(type, out var id))
			{
				throw new InvalidOperationException($"Input with type: {type.GetGenericName()} is not registered.");
			}

			return id;
		}

		private void RegisterEvent(Type type)
		{
			if (!_idsByEvents.TryAdd(type, _usedIds))
			{
				throw new Exception($"Duplicate event type registration. Type: {type.GetFullGenericName()}");
			}
			_registeredTypes.Add(type);
			_isEvent.Add(true);
			_usedIds++;
		}

		private void RegisterInput(Type type)
		{
			if (!_idsByInputs.TryAdd(type, _usedIds))
			{
				throw new Exception($"Duplicate input type registration. Type: {type.GetFullGenericName()}");
			}
			_registeredTypes.Add(type);
			_isEvent.Add(false);
			_usedIds++;
		}
	}
}
