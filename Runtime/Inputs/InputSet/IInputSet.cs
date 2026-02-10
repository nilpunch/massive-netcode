using System;
using System.IO;

namespace Massive.Netcode
{
	public interface IInputSet
	{
		Type InputType { get; }
		int DataSize { get; }
		int FullInputSize { get; }

		void ClearPrediction(int startTick, int endTick);
		void PopulateUpTo(int tick);
		void DiscardUpTo(int tick);
		void Reevaluate();

		void Reset(int startTick);

		void ReadApproved(int tick, int channel, Stream stream);
		void ReadFullInput(int tick, int channel, Stream stream);
		void Write(int tick, int channel, Stream stream);
		void WriteFullInput(int tick, int channel, Stream stream);
		void Skip(Stream stream);

		int GetUsedChannels(int tick);
		int GetFreshInputsCount(int tick);
		bool IsFresh(int tick, int channel);
		MaskEnumerator GetFreshInputs(int tick);
	}
}
