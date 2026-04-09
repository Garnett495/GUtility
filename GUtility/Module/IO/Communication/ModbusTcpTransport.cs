using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using GUtility.Module.IO.Abstractions;
using NModbus;

namespace GUtility.Module.IO.Communication
{
    /// <summary>
    /// Modbus TCP 傳輸層實作。
    /// </summary>
    public class ModbusTcpTransport : IGModbusTransport, IDisposable
    {
        private TcpClient _client;
        private IModbusMaster _master;

        public bool IsConnected
        {
            get
            {
                return _client != null && _client.Connected && _master != null;
            }
        }

        public bool Connect(string ip, int port)
        {
            Disconnect();

            try
            {
                _client = new TcpClient();
                _client.Connect(ip, port);

                ModbusFactory factory = new ModbusFactory();
                _master = factory.CreateMaster(_client);

                return true;
            }
            catch
            {
                Disconnect();
                return false;
            }
        }

        public void Disconnect()
        {
            if (_master != null)
            {
                var disposableMaster = _master as IDisposable;
                if (disposableMaster != null)
                {
                    disposableMaster.Dispose();
                }
                _master = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        public bool[] ReadCoils(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            EnsureConnected();
            return _master.ReadCoils(slaveId, startAddress, numberOfPoints);
        }

        public bool[] ReadInputs(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            EnsureConnected();
            return _master.ReadInputs(slaveId, startAddress, numberOfPoints);
        }

        public ushort[] ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            EnsureConnected();
            return _master.ReadHoldingRegisters(slaveId, startAddress, numberOfPoints);
        }

        public void WriteSingleCoil(byte slaveId, ushort coilAddress, bool value)
        {
            EnsureConnected();
            _master.WriteSingleCoil(slaveId, coilAddress, value);
        }

        public void WriteMultipleCoils(byte slaveId, ushort startAddress, bool[] data)
        {
            EnsureConnected();
            _master.WriteMultipleCoils(slaveId, startAddress, data);
        }

        public void WriteSingleRegister(byte slaveId, ushort registerAddress, ushort value)
        {
            EnsureConnected();
            _master.WriteSingleRegister(slaveId, registerAddress, value);
        }

        public void WriteMultipleRegisters(byte slaveId, ushort startAddress, ushort[] data)
        {
            EnsureConnected();
            _master.WriteMultipleRegisters(slaveId, startAddress, data);
        }

        private void EnsureConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Modbus TCP transport is not connected.");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
