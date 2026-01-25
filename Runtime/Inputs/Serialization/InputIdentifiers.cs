using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Massive.Netcode
{
	public class InputIdentifiers
	{
		private int _registeredIdCount;

		private readonly Dictionary<Type, int> _idsByInputs = new Dictionary<Type, int>();
		private readonly Dictionary<Type, int> _idsByEvents = new Dictionary<Type, int>();

		private readonly FastList<Type> _registeredTypes = new FastList<Type>();

		public InputIdentifiers(int registeredIdOffset = (int)SpecialPacketType.SpecialPacketsCount)
		{
			_registeredIdCount = registeredIdOffset;
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

		public Type GetType(int inputId)
		{
			return _registeredTypes[inputId];
		}

		private void RegisterEvent(Type type)
		{
			if (!_idsByEvents.TryAdd(type, _registeredIdCount))
			{
				throw new Exception($"Duplicate event type registration. Type: {type.GetFullGenericName()}");
			}
			_registeredTypes.Add(type);
			_registeredIdCount++;
		}

		private void RegisterInput(Type type)
		{
			if (!_idsByInputs.TryAdd(type, _registeredIdCount))
			{
				throw new Exception($"Duplicate input type registration. Type: {type.GetFullGenericName()}");
			}
			_registeredTypes.Add(type);
			_registeredIdCount++;
		}
	}
}
