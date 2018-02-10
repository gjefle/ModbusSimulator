using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator.models
{
    public class RegisterValue
    {
        [Key]
        public int RegisterValueId { get; set; }

        [ForeignKey(nameof(NodeConfig))]
        public int NodeConfigId { get; set; }

        public NodeConfig NodeConfig { get; set; }

        public string Name { get; set; }
        public RegisterType RegisterType { get; set; }
        public int RegisterNumber { get; set; }
        public ushort Value { get; set; }
    }
}
