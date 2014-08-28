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
            Console.WriteLine("Eccoci");

            try
            {

                SerialPort port = new SerialPort("COM2", 19200, Parity.None, 8);
                if (port.IsOpen)
                    port.Close();
                port.Open();
                Gunnebo.MFLAFLGate GateObj = new Gunnebo.MFLAFLGate(1, port);

                Console.WriteLine("Modbus Versoin: {0:X}", GateObj.ModbusProtocolVersion);
                Console.WriteLine("Firmware Version: {0:X}", GateObj.FirmwareVersion);
                Console.WriteLine("Firmware Revision: {0:X}", GateObj.FirmwareRevision);
                Console.WriteLine("Aisle Mode: {0}", GateObj.AisleMode);
                Console.WriteLine("Running Mode: {0}", GateObj.RunningMode);
                Console.WriteLine("Priority Direction: {0}", GateObj.PriorotyDirection);
                Console.WriteLine("Transit Mode A: {0}", GateObj.TransitMode_A);
                Console.WriteLine("Stack A: {0}", GateObj.StackA);
                Console.WriteLine("Transit Mode B: {0}", GateObj.TransitMode_B);
                Console.WriteLine("Stack B: {0}", GateObj.StackB);
                Console.WriteLine("Fault Register: {0:X}-{0:X}-{0:X}-{0:X}", GateObj.FaultRegister[0], GateObj.FaultRegister[1], GateObj.FaultRegister[2], GateObj.FaultRegister[3]);
                Console.WriteLine("Fraud Code: {0}", GateObj.FraudCode);
                Console.WriteLine("Sensors: {0:X}", GateObj.Sensors);
                Console.WriteLine("Sensor Mask: {0:X}", GateObj.SensorMask);
                Console.WriteLine("{0}", GateObj.Flags.ToStringStatus());
                Console.WriteLine("Ext Control: {0}", GateObj.GetExtControl());
                Console.WriteLine("Ext Running Mode : {0}", GateObj.GetExtRunningMode());
                Console.WriteLine("Ext Aisle Mode : {0}", GateObj.GetExtAisleMode());
                Console.WriteLine("Ext A Mode : {0}", GateObj.GetExtAMode());
                Console.WriteLine("Ext B Mode : {0}", GateObj.GetExtBMode());
                Console.WriteLine("Ext A Stack : {0}", GateObj.GetExtReaderAStack());
                Console.WriteLine("Ext B Stack : {0}", GateObj.GetExtReaderBStack());
                Console.WriteLine("Buzzer runtime (ms) : {0}", GateObj.GetExtBuzzRuntime());
                Console.WriteLine("Ext Count A : {0}", GateObj.GetExtCountA());
                Console.WriteLine("Ext Count B : {0}", GateObj.GetExtCountB());
                Console.WriteLine("Buzzer Enabled : {0}", GateObj.GetBuzzEnabled());
                Console.WriteLine("Ext Passage Timeout : {0} (ms)", GateObj.GetExtPassageTimeout()*10);
                Console.WriteLine("Ext Fault Door Status : {0}", GateObj.GetExtFaultDoorStatus());
                Console.WriteLine("Ext Transit A Auth : {0}", GateObj.GetExtTransitAAuth());
                Console.WriteLine("Ext Transit B Auth : {0}", GateObj.GetExtTransitBAuth());
                
                GateObj.SetFlafhRedLight();
                GateObj.SetBuzz(1000);
                System.Threading.Thread.Sleep(1000);
                GateObj.SetResetLight();
                System.Threading.Thread.Sleep(3000);
                GateObj.OpenADir_SingleTransit();
                //GateObj.SetTransitMode();
                //System.Threading.Thread.Sleep(5000);
                //GateObj.OpenADir_SingleTransit();

            }
            catch(SystemException ex)
            {
                Console.WriteLine(ex.Message);
            }
           
            /*
             *  Writing
             */
            //startAddress = 1;

            // write three coils
            //master.WriteMultipleCoils(slaveId, startAddress, new bool[] { true, false, true });
            Console.ReadLine();
            //port.Close();
        }
    }
}
