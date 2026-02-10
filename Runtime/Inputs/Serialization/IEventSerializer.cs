using System.IO;

namespace Massive.Netcode
{
	public interface IEventSerializer<T> where T : IEvent
	{
		int DataSize { get; }

		void Write(T data, Stream stream);

		T Read(Stream stream);
	}
}
