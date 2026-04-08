using GUtility.MotorController.MotorController;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUtility.MotorController
{
    public class ModbusTransport : IDisposable
    {
        private readonly object _ioLock = new object();
        private readonly MotorConfig _config;
        private SerialPort _serialPort;
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _disposed = false;

        public bool IsOpen
        {
            get
            {
                return _serialPort != null && _serialPort.IsOpen;
            }
        }

        public ModbusTransport(MotorConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            _config = config;
        }

        public void Open()
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort(
                    _config.PortName,
                    _config.BaudRate,
                    _config.Parity,
                    _config.DataBits,
                    _config.StopBits);

                _serialPort.ReadTimeout = _config.ResponseTimeoutMs;
                _serialPort.WriteTimeout = _config.ResponseTimeoutMs;
            }

            if (_serialPort.IsOpen)
                return;

            _serialPort.Open();
        }

        public void Close()
        {
            if (_serialPort == null) return;
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }

        public ModbusResponse SendAndReceive(byte[] request, int expectedResponseLength)
        {
            if (request == null || request.Length == 0)
            {
                return ModbusResponse.Fail("Request is null or empty.");
            }

            if (!IsOpen)
            {
                return ModbusResponse.Fail("SerialPort is not open.");
            }

            int tryCount = _config.RetryCount + 1;
            int i;

            for (i = 0; i < tryCount; i++)
            {
                ModbusResponse response = SendAndReceiveInternal(request, expectedResponseLength);
                if (response.Success)
                    return response;
            }

            return ModbusResponse.Fail("SendAndReceive retry failed.");
        }

        private ModbusResponse SendAndReceiveInternal(byte[] request, int expectedResponseLength)
        {
            lock (_ioLock)
            {
                try
                {
                    //WaitRequestGap();
                    Thread.Sleep(100);

                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();

                    _serialPort.Write(request, 0, request.Length);

                    while (_serialPort.BytesToWrite > 0)
                        Thread.Sleep(1);

                    byte[] response = ReadExpectedResponse(expectedResponseLength, _config.ResponseTimeoutMs);
                    if (response == null || response.Length == 0)
                        return ModbusResponse.Fail("No response.");

                    return ModbusResponse.Ok(response);
                }
                catch (Exception ex)
                {
                    return ModbusResponse.Fail(ex.Message);
                }
            }
        }

        private void WaitRequestGap()
        {
            int elapsed = (int)(DateTime.Now - _lastRequestTime).TotalMilliseconds;
            if (elapsed < _config.MinRequestIntervalMs)
                Thread.Sleep(_config.MinRequestIntervalMs - elapsed);

            _lastRequestTime = DateTime.Now;
        }

        private byte[] ReadExpectedResponse(int expectedLength, int timeoutMs)
        {
            List<byte> buffer = new List<byte>();
            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                int bytes = _serialPort.BytesToRead;
                if (bytes > 0)
                {
                    byte[] temp = new byte[bytes];
                    int read = _serialPort.Read(temp, 0, temp.Length);
                    int i;
                    for (i = 0; i < read; i++)
                        buffer.Add(temp[i]);

                    if (buffer.Count >= expectedLength)
                        return buffer.ToArray();
                }

                Thread.Sleep(2);
            }

            if (buffer.Count > 0)
                return buffer.ToArray();

            return null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Close();
            if (_serialPort != null)
            {
                _serialPort.Dispose();
                _serialPort = null;
            }
        }
    }

    public class ModbusResponse
    {
        public bool Success { get; private set; }
        public byte[] RawData { get; private set; }
        public string ErrorMessage { get; private set; }

        private ModbusResponse() { }

        public static ModbusResponse Ok(byte[] raw)
        {
            return new ModbusResponse()
            {
                Success = true,
                RawData = raw,
                ErrorMessage = null
            };
        }

        public static ModbusResponse Fail(string message)
        {
            return new ModbusResponse()
            {
                Success = false,
                RawData = null,
                ErrorMessage = message
            };
        }
    }
}
