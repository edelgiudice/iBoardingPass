using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Modbus;
using System.IO.Ports;

namespace Test
{
    class Program
    {
        #region Test functions

        #region Modbus RTU slave

        /// <summary>
        /// Test a modbus RTU slave
        /// </summary>
        static void Test_ModbusRTUSlave()
        {
            byte unit_id = 1;
            // Created datastore for unit ID 1
            Datastore ds = new Datastore(unit_id);
            // Crete instance of modbus serial RTU (replace COMx with a free serial port - ex. COM5)
            ModbusSlaveSerial ms = new ModbusSlaveSerial(new Datastore[] { ds }, ModbusSerialType.RTU, "COM1", 9600, 8, Parity.Even, StopBits.One, Handshake.None);
            // Start listen
            ms.StartListen();
            // Print and write some registers...
            Random rnd = new Random();
            while (true)
            {
                Console.Write(
                    "---------------------- READING ----------------------" + Environment.NewLine +
                    "Holding register n.1  : " + ms.ModbusDB.Single(x => x.UnitID == unit_id).HoldingRegisters[0].ToString("D5") + Environment.NewLine +
                    "Holding register n.60 : " + ms.ModbusDB.Single(x => x.UnitID == unit_id).HoldingRegisters[59].ToString("D5") + Environment.NewLine +
                    "Coil register    n.32 : " + ms.ModbusDB.Single(x => x.UnitID == unit_id).Coils[31].ToString() + Environment.NewLine +
                    "---------------------- WRITING ----------------------" + Environment.NewLine);
                ms.ModbusDB.Single(x => x.UnitID == unit_id).HoldingRegisters[1] = (ushort)rnd.Next(ushort.MinValue, ushort.MaxValue);
                Console.WriteLine(
                    "Holding register n.2  : " + ms.ModbusDB.Single(x => x.UnitID == unit_id).HoldingRegisters[1].ToString("D5"));
                ms.ModbusDB.Single(x => x.UnitID == unit_id).Coils[15] = Convert.ToBoolean(rnd.Next(0, 1));
                Console.WriteLine(
                    "Coil register    n.16 : " + ms.ModbusDB.Single(x => x.UnitID == unit_id).Coils[15].ToString());
                // Exec the cicle each 2 seconds
                Thread.Sleep(2000);
            }
        }

        #endregion

        #region Modbus RTU master

        /// <summary>
        /// Test modbus RTU master function on a slave RTU id = 5
        /// </summary>
        static void Test_ModbusRTUMaster()
        {
            byte unit_id = 5;
            // Crete instance of modbus serial RTU (replace COMx with a free serial port - ex. COM5)
            ModbusMasterSerial mm = new ModbusMasterSerial(ModbusSerialType.RTU, "COM20", 9600, 8, Parity.Even, StopBits.One, Handshake.None);
            // Exec the connection
            mm.Connect();
            // Read and write some registers on RTU n. 5
            Random rnd = new Random();
            while (true)
            {
                Console.Write(
                    "---------------------- READING ----------------------" + Environment.NewLine +
                    "Holding register n.1  : " + mm.ReadHoldingRegisters(unit_id, 0, 1).First().ToString("D5") + Environment.NewLine +
                    "Input register   n.41 : " + mm.ReadInputRegisters(unit_id, 40, 1).First().ToString("D5") + Environment.NewLine +
                    "Coil register    n 23 : " + mm.ReadCoils(unit_id, 22, 1).First().ToString() + Environment.NewLine +
                    "---------------------- WRITING ----------------------" + Environment.NewLine);
                mm.WriteSingleRegister(unit_id, 4, (ushort)rnd.Next(ushort.MinValue, ushort.MaxValue));
                Console.WriteLine(
                    "Holding register n.5  : " + mm.ReadHoldingRegisters(unit_id, 4, 1).First().ToString("D5") + Environment.NewLine);
                mm.WriteSingleCoil(unit_id, 2, Convert.ToBoolean(rnd.Next(0, 1)));
                Console.WriteLine(
                    "Coil register    n.3  : " + mm.ReadCoils(unit_id, 2, 1).First().ToString() + Environment.NewLine);
                // Exec the cicle each 2 seconds
                Thread.Sleep(2000);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Enter test code here...
            
            // Some default tests...uncomment to use.
            
            //Test_ModbusRTUMaster();
            //Test_ModbusRTUSlave();
        }
    }
}
