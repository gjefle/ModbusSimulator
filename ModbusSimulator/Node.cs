using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;

namespace ModbusSimulator
{
    public class Node : IDisposable
    {
        private readonly Thread _thread;
        protected ModbusSlave SlaveNode;
        private TcpListener _listener;
        private IPAddress _ip;
        private TaskAwaiter modbusNodeTask;

        //private readonly Semaphore _semaphore;
        public Node(string name, IPAddress ip)
        {
            //_semaphore = semaphore;
            _ip = ip;
            _thread = new Thread(Runner) { Name = name };
            _listener = new TcpListener(_ip, 502);
            _listener.Start();
            SlaveNode = ModbusTcpSlave.CreateTcp(0, _listener);
            SlaveNode.DataStore = DataStoreFactory.CreateDefaultDataStore();
            _thread.Start();
        }

        public ushort GetCommands(int start) => SlaveNode.DataStore.HoldingRegisters[start];
        private void Runner()
        {
            modbusNodeTask = SlaveNode.ListenAsync().GetAwaiter();
            try
            {
                modbusNodeTask.GetResult();
            }
            catch (TaskCanceledException te) { }
        }

        public void Dispose()
        {
            _listener.Stop();
            _thread.Abort();
        }
    }
}
