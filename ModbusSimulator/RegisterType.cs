using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    public enum RegisterType
    {
        Coil = 0,
        DiscreteInput = 1,
        InputRegister = 3,
        HoldingRegister = 4
    }
}
