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
		/// Returns local order of appened event.
		/// </summary>
		int AppendActualDefault(int tick, int channel);
		void ReadData(int tick, int localOrder, int channel, Stream stream);
		void WriteData(int tick, int localOrder, Stream stream);
		void SkipData(Stream stream);

		int GetEventsCount(int tick);
		int GetEventChannel(int tick, int localOrder);
		LocalOrdersEnumerator GetEventsLocalOrders(int tick);
	}
}
