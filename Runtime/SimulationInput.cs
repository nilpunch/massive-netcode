namespace Massive.Netcode
{
	public class SimulationInput
	{
		public ClientInput ClientInput { get; }

		public Time Time { get; }

		public SimulationInput(ClientInput clientInput, Time time)
		{
			ClientInput = clientInput;
			Time = time;
		}
	}
}
