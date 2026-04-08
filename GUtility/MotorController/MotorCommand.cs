using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.MotorController
{
    public class MotorCommand
    {
        public Guid CommandId { get; private set; }
        public int MotorIndex { get; set; }
        public MotorCommandType CommandType { get; set; }
        public MotorCommandPriority Priority { get; set; }

        public int Value { get; set; }
        public bool BoolValue { get; set; }

        public DateTime CreateTime { get; private set; }

        public MotorCommand()
        {
            CommandId = Guid.NewGuid();
            CreateTime = DateTime.Now;
            Priority = MotorCommandPriority.Normal;
            CommandType = MotorCommandType.None;
        }

        public static MotorCommand CreateMoveAbsolute(int motorIndex, int target)
        {
            return new MotorCommand()
            {
                MotorIndex = motorIndex,
                CommandType = MotorCommandType.MoveAbsolute,
                Value = target,
                Priority = MotorCommandPriority.Normal
            };
        }

        public static MotorCommand CreateMoveRelative(int motorIndex, int distance)
        {
            return new MotorCommand()
            {
                MotorIndex = motorIndex,
                CommandType = MotorCommandType.MoveRelative,
                Value = distance,
                Priority = MotorCommandPriority.Normal
            };
        }

        public static MotorCommand CreateHome(int motorIndex)
        {
            return new MotorCommand()
            {
                MotorIndex = motorIndex,
                CommandType = MotorCommandType.Home,
                Priority = MotorCommandPriority.Normal
            };
        }

        public static MotorCommand CreateStop(int motorIndex)
        {
            return new MotorCommand()
            {
                MotorIndex = motorIndex,
                CommandType = MotorCommandType.Stop,
                Priority = MotorCommandPriority.High
            };
        }

        public static MotorCommand CreateClearAlarm(int motorIndex)
        {
            return new MotorCommand()
            {
                MotorIndex = motorIndex,
                CommandType = MotorCommandType.ClearAlarm,
                Priority = MotorCommandPriority.High
            };
        }

        public static MotorCommand CreateServoFree(int motorIndex, bool free)
        {
            return new MotorCommand()
            {
                MotorIndex = motorIndex,
                CommandType = MotorCommandType.ServoFree,
                BoolValue = free,
                Priority = MotorCommandPriority.Normal
            };
        }

        public static MotorCommand CreateRefreshLite(int motorIndex)
        {
            return new MotorCommand()
            {
                MotorIndex = motorIndex,
                CommandType = MotorCommandType.RefreshLite,
                Priority = MotorCommandPriority.Low
            };
        }

        public static MotorCommand CreateRefreshFull(int motorIndex)
        {
            return new MotorCommand()
            {
                MotorIndex = motorIndex,
                CommandType = MotorCommandType.RefreshFull,
                Priority = MotorCommandPriority.Low
            };
        }

        public override string ToString()
        {
            return string.Format("[{0}] Motor={1}, Type={2}, Value={3}, Bool={4}, Priority={5}",
                CommandId, MotorIndex, CommandType, Value, BoolValue, Priority);
        }
    }
}
