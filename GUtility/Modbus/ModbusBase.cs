using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Modbus
{  
    public abstract class ModbusBase : IDisposable
    {
        protected string _IP;
        protected int _Port;
        protected byte _SlaveID;
        protected bool _IsRunning;


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

        public bool IsRunning
        {
            get { return !_IsRunning; }
            set { _IsRunning = value; }
        }

        public ModbusBase()
        {
            _IP = "127.0.0.1";
            _Port = 502;
            _SlaveID = 1;
            _IsRunning = false;
        }


        public abstract void SetDO(int index, bool value);
        public abstract bool ReadDO(int index);

        public abstract void SetDI(int index, bool value);
        public abstract bool ReadDI(int index);

        public abstract ushort ReadAO(int index);
        public abstract void SetAO(int index, ushort value);

        public abstract ushort ReadAI(int index);
        public abstract void SetAI(int index, ushort value);

        public abstract void Dispose();
    }
}
