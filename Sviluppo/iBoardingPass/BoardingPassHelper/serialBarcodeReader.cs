using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace iBoardingPass
{
    public class serialBarcodeReader
    {
        public delegate void SerialMessageReceived(string Message);
       

        public event SerialMessageReceived OnMessageReceived;
        private SerialPort _serialPortReader;
        public string PortName
        {
            get
            {
                if (_serialPortReader != null)
                {
                    return _serialPortReader.PortName;
                }
                else
                    return string.Empty;
            }
        }
        public int BaudRate
        {
            get
            {
                if (_serialPortReader != null)
                {
                    return _serialPortReader.BaudRate;
                }
                else
                    return 0;
            }
        }
        public int DataBits
        {
            get
            {
                if (_serialPortReader != null)
                {
                    return _serialPortReader.DataBits;
                }
                else
                    return 0;
            }
        }
        public Parity Parity
        {
            get
            {
                if (_serialPortReader != null)
                {
                    return _serialPortReader.Parity;
                }
                else
                    return Parity.None;
            }
        }

        private char[] _trailStart;
        private char[] _trailEnd;
        private bool _isTrailMode;
        private int _trailState=0;
        private string _tmpBuffer;

        public void SetTrailMode(char[] TrailStart, char[] TrailEnd)
        {
            _isTrailMode = true;
            _trailStart = TrailStart;
            _trailEnd = TrailEnd;
        }

        public void SetTrailMode(byte[] TrailStart, byte[] TrailEnd)
        {
            _isTrailMode = true;
            _trailStart = System.Text.Encoding.ASCII.GetString(TrailStart).ToCharArray();
            _trailEnd = System.Text.Encoding.ASCII.GetString(TrailEnd).ToCharArray(); 
        }

       

        public void ResetTrailMode()
        {
            _isTrailMode = false;
        }

        public void Open()
        {
            if (_serialPortReader.IsOpen)
            {
                _serialPortReader.Close();
            }
            _serialPortReader.Open();
        }

        public void Close()
        {
            if (_serialPortReader.IsOpen)
            {
                _serialPortReader.Close();
            }
        }

        public serialBarcodeReader(string portName, int baudRate, Parity parity, int dataBits)
        {
            _serialPortReader = new SerialPort(portName, baudRate, parity, dataBits);
            
            _serialPortReader.DataReceived += _serialPortReader_DataReceived;
           
        }

        void _serialPortReader_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string tmpStr=_serialPortReader.ReadExisting();
            if (_isTrailMode)
            {
                _tmpBuffer += tmpStr;
                ParseString(_tmpBuffer);
            }
            else
            {
                if (OnMessageReceived != null)
                    OnMessageReceived(tmpStr);
            }
        }

        private string ParseString(string TmpStr)
        {
            //Console.WriteLine(">>>>>>>> " + TmpStr);
            if (_trailState == 0)
            {
                int indexOfStart = TmpStr.IndexOf(new string(_trailStart), 0);
                if (indexOfStart >= 0)
                {
                    _trailState = 1;
                    int startSubString=indexOfStart + new string(_trailStart).Length;
                    string newTmp = TmpStr.Substring(startSubString);
                    _tmpBuffer = newTmp;
                    return ParseString(_tmpBuffer);
                }
                else
                {
                    return string.Empty;
                }
            }
            if (_trailState == 1)
            {
                int indexOfEnd = TmpStr.IndexOf(new string(_trailEnd), 0);
                if (indexOfEnd >= 0)
                {
                    _trailState = 0;
                    string newTmp = TmpStr.Substring(0, indexOfEnd);
                    int endSubString=indexOfEnd + new string(_trailEnd).Length;
                    
                    //Data Received
                    if (OnMessageReceived != null)
                        OnMessageReceived(newTmp);
                    _tmpBuffer = string.Empty;
                    _tmpBuffer = TmpStr.Substring(endSubString);
                    return ParseString(_tmpBuffer);
                    
                }
                else
                {
                    return TmpStr;
                }
            }
            return TmpStr;
        }
    }
}
