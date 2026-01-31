using System.IO;

namespace Massive.Netcode
{
	public interface IReadSerializer
	{
		void ReadOne(Stream stream);
		void ReadAll(Stream stream);
	}
}
