using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.MotorController
{
    namespace MotorController
    {
        public class MotorConfig
        {
            public string PortName { get; set; }
            public int BaudRate { get; set; }
            public int DataBits { get; set; }
            public System.IO.Ports.Parity Parity { get; set; }
            public System.IO.Ports.StopBits StopBits { get; set; }

            public int AxisCount { get; set; }

            public int DefaultMoveSpeed { get; set; }
            public int DefaultAcceleration { get; set; }
            public int DefaultDeceleration { get; set; }

            public int DefaultJogDistance { get; set; }

            public int IdlePollingIntervalMs { get; set; }
            public int MovingPollingIntervalMs { get; set; }
            public int FullPollingIntervalMs { get; set; }

            public int MinRequestIntervalMs { get; set; }
            public int ResponseTimeoutMs { get; set; }
            public int RetryCount { get; set; }

            public int CommandLoopSleepMs { get; set; }
            public int PollingLoopSleepMs { get; set; }

            public int CommunicationTimeoutAlarmMs { get; set; }
            public int CommunicationAbnormalAlarmCount { get; set; }

            public MotorConfig()
            {
                PortName = "COM1";
                BaudRate = 115200;
                DataBits = 8;
                Parity = System.IO.Ports.Parity.None;
                StopBits = System.IO.Ports.StopBits.One;

                AxisCount = 2;

                DefaultMoveSpeed = 200;
                DefaultAcceleration = 2000;
                DefaultDeceleration = 2000;
                DefaultJogDistance = 100;

                IdlePollingIntervalMs = 800;
                MovingPollingIntervalMs = 300;
                FullPollingIntervalMs = 2000;

                MinRequestIntervalMs = 80;
                ResponseTimeoutMs = 200;
                RetryCount = 1;

                CommandLoopSleepMs = 5;
                PollingLoopSleepMs = 20;
            }
        }
    }
}
