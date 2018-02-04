using System;
using System.Collections.Generic;
using System.Configuration;
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
            var nodeConfigs = _ctx.NodeConfig.ToList();
            foreach (var nodeConfig in nodeConfigs)
            {
                Nodes.Add(new Node(nodeConfig.Name, nodeConfig.NodeConfigId, IPAddress.Parse(nodeConfig.Ip)));
            }
        }

        public void AddNodeSimulation(byte[] ipAddress, RegisterType type, int number, string name)
        {
            var nodeConfigs = _ctx.NodeConfig.ToList();
            var ip = new IPAddress(ipAddress);
            var nodeConfig = nodeConfigs.FirstOrDefault(n => n.Ip == ipAddress.ToString());
            
            if(nodeConfig == null)
            {
                nodeConfig = new NodeConfig
                {
                    Name = name,
                    Ip = ip.ToString()
                };
                _ctx.NodeConfig.Add(nodeConfig);
            }
            var registerNumber = (int) type * 10000 + number;
            // TODO Add register
            

            _ctx.SaveChanges();
            Nodes.Add(new Node(nodeConfig.Name, nodeConfig.NodeConfigId, new IPAddress(ipAddress)));
        }

        public void RemoveNodeSimulation(int id)
        {
            var nodeConfig = _ctx.NodeConfig.FirstOrDefault(n => n.NodeConfigId == id);
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
