using System.IO;

namespace Massive.Netcode
{
	public interface IEventSerializer<T> where T : IEvent
	{
		void Write(T data, Stream stream);

		T Read(Stream stream);
	}
}
