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
using ModbusSimulator.models;

namespace ModbusSimulator
{
    public class Node : IDisposable
    {
        private readonly Thread _thread;
        protected ModbusSlave SlaveNode;
        private TcpListener _listener;
        private IPAddress _ip;
        
        private TaskAwaiter modbusNodeTask;
        private List<RegisterValue> _activeRegisters;

        public Node(int id, IPAddress ip, List<RegisterValue> activeRegisters)
        {
            Id = id;
            _ip = ip;
            Ip = ip.ToString();
            _thread = new Thread(Runner) { Name = ip.ToString() };
            _listener = new TcpListener(_ip, 502);
            _listener.Start();
            SlaveNode = ModbusTcpSlave.CreateTcp(0, _listener);
            SlaveNode.DataStore = DataStoreFactory.CreateDefaultDataStore();
            _thread.Start();
            ActiveRegisters = activeRegisters;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public List<RegisterValue> ActiveRegisters
        {
            get => _activeRegisters;
            set
            {
                _activeRegisters = value;
                OnRegisterChanges();
            }
        }

        public void OnRegisterChanges()
        {
            if (ActiveRegisters == null) return;
            foreach (var activeRegister in ActiveRegisters)
            {
                switch (activeRegister.RegisterType)
                {
                    case RegisterType.Coil:
                        SlaveNode.DataStore.CoilDiscretes[activeRegister.RegisterNumber] = Convert.ToBoolean(activeRegister.Value); break;
                    case RegisterType.DiscreteInput:
                        SlaveNode.DataStore.InputDiscretes[activeRegister.RegisterNumber] = Convert.ToBoolean(activeRegister.Value); break;
                    case RegisterType.InputRegister:
                        SlaveNode.DataStore.InputRegisters[activeRegister.RegisterNumber] = activeRegister.Value; break;
                    case RegisterType.HoldingRegister:
                        SlaveNode.DataStore.HoldingRegisters[activeRegister.RegisterNumber] = activeRegister.Value; break;
                }
            }
        }

        public string Ip { get; set; }

        public bool GetCoil(int start) => SlaveNode.DataStore.CoilDiscretes[start];
        public bool GetDiscreteInput(int start) => SlaveNode.DataStore.InputDiscretes[start];
        public ushort GetInputRegister(int start) => SlaveNode.DataStore.InputRegisters[start];
        public ushort GetHoldingRegister(int start) => SlaveNode.DataStore.HoldingRegisters[start];
        
        
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
