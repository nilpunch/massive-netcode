using System.IO;

namespace Massive.Netcode
{
	public interface IEventSet
	{
		void PopulateUpTo(int tick);
		void DiscardUpTo(int tick);

		void Reset(int startTick);

		void Read(int tick, Stream stream);
		void Write(int tick, int localOrder, Stream stream);
		void WriteAll(int tick, Stream stream);
		void Skip(Stream stream);
	}
}
