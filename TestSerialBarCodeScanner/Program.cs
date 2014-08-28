using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iBoardingPass;

namespace TestSerialBarCodeScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            serialBarcodeReader reader = new serialBarcodeReader("COM7", 9600, System.IO.Ports.Parity.None, 8);
            reader.AddParser(new byte[] { 0x02, 0x36 }, new byte[] { 0x0D, 0x03 });
            reader.AddParser(new byte[] { 0x02, 0x38 }, new byte[] { 0x0D, 0x03 });
            reader.OnMessageReceived += reader_OnMessageReceived;
            reader.Open();
            Console.ReadLine();
            reader.Close();
        }

        static void reader_OnMessageReceived(string Message)
        {
            Console.WriteLine(Message);
            try
            {
                BoardingPassData _BpD = BoardingPassData.Parse(Message);

                if (_BpD != null)
                {
                    Console.WriteLine("Flight N." + _BpD.FlightNumber);
                    Console.WriteLine("Date: "+BPHelper.GetDateFromJulian(Convert.ToInt32(_BpD.DateOfFlight)).ToShortDateString());
                    Console.WriteLine("From City: "+_BpD.FromCity);
                    Console.WriteLine("To City: "+_BpD.ToCity);
                    Console.WriteLine("Carrier: "+_BpD.OperatingCarrier);
                    Console.WriteLine("Passenger Name: "+ _BpD.PassengerName);


                    bool GoNoGo = true;
                    string resultCheck = string.Empty;
                    string StatoVolo = string.Empty;
                    string OraPrevistaVolo = string.Empty;
                    string OraEffettivaVolo = string.Empty;

                    

                    //Inserisce il dato letto nel DB e verifica la carta di imbarco
                    try
                    {
                        resultCheck = BPHelper.CheckBoardingPass(@"Data Source=EASYGATE01-PC\SQLEXPRESS;Initial Catalog=EasyGate;Persist Security Info=True;Trusted_Connection=True",
                            "CAG",
                            _BpD,
                            1,
                            3,
                            false,
                            out StatoVolo,
                            out OraPrevistaVolo,
                            out OraEffettivaVolo);
                        Console.WriteLine("Result check: "+resultCheck);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            catch (SystemException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
