using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.MotorController
{
    public enum MotorControllerState
    {
        Disconnected = 0,
        Connected = 1,
        Running = 2,
        Error = 3
    }

    public enum MotorRuntimeState
    {
        Unknown = 0,
        Idle = 1,
        Moving = 2,
        Homing = 3,
        Stopping = 4,
        Alarm = 5,
        Disabled = 6
    }

    public enum MotorCommandType
    {
        None = 0,
        MoveAbsolute = 1,
        MoveRelative = 2,
        Home = 3,
        Stop = 4,
        ClearAlarm = 5,
        ServoFree = 6,
        RefreshLite = 7,
        RefreshFull = 8
    }

    public enum MotorCommandPriority
    {
        High = 0,
        Normal = 1,
        Low = 2
    }

    public enum MoveMode
    {
        Absolute = 1,
        Relative = 2
    }

    public enum MotorAxis
    {
        Axis1 = 0,
        Axis2 = 1
    }

    public enum CommsFunctionCode
    {
        ReadReg = 0x03,
        WriteSingleReg = 0x06,
        Diagnosis = 0x08,
        WriteMultiReg = 0x10,
        ReadWriteReg = 0x17
    }  

    public enum DriverInputStatusBit
    {
        M0 = 0,
        M1 = 1,
        M2 = 2,
        START = 3,
        HOME = 4,
        STOP = 5,
        AWO = 6,
        ALM_RST = 7,
        SSTART = 11,
        FW_JOG_P = 12,
        RV_JOG_P = 13,
        FW_POS = 14,
        RV_POS = 15
    }

    public enum DriverOutputStatusBit
    {
        M0_R = 0,
        M1_R = 1,
        M2_R = 2,
        START_R = 3,
        HOME_END = 4,
        READY = 5,
        INFO = 6,
        ALM_A = 7,
        SYS_BSY = 8,
        AREA0 = 9,
        AREA1 = 10,
        TIME_UP = 12,
        MOVE = 13
    }

    public enum DirectIOInputStatusBits
    {
        DIN0 = 0,
        DIN1 = 1,
        DIN2 = 2,
        DIN3 = 3,
        DIN4 = 4,
        DIN5 = 5,
        DIN6 = 6
    }

    public enum DirectIOInputBit
    {
        DIN0 = 0,
        DIN1 = 1,
        DIN2 = 2,
        DIN3 = 3,
        DIN4 = 4,
        DIN5 = 5,
        DIN6 = 6
    }

    public enum DirectIOOutputBit
    {
        DOUT0 = 0,
        DOUT1 = 1
    }

    public enum MonitorCommandAddress
    {
        CurrentCommunicationError = 0x00AC,
        CommandPosition = 0x00C6,
        CommandSpeedRpm = 0x00C8,
        CommandSpeedHz = 0x00CA,
        RemainingDelayTime = 0x00D2,
        DirectIO = 0x00D4,
        TargetPosition = 0x00DE,
        NextDataNo = 0x00E0,
        LoopReturnNo = 0x00E2
    }

    public enum FunctionCodeAddress
    {
        DriverInputStatus = 0x007D,   // 起始位址，讀1筆時會讀 007C/007D 這個命令的值
        DriverOutputStatus = 0x007F   // 起始位址，讀1筆時會讀 007E/007F
    }
}
