using System.IO;

namespace Massive.Netcode
{
	public interface IInputSerializer : IReadSerializer
	{
		void WriteOne(int tick, int channel, Stream stream);
		void WriteFullSync(int tick, Stream stream);
	}
}
