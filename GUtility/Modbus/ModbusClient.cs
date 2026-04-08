using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NModbus;
using NModbus.Device;
using System.Net.Sockets;

namespace GUtility.Modbus
{
    /// <summary>
    /// 主動建立TCP連線至Modbus Server，並提供讀寫數位輸入/輸出和類比輸入/輸出的功能。
    /// </summary>
    /// <remarks>Modbus Master</remarks>
    public class ModbusClient : ModbusBase
    {
        private TcpClient _Client;
        private IModbusMaster _Master;

        public bool Connect()
        {
            if (_IsRunning)
                return true;

            try
            {
                _Client = new TcpClient();
                _Client.Connect(_IP, _Port);

                var factory = new ModbusFactory();
                _Master = factory.CreateMaster(_Client);

                _IsRunning = true;
                return true;
            }
            catch(Exception ex)
            {
                _IsRunning = false;
                return false;
            }
        }

        public void Disconnect()
        {
            _Client?.Close();
        }

        public override void SetDO(int index, bool value)
        {
            _Master.WriteSingleCoil( _SlaveID, (ushort)index, value);
        }

        public override bool ReadDO(int index)
        {
            return _Master.ReadCoils( _SlaveID, (ushort)index, 1)[0];
        }


        public override bool ReadDI(int index)
        {
            return _Master.ReadInputs( _SlaveID, (ushort)index, 1)[0];
        }

        public override void SetDI(int index, bool value)
        {
            // Client 一般不會寫 DI，測試用可留空
        }


        public override ushort ReadAO(int index)
        {
            return _Master.ReadHoldingRegisters(_SlaveID, (ushort)index, 1)[0];
        }

        public override void SetAO(int index, ushort value)
        {
            _Master.WriteSingleRegister(_SlaveID, (ushort)index, value);
        }

        public override ushort ReadAI(int index)
        {
            return _Master.ReadInputRegisters(_SlaveID, (ushort)index, 1)[0];
        }

        public override void SetAI(int index, ushort value)
        {
            // Client 一般不會寫 AI，測試用可留空
        }


        public override void Dispose()
        {
            Disconnect();
        }

    }
}
