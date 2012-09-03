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
    public class PLC
    {
        #region Constants

        private const string STRLOG_ClassName = "PLC";

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
        private const string STRLOG_Enable = " Enable: ";
        private const string STRLOG_Success = " Success: ";
        private const string STRLOG_Ok = " Ok: ";
        private const string STRLOG_IsOk = " Ok";
        private const string STRLOG_HasFailed = " Failed!";
        private const string STRLOG_NoChange = "No Change.";

        //
        // String constants for error messages
        //
        private const string STRERR_UnableToOpenNetworkConnection = "Unable to open network connection: ";
        private const string STRERR_ReadAllRegistersFailed = "Read all registers failed!";
        private const string STRERR_RegisterWriteReadMismatch = "Register write/read mismatch!";
        private const string STRERR_CommandFailedToStart = "Command failed to start!";
        private const string STRERR_CommandFailedToComplete = "Command failed to complete!";

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

        private const ushort MODBUSREGS_COILS_BASEADDRESS = 00001;
        private const ushort MODBUSREGS_INPUTS_BASEADDRESS = 10001;
        private const ushort MODBUSREGS_INPUTREGS_BASEADDRESS = 30001;

        private enum ModbusRegs_Coils_BW
        {
            ContactorAClose = 00201,
            ContactorAOpen = 00202,
            ContactorBClose = 00203,
            ContactorBOpen = 00204,
            MOPIncrement = 00205, // Motor Operated Potentiometer
            MOPDecrement = 00206,
            MOPInitialise = 00207,
            SyncFieldEnable = 00208,
            SyncFieldDisable = 00209,
            SyncCheckEnable = 00210,
            SyncCheckDisable = 00211,
            DCDriveEnable = 00212,
            DCDriveDisable = 00213,
        }

        private enum ModbusRegs_Inputs_BO
        {
            EmergencyStop = 10051,
            ContactorAClosed = 10052,
            ContactorBClosed = 10053,
            DCDriveOk = 10054,
            DCDriveInReverse = 10055,
            InSynchronism = 10056,
            ProtectionTrip = 10057,
            MOPLowerLimit = 10058,
            MOPUpperLimit = 10059,
            TemperatureTrip = 10060,
            ProtectionCBTrip = 10061,

            ContactorACloseCmdStat = 10076,
            ContactorBCloseCmdStat = 10077,
            MOPIncrementCmdStat = 10078,
            MOPDecrementCmdStat = 10079,
            MOPInitialiseCmdStat = 10080,
            SyncFieldEnableCmdStat = 10081,
            SyncCheckEnableCmdStat = 10082,
            DCDriveEnableCmdStat = 10083,
        };

        private enum ModbusRegs_InputRegs_RO
        {
            GenFieldCurrent = 30053,
        };

        private struct RegisterMapping_Coils
        {
            public int register;
            public string comment;

            public RegisterMapping_Coils(int register, string comment)
            {
                this.register = register;
                this.comment = comment;
            }
        }

        private struct RegisterMapping_Inputs
        {
            public int register;
            public string comment;
            public bool value;

            public RegisterMapping_Inputs(int register, string comment)
            {
                this.register = register;
                this.comment = comment;
                value = false;
            }
        }

        private struct RegisterMapping_InputRegs
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

            public RegisterMapping_InputRegs(int register, short raw_zero, short raw_full, short raw_offset, double eng_zero, double eng_full, string units, string comment)
            {
                this.register = register;
                this.raw = new short[RAW_LENGTH] { raw_zero, raw_full, raw_offset };
                this.eng = new double[ENG_LENGTH] { eng_zero, eng_full };
                this.units = units;
                this.comment = comment;
                value = 0;
            }
        }

        private RegisterMapping_Coils[] RegisterMap_Coils = new RegisterMapping_Coils[] {
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.ContactorAClose, "Close contactor A"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.ContactorAOpen, "Open contactor A"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.ContactorBClose, "Close contactor B"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.ContactorBOpen, "Open contactor B"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.MOPIncrement, "Increment MOP"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.MOPDecrement, "Decrement MOP"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.MOPInitialise, "Initialise MOP"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.SyncFieldEnable, "Enable sync. field"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.SyncFieldDisable, "Disable sync. field"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.SyncCheckEnable, "Enable sync. check"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.SyncCheckDisable, "Disable sync. check"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.DCDriveEnable, "Enable DC drive"),
            new RegisterMapping_Coils((int)ModbusRegs_Coils_BW.DCDriveDisable, "Disable DC drive"),
        };

        private RegisterMapping_Inputs[] RegisterMap_Inputs = new RegisterMapping_Inputs[] {
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.EmergencyStop, "Emergency stop NOT active"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.ContactorAClosed, "Contactor A closed"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.ContactorBClosed, "Contactor B closed"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.DCDriveOk, "DC Drive Ok"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.DCDriveInReverse, "DC Drive in reverse"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.InSynchronism, "In synchronism"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.ProtectionTrip, "Protection trip"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.MOPLowerLimit, "MOP lower limit reached"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.MOPUpperLimit, "MOP upper limit reached"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.TemperatureTrip, "Temperature tripped"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.ProtectionCBTrip, "Protection circuit breaker NOT tripped"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.ContactorACloseCmdStat, "Contactor A Close command status"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.ContactorBCloseCmdStat, "Contactor B Close command status"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.MOPIncrementCmdStat, "MOP Increment command status"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.MOPDecrementCmdStat, "MOP Decrement command status"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.MOPInitialiseCmdStat, "MOP Initialise command status"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.SyncFieldEnableCmdStat, "Sync Field Enable command status"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.SyncCheckEnableCmdStat, "Sync Check Enable command status"),
            new RegisterMapping_Inputs((int)ModbusRegs_Inputs_BO.DCDriveEnableCmdStat, "DC Drive Enable command status"),
        };

        private RegisterMapping_InputRegs[] RegisterMap_InputRegs = new RegisterMapping_InputRegs[] {
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenFieldCurrent, -32768, 32767, 0, -32.768, 32.767, "Amps", "Gen field current"),
        };

        #endregion // Registers

        #region Properties

        private bool doInitialise;
        private int initialiseDelay;
        private string lastError;
        private PowerMeter powerMeter;
        private SyncMonitor syncMonitor;
        private bool connectionOpen;

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

        public PowerMeter PowerMeter
        {
            get { return this.powerMeter; }
        }

        public SyncMonitor SyncMonitor
        {
            get { return this.syncMonitor; }
        }

        public bool ConnectionOpen
        {
            get { return this.connectionOpen; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public PLC(XmlNode xmlNodeEquipmentConfig, DriverMachine.KeepAliveCallback keepAliveCallback)
        {
            const string STRLOG_MethodName = "PLC";

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
                this.lastError = null;
                this.connectionOpen = false;

                //
                // Get the IP address and port number to use
                //
                XmlNode xmlNodePLC = XmlUtilities.GetXmlNode(xmlNodeEquipmentConfig, Consts.STRXML_plc);
                XmlNode xmlNodeNetwork = XmlUtilities.GetXmlNode(xmlNodePLC, Consts.STRXML_network, false);
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
                XmlNode xmlNodeModbus = XmlUtilities.GetXmlNode(xmlNodePLC, Consts.STRXML_modbus, false);
                this.slaveId = XmlUtilities.GetIntValue(xmlNodeModbus, Consts.STRXML_slaveId);
                Logfile.Write(STRLOG_ModbusSlaveId + this.slaveId.ToString());

                //
                // Get the flag to determine if equipment initialisation is required
                //
                this.doInitialise = XmlUtilities.GetBoolValue(xmlNodePLC, Consts.STRXML_doInitialise, false);
                Logfile.Write(STRLOG_DoInitialise + this.doInitialise.ToString());

                //
                // Get the time it takes to initialise
                //
                this.initialiseDelay = XmlUtilities.GetIntValue(xmlNodePLC, Consts.STRXML_initialiseDelay);

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

                this.syncMonitor = new SyncMonitor(this.ipaddr, this.ipport);
                this.syncMonitor.ReceiveTimeout = this.receiveTimeout;
                this.syncMonitor.SlaveId = this.slaveId;
                this.powerMeter = new PowerMeter(this.ipaddr, this.ipport);
                this.powerMeter.ReceiveTimeout = this.receiveTimeout;
                this.powerMeter.SlaveId = this.slaveId;
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
                        this.OpenContactorA() == true &&
                        this.ResetSyncField() == true &&
                        this.OpenContactorB() == true &&
                        this.EnableSyncField(false) == true &&
                        this.EnableDCDrive(false) == true &&
                        this.EnableSyncCheck(false) == true
                        );
#if NOTYET
                        if (success == true)
                        {
                            //
                            // Increase MOP until the upper limit is reached
                            //
                            int count = 0;
                            while (count < 25)
                            {
                                Trace.Write(String.Format("[{0}] ", count));

                                bool ok = false;
                                if ((success = this.IncreaseSyncField(ref ok)) == true)
                                {
                                    if (ok == false)
                                    {
                                        //
                                        // Field current could not be increased any further
                                        //
                                        bool upperlimit = false;
                                        if ((success = this.GetMopUpperLimit(ref upperlimit)) == true)
                                        {
                                            //
                                            // Determine if upper limt has been reached
                                            //
                                            success = (upperlimit == true);
                                        }
                                        break;
                                    }
                                }
                                count++;
                            }
                        }
                        if (success == true)
                        {
                            //
                            // Decrease MOP until the lower limit is reached
                            //
                            int count = 0;
                            while (count < 25)
                            {
                                Trace.Write(String.Format("[{0}] ", count));

                                bool ok = false;
                                if ((success = this.DecreaseSyncField(ref ok)) == true)
                                {
                                    if (ok == false)
                                    {
                                        //
                                        // Field current could not be decreased any further
                                        //
                                        bool lowerlimit = false;
                                        if ((success = this.GetMopLowerLimit(ref lowerlimit)) == true)
                                        {
                                            //
                                            // Determine if lower limt has been reached
                                            //
                                            success = (lowerlimit == true);
                                        }
                                        break;
                                    }
                                }
                                count++;
                            }
                        }
#endif
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
                this.powerMeter.ModbusIpMaster = this.modbusIpMaster;
                this.syncMonitor.ModbusIpMaster = this.modbusIpMaster;
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
#if x
                //
                // The register map contains a single contiguous sets of registers
                //
                bool[] values = null;
                int count = RegisterMap_Coils.Length;
                if (this.ReadCoils((int)ModbusRegs_Coils_BW.ContactorAClose, ref values, count) == true)
                {
                    for (int i = 0; i < RegisterMap_Coils.Length; i++)
                    {
                        RegisterMapping_Coils mapping = RegisterMap_Coils[i];
                        logMessage = String.Format("[{0,5:d}]: Value: {1}  {2}", mapping.register, ((values[i] == true) ? 1 : 0), mapping.comment);
                        Logfile.Write(logMessage);
                        Trace.WriteLine(logMessage);
                    }
                }
                Trace.WriteLine(String.Empty);
#endif

                //
                // Input discretes - Two contiguous sets of registers, read and combine
                //
                bool[] values1 = null;
                bool[] values2 = null;
                int count1 = (int)ModbusRegs_Inputs_BO.ProtectionCBTrip - (int)ModbusRegs_Inputs_BO.EmergencyStop + 1;
                int count2 = (int)ModbusRegs_Inputs_BO.DCDriveEnableCmdStat - (int)ModbusRegs_Inputs_BO.ContactorACloseCmdStat + 1;
                if (this.ReadInputs((int)ModbusRegs_Inputs_BO.EmergencyStop, ref values1, count1) == true &&
                    this.ReadInputs((int)ModbusRegs_Inputs_BO.ContactorACloseCmdStat, ref values2, count2) == true)
                {
                    //
                    // Combine the arrays into a single array
                    //
                    bool[] values = new bool[values1.Length + values2.Length];
                    values1.CopyTo(values, 0);
                    values2.CopyTo(values, values1.Length);
                    for (int i = 0; i < values.Length; i++)
                    {
                        RegisterMapping_Inputs mapping = RegisterMap_Inputs[i];
                        logMessage = String.Format("[{0,5:d}]: Value: {1}  {2}", mapping.register, ((values[i] == true) ? 1 : 0), mapping.comment);
                        Logfile.Write(logMessage);
                        Trace.WriteLine(logMessage);
                    }
                }
                Trace.WriteLine(String.Empty);

                //
                // Input registers - Lots of gaps so have to read one register at a time
                //
                for (int i = 0; i < RegisterMap_InputRegs.Length; i++)
                {
                    RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[i];
                    logMessage = String.Format("[{0,-5:d}]: ", mapping.register);

                    //
                    // Read the register
                    //
                    ushort value = 0;
                    if (this.ReadInputRegister(mapping.register, ref value) == true)
                    {
                        //
                        // Convert the register value to eng. units
                        //
                        double engValue = this.ConvertRawToEng(value, mapping.raw, mapping.eng);
                        logMessage += String.Format("Value: {0,-6}  Eng: {1,6:f03} {2,-5} {3}", value, engValue, mapping.units, mapping.comment);
                    }
                    Logfile.Write(logMessage);
                    Trace.WriteLine(logMessage);
                }
                Trace.WriteLine(String.Empty);

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                Trace.WriteLine(ex.Message);
                this.lastError = ex.Message;
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
                success = this.ReadInputRegister((int)ModbusRegs_InputRegs_RO.GenFieldCurrent, ref value);
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ReadPowermeterRegisters()
        {
            return this.powerMeter.ReadRegisters();
        }

        //-------------------------------------------------------------------------------------------------//

        public bool EnableSyncField(bool enable)
        {
            const string STRLOG_MethodName = "EnableSyncField";

            string logMessage = STRLOG_Enable + enable.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            ModbusRegs_Inputs_BO regCmdStat = ModbusRegs_Inputs_BO.SyncFieldEnableCmdStat;
            ModbusRegs_Coils_BW regCoils = (enable == true) ? ModbusRegs_Coils_BW.SyncFieldEnable : ModbusRegs_Coils_BW.SyncFieldDisable;

            //
            // Check if the sync field needs to be enabled/disabled
            //
            bool isActive = false;
            if ((success = this.ReadInput((int)regCmdStat, ref isActive)) == true)
            {
                Trace.Write(String.Format(" {0}({1}): ", STRLOG_MethodName, enable.ToString()));

                if (isActive != enable)
                {
                    //
                    // Write the command
                    //
                    if ((success = this.WriteCoil(regCoils, true)) == true)
                    {
                        bool timedOut = false;

                        //
                        // Wait for the command to start executing
                        //
                        if ((success = this.WaitUntil(enable, regCmdStat, 2, ref timedOut)) == true)
                        {
                            success = (timedOut == false);
                            if (success == true)
                            {
                                //
                                // Wait a moment for the field energy to settle
                                //
                                this.WaitDelay(3);
                            }
                            else
                            {
                                // Command failed to start
                                Logfile.WriteError(STRERR_CommandFailedToStart);
                            }
                        }
                    }

                    Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);
                }
                else
                {
                    Trace.WriteLine(STRLOG_NoChange);
                }
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool EnableSyncCheck(bool enable)
        {
            const string STRLOG_MethodName = "EnableSyncCheck";

            string logMessage = STRLOG_Enable + enable.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            ModbusRegs_Inputs_BO regCmdStat = ModbusRegs_Inputs_BO.SyncCheckEnableCmdStat;
            ModbusRegs_Coils_BW regCoils = (enable == true) ? ModbusRegs_Coils_BW.SyncCheckEnable : ModbusRegs_Coils_BW.SyncCheckDisable;

            //
            // Check if the sync check needs to be enabled/disabled
            //
            bool isActive = false;
            if ((success = this.ReadInput((int)regCmdStat, ref isActive)) == true)
            {
                Trace.Write(String.Format(" {0}({1}): ", STRLOG_MethodName, enable.ToString()));

                if (isActive != enable)
                {
                    //
                    // Write the command
                    //
                    if ((success = this.WriteCoil(regCoils, true)) == true)
                    {
                        bool timedOut = false;

                        //
                        // Wait for the command to start executing
                        //
                        if ((success = this.WaitUntil(enable, regCmdStat, 2, ref timedOut)) == true)
                        {
                            success = (timedOut == false);
                            if (timedOut == true)
                            {
                                // Command failed to start
                                Logfile.WriteError(STRERR_CommandFailedToStart);
                            }
                        }
                    }

                    Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);
                }
                else
                {
                    Trace.WriteLine(STRLOG_NoChange);
                }
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool EnableDCDrive(bool enable)
        {
            const string STRLOG_MethodName = "EnableDCDrive";

            string logMessage = STRLOG_Enable + enable.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            ModbusRegs_Inputs_BO regCmdStat = ModbusRegs_Inputs_BO.DCDriveEnableCmdStat;
            ModbusRegs_Coils_BW regCoils = (enable == true) ? ModbusRegs_Coils_BW.DCDriveEnable : ModbusRegs_Coils_BW.DCDriveDisable;

            //
            // Check if the DC drive needs to be enabled/disabled
            //
            bool isActive = false;
            if ((success = this.ReadInput((int)regCmdStat, ref isActive)) == true)
            {
                Trace.Write(String.Format(" {0}({1}): ", STRLOG_MethodName, enable.ToString()));

                if (isActive != enable)
                {
                    //
                    // Write the command
                    //
                    if ((success = this.WriteCoil(regCoils, true)) == true)
                    {
                        bool timedOut = false;

                        //
                        // Wait for the command to start executing
                        //
                        if ((success = this.WaitUntil(enable, regCmdStat, 2, ref timedOut)) == true)
                        {
                            success = (timedOut == false);
                            if (timedOut == true)
                            {
                                // Command failed to start
                                Logfile.WriteError(STRERR_CommandFailedToStart);
                            }
                        }
                    }

                    Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);
                }
                else
                {
                    Trace.WriteLine(STRLOG_NoChange);
                }
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ResetSyncField()
        {
            const string STRLOG_MethodName = "ResetSyncField";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            //
            // Check if the MOP lower limit is active
            //
            bool isActive= false;
            if ((success = this.ReadInput((int)ModbusRegs_Inputs_BO.MOPLowerLimit, ref isActive)) == true)
            {
                if (isActive == false)
                {
                    ModbusRegs_Coils_BW regCoils = ModbusRegs_Coils_BW.MOPInitialise;
                    ModbusRegs_Inputs_BO regCmdStat = ModbusRegs_Inputs_BO.MOPInitialiseCmdStat;

                    Trace.Write(String.Format(" {0}: ", STRLOG_MethodName));

                    //
                    // Write the command
                    //
                    if ((success = this.WriteCoil(regCoils, true)) == true)
                    {
                        bool timedOut = false;

                        //
                        // Wait for the command to start executing
                        //
                        if ((success = this.WaitUntil(true, regCmdStat, 1, ref timedOut)) == true)
                        {
                            success = (timedOut == false);
                            if (timedOut == true)
                            {
                                // Command failed to start
                                Logfile.WriteError(STRERR_CommandFailedToStart);
                            }
                            else
                            {
                                //
                                // Wait for the command to finish executing
                                //
                                if ((success = this.WaitUntil(false, regCmdStat, 5, ref timedOut)) == true)
                                {
                                    success = (timedOut == false);
                                    if (timedOut == true)
                                    {
                                        // Command failed to complete
                                        Logfile.WriteError(STRERR_CommandFailedToComplete);
                                    }
                                }
                            }
                        }
                    }
                    Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);
                }
                else
                {
                    //
                    // Contactor is already open
                    //
                }
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool IncreaseSyncField(ref bool ok)
        {
            const string STRLOG_MethodName = "IncreaseSyncField";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            ok = false;

            ModbusRegs_Coils_BW regCoils = ModbusRegs_Coils_BW.MOPIncrement;
            ModbusRegs_Inputs_BO regCmdStat = ModbusRegs_Inputs_BO.MOPIncrementCmdStat;

            Trace.Write(String.Format(" {0}: ", STRLOG_MethodName));

            //
            // Write the command
            //
            if ((success = this.WriteCoil(regCoils, true)) == true)
            {
                bool timedOut = false;

                //
                // Wait for the command to start executing
                //
                if ((success = this.WaitUntil(true, regCmdStat, 1, ref timedOut)) == true)
                {
                    if (timedOut == false)
                    {
                        //
                        // Wait for the command to finish executing
                        //
                        if ((success = this.WaitUntil(false, regCmdStat, 5, ref timedOut)) == true)
                        {
                            if (timedOut == false)
                            {
                                //
                                // Command completed without any timeouts
                                //
                                ok = true;
                            }
                        }
                    }
                }
            }

            Trace.WriteLine((success == true && ok == true) ? STRLOG_IsOk : STRLOG_HasFailed);

            string logMessage = STRLOG_Success + success.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_Ok + ok.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool DecreaseSyncField(ref bool ok)
        {
            const string STRLOG_MethodName = "DecreaseSyncField";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            ok = false;

            ModbusRegs_Coils_BW regCoils = ModbusRegs_Coils_BW.MOPDecrement;
            ModbusRegs_Inputs_BO regCmdStat = ModbusRegs_Inputs_BO.MOPDecrementCmdStat;

            Trace.Write(String.Format(" {0}: ", STRLOG_MethodName));

            //
            // Write the command
            //
            if ((success = this.WriteCoil(regCoils, true)) == true)
            {
                bool timedOut = false;

                //
                // Wait for the command to start executing
                //
                if ((success = this.WaitUntil(true, regCmdStat, 1, ref timedOut)) == true)
                {
                    if (timedOut == false)
                    {
                        //
                        // Wait for the command to finish executing
                        //
                        if ((success = this.WaitUntil(false, regCmdStat, 5, ref timedOut)) == true)
                        {
                            if (timedOut == false)
                            {
                                //
                                // Command completed without any timeouts
                                //
                                ok = true;
                            }
                        }
                    }
                }
            }

            Trace.WriteLine((success == true && ok == true) ? STRLOG_IsOk : STRLOG_HasFailed);

            string logMessage = STRLOG_Success + success.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_Ok + ok.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool CloseContactorA(ref bool ok)
        {
            const string STRLOG_MethodName = "CloseContactorA";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            ok = false;

            ModbusRegs_Coils_BW regCoils = ModbusRegs_Coils_BW.ContactorAClose;
            ModbusRegs_Inputs_BO regCmdStat = ModbusRegs_Inputs_BO.ContactorACloseCmdStat;
            ModbusRegs_Inputs_BO regStatus = ModbusRegs_Inputs_BO.ContactorAClosed;

            Trace.Write(String.Format(" {0}: ", STRLOG_MethodName));

            //
            // Write the command
            //
            if ((success = this.WriteCoil(regCoils, true)) == true)
            {
                bool timedOut = false;

                //
                // Wait for the command to start executing
                //
                if ((success = this.WaitUntil(true, regCmdStat, 2, ref timedOut)) == true)
                {
                    if (timedOut == false)
                    {
                        //
                        // Check that contactor A is closed
                        //
                        if ((success = this.WaitUntil(true, regStatus, 1, ref timedOut)) == true)
                        {
                            if (timedOut == false)
                            {
                                //
                                // Command completed without any timeouts
                                //
                                ok = true;

                                //
                                // Wait a moment for the power to settle
                                //
                                this.WaitDelay(3);
                            }
                        }
                    }
                }
            }

            Trace.WriteLine((success == true && ok == true) ? STRLOG_IsOk : STRLOG_HasFailed);

            string logMessage = STRLOG_Success + success.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_Ok + ok.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool OpenContactorA()
        {
            const string STRLOG_MethodName = "OpenContactorA";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            //
            // Check if the contactor is closed
            //
            bool isClosed = false;
            if ((success = this.ReadInput((int)ModbusRegs_Inputs_BO.ContactorAClosed, ref isClosed)) == true)
            {
                if (isClosed == true)
                {
                    //
                    // Contactor is closed, need to open it
                    //
                    ModbusRegs_Coils_BW regCoils = ModbusRegs_Coils_BW.ContactorAOpen;
                    ModbusRegs_Inputs_BO regStatus = ModbusRegs_Inputs_BO.ContactorAClosed;

                    Trace.Write(String.Format(" {0}: ", STRLOG_MethodName));

                    //
                    // Write the command
                    //
                    if ((success = this.WriteCoil(regCoils, true)) == true)
                    {
                        bool timedOut = false;

                        //
                        // Check that contactor A is not closed
                        //
                        if ((success = this.WaitUntil(false, regStatus, 1, ref timedOut)) == true)
                        {
                            success = (timedOut == false);
                            if (timedOut == true)
                            {
                                // Command failed to complete
                                Logfile.WriteError(STRERR_CommandFailedToComplete);
                            }
                        }
                    }
                    Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);
                }
                else
                {
                    //
                    // Contactor is already open
                    //
                }
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool CloseContactorB()
        {
            const string STRLOG_MethodName = "CloseContactorB";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            ModbusRegs_Coils_BW regCoils = ModbusRegs_Coils_BW.ContactorBClose;
            ModbusRegs_Inputs_BO regCmdStat = ModbusRegs_Inputs_BO.ContactorBCloseCmdStat;
            ModbusRegs_Inputs_BO regStatus = ModbusRegs_Inputs_BO.ContactorBClosed;

            Trace.Write(String.Format(" {0}: ", STRLOG_MethodName));

            //
            // Write the command
            //
            if ((success = this.WriteCoil(regCoils, true)) == true)
            {
                bool timedOut = false;

                //
                // Wait for the command to start executing
                //
                if ((success = this.WaitUntil(true, regCmdStat, 5, ref timedOut)) == true)
                {
                    success = (timedOut == false);
                    if (timedOut == true)
                    {
                        // Command failed to start
                        Logfile.WriteError(STRERR_CommandFailedToStart);
                    }
                    else
                    {
                        //
                        // Check that contactor B is closed
                        //
                        if ((success = this.WaitUntil(true, regStatus, 1, ref timedOut)) == true)
                        {
                            success = (timedOut == false);
                            if (timedOut == true)
                            {
                                // Command failed to complete
                                Logfile.WriteError(STRERR_CommandFailedToComplete);
                            }
                        }
                    }
                }
            }

            Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool OpenContactorB()
        {
            const string STRLOG_MethodName = "OpenContactorB";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            //
            // Check if the contactor is closed
            //
            bool isClosed = false;
            if ((success = this.ReadInput((int)ModbusRegs_Inputs_BO.ContactorBClosed, ref isClosed)) == true)
            {
                if (isClosed == true)
                {
                    //
                    // Contactor is closed, need to open it
                    //
                    ModbusRegs_Coils_BW regCoils = ModbusRegs_Coils_BW.ContactorBOpen;
                    ModbusRegs_Inputs_BO regStatus = ModbusRegs_Inputs_BO.ContactorBClosed;

                    Trace.Write(String.Format(" {0}: ", STRLOG_MethodName));

                    //
                    // Write the command
                    //
                    if ((success = this.WriteCoil(regCoils, true)) == true)
                    {
                        bool timedOut = false;

                        //
                        // Check that contactor B is not closed
                        //
                        if ((success = this.WaitUntil(false, regStatus, 1, ref timedOut)) == true)
                        {
                            success = (timedOut == false);
                            if (timedOut == true)
                            {
                                // Command failed to complete
                                Logfile.WriteError(STRERR_CommandFailedToComplete);
                            }
                        }
                    }
                    Trace.WriteLine((success == true) ? STRLOG_IsOk : STRLOG_HasFailed);
                }
                else
                {
                    //
                    // Contactor is already open
                    //
                }
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetEmergencyStoppedStatus(ref bool value)
        {
            bool success = this.ReadInput((int)ModbusRegs_Inputs_BO.EmergencyStop, ref value);

            // Convert to positive logic
            value = !value;

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetProtectionCBTrippedStatus(ref bool value)
        {
            bool success = this.ReadInput((int)ModbusRegs_Inputs_BO.ProtectionCBTrip, ref value);

            // Convert to positive logic
            value = !value;

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetMopLowerLimitStatus(ref bool value)
        {
            return this.ReadInput((int)ModbusRegs_Inputs_BO.MOPLowerLimit, ref value);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetMopUpperLimitStatus(ref bool value)
        {
            return this.ReadInput((int)ModbusRegs_Inputs_BO.MOPUpperLimit, ref value);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetDCDriveStatus(ref bool value)
        {
            return this.ReadInput((int)ModbusRegs_Inputs_BO.DCDriveOk, ref value);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetSynchronismStatus(ref bool value)
        {
            return this.ReadInput((int)ModbusRegs_Inputs_BO.InSynchronism, ref value);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetSyncFieldCurrent(ref double current, ref string units)
        {
            bool success;

            //
            // Find the register mapping
            //
            int index = this.FindRegisterMapping(ModbusRegs_InputRegs_RO.GenFieldCurrent);
            RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[index];

            //
            // Read the register
            //
            ushort value = 0;
            if ((success = this.ReadInputRegister(mapping.register, ref value)) == true)
            {
                // Convert the register value to eng. units
                current = this.ConvertRawToEng(value, mapping.raw, mapping.eng);
                units = mapping.units;
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetSyncVoltage(ref double voltage, ref string units)
        {
            return this.powerMeter.GetPhaseToPhaseVoltage(ref voltage, ref units);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetSyncFrequency(ref double frequency, ref string units)
        {
            return this.powerMeter.GetFrequency(ref frequency, ref units);
        }

        //=================================================================================================//

        /// <summary>
        /// Read the register until it contains the specified value or until the timeout (in seconds) occurs.
        /// While this is happening, this.KeepAlive() must be called each second.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="regInput"></param>
        /// <param name="timeout"></param>
        /// <param name="timedOut"></param>
        /// <returns></returns>
        private bool WaitUntil(bool value, ModbusRegs_Inputs_BO regInput, int timeout, ref bool timedOut)
        {
            bool success = false;

            //
            // Check the register every half second
            //
            timeout *= 2;
            timedOut = true;
            for (int i = 0; i < timeout; i++)
            {
                //
                // Read the register
                //
                bool regValue = false;
                if ((success = this.ReadInput((int)regInput, ref regValue)) == false)
                {
                    // Read failed - Exit timeout loop
                    timedOut = false;
                    break;
                }

                //
                // Compare the register value with what is required
                //
                if (regValue == value)
                {
                    // Have desired value - Exit timeout loop
                    timedOut = false;
                    break;
                }

                //
                // Wait for half a second
                //
                Trace.Write((regValue == true) ? "1" : "0");
                Thread.Sleep(500);

                //
                // Call the keep-alive each second
                //
                if (i % 2 == 1)
                {
                    if (this.keepAliveCallback != null)
                    {
                        this.keepAliveCallback();
                    }
                }
            }

            //
            // Check if timeout
            //
            if (timedOut == true)
            {
                Trace.Write("T");
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool WriteCoil(ModbusRegs_Coils_BW register, bool value)
        {
            bool success = false;
            this.lastError = null;

            try
            {
                //
                // Convert register to Modbus protocol address
                //
                ushort protAddress = (ushort)((int)register - MODBUSREGS_COILS_BASEADDRESS);

                //
                // Write value
                //
                this.modbusIpMaster.WriteSingleCoil((byte)this.slaveId, protAddress, value);

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

        private bool ReadInput(int register, ref bool value)
        {
            bool[] values = null;

            bool success = this.ReadInputs(register, ref values, 1);
            value = values[0];

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool ReadInputs(int register, ref bool[] values, int count)
        {
            bool success = false;
            this.lastError = null;

            try
            {
                //
                // Convert register to Modbus protocol address
                //
                ushort protAddress = (ushort)(register - MODBUSREGS_INPUTS_BASEADDRESS);

                //
                // Read input
                //
                values = this.modbusIpMaster.ReadInputs((byte)this.slaveId, protAddress, (ushort)count);

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

        private bool ReadInputRegister(int register, ref ushort value)
        {
            ushort[] values = null;

            bool success = this.ReadInputRegisters(register, ref values, 1);
            value = values[0];

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool ReadInputRegisters(int register, ref ushort[] value, int count)
        {
            bool success = false;
            this.lastError = null;

            try
            {
                //
                // Convert register to Modbus protocol address
                //
                ushort protAddress = (ushort)(register - MODBUSREGS_INPUTREGS_BASEADDRESS);

                //
                // Read multiple inputs
                //
                value = this.modbusIpMaster.ReadInputRegisters((byte)this.slaveId, protAddress, (ushort)count);

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
            if (eng[RegisterMapping_InputRegs.ENG_FULL] - eng[RegisterMapping_InputRegs.ENG_ZERO] > 0)
            {
                engValue = (engValue + raw[RegisterMapping_InputRegs.RAW_OFFSET])
                    * (eng[RegisterMapping_InputRegs.ENG_FULL] - eng[RegisterMapping_InputRegs.ENG_ZERO])
                    / (raw[RegisterMapping_InputRegs.RAW_FULL] - raw[RegisterMapping_InputRegs.RAW_ZERO]);
            }

            return engValue;
        }

        //-------------------------------------------------------------------------------------------------//

        private ushort ConvertEngToRaw(double engValue, double[] eng, short[] raw)
        {
            double rawValue = engValue;

            //
            // Unscale the value and subtract the offset
            //
            if (eng[RegisterMapping_InputRegs.ENG_FULL] - eng[RegisterMapping_InputRegs.ENG_ZERO] > 0)
            {
                rawValue = rawValue
                    * (raw[RegisterMapping_InputRegs.RAW_FULL] - raw[RegisterMapping_InputRegs.RAW_ZERO])
                    / (eng[RegisterMapping_InputRegs.ENG_FULL] - eng[RegisterMapping_InputRegs.ENG_ZERO])
                    - raw[RegisterMapping_InputRegs.RAW_OFFSET];
            }

            //
            // Convert from signed double to unsigned short
            //
            return (ushort)((short)rawValue);
        }

        //-------------------------------------------------------------------------------------------------//

        private int FindRegisterMapping(ModbusRegs_InputRegs_RO register)
        {
            int index = -1;

            for (int i = 0; i < RegisterMap_InputRegs.Length; i++)
            {
                if ((int)register == RegisterMap_InputRegs[i].register)
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
