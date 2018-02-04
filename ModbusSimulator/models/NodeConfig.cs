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
        private int[] _data;
        [Key]
        public int NodeConfigId { get; set; }

        public string Name { get; set; }

        public string Ip { get; set; }

        public string ActiveRegistersInternal { get; set; }
        public int[] ActiveRegisters
        {
            get
            {
                return Array.ConvertAll(ActiveRegistersInternal.Split(';'), int.Parse);
            }
            set
            {
                _data = value;
                ActiveRegistersInternal = String.Join(";", _data.Select(p => p.ToString()).ToArray());
            }
        }
    }
}
