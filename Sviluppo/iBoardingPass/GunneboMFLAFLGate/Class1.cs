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

    public class MFLAFLGate
    {
        private IModbusMaster _master;
        private byte _slaveId;

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

        public MFLAFLGate(byte slaveId, SerialPort port)
        {
            _slaveId = slaveId;
            _master = ModbusSerialMaster.CreateRtu(port);
        }

        public MFLAFLGate(byte slaveId, TcpClient client)
        {
            _slaveId = slaveId;
            _master = ModbusIpMaster.CreateIp(client);
        }
    }
}
