using GUtility.MotorController.MotorController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUtility.MotorController
{
    public class MotorControllerManager : IDisposable
    {
        private readonly object _stateLock = new object();

        private readonly MotorConfig _config;
        private readonly CommandScheduler _scheduler;
        private readonly ModbusTransport _transport;

        private MotorAgent[] _agents;

        private Thread _commandWorker;
        private Thread _pollingWorker;

        private volatile bool _isRunning;
        private volatile bool _disposed;

        private MotorControllerState _controllerState;


        public MotorAgent[] MotorAgents { get { return _agents; } set { _agents = value; } }

        public MotorControllerState ControllerState
        {
            get
            {
                lock (_stateLock)
                {
                    return _controllerState;
                }
            }
            private set
            {
                lock (_stateLock)
                {
                    _controllerState = value;
                }
            }
        }

        public bool IsConnected
        {
            get { return _transport != null && _transport.IsOpen; }
        }

        public string PortName { get { return _config.PortName; } }

        public MotorControllerManager(MotorConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");

            _config = config;
            _scheduler = new CommandScheduler();
            _transport = new ModbusTransport(_config);
            _controllerState = MotorControllerState.Disconnected;
        }

        public void Connect()
        {
            _transport.Open();
            BuildAgents();
            ControllerState = MotorControllerState.Connected;
        }

        public void Disconnect()
        {
            Stop();
            _transport.Close();
            ControllerState = MotorControllerState.Disconnected;
        }

        public void Start()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Transport is not connected.");

            if (_isRunning)
                return;

            _isRunning = true;

            _commandWorker = new Thread(CommandLoop);
            _commandWorker.IsBackground = true;
            _commandWorker.Name = "MotorCommandWorker";
            _commandWorker.Start();

            _pollingWorker = new Thread(PollingLoop);
            _pollingWorker.IsBackground = true;
            _pollingWorker.Name = "MotorPollingWorker";
            _pollingWorker.Start();

            ControllerState = MotorControllerState.Running;
        }

        public void Stop()
        {
            _isRunning = false;

            if (_commandWorker != null && _commandWorker.IsAlive)
                _commandWorker.Join(500);

            if (_pollingWorker != null && _pollingWorker.IsAlive)
                _pollingWorker.Join(500);
        }

        public void EnqueueCommand(MotorCommand command)
        {
            _scheduler.Enqueue(command);
        }

        public void MoveAbsolute(int motorIndex, int targetPosition)
        {
            ValidateMotorIndex(motorIndex);
            EnqueueCommand(MotorCommand.CreateMoveAbsolute(motorIndex, targetPosition));
        }

        public void MoveRelative(int motorIndex, int distance)
        {
            ValidateMotorIndex(motorIndex);
            EnqueueCommand(MotorCommand.CreateMoveRelative(motorIndex, distance));
        }

        public void Home(int motorIndex)
        {
            ValidateMotorIndex(motorIndex);
            EnqueueCommand(MotorCommand.CreateHome(motorIndex));
        }

        public void StopMotor(int motorIndex)
        {
            ValidateMotorIndex(motorIndex);
            EnqueueCommand(MotorCommand.CreateStop(motorIndex));
        }

        public void ClearAlarm(int motorIndex)
        {
            ValidateMotorIndex(motorIndex);
            EnqueueCommand(MotorCommand.CreateClearAlarm(motorIndex));
        }

        public void SetServoFree(int motorIndex, bool isFree)
        {
            ValidateMotorIndex(motorIndex);
            EnqueueCommand(MotorCommand.CreateServoFree(motorIndex, isFree));
        }

        public MotorState GetMotorState(int motorIndex)
        {
            ValidateMotorIndex(motorIndex);
            return _agents[motorIndex].GetStateSnapshot();
        }

        public MotorState[] GetAllMotorStates()
        {
            MotorState[] result = new MotorState[_agents.Length];
            int i;
            for (i = 0; i < _agents.Length; i++)
            {
                result[i] = _agents[i].GetStateSnapshot();
            }
            return result;
        }

        private void BuildAgents()
        {
            _agents = new MotorAgent[_config.AxisCount];
            int i;
            for (i = 0; i < _config.AxisCount; i++)
            {
                _agents[i] = new MotorAgent(i, (byte)(i + 1), _config, _transport);
            }
        }

        private void CommandLoop()
        {
            while (_isRunning)
            {
                try
                {
                    MotorCommand cmd;
                    if (_scheduler.TryDequeue(out cmd))
                    {
                        string error;
                        _agents[cmd.MotorIndex].ExecuteCommand(cmd, out error);
                    }
                    else
                    {
                        Thread.Sleep(_config.CommandLoopSleepMs);
                    }
                }
                catch
                {
                    ControllerState = MotorControllerState.Error;
                    Thread.Sleep(50);
                }
            }
        }

        private void PollingLoop()
        {
            while (_isRunning)
            {
                try
                {
                    if (_scheduler.Count > 0)
                    {
                        Thread.Sleep(_config.PollingLoopSleepMs);
                        continue;
                    }

                    int i;
                    for (i = 0; i < _agents.Length; i++)
                    {
                        if (_agents[i].ShouldLitePolling())
                        {
                            _scheduler.Enqueue(MotorCommand.CreateRefreshLite(i));
                        }

                        if (_agents[i].ShouldFullPolling())
                        {
                            _scheduler.Enqueue(MotorCommand.CreateRefreshFull(i));
                        }
                    }

                    Thread.Sleep(_config.PollingLoopSleepMs);
                }
                catch
                {
                    ControllerState = MotorControllerState.Error;
                    Thread.Sleep(50);
                }
            }
        }

        private void ValidateMotorIndex(int motorIndex)
        {
            if (_agents == null)
                throw new InvalidOperationException("Motor agents are not initialized.");

            if (motorIndex < 0 || motorIndex >= _agents.Length)
                throw new ArgumentOutOfRangeException("motorIndex");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Stop();

            if (_transport != null)
                _transport.Dispose();
        }
    }
}
