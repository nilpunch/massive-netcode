using System.IO;

namespace Massive.Netcode
{
	public interface IInputSerializer<T> where T : IInput
	{
		int DataSize { get; }
		int FullInputSize { get; }

		void Write(T data, Stream stream);
		void WriteFullInput(Input<T> data, Stream stream);

		T Read(Stream stream);
		Input<T> ReadFullInput(Stream stream);
	}
}
