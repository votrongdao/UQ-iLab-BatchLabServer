using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using Library.Lab;
using Modbus.Device;

namespace Library.LabEquipment.Drivers
{
    public class DCDrive
    {
        #region Constants

        private const string STRLOG_ClassName = "DCDrive";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_IPaddr = " IPaddr: ";
        private const string STRLOG_IPport = " IPport: ";
        private const string STRLOG_ReceiveTimout = " Receive Timout: ";
        private const string STRLOG_MilliSecs = " millisecs";
        private const string STRLOG_ModbusSlaveId = " Modbus Slave Id: ";
        private const string STRLOG_DoInitialise = " DoInitialise: ";
        private const string STRLOG_NotInitialised = "Not Initialised!";
        private const string STRLOG_Initialised = " Initialised: ";
        private const string STRLOG_Success = " Success: ";
        private const string STRLOG_IsOk = " Ok";
        private const string STRLOG_HasFailed = " Failed!";
        private const string STRLOG_Speed = " Speed: ";
        private const string STRLOG_Torque = " Torque: ";

        //
        // String constants for error messages
        //
        private const string STRERR_UnableToOpenNetworkConnection = "Unable to open network connection: ";
        private const string STRERR_RegisterWriteReadMismatch = "Register write - read mismatch!";

        #endregion

        #region Variables

        //
        // Local variables
        //
        private Logfile.LoggingLevels logLevel;
        private string ipaddr;
        private int ipport;
        private bool initialised;
        private TcpClient tcpClient;
        private int receiveTimeout;
        private ModbusIpMaster modbusIpMaster;
        private int slaveId;
        private DriverMachine.KeepAliveCallback keepAliveCallback;

        #endregion

        #region Registers

        private const ushort MODBUSREGS_HOLDING_BASEADDRESS = 40001;

        //
        // Modbus function code: 03h (Read Holding Registers [4x])
        //
        private enum ModbusRegs_Holding_RW
        {
            DCControlWord = 40001,
            DCControlSpeed = 40002,
            DCControlTorque = 40003,
            Motor1SpeedMinLimit = 42001,
            Motor1SpeedMaxLimit = 42002,
            Motor1TorqueMaxLimit = 42005,
            Motor1TorqueMinLimit = 42006,
            SpeedRampAccelTime = 42201,
            Motor1FieldRefExtLimit = 44506,
            Motor1FieldRefTrim = 44517,
        };

        //
        // DC drive control word bits
        //
        private const ushort DCW_FIELD_ON = 0x0001;  // (Off1N)
        private const ushort DCW_OFF2N = 0x0002;  // (EM Off / Coast Off)
        private const ushort DCW_OFF3N = 0x0004;  // (E-Stop)
        private const ushort DCW_RUN = 0x0008;
        private const ushort DCW_RAMP_OUT_ZERO = 0x0010;
        private const ushort DCW_RAMP_HOLD = 0x0020;
        private const ushort DCW_RAMP_ZERO = 0x0040;
        private const ushort DCW_RESET = 0x0080;
        private const ushort DCW_INCHING1 = 0x0100;
        private const ushort DCW_INCHING2 = 0x0200;
        private const ushort DCW_REMOTE_CMD = 0x0400;
        private const ushort DCW_DIRECTION = 0x0800;  // 0 = Forward, 1 = Reverse
        private const ushort DCW_TORQUE_MODE = 0x1000; // 0 = Speed, 1 = Torque
        private const ushort DCW_AUX_CONTROL3 = 0x2000;
        private const ushort DCW_AUX_CONTROL4 = 0x4000;
        private const ushort DCW_AUX_CONTROL5 = 0x8000;

        // DC control word - Default value 0x0476
        private const ushort DCW_DEFAULT = DCW_REMOTE_CMD |
                                DCW_RAMP_ZERO | DCW_RAMP_HOLD | DCW_RAMP_OUT_ZERO |
                                DCW_OFF3N | DCW_OFF2N;

        //
        // DC Status word bits
        //
        private const ushort DCSTATUS_RDY_ON = 0x0001;
        private const ushort DCSTATUS_RDY_RUN = 0x0002;
        private const ushort DCSTATUS_RDY_REF = 0x0004;
        private const ushort DCSTATUS_TRIPPED = 0x0008;
        private const ushort DCSTATUS_OFF2N_STATUS = 0x0010;
        private const ushort DCSTATUS_OFF3N_STATUS = 0x0020;
        private const ushort DCSTATUS_ON_INHIBITED = 0x0040;
        private const ushort DCSTATUS_ALARM = 0x0080;
        private const ushort DCSTATUS_AT_SETPOINT = 0x0100;
        private const ushort DCSTATUS_TORQUE_ABOVE_LIMIT = 0x0200;

        private enum ModbusRegs_Holding_RO
        {
            ActualSpeedFeedback = 40101,
            ActualSpeedEMF = 40102,
            ActualSpeedEncoder = 40103,
            ActualSpeedMotor = 40104,
            ActualSpeedTacho = 40105,
            RelativeActualMotorCurrent = 40106,
            FilteredMotorTorque = 40107,
            MotorTorque = 40108,
            RelativeActualArmatureVoltage = 40113,
            ActualArmatureVoltage = 40114,
            RelativeActualEMF = 40117,
            Motor1RelativeActualFieldCurrent = 40129,
            Motor1ActualFieldCurrent = 40130,
            Motor2RelativeActualFieldCurrent = 40131,
            Motor2ActualFieldCurrent = 40132,
            ActualMainsFrequency = 40138,
            TorqueReference1 = 40208,
            TorqueReference2 = 40209,
            TorqueReference3 = 40210,
            TorqueReference4 = 40211,
            UsedTorqueReference = 40213,
            TorqueCorrection = 40214,
            TorqueMaximumAll = 40219,
            TorqueMinimumAll = 40220,
            UsedTorqueMaximum = 40222,
            UsedTorqueMinimum = 40223,
            RelativeExtTorqueRefValue = 40224,
            ActualUsedTorqueLimit = 40226,
        };

        private struct RegisterMapping
        {
            public const int RAW_ZERO = 0;
            public const int RAW_FULL = 1;
            public const int RAW_OFFSET = 2;
            public const int RAW_LENGTH = 3;
            public const int ENG_ZERO = 0;
            public const int ENG_FULL = 1;
            public const int ENG_LENGTH = 2;

            public int register;
            public short[] raw;
            public double[] eng;
            public string units;
            public string comment;
            public double value;

            public RegisterMapping(int register, short raw_zero, short raw_full, short raw_offset, double eng_zero, double eng_full, string units, string comment)
            {
                this.register = register;
                this.raw = new short[RAW_LENGTH] { raw_zero, raw_full, raw_offset };
                this.eng = new double[ENG_LENGTH] { eng_zero, eng_full };
                this.units = units;
                this.comment = comment;
                value = 0;
            }
        }

        private RegisterMapping[] RegisterMapRW = new RegisterMapping[] {
            new RegisterMapping((int)ModbusRegs_Holding_RW.DCControlWord, 0, 0, 0, 0, 0, String.Empty, "DC Control Word"),
            new RegisterMapping((int)ModbusRegs_Holding_RW.DCControlSpeed, -20000, 20000, 0, -1600, 1600, "RPM", "DC Control Ref1 (Speed)"),
            new RegisterMapping((int)ModbusRegs_Holding_RW.DCControlTorque, -32768, 32767, 0, -327.68, 327.67, "%", "DC Control Ref2 (Torque)"),
            new RegisterMapping((int)ModbusRegs_Holding_RW.Motor1SpeedMinLimit, -10000, 0, 0, -10000, 0, "RPM", "Motor 1 Speed Minimum Limit"),
            new RegisterMapping((int)ModbusRegs_Holding_RW.Motor1SpeedMaxLimit, 0, 10000, 0, 0, 10000, "RPM", "Motor 1 Speed Maximum Limit"),
            new RegisterMapping((int)ModbusRegs_Holding_RW.Motor1TorqueMaxLimit, 0, 32500, 0, 0, 325, "%", "Motor 1 Torque Maximum Limit"),
            new RegisterMapping((int)ModbusRegs_Holding_RW.Motor1TorqueMinLimit, -32500, 0, 0, -325, 0, "%", "Motor 1 Torque Minimum Limit"),
            new RegisterMapping((int)ModbusRegs_Holding_RW.SpeedRampAccelTime, 0, 30000, 0, 0, 300, "Secs", "Speed Ramp Accel. Time"),
            new RegisterMapping((int)ModbusRegs_Holding_RW.Motor1FieldRefExtLimit, 0, 10000, 0, 0, 100, "%", "Motor 1 Field Ref. Ext Limit"),
            new RegisterMapping((int)ModbusRegs_Holding_RW.Motor1FieldRefTrim, -200, 200, 0, -20, 20, "%", "Motor 1 Field Ref. Trim"),
        };

        private RegisterMapping[] RegisterMapRO = new RegisterMapping[] {
            new RegisterMapping((int)ModbusRegs_Holding_RO.ActualSpeedFeedback, 0, 2000, 0, 0, 2000, "RPM", "Filtered actual speed feedback"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.ActualSpeedEMF, 0, 2000, 0, 0, 2000, "RPM", "Actual speed calculated from EMF"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.ActualSpeedEncoder, 0, 2000, 0, 0, 2000, "RPM", "Actual speed with pulse encoder"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.ActualSpeedMotor, 0, 2000, 0, 0, 2000, "RPM", "Actual motor speed"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.ActualSpeedTacho, 0, 0, 0, 0, 0, "RPM", "Actual motor speed (analog tacho)"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.RelativeActualMotorCurrent, -10000, 10000, 0, -100, 100, "%", "Relative actual motor current"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.FilteredMotorTorque, 0, 10000, 0, 0, 100, "%", "Filtered motor torque"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.MotorTorque, 0, 10000, 0, 0, 100, "%", "Motor torque"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.RelativeActualArmatureVoltage, 0, 32767, 0, 0, 327.67, "%", "Relative actual armature voltage"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.ActualArmatureVoltage, -400, 400, 0, -400, 400, "V", "Actual armature voltage"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.RelativeActualEMF, -32768, 32767, 0, -327.68, 327.67, "%", "Relative actual EMF"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.Motor1RelativeActualFieldCurrent, 0, 10000, 0, 0, 100, "%", "Motor 1 rel. actual field current"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.Motor1ActualFieldCurrent, 0, 200, 0, 0, 2, "A", "Motor 1 actual field current"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.Motor2RelativeActualFieldCurrent, 0, 10000, 0, 0, 100, "%", "Motor 2 rel. actual field current"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.Motor2ActualFieldCurrent, 0, 200, 0, 0, 2, "A", "Motor 2 actual field current"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.ActualMainsFrequency, 0, 10000, 0, 0, 100, "Hz", "Actual mains frequency"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.TorqueReference1, -32768, 32767, 0, -327.68, 327.67, "%", "Torque Reference 1"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.TorqueReference2, -32768, 32767, 0, -327.68, 327.67, "%", "Torque Reference 2"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.TorqueReference3, -32768, 32767, 0, -327.68, 327.67, "%", "Torque Reference 3"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.TorqueReference4, -32768, 32767, 0, -327.68, 327.67, "%", "Torque Reference 4"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.UsedTorqueReference, -32768, 32767, 0, -327.68, 327.67, "%", "Used torque reference"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.TorqueCorrection, -32768, 32767, 0, -327.68, 327.67, "%", "Torque correction"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.TorqueMaximumAll, -32768, 32767, 0, -327.68, 327.67, "%", "Torque maximum all"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.TorqueMinimumAll, -32768, 32767, 0, -327.68, 327.67, "%", "Torque minimum all"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.UsedTorqueMaximum, -32768, 32767, 0, -327.68, 327.67, "%", "Used torque maximum"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.RelativeExtTorqueRefValue, -32768, 32767, 0, -327.68, 327.67, "%", "Rel. external torque ref. value"),
            new RegisterMapping((int)ModbusRegs_Holding_RO.ActualUsedTorqueLimit, -32768, 32767, 0, -327.68, 327.67, "%", "Actual used torque limit"),
        };

        //
        // Default values for control registers
        //
        public const int DEFAULT_Motor1SpeedMinLimit = 0;
        public const int DEFAULT_Motor1SpeedMaxLimit = 1530;
        public const int DEFAULT_Motor1TorqueMinLimit = -100;
        public const int DEFAULT_Motor1TorqueMaxLimit = 100;
        public const int DEFAULT_SpeedRampAccelTime = 5;
        public const int DEFAULT_Motor1FieldRefExtLimit = 100;
        public const int DEFAULT_Motor1FieldRefTrim = 0;
        public const int DEFAULT_Motor1TorqueMinLimit_Synchronism = -5;
        public const int DEFAULT_Motor1TorqueMaxLimit_Synchronism = 10;

        //
        // Wait delays measured in seconds
        //
        private const int DELAY_Reset = 5;
        private const int DELAY_FieldEnergise = 5;
        private const int DELAY_ChangeSpeed = DEFAULT_SpeedRampAccelTime + 2;
        private const int DELAY_DriveCooldown = 30;

        #endregion // Registers

        #region Properties

        private bool online;
        private string statusMessage;
        private bool doInitialise;
        private int initialiseDelay;
        private string lastError;
        private bool connectionOpen;
        private double speedSetpoint;
        private double torqueMinLimitSetpoint;
        private double torqueMaxLimitSetpoint;

        /// <summary>
        /// Returns true if the hardware has been initialised successfully and is ready for use.
        /// </summary>
        public bool Online
        {
            get { return this.online; }
        }

        public string StatusMessage
        {
            get { return this.statusMessage; }
        }

        public bool DoInitialise
        {
            get { return this.doInitialise; }
            set { this.doInitialise = value; }
        }

        /// <summary>
        /// Time in seconds for the equipment to initialise after power has been applied.
        /// </summary>
        public int InitialiseDelay
        {
            get { return this.initialiseDelay; }
        }

        public string LastError
        {
            get
            {
                string errorMsg = lastError;
                lastError = null;
                return errorMsg;
            }
        }

        public bool ConnectionOpen
        {
            get { return this.connectionOpen; }
        }

        public double SpeedSetpoint
        {
            get { return this.speedSetpoint; }
        }

        public double TorqueMinLimitSetpoint
        {
            get { return this.torqueMinLimitSetpoint; }
        }

        public double TorqueMaxLimitSetpoint
        {
            get { return this.torqueMaxLimitSetpoint; }
        }

        #endregion // Properties

        //-------------------------------------------------------------------------------------------------//

        public DCDrive(XmlNode xmlNodeEquipmentConfig, DriverMachine.KeepAliveCallback keepAliveCallback)
        {
            const string STRLOG_MethodName = "DCDrive";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            try
            {
                //
                // Initialise local variables
                //
                this.keepAliveCallback = keepAliveCallback;
                this.initialised = false;

                //
                // Initialise properties
                //
                this.online = false;
                this.statusMessage = STRLOG_NotInitialised;
                this.lastError = null;
                this.connectionOpen = false;
                this.speedSetpoint = 0;

                //
                // Get the IP address and port number to use
                //
                XmlNode xmlNodeDCdrive = XmlUtilities.GetXmlNode(xmlNodeEquipmentConfig, Consts.STRXML_dcDrive);
                XmlNode xmlNodeNetwork = XmlUtilities.GetXmlNode(xmlNodeDCdrive, Consts.STRXML_network, false);
                this.ipaddr = XmlUtilities.GetXmlValue(xmlNodeNetwork, Consts.STRXML_ipAddr, false);
                IPAddress ipaddr = IPAddress.Parse(this.ipaddr);
                this.ipport = XmlUtilities.GetIntValue(xmlNodeNetwork, Consts.STRXML_ipPort);
                Logfile.Write(STRLOG_IPaddr + this.ipaddr.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_IPport + this.ipport.ToString());

                //
                // Get the network timeouts
                //
                XmlNode xmlNodeTimeouts = XmlUtilities.GetXmlNode(xmlNodeNetwork, Consts.STRXML_timeouts, false);
                this.receiveTimeout = XmlUtilities.GetIntValue(xmlNodeTimeouts, Consts.STRXML_receive);
                Logfile.Write(STRLOG_ReceiveTimout + this.receiveTimeout.ToString() + STRLOG_MilliSecs);

                //
                // Get Modbus slave identity
                //
                XmlNode xmlNodeModbus = XmlUtilities.GetXmlNode(xmlNodeDCdrive, Consts.STRXML_modbus, false);
                this.slaveId = XmlUtilities.GetIntValue(xmlNodeModbus, Consts.STRXML_slaveId);
                Logfile.Write(STRLOG_ModbusSlaveId + this.slaveId.ToString());

                //
                // Get the flag to determine if equipment initialisation is required
                //
                this.doInitialise = XmlUtilities.GetBoolValue(xmlNodeDCdrive, Consts.STRXML_doInitialise, false);
                Logfile.Write(STRLOG_DoInitialise + this.doInitialise.ToString());

                //
                // Get the time it takes to initialise
                //
                this.initialiseDelay = XmlUtilities.GetIntValue(xmlNodeDCdrive, Consts.STRXML_initialiseDelay);

                //
                // Determine the logging level for this class
                //
                try
                {
                    this.logLevel = (Logfile.LoggingLevels)Utilities.GetIntAppSetting(STRLOG_ClassName);
                }
                catch
                {
                    this.logLevel = Logfile.LoggingLevels.Minimum;
                }
                Logfile.Write(Logfile.STRLOG_LogLevel + this.logLevel.ToString());
            }
            catch (Exception ex)
            {
                //
                // Log the message and throw the exception back to the caller
                //
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Initialise()
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            Logfile.Write(STRLOG_Initialised + this.initialised.ToString());

            if (this.initialised == false)
            {
                bool success = true;

                if (this.doInitialise == true)
                {
                    success = (
                        this.SetDCControlWord(DCW_DEFAULT | DCW_RESET, DELAY_Reset) == true &&
                        this.SetDCControlWord(DCW_DEFAULT, 0) == true &&
                        this.SetEngValue(ModbusRegs_Holding_RW.DCControlSpeed, 0, 0) == true &&
                        this.SetEngValue(ModbusRegs_Holding_RW.Motor1SpeedMinLimit, DEFAULT_Motor1SpeedMinLimit, 0) == true &&
                        this.SetEngValue(ModbusRegs_Holding_RW.Motor1SpeedMaxLimit, DEFAULT_Motor1SpeedMaxLimit, 0) == true &&
                        this.SetEngValue(ModbusRegs_Holding_RW.Motor1TorqueMinLimit, DEFAULT_Motor1TorqueMinLimit, 0) == true &&
                        this.SetEngValue(ModbusRegs_Holding_RW.Motor1TorqueMaxLimit, DEFAULT_Motor1TorqueMaxLimit, 0) == true &&
                        this.SetEngValue(ModbusRegs_Holding_RW.Motor1FieldRefExtLimit, DEFAULT_Motor1FieldRefExtLimit, 0) == true &&
                        this.SetEngValue(ModbusRegs_Holding_RW.Motor1FieldRefTrim, DEFAULT_Motor1FieldRefTrim, 0) == true &&
                        this.SetEngValue(ModbusRegs_Holding_RW.SpeedRampAccelTime, DEFAULT_SpeedRampAccelTime, 0) == true
                        );
                }

                //
                // Initialisation is complete
                //
                this.initialised = success;
            }

            string logMessage = STRLOG_Initialised + this.initialised.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return this.initialised;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool OpenConnection()
        {
            const string STRLOG_MethodName = "OpenConnection";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            this.lastError = null;

            try
            {
                //
                // Open a connection to the specified IP address and port number
                //
                this.tcpClient = new TcpClient(this.ipaddr, this.ipport);
                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                this.lastError = STRERR_UnableToOpenNetworkConnection + this.ipaddr + ":" + this.ipport.ToString();
            }

            if (success == true)
            {
                this.tcpClient.ReceiveTimeout = this.receiveTimeout;
                this.modbusIpMaster = ModbusIpMaster.CreateTcp(tcpClient);
            }

            this.connectionOpen = success;

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool CloseConnection()
        {
            const string STRLOG_MethodName = "CloseConnection";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            this.lastError = null;

            if (this.tcpClient != null)
            {
                try
                {
                    NetworkStream networkStream = this.tcpClient.GetStream();
                    if (networkStream != null)
                    {
                        networkStream.Close();
                    }
                    this.tcpClient.Close();
                    this.tcpClient = null;

                    success = true;
                }
                catch (Exception ex)
                {
                    this.lastError = ex.Message;
                    Logfile.WriteError(ex.Message);
                }
            }

            this.connectionOpen = false;

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ReadRegisters()
        {
            const string STRLOG_MethodName = "ReadRegisters";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            this.lastError = null;
            string logMessage;

            try
            {
                for (int i = 0; i < RegisterMapRW.Length; i++)
                {
                    RegisterMapping mapping = RegisterMapRW[i];
                    logMessage = String.Format("[{0,-5:d}]: ", mapping.register);

                    //
                    // Read the register
                    //
                    ushort value = 0;
                    if (this.ReadHoldingRegister(mapping.register, ref value) == true)
                    {
                        //
                        // Convert the register value to eng. units and store 
                        //
                        double engValue = this.ConvertRawToEng(value, mapping.raw, mapping.eng);
                        logMessage += String.Format("Value: {0,-6}  Eng: {1,-10} {2,-5} {3}", value, engValue, mapping.units, mapping.comment);

                        //ushort regValue = this.ConvertEngToRaw(mapping.value, mapping.eng, mapping.raw);
                        //Trace.WriteLine(" Raw: " + String.Format("{0,-6}", regValue) + String.Format(" (0x{0:X4})", regValue));
                    }
                    Logfile.Write(logMessage);
                    Trace.WriteLine(logMessage);
                }
                Trace.WriteLine(String.Empty);

                for (int i = 0; i < RegisterMapRO.Length; i++)
                {
                    RegisterMapping mapping = RegisterMapRO[i];
                    logMessage = String.Format("[{0,-5:d}]: ", mapping.register);

                    //
                    // Read the register
                    //
                    ushort value = 0;
                    if (this.ReadHoldingRegister(mapping.register, ref value) == true)
                    {
                        //
                        // Convert the register value to eng. units and store 
                        //
                        double engValue = this.ConvertRawToEng(value, mapping.raw, mapping.eng);
                        logMessage += String.Format("Value: {0,-6}  Eng: {1,-10} {2,-5} {3}", value, engValue, mapping.units, mapping.comment);

                        //ushort regValue = this.ConvertEngToRaw(mapping.value, mapping.eng, mapping.raw);
                        //Trace.WriteLine(" Raw: " + String.Format("{0,-6}", regValue) + String.Format(" (0x{0:X4})", regValue));
                    }
                    Logfile.Write(logMessage);
                    Trace.WriteLine(logMessage);
                }
                Trace.WriteLine(String.Empty);

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
                Trace.WriteLine(ex.Message);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool KeepAlive()
        {
            bool success = true;

            if (this.connectionOpen == true)
            {
                ushort value = 0;
                success = this.ReadHoldingRegister((int)ModbusRegs_Holding_RW.DCControlWord, ref value);
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StartSpeedMode(double speed)
        {
            const string STRLOG_MethodName = "StartSpeedMode";

            string logMessage = STRLOG_Speed + speed.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success;
            double actualSpeed = 0;

            //
            // Set the speed setpoint before starting the drive
            //
            if ((success = this.SetEngValue(ModbusRegs_Holding_RW.DCControlSpeed, speed, 0)) == true)
            {
                //
                // Update speed setpoint
                //
                this.speedSetpoint = speed;

                //
                // Enable the drive field and cooling fan
                //
                if ((success = this.SetDCControlWord(DCW_DEFAULT | DCW_FIELD_ON, DELAY_FieldEnergise)) == true)
                {
                    //
                    // Wait a moment for the field energy to settle
                    //
                    this.WaitDelay(3);

                    //
                    // Start the drive and give the speed some time to settle
                    //
                    if ((success = this.SetDCControlWord(DCW_DEFAULT | DCW_FIELD_ON | DCW_RUN, DEFAULT_SpeedRampAccelTime + 3)) == true)
                    {
                        if (speed != 0)
                        {
                            //
                            // Check that the DC drive is running at the desired speed
                            //
                            string units = String.Empty;
                            if ((success = this.GetSpeed(ref actualSpeed, ref units)) == true)
                            {
                                if ((success = (actualSpeed > speed - 10 && actualSpeed < speed + 10)) == false)
                                {
                                    //
                                    // ERROR: The drive did not start - does this sometimes. Why?
                                    //
                                    Trace.WriteLine("ERROR: The drive did not start! - Desired speed: " + speed.ToString() +
                                        " - Actual speed: " + actualSpeed.ToString("f0"));
                                }
                            }
                        }
                    }
                }
            }

            logMessage = STRLOG_Success + success.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_Speed + ((int)actualSpeed).ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ChangeSpeed(double speed)
        {
            return this.ChangeSpeed(speed, true);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ChangeSpeed(double speed, bool verify)
        {
            const string STRLOG_MethodName = "ChangeSpeed";

            string logMessage = STRLOG_Speed + speed.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success;
            double actualSpeed = 0;

            if ((success = this.SetEngValue(ModbusRegs_Holding_RW.DCControlSpeed, speed, DELAY_ChangeSpeed)) == true)
            {
                //
                // Update speed setpoint
                //
                this.speedSetpoint = speed;

                if (verify == true)
                {
                    //
                    // Check that the speed has changed to the desired value
                    //
                    string units = String.Empty;
                    if ((success = this.GetSpeed(ref actualSpeed, ref units)) == true)
                    {
                        success = (actualSpeed > speed - 5 && actualSpeed < speed + 5);
                    }
                }
            }

            logMessage = STRLOG_Success + success.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_Speed + ((int)actualSpeed).ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Stop the DC drive by setting the minimum and maximum torque limits to their defaults (-100% and 100%)
        /// and then setting its speed to 0 RPM. The DC drive is then allowed some time to cool down.
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            bool success;

            //
            // Set DC drive speed to zero
            //
            success = (
                this.SetMinTorqueLimit(DEFAULT_Motor1TorqueMinLimit) == true &&
                this.SetMaxTorqueLimit(DEFAULT_Motor1TorqueMaxLimit) == true &&
                this.ChangeSpeed(0) == true &&
                this.SetDCControlWord(DCW_DEFAULT, DELAY_DriveCooldown) == true
                );

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetControlSpeed(ref double speed, ref string units)
        {
            return this.GetEngValue(ModbusRegs_Holding_RW.DCControlSpeed, ref speed, ref units);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetSpeed(ref double speed, ref string units)
        {
            return this.GetEngValue(ModbusRegs_Holding_RO.ActualSpeedEncoder, ref speed, ref units);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetTorque(ref double torque, ref string units)
        {
            return this.GetEngValue(ModbusRegs_Holding_RO.MotorTorque, ref torque, ref units);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetMinTorqueLimit(double torque)
        {
            const string STRLOG_MethodName = "SetMinTorqueLimit";

            string logMessage = STRLOG_Torque + torque.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success;
            
            if ((success = this.SetEngValue(ModbusRegs_Holding_RW.Motor1TorqueMinLimit, torque, 0)) == true)
            {
                //
                // Update torque minimum limit setpoint
                //
                this.torqueMinLimitSetpoint = torque;
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetMaxTorqueLimit(double torque)
        {
            const string STRLOG_MethodName = "SetMaxTorqueLimit";

            string logMessage = STRLOG_Torque + torque.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success;

            if ((success = this.SetEngValue(ModbusRegs_Holding_RW.Motor1TorqueMaxLimit, torque, 0)) == true)
            {
                //
                // Update torque maximum limit setpoint
                //
                this.torqueMaxLimitSetpoint = torque;
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //=================================================================================================//

        private bool GetEngValue(ModbusRegs_Holding_RW register, ref double engValue, ref string units)
        {
            bool success;

            //
            // Find the register mapping
            //
            int index = this.FindRegisterMapping(register);
            RegisterMapping mapping = RegisterMapRW[index];

            //
            // Read the register
            //
            ushort regValue = 0;
            if ((success = this.ReadHoldingRegister((int)register, ref regValue)) == true)
            {
                //
                // Convert the register value to eng. units
                //
                engValue = this.ConvertRawToEng(regValue, mapping.raw, mapping.eng);
                units = mapping.units;
            }

            //Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool GetEngValue(ModbusRegs_Holding_RO register, ref double engValue, ref string units)
        {
            bool success;

            //
            // Find the register mapping
            //
            int index = this.FindRegisterMapping(register);
            RegisterMapping mapping = RegisterMapRO[index];

            //
            // Read the register
            //
            ushort regValue = 0;
            if ((success = this.ReadHoldingRegister((int)register, ref regValue)) == true)
            {
                //
                // Convert the register value to eng. units
                //
                engValue = this.ConvertRawToEng(regValue, mapping.raw, mapping.eng);
                units = mapping.units;
            }

            //Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool SetEngValue(ModbusRegs_Holding_RW register, double engValue, int delay)
        {
            RegisterMapping mapping = RegisterMapRW[this.FindRegisterMapping(register)];

            Trace.Write(String.Format(" {0}: {1} {2}", register, engValue, mapping.units));

            ushort regValue = this.ConvertEngToRaw(engValue, mapping.eng, mapping.raw);

            bool success = this.WriteHoldingRegister(register, regValue, delay);

            Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool SetDCControlWord(ushort value, int seconds)
        {
            ModbusRegs_Holding_RW register = ModbusRegs_Holding_RW.DCControlWord;

            Trace.Write(String.Format(" {0}: 0x{1:X04} ({2})", register, value, value));

            bool success = this.WriteHoldingRegister(register, value, seconds);

            Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool ReadHoldingRegister(int register, ref ushort value)
        {
            ushort[] values = null;

            bool success = this.ReadHoldingRegisters(register, ref values, 1);
            value = values[0];

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool ReadHoldingRegisters(int register, ref ushort[] values, int count)
        {
            bool success = false;
            this.lastError = null;

            try
            {
                //
                // Convert register to Modbus protocol address
                //
                ushort protAddress = (ushort)(register - MODBUSREGS_HOLDING_BASEADDRESS);

                //
                // Read register
                //
                values = this.modbusIpMaster.ReadHoldingRegisters((byte)this.slaveId, protAddress, (ushort)count);

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
                Trace.WriteLine(ex.Message);
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool WriteHoldingRegister(ModbusRegs_Holding_RW register, ushort value, int seconds)
        {
            bool success = false;
            this.lastError = null;

            try
            {
                //
                // Convert register to Modbus protocol address
                //
                ushort protAddress = (ushort)((int)register - MODBUSREGS_HOLDING_BASEADDRESS);

                //
                // Write register
                //
                this.modbusIpMaster.WriteSingleRegister((byte)this.slaveId, protAddress, value);

                //
                // Read the value back and compare it with the value written
                //
                ushort[] inregs = this.modbusIpMaster.ReadHoldingRegisters((byte)this.slaveId, protAddress, (ushort)1);
                if (inregs[0] != value)
                {
                    throw new Exception(STRERR_RegisterWriteReadMismatch);
                }

                //
                // Wait for action to occur
                //
                WaitDelay(seconds);

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
                Trace.WriteLine(ex.Message);
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private double ConvertRawToEng(ushort rawValue, short[] raw, double[] eng)
        {
            //
            // Convert from unsigned short to signed double
            //
            double engValue = (double)((short)rawValue);

            //
            // Add the offset to the value and scale
            //
            if (eng[RegisterMapping.ENG_FULL] - eng[RegisterMapping.ENG_ZERO] > 0)
            {
                engValue = engValue + raw[RegisterMapping.RAW_OFFSET];
                engValue = engValue * (eng[RegisterMapping.ENG_FULL] - eng[RegisterMapping.ENG_ZERO]);
                engValue = engValue / (raw[RegisterMapping.RAW_FULL] - raw[RegisterMapping.RAW_ZERO]);
            }

            //Trace.WriteLine(string.Format("ConvertRawToEng(): rawValue = {0}  engValue = {1}", rawValue, engValue));

            return engValue;
        }

        //-------------------------------------------------------------------------------------------------//

        private ushort ConvertEngToRaw(double engValue, double[] eng, short[] raw)
        {
            double rawValue = engValue;

            //
            // Unscale the value and subtract the offset
            //
            if (eng[RegisterMapping.ENG_FULL] - eng[RegisterMapping.ENG_ZERO] > 0)
            {
                rawValue = rawValue * (raw[RegisterMapping.RAW_FULL] - raw[RegisterMapping.RAW_ZERO]);
                rawValue = rawValue / (eng[RegisterMapping.ENG_FULL] - eng[RegisterMapping.ENG_ZERO]);
                rawValue = rawValue - raw[RegisterMapping.RAW_OFFSET];
            }

            //Trace.WriteLine(string.Format("ConvertEngToRaw(): engValue = {0}  rawValue = {1}", engValue, rawValue));

            //
            // Convert from signed double to unsigned short
            //
            return (ushort)((short)rawValue);
        }

        //-------------------------------------------------------------------------------------------------//

        private int FindRegisterMapping(ModbusRegs_Holding_RW register)
        {
            int index = -1;

            for (int i = 0; i < RegisterMapRW.Length; i++)
            {
                if ((int)register == RegisterMapRW[i].register)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        //-------------------------------------------------------------------------------------------------//

        private int FindRegisterMapping(ModbusRegs_Holding_RO register)
        {
            int index = -1;

            for (int i = 0; i < RegisterMapRO.Length; i++)
            {
                if ((int)register == RegisterMapRO[i].register)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool WaitDelay(int seconds)
        {
            bool success = true;

            for (int i = 0; i < seconds; i++)
            {
                Trace.Write(".");
                Thread.Sleep(1000);

                if (this.keepAliveCallback != null)
                {
                    this.keepAliveCallback();
                }
            }

            return success;
        }


    }
}
