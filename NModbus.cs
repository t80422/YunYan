using Modbus.Device;
using System;
using System.Net.Sockets;

namespace YunYan
{
    internal class NModbus : IDisposable
    {
        private TcpClient _tcpClient;
        private ModbusIpMaster _master;
        private string _ipAddress;
        private int _port;
        private bool _isConnected;
        private byte _uid;


        public NModbus(string ip, int port, byte UID)
        {
            _ipAddress = ip;
            _port = port;
            _isConnected = false;
            _uid = UID;
        }

        public void Connect()
        {
            if (!_isConnected)
            {
                try
                {
                    _tcpClient = new TcpClient(_ipAddress, _port);
                    _master = ModbusIpMaster.CreateIp(_tcpClient);
                    _tcpClient.ReceiveTimeout = 1000; // 設置超時時間為1000毫秒
                    _isConnected = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error connecting to Modbus server: " + ex.Message);
                    _isConnected = false;
                }
            }
        }

        public ushort[] GetData()
        {
            if (!_isConnected)
                Connect();

            try
            {
                return _master.ReadHoldingRegisters(_uid, 0, 50);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading Modbus data: " + ex.Message);
                _isConnected = false;

                return new ushort[50];
            }
        }

        public void Dispose()
        {
            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _isConnected = false;
            }
        }
    }
}
