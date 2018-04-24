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
        private Task _task;
        private CancellationTokenSource _nodeToken;
        private Semaphore _sem;

        public Node(int id, IPAddress ip, List<RegisterValue> activeRegisters)
        {
            _sem = new Semaphore(1,1);
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
            StartNodeUpdate();
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

        private void StartNodeUpdate()
        {
            _task = Task.Run(async () =>
            {
                _nodeToken = new CancellationTokenSource();
                while (!_nodeToken.IsCancellationRequested)
                {
                    var waitTask = Task.Delay(200, _nodeToken.Token);
                    _sem.WaitOne();
                    foreach (var activeRegister in ActiveRegisters)
                    {
                        switch (activeRegister.RegisterType)
                        {
                            case RegisterType.Coil:
                                activeRegister.Value = Convert.ToUInt16(SlaveNode.DataStore.CoilDiscretes[activeRegister.RegisterNumber]);  break;
                            case RegisterType.DiscreteInput:
                                activeRegister.Value = Convert.ToUInt16(SlaveNode.DataStore.InputDiscretes[activeRegister.RegisterNumber]); break;
                            case RegisterType.InputRegister:
                                activeRegister.Value = 14; break;
                            case RegisterType.HoldingRegister:
                                activeRegister.Value = SlaveNode.DataStore.HoldingRegisters[activeRegister.RegisterNumber -1]; break;
                        }
                    }
                    _sem.Release();
                    await waitTask;
                }
            });
        }

        public void OnRegisterChanges()
        {
            if (ActiveRegisters == null) return;
            _sem.WaitOne();
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
            _sem.Release();
        }

        public string Ip { get; set; }
        
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
