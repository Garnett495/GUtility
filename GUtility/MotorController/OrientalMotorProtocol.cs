using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.MotorController
{
    public static class OrientalMotorProtocol
    {
        public class ProtocolResult
        {
            public bool Success;
            public string ErrorMessage;
            public int StartAddress;
            public List<int> Data;

            public ProtocolResult()
            {
                Data = new List<int>();
            }
        }

        public static byte[] BuildReadSingleRegister(byte stationNo, FunctionCodeAddress address)
        {
            byte[] cmd = new byte[6];
            cmd[0] = stationNo;
            cmd[1] = (byte)CommsFunctionCode.ReadReg;
            cmd[2] = (byte)(((int)address >> 8) & 0xFF);
            cmd[3] = (byte)((int)address & 0xFF);
            cmd[4] = 0x00;
            cmd[5] = 0x01;
            return AppendCRC16(cmd);
        }

        public static byte[] BuildWriteSingleRegister(byte stationNo, FunctionCodeAddress address, int value)
        {
            byte[] cmd = new byte[6];
            cmd[0] = stationNo;
            cmd[1] = (byte)CommsFunctionCode.WriteSingleReg;
            cmd[2] = (byte)(((int)address >> 8) & 0xFF);
            cmd[3] = (byte)((int)address & 0xFF);
            cmd[4] = (byte)((value >> 8) & 0xFF);
            cmd[5] = (byte)(value & 0xFF);
            return AppendCRC16(cmd);
        }

        public static byte[] BuildMoveCommand(
            byte stationNo,
            MoveMode mode,
            int targetPosition,
            int speed,
            int acceleration,
            int deceleration)
        {
            byte[] cmd = new byte[39];

            cmd[0] = stationNo;
            cmd[1] = (byte)CommsFunctionCode.WriteMultiReg;
            cmd[2] = 0x00;
            cmd[3] = 0x58;
            cmd[4] = 0x00;
            cmd[5] = 0x10;
            cmd[6] = 0x20;

            cmd[7] = 0x00;
            cmd[8] = 0x00;
            cmd[9] = 0x00;
            cmd[10] = 0x00;

            cmd[11] = 0x00;
            cmd[12] = 0x00;
            cmd[13] = 0x00;
            cmd[14] = (mode == MoveMode.Absolute) ? (byte)0x01 : (byte)0x02;

            byte[] target = IntTo4BytesBigEndian(targetPosition);
            cmd[15] = target[0];
            cmd[16] = target[1];
            cmd[17] = target[2];
            cmd[18] = target[3];

            byte[] spd = IntTo4BytesBigEndian(speed);
            cmd[19] = spd[0];
            cmd[20] = spd[1];
            cmd[21] = spd[2];
            cmd[22] = spd[3];

            byte[] acc = IntTo4BytesBigEndian(acceleration);
            cmd[23] = acc[0];
            cmd[24] = acc[1];
            cmd[25] = acc[2];
            cmd[26] = acc[3];

            byte[] dec = IntTo4BytesBigEndian(deceleration);
            cmd[27] = dec[0];
            cmd[28] = dec[1];
            cmd[29] = dec[2];
            cmd[30] = dec[3];

            cmd[31] = 0x00;
            cmd[32] = 0x00;
            cmd[33] = 0x03;
            cmd[34] = 0xE8;

            cmd[35] = 0x00;
            cmd[36] = 0x00;
            cmd[37] = 0x00;
            cmd[38] = 0x01;

            return AppendCRC16(cmd);
        }

        public static byte[] BuildReadRegisters(byte stationNo, int startAddress, int registerCount)
        {
            byte[] cmd = new byte[6];
            cmd[0] = stationNo;
            cmd[1] = (byte)CommsFunctionCode.ReadReg;
            cmd[2] = (byte)((startAddress >> 8) & 0xFF);
            cmd[3] = (byte)(startAddress & 0xFF);
            cmd[4] = (byte)((registerCount >> 8) & 0xFF);
            cmd[5] = (byte)(registerCount & 0xFF);
            return AppendCRC16(cmd);
        }

        public static byte[] BuildHomeCommand(byte stationNo)
        {
            int value = (1 << (int)DriverInputStatusBit.HOME);
            return BuildWriteSingleRegister(stationNo, FunctionCodeAddress.DriverInputStatus, value);
        }

        public static int GetExpectedResponseLengthForReadOneRegister()
        {
            return 7;
        }

        public static int GetExpectedResponseLengthForWriteSingleRegister()
        {
            return 8;
        }

        public static int GetExpectedResponseLengthForWriteMultiRegister()
        {
            return 8;
        }

        public static int GetExpectedResponseLengthForReadRegisters(int registerCount)
        {
            return 1 + 1 + 1 + (registerCount * 2) + 2;
        }

        public static ProtocolResult ParseResponse(byte[] response)
        {
            ProtocolResult ret = new ProtocolResult();

            if (response == null || response.Length < 5)
            {
                ret.Success = false;
                ret.ErrorMessage = "Response is null or too short.";
                return ret;
            }

            string crcError;
            if (!CheckCRC16Correct(response, out crcError))
            {
                ret.Success = false;
                ret.ErrorMessage = crcError;
                return ret;
            }

            byte functionCode = response[1];

            if ((functionCode & 0x80) != 0)
            {
                ret.Success = false;
                ret.ErrorMessage = "Modbus exception response. Code=" + response[2];
                return ret;
            }

            if (functionCode == (byte)CommsFunctionCode.ReadReg)
            {
                int byteCount = response[2];
                int i;
                for (i = 0; i < byteCount; i += 2)
                {
                    int value = (response[3 + i] << 8) | response[3 + i + 1];
                    ret.Data.Add(value);
                }
            }
            else if (functionCode == (byte)CommsFunctionCode.WriteSingleReg ||
                     functionCode == (byte)CommsFunctionCode.WriteMultiReg)
            {
                if (response.Length >= 6)
                {
                    ret.StartAddress = (response[2] << 8) | response[3];
                    ret.Data.Add((response[4] << 8) | response[5]);
                }
            }

            ret.Success = true;
            return ret;
        }

        public static int SetBit(int originalValue, int bit, bool onOff)
        {
            if (onOff)
                return originalValue | (1 << bit);

            return originalValue & ~(1 << bit);
        }

        public static byte[] AppendCRC16(byte[] data)
        {
            ushort crc = ComputeCRC16(data);

            byte low = (byte)(crc & 0xFF);
            byte high = (byte)((crc >> 8) & 0xFF);

            byte[] result = new byte[data.Length + 2];
            Array.Copy(data, result, data.Length);
            result[result.Length - 2] = low;
            result[result.Length - 1] = high;

            return result;
        }

        public static ushort ComputeCRC16(byte[] data)
        {
            ushort crc = 0xFFFF;
            int i;
            foreach (byte b in data)
            {
                crc ^= b;
                for (i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }

        public static bool CheckCRC16Correct(byte[] data, out string err)
        {
            err = null;

            if (data == null || data.Length < 4)
            {
                err = "CRC check failed: data too short.";
                return false;
            }

            int length = data.Length;
            byte[] procData = new byte[length - 2];
            Array.Copy(data, 0, procData, 0, length - 2);

            byte[] calc = AppendCRC16(procData);

            if (data[length - 2] != calc[length - 2] ||
                data[length - 1] != calc[length - 1])
            {
                err = "CRC mismatch.";
                return false;
            }

            return true;
        }

        public static byte[] IntTo4BytesBigEndian(int value)
        {
            return new byte[]
            {
                (byte)((value >> 24) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)(value & 0xFF)
            };
        }

        public static int ToInt32FromTwoRegisters(int highWord, int lowWord)
        {
            unchecked
            {
                uint value = ((uint)(ushort)highWord << 16) | (uint)(ushort)lowWord;
                return (int)value;
            }
        }

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
