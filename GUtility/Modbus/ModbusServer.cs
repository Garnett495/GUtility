using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NModbus;
using NModbus.Device;
using System.Net;
using System.Net.Sockets;

namespace GUtility.Modbus
{
    /// <summary>
    /// 等待連線並回應Modbus Master的請求，提供讀寫數位輸入/輸出和類比輸入/輸出的功能。
    /// </summary>
    /// <remarks>Modbus Slave</remarks>
    public class ModbusServer : ModbusBase
    {
        private TcpListener _Listener;
        private IModbusSlaveNetwork _Network;
        private IModbusSlave _Slave;


        public void Start()
        {
            if (_IsRunning)
                return;

            try
            {
                _Listener = new TcpListener(IPAddress.Parse(_IP), _Port);
                _Listener.Start();

                var factory = new ModbusFactory();
                _Network = factory.CreateSlaveNetwork(_Listener);
                _Slave = factory.CreateSlave(_SlaveID);

                _Network.AddSlave(_Slave);
                _Network.ListenAsync();

                _IsRunning = true;
            }
            catch(Exception ex)
            {
                _IsRunning = false;
            }
        }

        public void Stop()
        {
            try
            {
                if(_Listener != null)
                {
                    _Listener.Stop();
                    _Listener = null;
                }
                _Network = null;
                _Slave = null;
                
            }
            catch(Exception ex)
            {

            }
            finally
            {
                _IsRunning = false;
            }
        }

        public override void SetDO(int index, bool value)
        {
            _Slave.DataStore.CoilDiscretes.WritePoints((ushort)index, new bool[] { value });
        }

        public override bool ReadDO(int index)
        {
            return _Slave.DataStore.CoilDiscretes.ReadPoints((ushort)index, 1)[0];
        }

        public override void SetDI(int index, bool value)
        {
            _Slave.DataStore.CoilInputs.WritePoints((ushort)index, new bool[] { value });
        }

        public override bool ReadDI(int index)
        {
            return _Slave.DataStore.CoilInputs.ReadPoints((ushort)index, 1)[0];
        }

        public override ushort ReadAO(int index)
        {
            return _Slave.DataStore.HoldingRegisters.ReadPoints((ushort)index, 1)[0];
        }

        public override void SetAO(int index, ushort value)
        {
            _Slave.DataStore.HoldingRegisters.WritePoints((ushort)index, new ushort[] { value });
        }

        public override ushort ReadAI(int index)
        {
            return _Slave.DataStore.InputRegisters.ReadPoints((ushort)index, 1)[0];
        }

        public override void SetAI(int index, ushort value)
        {
            _Slave.DataStore.InputRegisters.WritePoints((ushort)index, new ushort[] { value });
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
