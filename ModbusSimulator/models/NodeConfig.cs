using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ModbusSimulator.models
{
    public class NodeConfig
    {
        private int[] _data;
        [Key]
        public int NodeConfigId { get; set; }


        public string Ip { get; set; }

        public List<RegisterValue> ActiveRegisters { get; set; }
    }
}
