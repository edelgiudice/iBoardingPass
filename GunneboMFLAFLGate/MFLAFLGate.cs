using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Modbus.Device;

namespace Gunnebo
{
    public enum GateRegister
    {
        REG_MODBUS_VERSION = 0,
        REG_FIRMWARE_VERSION = 1,
        REG_FIRMWARE_REVISION = 2,
        REG_AISLE_MODE = 5,
        REG_RUNNING_MODE = 6,
        REG_PRIORITY = 7,
        REG_MODE_A = 8,
        REG_STACK_A = 9,
        REG_MODE_B = 10,
        REG_STACK_B = 11,
        REG_FAULT_1 = 12,
        REG_FAULT_2 = 13,
        REG_FAULT_3 = 14,
        REG_FAULT_4 = 15,
        REG_FRAUD = 16,
        REG_SENSORS = 18,
        REG_SENSOR_MASK = 19,
        REG_SENSOR_TRANSIT_MASK = 20,
        REG_FLAGS = 21,
        REG_EXT_CONTROL = 27,
        REG_EXT_RUNNING_MODE = 28,
        REG_EXT_AISLE_MODE = 29,
        REG_EXT_MODE_A = 30,
        REG_EXT_MODE_B = 31,
        REG_EXT_READER_A_MAX_STACK = 32,
        REG_EXT_READER_B_MAX_STACK = 33,
        REG_EXT_VALIDATION_SENSORS = 34,
        REG_BUZZER_RUNTIME = 35,
        REG_EXT_COUNT_A = 38,
        REG_EXT_COUNT_B = 39,
        REG_EXT_BUZZER_ENABLE = 40,
        REG_EXT_ALARMS = 41,
        REG_EXT_PASSAGE_CANC_TIMEOUT = 42,
        REG_EXT_FAULT_DOOR_STATUS = 44,
        REG_EXT_TRANSIT_A_AUTH = 45,
        REG_EXT_TRANSIT_B_AUTH = 46,
        REG_PRINTER_LED = 47,
        REG_BUTTON = 48,
        REG_SUMMARY = 49,
        REG_CTRL_CLOSE_DELAY = 50,
        REG_BUZZER_SHUT_TIMER = 51,
        REG_INTRUSION_ZONE_1_TIMEOUT = 52,
        REG_INTRUSION_ZONE_2_TIMEOUT = 53,
        REG_WRONG_WAY_TIMEOUT = 54,
        REG_MODBUS_RW_COUNTS = 55,
        REG_LIGHT_COLOR_TABLE_NUMBER = 56,
        REG_CUSTOM_COLOR_MASTER_LIGHT2_LIGHT1 = 57,
        REG_CUSTOM_COLOR_MASTER_LIGHT4_LIGHT3 = 58,
        REG_CUSTOM_COLOR_SLAVE_LIGHT2_LIGHT1 = 59,
        REG_CUSTOM_COLOR_SLAVE_LIGHT4_LIGHT3 = 60
    }
    public enum AisleMode
    {
        AISLE_NC = 0, //Normally Closed Aisle
        AISLE_NO = 1, //Normally Open Aisle
    }
    public enum RunningMode
    {
        TRANSIT_MODE = 0,
        FIRE_ALARM_MODE = 1,
        CAN_TIMEOUT_MODE = 2,
        POWERDOWN = 3,
        HOLD_OPEN_MODE = 4,
        HOLD_CLOSE_MODE = 5,
        FAULT_MODE = 6
    }

    public enum PriorotyDirection
    {
        PRIORITY_NOT_DEFINED=0,
        PRIORITY_A=1,
        PRIORITY_B=2
    }

    public enum TransitMode
    {
        LOCKED_MODE=1,
        FREE_MODE=2,
        CONTROLLED_MODE=3
    }

    public enum FraudCode
    {
        FRAUD_NONE=0,
        FRAUD_INTRUSION_ZONE_1_VALIDATION=1,
        FRAUD_INTRUSION_ZONE_2_VALIDATION=2,
        FRAUD_INTRUSION_BLOCKED_AISLE=3,
        FRAUD_INTRUSION_LONG_TRANSIT=4,
        FRAUD_TAIL_GATING=5,
        FRAUD_WRONG_WAY=6
    }

    public enum StatusFlag
    {
        READER_A_ENABLE_DISABLE=0x0001,
        READER_B_ENABLE_DISABLE = 0x0002,
        COUNTING_PULSE_DIR_A=0x0004,
        COUNTING_PULSE_DIR_B= 0x0008,
        READER_A_ACK=0x0010,
        READER_B_ACK = 0x0020,
        PRIORITY_DIR_A = 0x0040,
        PRIORITY_DIR_B = 0x0080,
        DOORS_CLOSED=0x0100,
        DOORS_OPEN=0x0200,
        FRAUD_ALARM=0x0400,
        OUT_OF_SERVICE=0x0800,
        FIRE_ALARM_ECHO=0x1000,
        POWER_FAIL_ALARM=0x2000
    }

    public enum DoorsStatusInFault
    {
        CLOSE=0,
        OPEN_A=1,
        OPEN_B=2,
        STOP=3,
        DISABLED=4
    }

    public enum TransitSummary
    {
        PAX_DETECTED=0,
        GATE_OK=1,
        PAX_CANCEL=2,
        PAX_READY=3,
        PAX_NOT_READY=4,
        PAX_TAILGATE=5,
        PAX_ENTRY_FRAUD=6,
        PAX_EXIT_FRAUD=7,
        PAX_CLEAR_BTN=8,
        PAX_PRESENCE=9
    }

    /*
      Status Flags 
     *  0x0001: reader A enable/disable 
     *  0x0002: reader B enable/disable 
     *  0x0004: counting pulse in direction A 
     *  0x0008: counting pulse in direction B 
     *  0x0010: reader A acknowledge 
     *  0x0020: reader B acknowledge 
     *  0x0040: priority in direction A 
     *  0x0080: priority in direction B 
     *  0x0100: doors are closed 
     *  0x0200: doors are open 
     *  0x0400: fraud alarm 
     *  0x0800: out of service 
     *  0x1000: fire alarm echo 
     *  0x2000: power fail alarm 
    */
    public class StatusClass
    {
        public bool ReaderAEnabled { get {return  _readerAEnabled;} } private bool _readerAEnabled=false;
        public bool ReaderBEnabled { get {return  _readerBEnabled;} } private bool _readerBEnabled=false;
        public bool CountingADir { get {return  _countingADir;} } private bool _countingADir=false;
        public bool CountingBDir { get { return _countingBDir; } } private bool _countingBDir = false;
        public bool ReaderAAcK { get {return  _readerAAcK;} } private bool _readerAAcK=false;
        public bool ReaderABcK { get {return  _readerBAcK;} } private bool _readerBAcK=false;
        public bool PriorityDirA { get {return  _priorityDirA;} } private bool _priorityDirA=false;
        public bool PriorityDirB { get {return  _priorityDirB;}}  private bool _priorityDirB=false;
        public bool DoorsClosed { get {return  _doorsClosed;} } private bool _doorsClosed=false;
        public bool DoorsOpen { get {return  _doorsOpen;} } private bool _doorsOpen=false;
        public bool FraudAllarm { get {return  _fraudAllarm;} } private bool _fraudAllarm=false;
        public bool OutOfService { get {return  _outOfService;} } private bool _outOfService=false;
        public bool FireAlarmEcho { get {return  _fireAlarmEcho;} } private bool _fireAlarmEcho=false;
        public bool PowerFailAlarm { get {return  _powerFailAlarm;} } private bool _powerFailAlarm=false;

        public StatusClass (ushort StatusRegister)
        {
           this._readerAEnabled =  (((int)StatusRegister) & ((int)StatusFlag.READER_A_ENABLE_DISABLE))>0;
           this._readerBEnabled = (((int)StatusRegister) & ((int)StatusFlag.READER_B_ENABLE_DISABLE)) > 0;
           this._countingADir = (((int)StatusRegister) & ((int)StatusFlag.COUNTING_PULSE_DIR_A)) > 0;
           this._countingBDir = (((int)StatusRegister) & ((int)StatusFlag.COUNTING_PULSE_DIR_B)) > 0;
           this._readerAAcK = (((int)StatusRegister) & ((int)StatusFlag.READER_A_ACK)) > 0;
           this._readerBAcK = (((int)StatusRegister) & ((int)StatusFlag.READER_B_ACK)) > 0;
           this._priorityDirA = (((int)StatusRegister) & ((int)StatusFlag.PRIORITY_DIR_A)) > 0;
           this._priorityDirB = (((int)StatusRegister) & ((int)StatusFlag.PRIORITY_DIR_B)) > 0;
           this._doorsClosed = (((int)StatusRegister) & ((int)StatusFlag.DOORS_CLOSED)) > 0;
           this._doorsOpen = (((int)StatusRegister) & ((int)StatusFlag.DOORS_OPEN)) > 0;
           this._fraudAllarm = (((int)StatusRegister) & ((int)StatusFlag.FRAUD_ALARM)) > 0;
           this._outOfService = (((int)StatusRegister) & ((int)StatusFlag.OUT_OF_SERVICE)) > 0;
           this._fireAlarmEcho = (((int)StatusRegister) & ((int)StatusFlag.FIRE_ALARM_ECHO)) > 0;
           this._powerFailAlarm = (((int)StatusRegister) & ((int)StatusFlag.POWER_FAIL_ALARM)) > 0;
        }

        public string ToStringStatus()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Status Register");
            sb.AppendFormat("\tReader A Enabled: {0}\n", this.ReaderAEnabled);
            sb.AppendFormat("\tReader B Enabled: {0}\n", this.ReaderBEnabled);
            sb.AppendFormat("\tCounting A Direction: {0}\n", this.CountingADir);
            sb.AppendFormat("\tCounting B Direction: {0}\n", this.CountingBDir);
            sb.AppendFormat("\tReader A Ack: {0}\n", this.ReaderAAcK);
            sb.AppendFormat("\tReader B Ack: {0}\n", this.ReaderABcK);
            sb.AppendFormat("\tPriority Dir A: {0}\n", this.PriorityDirA);
            sb.AppendFormat("\tPriority Dir B: {0}\n", this.PriorityDirB);
            sb.AppendFormat("\tDoors Closed: {0}\n", this.DoorsClosed);
            sb.AppendFormat("\tDoors Open: {0}\n", this.DoorsOpen);
            sb.AppendFormat("\tFraud Alarm: {0}\n", this.FraudAllarm);
            sb.AppendFormat("\tOut of Service: {0}\n", this.OutOfService);
            sb.AppendFormat("\tFire Alarm Echo: {0}\n", this.FireAlarmEcho);
            sb.AppendFormat("\tPower Failure Alarm: {0}\n", this.PowerFailAlarm);

            return sb.ToString();
        }
    }


    public class MFLAFLGate:IDisposable
    {
        public delegate void ErrorHandler(ApplicationException ax);
        public event ErrorHandler onError;

        private IModbusMaster _master;
        private byte _slaveId;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
                                (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ushort ModbusProtocolVersion
        {
            get
            {
                ushort[] r=_master.ReadHoldingRegisters(_slaveId,(ushort) GateRegister.REG_MODBUS_VERSION, 1);
                return r[0];
            }
        }
        public ushort FirmwareVersion
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_FIRMWARE_VERSION, 1);
                return r[0];
            }
        }
        public ushort FirmwareRevision
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_FIRMWARE_REVISION, 1);
                return r[0];
            }
        }
        public AisleMode AisleMode
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_AISLE_MODE, 1);
                return (AisleMode)r[0];
            }
        }
        public RunningMode RunningMode
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_RUNNING_MODE, 1);
                return (RunningMode)r[0];
            }
        }
        public PriorotyDirection PriorotyDirection
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_PRIORITY, 1);
                return (PriorotyDirection)r[0];
            }
        }
        public TransitMode TransitMode_A
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_MODE_A, 1);
                return (TransitMode)r[0];
            }
        }
        public TransitMode TransitMode_B
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_MODE_B, 1);
                return (TransitMode)r[0];
            }
        }
        public byte StackA
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_STACK_A, 1);
                return (byte)r[0];
            }
        }
        public byte StackB
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_STACK_B, 1);
                return (byte)r[0];
            }
        }
        public ushort[] FaultRegister
        {
            get
            {
                return  _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_FAULT_1, 4);
            }
        }

        public FraudCode FraudCode
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_FRAUD, 1);
                return (FraudCode)r[0];
            }
        }

        public ushort Sensors
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_SENSORS, 1);
                return (ushort)r[0];
            }
        }

        public ushort SensorMask
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_SENSOR_MASK, 1);
                return (ushort)r[0];
            }
        }

        public StatusClass Flags
        {
            get
            {
                ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_FLAGS, 1);
                StatusClass rStatus = new StatusClass(r[0]);
                return rStatus;
            }
        }
        public MFLAFLGate(byte slaveId, SerialPort port)
        {
            Log.Info("Creazione oggetto MFLAFLGate - SlaveID:" + slaveId+" su porta seriale:"+port.PortName);
            _slaveId = slaveId;
            _master = ModbusSerialMaster.CreateRtu(port);
            _master.Transport.ReadTimeout = 1000;
            _master.Transport.WriteTimeout = 1500;
            _master.Transport.WaitToRetryMilliseconds = 500;
            _master.Transport.Retries = 2;
            
            port.ErrorReceived += port_ErrorReceived;
        }

        void port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Log.Warn("Errore segnalato dalla porta collegata al controller del tornello:" + e.EventType.ToString());
        }

        public MFLAFLGate(byte slaveId, TcpClient client)
        {
            _slaveId = slaveId;
            _master = ModbusIpMaster.CreateIp(client);
            
            _master.Transport.ReadTimeout = 1000;
            _master.Transport.WriteTimeout = 1500;
            _master.Transport.WaitToRetryMilliseconds = 500;
            _master.Transport.Retries = 2;
        }

        public bool GetExtControl()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_CONTROL, 1);
            if (r[0] == 1)
                return true;
            else
                return false;
        }

        public RunningMode GetExtRunningMode()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_RUNNING_MODE, 1);
            return (RunningMode)r[0];
        }

        public AisleMode GetExtAisleMode()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_AISLE_MODE, 1);
            return (AisleMode)r[0]; 
        }

        public ushort GetExtAMode()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_MODE_A, 1);
            return (ushort)r[0];
        }

        public ushort GetExtBMode()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_MODE_B, 1);
            return (ushort)r[0];
        }

        public ushort GetExtReaderAStack()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_READER_A_MAX_STACK, 1);
            return (ushort)r[0];
        }

        public ushort GetExtReaderBStack()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_READER_B_MAX_STACK, 1);
            return (ushort)r[0];
        }

        public int GetExtBuzzRuntime()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_BUZZER_RUNTIME, 1);
            return ((ushort)r[0])*128;
        }

        public int GetExtCountA()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_COUNT_A, 1);
            return ((ushort)r[0]);
        }

        public int GetExtCountB()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_COUNT_B, 1);
            return ((ushort)r[0]);
        }

        public bool GetBuzzEnabled()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_BUZZER_ENABLE, 1);
            if (r[0] == 1)
                return true;
            else
                return false;
        }

        public int GetExtPassageTimeout()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_PASSAGE_CANC_TIMEOUT, 1);
            return ((int)r[0]);
        }

      

        public DoorsStatusInFault GetExtFaultDoorStatus()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_FAULT_DOOR_STATUS, 1);
            return ((DoorsStatusInFault)r[0]);
        }

        public ushort GetExtTransitAAuth()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_TRANSIT_A_AUTH, 1);
            return ((ushort)r[0]);
        }

        public ushort GetExtTransitBAuth()
        {
            ushort[] r = _master.ReadHoldingRegisters(_slaveId, (ushort)GateRegister.REG_EXT_TRANSIT_B_AUTH, 1);
            return ((ushort)r[0]);
        }


        public void OpenADir_SingleTransit()
        {
            Log.Debug("Open A in single Transit");  
            try
            {
                _master.WriteSingleRegister(_slaveId, (ushort)GateRegister.REG_EXT_TRANSIT_A_AUTH, 1);
            }
            catch(SystemException ex)
            {
                Log.Error("Eccezione OpenADir_SingleTransit " + ex.ToString());
                if(onError!=null)
                {
                    onError(new ApplicationException("Eccezione OpenADir_SingleTransit ", ex));
                }
            }
            
        }

        public void CancelA_SingleTransit()
        {
            Log.Debug("Cancel A Transit");  
            try
            {
                _master.WriteSingleRegister(_slaveId, (ushort)GateRegister.REG_EXT_TRANSIT_A_AUTH, 2);
            }
            catch (SystemException ex)
            {
                Log.Error("Eccezione CancelA_SingleTransit " + ex.ToString());
                if (onError != null)
                {
                    onError(new ApplicationException("Eccezione CancelA_SingleTransit ", ex));
                }
            }
        }

        public void SetTransitMode()
        {
            Log.Debug("Set Transit Mode");  
            try
            {
                
                _master.WriteSingleRegister(_slaveId, (ushort)GateRegister.REG_EXT_RUNNING_MODE, (ushort)RunningMode.TRANSIT_MODE);
            }
            catch (SystemException ex)
            {
                Log.Error("Eccezione SetTransitMode " + ex.ToString());
                if (onError != null)
                {
                    onError(new ApplicationException("Eccezione SetTransitMode ", ex));
                }
            }
        }

        public void SetBuzz(int buzzTime)
        {
            Log.Debug("Set Buzzer");  
            try
            {
                ushort time = (ushort)((double)buzzTime / 128d);
                _master.WriteSingleRegister(_slaveId, (ushort)GateRegister.REG_BUZZER_RUNTIME, time);
            }
            catch (SystemException ex)
            {
                Log.Error("Eccezione SetBuzz " + ex.ToString());
                if (onError != null)
                {
                    onError(new ApplicationException("Eccezione SetBuzz ", ex));
                }
            }
        }

        public void SetFlafhRedLight()
        {

            Log.Debug("Set Flashing Red Light");
            try
            {
                ushort colorCode = 0x2222;
                _master.WriteSingleRegister(_slaveId, (ushort)GateRegister.REG_CUSTOM_COLOR_MASTER_LIGHT2_LIGHT1, colorCode);
            }
            catch (SystemException ex)
            {
                Log.Error("Eccezione SetFlafhRedLight " + ex.ToString());
                if (onError != null)
                {
                    onError(new ApplicationException("Eccezione SetFlafhRedLight ", ex));
                }
            }
        }

        public void SetResetLight()
        {
            Log.Debug("Reset Light");
            try
            {
                ushort colorCode = 0xffff;
                _master.WriteSingleRegister(_slaveId, (ushort)GateRegister.REG_CUSTOM_COLOR_MASTER_LIGHT2_LIGHT1, colorCode);
            }
            catch (SystemException ex)
            {
                Log.Error("Eccezione SetResetLight " + ex.ToString());
                if (onError != null)
                {
                    onError(new ApplicationException("Eccezione SetResetLight ", ex));
                }
            }
        }

        public void Dispose()
        {
            Log.Info("Dispose Master");
            try
            {
                _master.Dispose();
            }
            catch (SystemException ex)
            {
                Log.Error("Eccezione Dispose " + ex.ToString());
                if (onError != null)
                {
                    onError(new ApplicationException("Eccezione Dispose ", ex));
                }
            }
        }
    }
}
