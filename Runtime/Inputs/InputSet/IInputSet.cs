using System.IO;

namespace Massive.Netcode
{
	public interface IInputSet
	{
		void PopulateUpTo(int tick);
		void DiscardUpTo(int tick);
		void Reevaluate();

		void Reset(int startTick);

		void Read(Stream stream);
		void Write(int tick, int channel, Stream stream);
		void WriteAll(int tick, Stream stream);
	}
}
