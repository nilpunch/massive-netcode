using System;

namespace Massive.Netcode
{
	public class CommandTimeline<TCommand> : ICommandTimeline
	{
		private readonly Func<TCommand, int, TCommand> _prediction;
		private readonly CyclicList<bool> _isPredicted;
		private readonly CyclicList<TCommand> _commands;

		private int _lastFrameWithCommand;

		public CommandTimeline(int startFrom, int bufferSize, TCommand initial, Func<TCommand, int, TCommand> prediction)
		{
			_prediction = prediction;
			_commands = new CyclicList<TCommand>(bufferSize, startFrom);
			_isPredicted = new CyclicList<bool>(bufferSize, startFrom);

			_commands.Add(initial);
			_isPredicted.Add(false);
			_lastFrameWithCommand = _commands.HeadIndex;
		}

		public event Action<int> CommandChanged;

		public TCommand GetCommand(int frame)
		{
			return _commands[frame];
		}

		public void PopulateCommandsUpTo(int frame)
		{
			while (_commands.TailIndex - 1 < frame)
			{
				_commands.Add(_prediction(_commands[_lastFrameWithCommand], _commands.TailIndex - _lastFrameWithCommand));
				_isPredicted.Add(false);
			}
		}

		public void InsertCommand(int frame, TCommand command)
		{
			if (frame < _commands.HeadIndex || frame >= _commands.TailIndex)
			{
				throw new Exception();
			}

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
