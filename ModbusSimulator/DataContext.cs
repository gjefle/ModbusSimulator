using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModbusSimulator.models;

namespace ModbusSimulator
{
    public class DataContext: DbContext
    {
        public DataContext(string connectionString): base(connectionString) {}

        public DbSet<NodeConfig> NodeConfig { get; set; }

        public DbSet<RegisterValue> RegisterValue { get; set; }
        
    }
}
