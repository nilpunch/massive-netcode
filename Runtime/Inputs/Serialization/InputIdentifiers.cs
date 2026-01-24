using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Massive.Netcode.Serialization
{
	public class InputIdentifiers
	{
		private readonly int _idOffset;

		private readonly Dictionary<Type, int> _idsByInputs = new Dictionary<Type, int>();
		private readonly Dictionary<Type, int> _idsByEvents = new Dictionary<Type, int>();
		private readonly Dictionary<int, Type> _inputsByIds = new Dictionary<int, Type>();
		private readonly Dictionary<int, Type> _eventsByIds = new Dictionary<int, Type>();

		private readonly FastList<Type> _inputs = new FastList<Type>();
		private readonly FastList<Type> _events = new FastList<Type>();

		public InputIdentifiers(int idOffset = 0)
		{
			_idOffset = idOffset;
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

		public Type GetInputType(int inputId)
		{
			return _inputsByIds[inputId];
		}

		public Type GetEventType(int eventId)
		{
			return _eventsByIds[eventId];
		}

		private void RegisterEvent(Type type)
		{
			if (!_idsByEvents.TryAdd(type, _events.Count + _idOffset))
			{
				throw new Exception($"Duplicate event type registration. Type: {type.GetFullGenericName()}");
			}
			_eventsByIds.Add(_events.Count + _idOffset, type);
			_events.Add(type);
		}

		private void RegisterInput(Type type)
		{
			if (!_idsByInputs.TryAdd(type, _inputs.Count + _idOffset))
			{
				throw new Exception($"Duplicate input type registration. Type: {type.GetFullGenericName()}");
			}
			_inputsByIds.Add(_inputs.Count + _idOffset, type);
			_inputs.Add(type);
		}
	}
}
