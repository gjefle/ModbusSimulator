using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator.models
{
    public class NodeConfig
    {
        private double[] _data;
        [Key]
        public int NodeConfigId { get; set; }

        public string Name { get; set; }
        public byte[] Ip { get; set; }

        public string ActiveRegistersInternal { get; set; }
        public double[] ActiveRegisters
        {
            get
            {
                return Array.ConvertAll(ActiveRegistersInternal.Split(';'), Double.Parse);
            }
            set
            {
                _data = value;
                ActiveRegistersInternal = String.Join(";", _data.Select(p => p.ToString()).ToArray());
            }
        }
    }
}
