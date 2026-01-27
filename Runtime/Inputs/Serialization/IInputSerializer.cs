using System.IO;

namespace Massive.Netcode
{
	public interface IInputSerializer
	{
		void ReadActual(Stream stream);
		void ReadFullSync(Stream stream);
	}
}
