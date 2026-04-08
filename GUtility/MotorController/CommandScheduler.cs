using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.MotorController
{
    public class CommandScheduler
    {
        private readonly ConcurrentQueue<MotorCommand> _highQueue;
        private readonly ConcurrentQueue<MotorCommand> _normalQueue;
        private readonly ConcurrentQueue<MotorCommand> _lowQueue;

        public CommandScheduler()
        {
            _highQueue = new ConcurrentQueue<MotorCommand>();
            _normalQueue = new ConcurrentQueue<MotorCommand>();
            _lowQueue = new ConcurrentQueue<MotorCommand>();
        }

        public void Enqueue(MotorCommand command)
        {
            if (command == null) return;

            switch (command.Priority)
            {
                case MotorCommandPriority.High:
                    _highQueue.Enqueue(command);
                    break;
                case MotorCommandPriority.Low:
                    _lowQueue.Enqueue(command);
                    break;
                default:
                    _normalQueue.Enqueue(command);
                    break;
            }
        }

        public bool TryDequeue(out MotorCommand command)
        {
            if (_highQueue.TryDequeue(out command))
                return true;

            if (_normalQueue.TryDequeue(out command))
                return true;

            if (_lowQueue.TryDequeue(out command))
                return true;

            command = null;
            return false;
        }

        public int Count
        {
            get
            {
                return _highQueue.Count + _normalQueue.Count + _lowQueue.Count;
            }
        }

        public void Clear()
        {
            MotorCommand cmd;
            while (_highQueue.TryDequeue(out cmd)) { }
            while (_normalQueue.TryDequeue(out cmd)) { }
            while (_lowQueue.TryDequeue(out cmd)) { }
        }
    }
}
