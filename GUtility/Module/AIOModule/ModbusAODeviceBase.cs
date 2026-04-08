using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module
{
    public abstract class ModbusAODeviceBase : IAODeviceController
    {
        protected string _IP;
        protected int _Port;
        protected byte _SlaveID;

        protected TcpClient _Client;
        protected IModbusMaster _Master;

        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        public byte SlaveID
        {
            get { return _SlaveID; }
            set { _SlaveID = value; }
        }

        public bool IsConnected { get; protected set; }


        protected ModbusAODeviceBase(string ip, int port, byte slaveId)
        {
            _IP = ip;
            _Port = port;
            _SlaveID = slaveId;
            IsConnected = false;
        }

        public virtual bool Connect()
        {
            try
            {
                _Client = new TcpClient();
                _Client.Connect(_IP, _Port);

                ModbusFactory factory = new ModbusFactory();
                _Master = factory.CreateMaster(_Client);

                IsConnected = (_Client != null && _Client.Connected && _Master != null);
                return IsConnected;
            }
            catch
            {
                IsConnected = false;
                _Master = null;


                if (_Client != null)
                {
                    _Client.Close();
                    _Client = null;
                }

                return false;
            }
        }

        public virtual bool Reconnect()
        {
            Disconnect();
            return Connect();
        }

        public virtual void Disconnect()
        {
            if (_Client != null)
            {
                _Client.Close();
                _Client = null;
            }

            _Master = null;
            IsConnected = false;
        }

        protected void EnsureConnected()
        {
            if (!IsConnected || _Client == null || _Master == null)
                throw new InvalidOperationException("AO device is not connected.");
        }

        protected void WriteHoldingRegister(int offset, ushort value)
        {
            EnsureConnected();
            _Master.WriteSingleRegister(_SlaveID, (ushort)offset, value);
        }

        protected ushort ReadHoldingRegister(int offset)
        {
            EnsureConnected();
            return _Master.ReadHoldingRegisters(_SlaveID, (ushort)offset, 1)[0];
        }

        public abstract void SetChannelRaw(int channel, ushort rawValue);

        public abstract void SetChannelValue(int channel, double actualValue);

        public abstract void SetChannelsRaw(ushort[] rawValues);

        public abstract void ResetAll();

        public virtual void Dispose()
        {
            Disconnect();
        }
    }
}
