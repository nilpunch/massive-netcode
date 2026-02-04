using System.IO;

namespace Massive.Netcode
{
	public interface IInputSerializer<T> where T : IInput
	{
		int DataSize { get; }
		int InputSize { get; }

		void WriteData(T data, Stream stream);
		void WriteInput(Input<T> data, Stream stream);

		T ReadData(Stream stream);
		Input<T> ReadInput(Stream stream);
	}
}
