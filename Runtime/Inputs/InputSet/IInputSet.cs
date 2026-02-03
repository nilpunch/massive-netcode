using System;
using System.IO;

namespace Massive.Netcode
{
	public interface IInputSet
	{
		Type InputType { get; }

		void PopulateUpTo(int tick);
		void DiscardUpTo(int tick);
		void Reevaluate();

		void Reset(int startTick);

		void ReadData(int tick, int channel, Stream stream);
		void ReadInput(int tick, int channel, Stream stream);
		void WriteData(int tick, int channel, Stream stream);
		void WriteInput(int tick, int channel, Stream stream);
		void SkipData(Stream stream);

		int GetUsedChannels(int tick);
	}
}
