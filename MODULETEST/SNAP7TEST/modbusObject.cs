using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using System.IO.Ports;




namespace SNAP7TEST
{
    class modbusObject
    {
        public SerialPort port;
        public static Dictionary<string,int> holding_addr;
        public modbusObject(byte slaveID,string portName,int baudrate=115200,Parity parity=Parity.None)
        {
            port = new SerialPort();
            port.BaudRate = baudrate;
            port.DataBits = 8;
            port.Parity = parity;
            port.StopBits = StopBits.One;
            port.PortName = portName;
        }
        public IModbusMaster CreateRtuMaster()
        {
            return ModbusSerialMaster.CreateRtu(port); 
        }
        
    }
}
