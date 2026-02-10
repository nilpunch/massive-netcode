using System;
using System.IO;

namespace Massive.Netcode
{
	public interface IEventSet
	{
		Type EventType { get; }
		int EventDataSize { get; }

		void ClearPrediction(int startTick, int endTick);
		void PopulateUpTo(int tick);
		void DiscardUpTo(int tick);

		void Reset(int startTick);

		/// <summary>
		/// Returns order of appened event.
		/// </summary>
		int AppendApprovedDefault(int tick, int channel);

		void ReadApproved(int tick, int order, int channel, Stream stream);
		void Write(int tick, int order, Stream stream);
		void Skip(Stream stream);

		int GetEventsCount(int tick);
		int GetEventChannel(int tick, int order);
		MaskEnumerator GetAllEvents(int tick);
	}
}
