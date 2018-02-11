using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ModbusSimulator.models;

namespace ModbusSimulator
{
    public class NodesRunner
    {
        public List<Node> Nodes;
        private DataContext _ctx;

        public NodesRunner()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _ctx = new DataContext(connectionString);
            StartNodeSimulation();
        }

        public void StartNodeSimulation()
        {
            Nodes = new List<Node>();
            var nodeConfigs = _ctx.NodeConfig
                .Include(n => n.ActiveRegisters)
                .ToList();
            foreach (var nodeConfig in nodeConfigs)
            {
                Nodes.Add(new Node(nodeConfig.NodeConfigId, IPAddress.Parse(nodeConfig.Ip), nodeConfig.ActiveRegisters));
            }
        }

        public void RemoveNodeRegiser(RegisterValue reg)
        {
            var registerValue = _ctx.RegisterValue
                .FirstOrDefault(n => n.RegisterValueId == reg.RegisterValueId);

            if (registerValue == null) return;
            // Remove from DB
            _ctx.RegisterValue.Remove(registerValue);
            _ctx.SaveChanges();

            // Clear register on node
            UpdateNodeValue(registerValue.NodeConfigId, registerValue.RegisterValueId, 0);

        }

        public void UpdateNodeValue(int nodeConfigId, int registerId, ushort value)
        {
            var nodeConfig = _ctx.NodeConfig
                .Include(n => n.ActiveRegisters)
                .FirstOrDefault(n => n.NodeConfigId == nodeConfigId);
            var register = nodeConfig?.ActiveRegisters.FirstOrDefault(r => r.RegisterValueId == registerId);

            if (register == null) return;
            register.Value = value;
            _ctx.SaveChanges();
            // Update Node
            var node = Nodes.FirstOrDefault(n => n.Id == nodeConfigId);
            node.ActiveRegisters = nodeConfig.ActiveRegisters;
        }
        public void AddNodeSimulation(byte[] ipAddress, RegisterType type, int number, string name)
        {
            var nodeConfigs = _ctx.NodeConfig
                .Include(n => n.ActiveRegisters)
                .ToList();

            var ip = new IPAddress(ipAddress);
            var nodeConfig = nodeConfigs.FirstOrDefault(n => n.Ip == new IPAddress(ipAddress).ToString());

            if (nodeConfig == null)
            {
                nodeConfig = new NodeConfig
                {
                    Ip = ip.ToString(),
                    ActiveRegisters = new List<RegisterValue>()
                    {
                        new RegisterValue
                        {
                         Name   = $"{name} {(int)type} - {number}",
                         RegisterNumber = number,
                         RegisterType = type,
                         Value = 0
                        }
                    }
                };
                _ctx.NodeConfig.Add(nodeConfig);
                _ctx.SaveChanges();
                Nodes.Add(new Node(nodeConfig.NodeConfigId, new IPAddress(ipAddress), nodeConfig.ActiveRegisters.ToList()));
            }
            else
            {
                var dbReg = nodeConfig.ActiveRegisters
                    .Any(r => r.RegisterNumber == number && r.RegisterType == type);
                if (dbReg) { return; }

                var reg = new RegisterValue
                {
                    Name = $"{name} {(int)type} - {number}",
                    RegisterNumber = number,
                    RegisterType = type,
                    Value = 0
                };
                var regList = nodeConfig.ActiveRegisters.ToList();
                regList.Add(reg);
                nodeConfig.ActiveRegisters = regList;
                _ctx.SaveChanges();

                // Update node
                var node = Nodes.FirstOrDefault(n => n.Id == nodeConfig.NodeConfigId);
                if (node != null)
                {
                    node.ActiveRegisters = nodeConfig.ActiveRegisters;
                }
            }
        }

        public void RemoveNodeSimulation(int id)
        {
            var nodeConfig = _ctx.NodeConfig
                .Include(n => n.ActiveRegisters)
                .FirstOrDefault(n => n.NodeConfigId == id);
            if (nodeConfig != null)
            {
                // Remove from db
                _ctx.NodeConfig.Remove(nodeConfig);
                _ctx.SaveChanges();

                // Find and delete node from Node list
                var node = Nodes.FirstOrDefault(n => n.Id == nodeConfig.NodeConfigId);
                Nodes.Remove(node);
            }
        }



    }
}
