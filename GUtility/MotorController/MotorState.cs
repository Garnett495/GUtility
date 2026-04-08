using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.MotorController
{
    public class MotorState
    {
        public int MotorIndex { get; set; }
        public int StationNo { get; set; }

        public MotorRuntimeState RuntimeState { get; set; }

        public int CommandPosition { get; set; }
        public int TargetPosition { get; set; }

        public int CommandSpeedHz { get; set; }
        public int CommandSpeedRpm { get; set; }
        public int RemainingDelayTimeMs { get; set; }
        public int TargetPositionFromDriver { get; set; }
        public int NextDataNo { get; set; }
        public int LoopReturnNo { get; set; }
        public int CurrentCommunicationErrorCode { get; set; }

        public bool IsReady { get; set; }
        public bool IsMoving { get; set; }
        public bool IsHoming { get; set; }
        public bool IsHomeEnd { get; set; }
        public bool IsAlarm { get; set; }
        public bool IsStop { get; set; }
        public bool IsServoFree { get; set; }

        // Direct I/O
        public int DirectIORaw { get; set; }
        public bool DIN0_FW_POS { get; set; }
        public bool DIN1_RV_POS { get; set; }
        public bool DIN2_STOP { get; set; }
        public bool DIN3_ALM_RST { get; set; }
        public bool DIN4_HOMES { get; set; }
        public bool DIN5_FW_LS { get; set; }
        public bool DIN6_RV_LS { get; set; }
        public bool DOUT0_ALM_B { get; set; }
        public bool DOUT1_TIM { get; set; }

        // 極限判斷
        public bool IsLimitForward { get; set; }
        public bool IsLimitReverse { get; set; }

        public int DriverInputRaw { get; set; }
        public int DriverOutputRaw { get; set; }

        public int ErrorCode { get; set; }
        public string LastErrorMessage { get; set; }

        public DateTime LastUpdateTime { get; set; }
        public DateTime LastCommandTime { get; set; }

        public MotorState Clone()
        {
            return new MotorState()
            {
                MotorIndex = this.MotorIndex,
                StationNo = this.StationNo,
                RuntimeState = this.RuntimeState,

                CommandPosition = this.CommandPosition,
                TargetPosition = this.TargetPosition,
                CommandSpeedHz = this.CommandSpeedHz,
                CommandSpeedRpm = this.CommandSpeedRpm,
                RemainingDelayTimeMs = this.RemainingDelayTimeMs,
                TargetPositionFromDriver = this.TargetPositionFromDriver,
                NextDataNo = this.NextDataNo,
                LoopReturnNo = this.LoopReturnNo,
                CurrentCommunicationErrorCode = this.CurrentCommunicationErrorCode,

                IsReady = this.IsReady,
                IsMoving = this.IsMoving,
                IsHoming = this.IsHoming,
                IsHomeEnd = this.IsHomeEnd,
                IsAlarm = this.IsAlarm,
                IsStop = this.IsStop,
                IsServoFree = this.IsServoFree,

                DirectIORaw = this.DirectIORaw,
                DIN0_FW_POS = this.DIN0_FW_POS,
                DIN1_RV_POS = this.DIN1_RV_POS,
                DIN2_STOP = this.DIN2_STOP,
                DIN3_ALM_RST = this.DIN3_ALM_RST,
                DIN4_HOMES = this.DIN4_HOMES,
                DIN5_FW_LS = this.DIN5_FW_LS,
                DIN6_RV_LS = this.DIN6_RV_LS,
                DOUT0_ALM_B = this.DOUT0_ALM_B,
                DOUT1_TIM = this.DOUT1_TIM,

                IsLimitForward = this.IsLimitForward,
                IsLimitReverse = this.IsLimitReverse,

                DriverInputRaw = this.DriverInputRaw,
                DriverOutputRaw = this.DriverOutputRaw,

                ErrorCode = this.ErrorCode,
                LastErrorMessage = this.LastErrorMessage,
                LastUpdateTime = this.LastUpdateTime,
                LastCommandTime = this.LastCommandTime
            };
        }
    }
}
