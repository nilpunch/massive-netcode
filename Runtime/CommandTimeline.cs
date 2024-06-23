using System;

namespace Massive.Netcode
{
	public class CommandTimeline<TCommand> : ICommandTimeline
	{
		private readonly Func<TCommand, int, TCommand> _prediction;
		private readonly CyclicList<bool> _isPredicted;
		private readonly CyclicList<TCommand> _commands;

		private int _lastFrameWithCommand;

		public CommandTimeline(int startFrame, int bufferSize, TCommand firstCommand, Func<TCommand, int, TCommand> prediction = null)
		{
			_prediction = prediction ?? RepeatPrediction;
			_commands = new CyclicList<TCommand>(bufferSize, startFrame);
			_isPredicted = new CyclicList<bool>(bufferSize, startFrame);

			_commands.Append(firstCommand);
			_isPredicted.Append(false);
			_lastFrameWithCommand = startFrame;
		}

		public event Action<int> CommandChanged;

		public static TCommand RepeatPrediction(TCommand command, int framesPassed) => command;

		public void Reset(int startFrame, TCommand firstCommand)
		{
			_commands.Reset(startFrame);
			_isPredicted.Reset(startFrame);

			_commands.Append(firstCommand);
			_isPredicted.Append(false);
			_lastFrameWithCommand = startFrame;
		}

		public TCommand GetCommand(int frame)
		{
			return _commands[frame];
		}

		public void PopulateCommandsUpTo(int frame)
		{
			while (_commands.TailIndex - 1 < frame)
			{
				_commands.Append(_prediction(_commands[_lastFrameWithCommand], _commands.TailIndex - _lastFrameWithCommand));
				_isPredicted.Append(false);
			}
		}

		public void InsertCommand(int frame, TCommand command)
		{
			_commands[frame] = command;
			_isPredicted[frame] = false;
			_lastFrameWithCommand = Math.Max(_lastFrameWithCommand, frame);
			ReevaluateFrom(frame);

			CommandChanged?.Invoke(frame);
		}

		private void ReevaluateFrom(int frame)
		{
			int lastConfirmedFrame = -1;
			for (int i = frame; i < _commands.TailIndex; i++)
			{
				if (_isPredicted[frame])
				{
					if (lastConfirmedFrame != -1)
					{
						_commands[frame] = _prediction(_commands[lastConfirmedFrame], frame - lastConfirmedFrame);
					}
				}
				else
				{
					lastConfirmedFrame = frame;
				}
			}
		}
	}
}
