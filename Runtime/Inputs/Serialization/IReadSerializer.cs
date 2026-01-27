using System.IO;

namespace Massive.Netcode
{
	public interface IReadSerializer
	{
		void ReadOne(Stream stream);
		void ReadFullSync(Stream stream);
	}
}
