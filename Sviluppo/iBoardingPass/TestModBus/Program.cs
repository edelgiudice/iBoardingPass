using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Modbus.Device;

namespace TestModBus
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
  *  Reading
  */
            TcpClient client = new TcpClient("127.0.0.1", 502);
            Gunnebo.MFLAFLGate GateObj = new Gunnebo.MFLAFLGate(1, client);

            Console.WriteLine("Modbus Versoin: {0:X}" , GateObj.ModbusProtocolVersion);
            Console.WriteLine("Firmware Version: {0:X}" , GateObj.FirmwareVersion);
            Console.WriteLine("Firmware Revision: {0:X}" , GateObj.FirmwareRevision);
            Console.WriteLine("Aisle Mode: {0}" , GateObj.AisleMode);
            /*
             *  Writing
             */
            //startAddress = 1;

            // write three coils
            //master.WriteMultipleCoils(slaveId, startAddress, new bool[] { true, false, true });
            Console.ReadLine();
        }
    }
}
