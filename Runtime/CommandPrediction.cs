namespace Massive.Netcode
{
	public delegate TCommand CommandPrediction<TCommand>(TCommand command, int framesPassed);
}
