using GUtility.MotorController.MotorController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.MotorController
{
    public class MotorAgent
    {
        private readonly object _stateLock = new object();

        private readonly int _motorIndex;
        private readonly byte _stationNo;
        private readonly MotorConfig _config;
        private readonly ModbusTransport _transport;

        private readonly MotorState _state;

        private DateTime _lastLitePollingTime = DateTime.MinValue;
        private DateTime _lastFullPollingTime = DateTime.MinValue;

        public int MotorIndex
        {
            get { return _motorIndex; }
        }

        public byte StationNo
        {
            get { return _stationNo; }
        }

        public MotorAgent(int motorIndex, byte stationNo, MotorConfig config, ModbusTransport transport)
        {
            _motorIndex = motorIndex;
            _stationNo = stationNo;
            _config = config;
            _transport = transport;

            _state = new MotorState();
            _state.MotorIndex = motorIndex;
            _state.StationNo = stationNo;
            _state.RuntimeState = MotorRuntimeState.Unknown;
            _state.LastUpdateTime = DateTime.MinValue;
        }

        public MotorState GetStateSnapshot()
        {
            lock (_stateLock)
            {
                return _state.Clone();
            }
        }

        public bool ShouldLitePolling()
        {
            int interval = IsMoving() ? _config.MovingPollingIntervalMs : _config.IdlePollingIntervalMs;
            return (DateTime.Now - _lastLitePollingTime).TotalMilliseconds >= interval;
        }

        public bool ShouldFullPolling()
        {
            return (DateTime.Now - _lastFullPollingTime).TotalMilliseconds >= _config.FullPollingIntervalMs;
        }

        public bool ExecuteCommand(MotorCommand command, out string error)
        {
            error = null;
            if (command == null)
            {
                error = "Command is null.";
                return false;
            }

            bool success = false;

            switch (command.CommandType)
            {
                case MotorCommandType.MoveAbsolute:
                    success = MoveAbsolute(command.Value, out error);
                    break;

                case MotorCommandType.MoveRelative:
                    success = MoveRelative(command.Value, out error);
                    break;

                case MotorCommandType.Home:
                    success = Home(out error);
                    break;

                case MotorCommandType.Stop:
                    success = Stop(out error);
                    break;

                case MotorCommandType.ClearAlarm:
                    success = ClearAlarm(out error);
                    break;

                case MotorCommandType.ServoFree:
                    success = SetServoFree(command.BoolValue, out error);
                    break;

                case MotorCommandType.RefreshLite:
                    success = RefreshLite(out error);
                    break;

                case MotorCommandType.RefreshFull:
                    success = RefreshFull(out error);
                    break;

                default:
                    error = "Unsupported command type.";
                    success = false;
                    break;
            }

            if (success)
            {
                lock (_stateLock)
                {
                    _state.LastCommandTime = DateTime.Now;
                }
            }

            return success;
        }

        public bool RefreshLite(out string error)
        {
            error = null;

            int driverOutputRaw;
            int commandPosition;
            int commandSpeedHz;
            int directIORaw;


            if (!ReadOneRegister((int)FunctionCodeAddress.DriverOutputStatus, out driverOutputRaw, out error))
                return false;

            if (!ReadInt32Register((int)MonitorCommandAddress.CommandPosition, out commandPosition, out error))
                return false;

            if (!ReadInt32Register((int)MonitorCommandAddress.CommandSpeedHz, out commandSpeedHz, out error))
                return false;

            if (!ReadInt32Register((int)MonitorCommandAddress.DirectIO, out directIORaw, out error))
                return false;

            lock (_stateLock)
            {
                _state.DriverOutputRaw = driverOutputRaw;
                _state.CommandPosition = commandPosition;
                _state.CommandSpeedHz = commandSpeedHz;

                _state.IsReady = GetBit(driverOutputRaw, DriverOutputStatusBit.READY);
                _state.IsMoving = GetBit(driverOutputRaw, DriverOutputStatusBit.MOVE);
                _state.IsAlarm = GetBit(driverOutputRaw, DriverOutputStatusBit.ALM_A);
                _state.IsHoming = (!GetBit(driverOutputRaw, DriverOutputStatusBit.HOME_END) && _state.IsMoving);
                _state.IsHomeEnd = GetBit(driverOutputRaw, DriverOutputStatusBit.HOME_END);

                ApplyDirectIOToState(_state, directIORaw);

                _state.RuntimeState = ResolveRuntimeState(_state);
                _state.LastUpdateTime = DateTime.Now;
            }

            _lastLitePollingTime = DateTime.Now;
            return true;
        }

        public bool RefreshFull(out string error)
        {
            error = null;

            if (!RefreshLite(out error))
                return false;

            int driverInputRaw;
            int targetPosition;
            int nextNo;
            int loopReturnNo;
            int currentCommError;

            if (!ReadOneRegister((int)FunctionCodeAddress.DriverInputStatus, out driverInputRaw, out error))
                return false;

            if (!ReadInt32Register((int)MonitorCommandAddress.TargetPosition, out targetPosition, out error))
                return false;

            if (!ReadInt32Register((int)MonitorCommandAddress.NextDataNo, out nextNo, out error))
                return false;

            if (!ReadInt32Register((int)MonitorCommandAddress.LoopReturnNo, out loopReturnNo, out error))
                return false;

            if (!ReadInt32Register((int)MonitorCommandAddress.CurrentCommunicationError, out currentCommError, out error))
                return false;

            lock (_stateLock)
            {
                _state.DriverInputRaw = driverInputRaw;
                _state.TargetPositionFromDriver = targetPosition;
                _state.NextDataNo = nextNo;
                _state.LoopReturnNo = loopReturnNo;
                _state.CurrentCommunicationErrorCode = currentCommError;

                _state.IsStop = GetBit(driverInputRaw, DriverInputStatusBit.STOP);
                _state.IsServoFree = GetBit(driverInputRaw, DriverInputStatusBit.AWO);

                _state.RuntimeState = ResolveRuntimeState(_state);
                _state.LastUpdateTime = DateTime.Now;
            }

            _lastFullPollingTime = DateTime.Now;
            return true;
        }

        #region ===== Movement Commands =====

        public bool MoveAbsolute(int targetPosition, out string error)
        {
            error = null;

            byte[] req = OrientalMotorProtocol.BuildMoveCommand(
                _stationNo,
                MoveMode.Absolute,
                targetPosition,
                _config.DefaultMoveSpeed,
                _config.DefaultAcceleration,
                _config.DefaultDeceleration);

            ModbusResponse resp = _transport.SendAndReceive(req, OrientalMotorProtocol.GetExpectedResponseLengthForWriteMultiRegister());

            if (!resp.Success)
            {
                UpdateError(resp.ErrorMessage);
                error = resp.ErrorMessage;
                return false;
            }

            OrientalMotorProtocol.ProtocolResult parsed = OrientalMotorProtocol.ParseResponse(resp.RawData);
            if (!parsed.Success)
            {
                UpdateError(parsed.ErrorMessage);
                error = parsed.ErrorMessage;
                return false;
            }

            lock (_stateLock)
            {
                _state.TargetPosition = targetPosition;
                _state.RuntimeState = MotorRuntimeState.Moving;
                _state.LastCommandTime = DateTime.Now;
            }

            return true;
        }

        public bool MoveRelative(int distance, out string error)
        {
            error = null;

            int newTarget;
            lock (_stateLock)
            {
                newTarget = _state.CommandPosition + distance;
            }

            byte[] req = OrientalMotorProtocol.BuildMoveCommand(
                _stationNo,
                MoveMode.Relative,
                distance,
                _config.DefaultMoveSpeed,
                _config.DefaultAcceleration,
                _config.DefaultDeceleration);

            ModbusResponse resp = _transport.SendAndReceive(
                req,
                OrientalMotorProtocol.GetExpectedResponseLengthForWriteMultiRegister());

            if (!resp.Success)
            {
                UpdateError(resp.ErrorMessage);
                error = resp.ErrorMessage;
                return false;
            }

            OrientalMotorProtocol.ProtocolResult parsed = OrientalMotorProtocol.ParseResponse(resp.RawData);
            if (!parsed.Success)
            {
                UpdateError(parsed.ErrorMessage);
                error = parsed.ErrorMessage;
                return false;
            }

            lock (_stateLock)
            {
                _state.TargetPosition = newTarget;
                _state.RuntimeState = MotorRuntimeState.Moving;
                _state.LastCommandTime = DateTime.Now;
            }

            return true;
        }

        public bool Home(out string error)
        {
            error = null;

            ModbusResponse resp = _transport.SendAndReceive(
                OrientalMotorProtocol.BuildHomeCommand(_stationNo),
                OrientalMotorProtocol.GetExpectedResponseLengthForWriteSingleRegister());

            if (!resp.Success)
            {
                UpdateError(resp.ErrorMessage);
                error = resp.ErrorMessage;
                return false;
            }

            OrientalMotorProtocol.ProtocolResult parsed = OrientalMotorProtocol.ParseResponse(resp.RawData);
            if (!parsed.Success)
            {
                UpdateError(parsed.ErrorMessage);
                error = parsed.ErrorMessage;
                return false;
            }

            lock (_stateLock)
            {
                _state.TargetPosition = 0;
                _state.RuntimeState = MotorRuntimeState.Homing;
                _state.LastCommandTime = DateTime.Now;
            }

            PulseDriverBit(DriverInputStatusBit.HOME, out error, null);

            return true;
        }

        public bool Stop(out string error)
        {
            error = null;
            return PulseDriverBit(DriverInputStatusBit.STOP, out error, MotorRuntimeState.Stopping);
        }

        public bool ClearAlarm(out string error)
        {
            error = null;
            return PulseDriverBit(DriverInputStatusBit.ALM_RST, out error, null);
        }


        public bool SetServoFree(bool isFree, out string error)
        {
            error = null;

            int current;
            if (!ReadSingleRegister(FunctionCodeAddress.DriverInputStatus, out current, out error))
                return false;

            int setValue = OrientalMotorProtocol.SetBit(current, (int)DriverInputStatusBit.AWO, isFree);

            ModbusResponse resp = _transport.SendAndReceive(
                OrientalMotorProtocol.BuildWriteSingleRegister(_stationNo, FunctionCodeAddress.DriverInputStatus, setValue),
                OrientalMotorProtocol.GetExpectedResponseLengthForWriteSingleRegister());

            if (!resp.Success)
            {
                UpdateError(resp.ErrorMessage);
                error = resp.ErrorMessage;
                return false;
            }

            OrientalMotorProtocol.ProtocolResult parsed = OrientalMotorProtocol.ParseResponse(resp.RawData);
            if (!parsed.Success)
            {
                UpdateError(parsed.ErrorMessage);
                error = parsed.ErrorMessage;
                return false;
            }

            lock (_stateLock)
            {
                _state.IsServoFree = isFree;
                _state.RuntimeState = ResolveRuntimeState(_state);
                _state.LastCommandTime = DateTime.Now;
            }

            return true;
        }

        #endregion

        #region ===== Parameters Setting =====

        public int MoveParSpeed { get { return _config.DefaultMoveSpeed; } }

        public int MoveParAcceleration { get { return _config.DefaultAcceleration; } }

        public int MoveParDeceleration { get { return _config.DefaultDeceleration; } }

        public void SetMoveParameters(MotorConfig config)
        {
            _config.DefaultMoveSpeed = config.DefaultMoveSpeed;
            _config.DefaultAcceleration = config.DefaultAcceleration;
            _config.DefaultDeceleration = config.DefaultDeceleration;
        }

        #endregion


        #region ===== Private Methods ======


        private bool PulseDriverBit(DriverInputStatusBit bit, out string error, MotorRuntimeState? stateAfterCommand)
        {
            error = null;

            int current;
            if (!ReadSingleRegister(FunctionCodeAddress.DriverInputStatus, out current, out error))
                return false;

            int setOn = OrientalMotorProtocol.SetBit(current, (int)bit, true);
            ModbusResponse retOn = _transport.SendAndReceive(
                OrientalMotorProtocol.BuildWriteSingleRegister(_stationNo, FunctionCodeAddress.DriverInputStatus, setOn),
                OrientalMotorProtocol.GetExpectedResponseLengthForWriteSingleRegister());

            if (!retOn.Success)
            {
                UpdateError(retOn.ErrorMessage);
                error = retOn.ErrorMessage;
                return false;
            }

            int setOff = OrientalMotorProtocol.SetBit(setOn, (int)bit, false);
            ModbusResponse retOff = _transport.SendAndReceive(
                OrientalMotorProtocol.BuildWriteSingleRegister(_stationNo, FunctionCodeAddress.DriverInputStatus, setOff),
                OrientalMotorProtocol.GetExpectedResponseLengthForWriteSingleRegister());

            if (!retOff.Success)
            {
                UpdateError(retOff.ErrorMessage);
                error = retOff.ErrorMessage;
                return false;
            }

            if (stateAfterCommand.HasValue)
            {
                lock (_stateLock)
                {
                    _state.RuntimeState = stateAfterCommand.Value;
                    _state.LastCommandTime = DateTime.Now;
                }
            }

            return true;
        }

        private bool ReadSingleRegister(FunctionCodeAddress address, out int value, out string error)
        {
            value = 0;
            error = null;

            ModbusResponse resp = _transport.SendAndReceive(
                OrientalMotorProtocol.BuildReadSingleRegister(_stationNo, address),
                OrientalMotorProtocol.GetExpectedResponseLengthForReadOneRegister());

            if (!resp.Success)
            {
                UpdateError(resp.ErrorMessage);
                error = resp.ErrorMessage;
                return false;
            }

            OrientalMotorProtocol.ProtocolResult parsed = OrientalMotorProtocol.ParseResponse(resp.RawData);
            if (!parsed.Success || parsed.Data.Count == 0)
            {
                UpdateError(parsed.ErrorMessage);
                error = parsed.ErrorMessage;
                return false;
            }

            value = parsed.Data[0];
            return true;
        }

        private void UpdateError(string error)
        {
            lock (_stateLock)
            {
                _state.LastErrorMessage = error;
                _state.LastUpdateTime = DateTime.Now;
            }
        }

        private bool IsMoving()
        {
            lock (_stateLock)
            {
                return _state.IsMoving;
            }
        }

        private static MotorRuntimeState ResolveRuntimeState(MotorState state)
        {
            if (state == null)
                return MotorRuntimeState.Unknown;

            if (state.IsAlarm)
                return MotorRuntimeState.Alarm;

            if (state.IsServoFree)
                return MotorRuntimeState.Disabled;

            if (state.IsHoming)
                return MotorRuntimeState.Homing;

            if (state.IsMoving)
                return MotorRuntimeState.Moving;

            if (state.IsStop)
                return MotorRuntimeState.Stopping;

            if (state.IsReady)
                return MotorRuntimeState.Idle;

            return MotorRuntimeState.Unknown;
        }

        private static bool GetBit(int rawValue, DriverOutputStatusBit bit)
        {
            return (rawValue & (1 << (int)bit)) != 0;
        }

        private static bool GetBit(int rawValue, DriverInputStatusBit bit)
        {
            return (rawValue & (1 << (int)bit)) != 0;
        }

        private bool ReadOneRegister(int address, out int value, out string error)
        {
            value = 0;
            error = null;

            ModbusResponse resp = _transport.SendAndReceive(
                OrientalMotorProtocol.BuildReadRegisters(_stationNo, address, 1),
                OrientalMotorProtocol.GetExpectedResponseLengthForReadRegisters(1));

            if (!resp.Success)
            {
                error = resp.ErrorMessage;
                UpdateError(error);
                return false;
            }

            OrientalMotorProtocol.ProtocolResult parsed = OrientalMotorProtocol.ParseResponse(resp.RawData);
            if (!parsed.Success || parsed.Data.Count < 1)
            {
                error = parsed.ErrorMessage;
                UpdateError(error);
                return false;
            }

            value = parsed.Data[0];
            return true;
        }

        private bool ReadInt32Register(int address, out int value, out string error)
        {
            value = 0;
            error = null;

            ModbusResponse resp = _transport.SendAndReceive(
                OrientalMotorProtocol.BuildReadRegisters(_stationNo, address, 2),
                OrientalMotorProtocol.GetExpectedResponseLengthForReadRegisters(2));

            if (!resp.Success)
            {
                error = resp.ErrorMessage;
                UpdateError(error);
                return false;
            }

            OrientalMotorProtocol.ProtocolResult parsed = OrientalMotorProtocol.ParseResponse(resp.RawData);
            if (!parsed.Success || parsed.Data.Count < 2)
            {
                error = parsed.ErrorMessage;
                UpdateError(error);
                return false;
            }

            value = OrientalMotorProtocol.ToInt32FromTwoRegisters(parsed.Data[0], parsed.Data[1]);
            return true;
        }

        private void ApplyDirectIOToState(MotorState state, int directIORaw)
        {
            // 先假設：低16-bit = DIN， 高16-bit = DOUT
            int din = directIORaw & 0xFFFF;
            int dout = (directIORaw >> 16) & 0xFFFF;

            state.DirectIORaw = directIORaw;

            state.DIN0_FW_POS = GetBitByIndex(din, (int)DirectIOInputBit.DIN0);
            state.DIN1_RV_POS = GetBitByIndex(din, (int)DirectIOInputBit.DIN1);
            state.DIN2_STOP = GetBitByIndex(din, (int)DirectIOInputBit.DIN2);
            state.DIN3_ALM_RST = GetBitByIndex(din, (int)DirectIOInputBit.DIN3);
            state.DIN4_HOMES = GetBitByIndex(din, (int)DirectIOInputBit.DIN4);
            state.DIN5_FW_LS = GetBitByIndex(din, (int)DirectIOInputBit.DIN5);
            state.DIN6_RV_LS = GetBitByIndex(din, (int)DirectIOInputBit.DIN6);

            state.DOUT0_ALM_B = GetBitByIndex(dout, (int)DirectIOOutputBit.DOUT0);
            state.DOUT1_TIM = GetBitByIndex(dout, (int)DirectIOOutputBit.DOUT1);

            // 極限對應：DIN5 = FW-LS、DIN6 = RV-LS
            state.IsLimitForward = state.DIN5_FW_LS;
            state.IsLimitReverse = state.DIN6_RV_LS;
        }

        private static bool GetBitByIndex(int rawValue, int bitIndex)
        {
            return (rawValue & (1 << bitIndex)) != 0;
        }

        #endregion

        public enum MonitorCommandAddress
        {
            CommandPosition = 0x00C6,   // 指令位置
            CommandSpeedRpm = 0x00C8,   // 指令速度(r/min)
            CommandSpeedHz = 0x00CA,    // 指令速度(Hz)
            RemainingDelayTime = 0x00D2,// 停留的剩餘時間(ms)
            DirectIO = 0x00D4,          // 直接 I/O 狀態
            TargetPosition = 0x00DE,    // 目標位置
            NextDataNo = 0x00E0,        // Next No.
            LoopReturnNo = 0x00E2,      // 環路返回 No.
            CurrentCommunicationError = 0x00AC // 現在通訊錯誤
        }
    }
}
