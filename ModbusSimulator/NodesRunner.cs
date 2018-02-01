using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    public class NodesRunner
    {
        private DataContext _ctx;

        public NodesRunner()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _ctx = new DataContext(connectionString);
        }




    }
}
