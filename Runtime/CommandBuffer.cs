using System;
using UnityEngine;

namespace Massive.Netcode
{
	public class CommandBuffer<TCommand> : ICommandBuffer
	{
		private readonly CommandPrediction<TCommand> _commandPrediction;
		private readonly CyclicList<bool> _isPredicted;
		private readonly CyclicList<TCommand> _commands;

		private int _lastFrameWithAnyCommand;

		public CommandBuffer(int startFrame, int bufferSize, TCommand firstCommand, CommandPrediction<TCommand> commandPrediction = null)
		{
			_commandPrediction = commandPrediction ?? RepeatPrediction;
			_commands = new CyclicList<TCommand>(bufferSize, startFrame);
			_isPredicted = new CyclicList<bool>(bufferSize, startFrame);

			_commands.Append(firstCommand);
			_isPredicted.Append(false);
			_lastFrameWithAnyCommand = startFrame;
		}

		public event Action<int> CommandChanged;

		public static TCommand RepeatPrediction(TCommand command, int framesPassed) => command;

		public void Reset(int startFrame, TCommand firstCommand)
		{
			_commands.Reset(startFrame);
			_isPredicted.Reset(startFrame);

			_commands.Append(firstCommand);
			_isPredicted.Append(false);
			_lastFrameWithAnyCommand = startFrame;
		}

		public TCommand GetCommand(int frame)
		{
			return _commands[frame];
		}

		public void PopulateCommandsUpTo(int frame)
		{
			while (_commands.TailIndex - 1 < frame)
			{
				_commands.Append(_commandPrediction(_commands[_lastFrameWithAnyCommand], _commands.TailIndex - _lastFrameWithAnyCommand));
				_isPredicted.Append(false);
			}
		}

		public void InsertCommand(int frame, TCommand command)
		{
			_commands[frame] = command;
			_isPredicted[frame] = false;
			_lastFrameWithAnyCommand = Math.Max(_lastFrameWithAnyCommand, frame);
			ReevaluateFrom(frame);

			CommandChanged?.Invoke(frame);
		}

		public void InsertManyCommands((int frame, TCommand command)[] commands)
		{
			var reevaluateFrame = int.MaxValue; 
			
			foreach (var (frame, command) in commands)
			{
				_commands[frame] = command;
				_isPredicted[frame] = false;
				_lastFrameWithAnyCommand = Math.Max(_lastFrameWithAnyCommand, frame);
				reevaluateFrame = Math.Min(reevaluateFrame, frame);
			}

			ReevaluateFrom(reevaluateFrame);

			CommandChanged?.Invoke(reevaluateFrame);
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
						_commands[frame] = _commandPrediction(_commands[lastConfirmedFrame], frame - lastConfirmedFrame);
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
