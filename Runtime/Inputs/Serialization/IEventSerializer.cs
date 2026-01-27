using System.IO;

namespace Massive.Netcode
{
	public interface IEventSerializer : IReadSerializer
	{
		void WriteOne(int tick, int localOrder, Stream stream);
		void WriteFullSync(int tick, Stream stream);
	}
}
