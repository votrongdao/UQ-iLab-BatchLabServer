using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using Library.Lab;
using Modbus.Device;

namespace Library.LabEquipment.Drivers
{
    public class RedLion
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "RedLion";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_MachineIP = " MachineIP: ";
        private const string STRLOG_MachinePort = " MachinePort: ";
        private const string STRLOG_InitialiseEquipment = " InitialiseEquipment: ";
        private const string STRLOG_MeasurementDelay = " MeasurementDelay: ";
        private const string STRLOG_NotInitialised = "Not Initialised!";
        private const string STRLOG_Initialising = "Initialising...";
        private const string STRLOG_Online = " Online: ";
        private const string STRLOG_Result = " Result: ";
        private const string STRLOG_Success = " Success: ";
        private const string STRLOG_FaultCode = " FaultCode: ";

        private const string STRLOG_ACDriveConfig = "ACDriveConfig: ";
        private const string STRLOG_DCDriveMutConfig = "DCDriveMutConfig: ";
        private const string STRLOG_DCDriveMutMode = "DCDriveMutMode: ";
        private const string STRLOG_Speed = " Speed: ";
        private const string STRLOG_Voltage = " Voltage: ";
        private const string STRLOG_Torque = " Torque: ";
        private const string STRLOG_Field = " Field: ";
        private const string STRLOG_FieldCurrent = " FieldCurrent: ";
        private const string STRLOG_Rpm = " RPM";
        private const string STRLOG_Volts = " Volts";
        private const string STRLOG_Percent = " %";
        private const string STRLOG_Amps = " Amps";

        //
        // String constants for error messages
        //
        private const string STRERR_MachineIPNotSpecified = "Machine IP not specified!";
        private const string STRERR_NumberIsNegative = "Number is negative!";
        private const string STRERR_NumberIsInvalid = "Number is invalid!";
        private const string STRERR_FailedToInitialise = "Failed to initialise!";
        private const string STRERR_FailedToResetACDrive = "Failed to reset AC drive!";
        private const string STRERR_ActiveFaultDetectedACDrive = "Active fault detected on AC drive!";

        //
        // Local variables
        //
        private Logfile.LoggingLevels logLevel;
        private bool initialised;
        private string lastError;
        private int machinePort;
        private TcpClient tcpClient;
        private ModbusIpMaster master;
        private ACDrive acDrive;
        private DCDriveMut dcDriveMut;

        #endregion

        #region Properties

        private bool online;
        private string statusMessage;
        private bool initialiseEquipment;
        private int measurementDelay;
        private string machineIP;

        /// <summary>
        /// Returns the time (in seconds) that it takes for the equipment to initialise.
        /// </summary>
        public int InitialiseDelay
        {
            get { return (this.initialised == false) ? this.GetInitialiseTime() : 0; }
        }

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

        public bool InitialiseEquipment
        {
            get { return this.initialiseEquipment; }
            set { this.initialiseEquipment = value; }
        }

        public int MeasurementDelay
        {
            get { return this.measurementDelay; }
            set { this.measurementDelay = value; }
        }

        public string MachineIP
        {
            get { return this.machineIP; }
        }

        #endregion

        #region Types

        //
        // AC Drive configurations
        //
        public enum ACDriveConfigs
        {
            Default, LowerCurrent, MaximumCurrent, MinimumTorque
        }

        //
        // DC Drive configurations
        //
        public enum DCDriveMutConfigs
        {
            Default, MinimumTorque
        }

        //
        // Start DC Drive Mut modes
        //
        public enum DCDriveMutModes
        {
            EnableOnly, Speed, Torque
        }

        //
        // Measurements to take
        //
        public struct Measurements
        {
            public int speed;          // RPM
            public int voltage;        // Volts
            public float fieldCurrent; // Amps
            public int load;           // Percent
        }

        //
        // Information about the AC Drive
        //
        public struct ACDriveInfo
        {
            public int minSpeed;
            public int maxSpeed;
        }

        //
        // Information about the DC Drive MUT
        //
        public struct DCDriveMutInfo
        {
            public int minSpeed;
            public int maxSpeed;
            public int defaultField;
            public int defaultTorque;
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public RedLion(XmlNode xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "RedLion";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.initialised = false;
            this.lastError = null;

            //
            // Initialise properties
            //
            this.online = false;
            this.statusMessage = STRLOG_NotInitialised;

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

            //
            // Get the IP address of the RedLion unit
            //
            this.machineIP = XmlUtilities.GetXmlValue(xmlNodeEquipmentConfig, Consts.STRXML_machineIP, false);
            IPAddress machineIP = IPAddress.Parse(this.machineIP);
            Logfile.Write(STRLOG_MachineIP + this.machineIP.ToString());

            //
            // Get the port number to use with the RedLion unit
            //
            this.machinePort = XmlUtilities.GetIntValue(xmlNodeEquipmentConfig, Consts.STRXML_machinePort);
            if (this.machinePort < 0)
            {
                throw new Exception(STRERR_NumberIsNegative);
            }
            Logfile.Write(STRLOG_MachinePort + this.machinePort.ToString());

            //
            // Get the intialise equipment flag
            //
            this.initialiseEquipment = XmlUtilities.GetBoolValue(xmlNodeEquipmentConfig, Consts.STRXML_initialiseEquipment, false);
            Logfile.Write(STRLOG_InitialiseEquipment + this.initialiseEquipment.ToString());

            //
            // Get the measurement delay
            //
            this.measurementDelay = XmlUtilities.GetIntValue(xmlNodeEquipmentConfig, Consts.STRXML_measurementDelay);
            Logfile.Write(STRLOG_MeasurementDelay + this.measurementDelay.ToString());

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public string GetLastError()
        {
            string lastError = this.lastError;
            this.lastError = null;
            return lastError;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Initialise()
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            if (this.initialised == false)
            {
                this.statusMessage = STRLOG_Initialising;

                try
                {
                    if (this.initialiseEquipment == true)
                    {
                        //
                        // Create a connection to the RedLion controller
                        //
                        if (this.CreateConnection() == false)
                        {
                            throw new Exception(this.GetLastError());
                        }

                        try
                        {
                            //
                            // Reset the AC drive
                            //
                            if (this.ResetACDrive() == false)
                            {
                                throw new Exception(this.GetLastError());
                            }

                            //
                            // Reset the DC drive
                            //
                            if (this.ResetDCDriveMut() == false)
                            {
                                throw new Exception(this.GetLastError());
                            }

                            //
                            // Configure the AC drive with default values
                            //
                            this.ConfigureACDrive(
                                ACDrive.DEFAULT_SpeedRampTime, ACDrive.DEFAULT_MaximumCurrent,
                                ACDrive.DEFAULT_MinimumTorque, ACDrive.DEFAULT_MaximumTorque);

                            //
                            // Configure the DC drive with default values
                            //
                            this.ConfigureDCDriveMut(
                                DCDriveMut.DEFAULT_MinSpeedLimit, DCDriveMut.DEFAULT_MaxSpeedLimit,
                                DCDriveMut.DEFAULT_MinTorqueLimit, DCDriveMut.DEFAULT_MaxTorqueLimit,
                                DCDriveMut.DEFAULT_SpeedRampTime);

                            //
                            // Disable drive power
                            //
                            acDrive.DisableDrivePower();
                        }
                        finally
                        {
                            this.CloseConnection();
                        }
                    }

                    //
                    // Initialisation is complete
                    //
                    this.initialised = true;
                    this.online = true;
                    this.statusMessage = StatusCodes.Ready.ToString();
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                    this.statusMessage = STRERR_FailedToInitialise;
                    this.lastError = ex.Message;
                }
            }

            string logMessage = STRLOG_Online + this.online.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return this.online;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetResetACDriveTime()
        {
            int executionTime = 0;

            executionTime += ACDrive.DELAY_EnableDrivePower;
            executionTime += ACDrive.DELAY_ResetDrive;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetConfigureACDriveTime()
        {
            int executionTime = 0;

            executionTime += ACDrive.DELAY_ConfigureSpeed;
            executionTime += ACDrive.DELAY_ConfigureTorque;
            executionTime += ACDrive.DELAY_ConfigureSpeedRampTime;
            executionTime += ACDrive.DELAY_ConfigureMaximumCurrent;
            executionTime += ACDrive.DELAY_ConfigureMaximumTorque;
            executionTime += ACDrive.DELAY_ConfigureMinimumTorque;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStartACDriveTime()
        {
            int executionTime = 0;

            executionTime += ACDrive.DELAY_StartDrive;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStopACDriveTime()
        {
            int executionTime = 0;

            executionTime += ACDrive.DELAY_StopDrive;
            executionTime += this.GetConfigureACDriveTime();
            executionTime += ACDrive.DELAY_DisableDrivePower;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetResetDCDriveMutTime()
        {
            int executionTime = 0;

            executionTime += DCDriveMut.DELAY_ResetDriveFault;
            executionTime += DCDriveMut.DELAY_ResetDrive;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetConfigureDCDriveMutTime()
        {
            int executionTime = 0;

            executionTime += DCDriveMut.DELAY_ConfigureSpeed;
            executionTime += DCDriveMut.DELAY_ConfigureTorque;
            executionTime += DCDriveMut.DELAY_ConfigureField;
            executionTime += DCDriveMut.DELAY_ConfigureMinSpeedLimit;
            executionTime += DCDriveMut.DELAY_ConfigureMaxSpeedLimit;
            executionTime += DCDriveMut.DELAY_ConfigureMinTorqueLimit;
            executionTime += DCDriveMut.DELAY_ConfigureMaxTorqueLimit;
            executionTime += DCDriveMut.DELAY_ConfigureSpeedRampTime;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStartDCDriveMutTime(DCDriveMutModes dcDriveMutMode)
        {
            int executionTime = 0;

            executionTime += DCDriveMut.DELAY_SetMainContactorOn;

            switch (dcDriveMutMode)
            {
                case DCDriveMutModes.EnableOnly:
                    // Don't start the drive
                    break;

                case DCDriveMutModes.Speed:
                    executionTime += DCDriveMut.DELAY_StartDrive;
                    break;

                case DCDriveMutModes.Torque:
                    executionTime += DCDriveMut.DELAY_StartDriveTorque;
                    break;
            }

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStopDCDriveMutTime()
        {
            int executionTime = 0;

            executionTime += DCDriveMut.DELAY_ResetDrive;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetSetSpeedACDriveTime()
        {
            int executionTime = 0;

            executionTime += ACDrive.DELAY_SetSpeed;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetSetSpeedDCDriveMutTime()
        {
            int executionTime = 0;

            executionTime += DCDriveMut.DELAY_SetSpeed;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetSetTorqueDCDriveMutTime()
        {
            int executionTime = 0;

            executionTime += DCDriveMut.DELAY_SetTorque;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetSetFieldDCDriveMutTime()
        {
            int executionTime = 0;

            executionTime += DCDriveMut.DELAY_SetField;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetTakeMeasurementTime()
        {
            int executionTime = 0;

            executionTime += this.measurementDelay;

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public ACDriveInfo GetACDriveInfo()
        {
            ACDriveInfo aCDriveInfo = new ACDriveInfo();
            aCDriveInfo.minSpeed = ACDrive.MINIMUM_Speed;
            aCDriveInfo.maxSpeed = ACDrive.MAXIMUM_Speed;

            return aCDriveInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        public DCDriveMutInfo GetDCDriveMutInfo()
        {
            DCDriveMutInfo dCDriveMutInfo = new DCDriveMutInfo();
            dCDriveMutInfo.minSpeed = DCDriveMut.MINIMUM_Speed;
            dCDriveMutInfo.maxSpeed = DCDriveMut.MAXIMUM_Speed;
            dCDriveMutInfo.defaultField = DCDriveMut.DEFAULT_Field;
            dCDriveMutInfo.defaultTorque = DCDriveMut.DEFAULT_Torque;

            return dCDriveMutInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool CreateConnection()
        {
            const string STRLOG_MethodName = "CreateConnection";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            try
            {
                this.tcpClient = new TcpClient(this.machineIP, this.machinePort);
                this.master = ModbusIpMaster.CreateTcp(this.tcpClient);
                this.acDrive = new ACDrive(this.master);
                this.dcDriveMut = new DCDriveMut(this.master);

                success = true;
                this.lastError = null;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

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

                    success = true;
                    this.lastError = null;
                }
                catch (Exception ex)
                {
                    this.lastError = ex.Message;
                    Logfile.WriteError(ex.Message);
                }
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ResetACDrive()
        {
            const string STRLOG_MethodName = "ResetACDrive";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Enable drive power and reset AC drive
                //
                acDrive.EnableDrivePower();
                int faultCode = acDrive.ReadActiveFault();
                acDrive.ResetDrive();
                faultCode = acDrive.ReadActiveFault();
                if (faultCode != 0)
                {
                    throw new Exception(STRERR_FailedToResetACDrive + STRLOG_FaultCode + faultCode.ToString());
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ConfigureACDrive(ACDriveConfigs acDriveConfig)
        {
            const string STRLOG_MethodName = "ConfigureACDrive";

            string logMessage = STRLOG_ACDriveConfig + acDriveConfig.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Configure the AC drive
                //
                switch (acDriveConfig)
                {
                    case ACDriveConfigs.Default:

                        this.ConfigureACDrive(
                            ACDrive.DEFAULT_SpeedRampTime, ACDrive.DEFAULT_MaximumCurrent,
                            ACDrive.DEFAULT_MinimumTorque, ACDrive.DEFAULT_MaximumTorque);
                        break;

                    case ACDriveConfigs.LowerCurrent:

                        this.ConfigureACDrive(
                            ACDrive.DEFAULT_SpeedRampTime, ACDrive.LOWER_MaximumCurrent,
                            ACDrive.DEFAULT_MinimumTorque, ACDrive.DEFAULT_MaximumTorque);
                        break;

                    case ACDriveConfigs.MaximumCurrent:

                        this.ConfigureACDrive(
                            ACDrive.DEFAULT_SpeedRampTime, ACDrive.MAXIMUM_MaximumCurrent,
                            ACDrive.DEFAULT_MinimumTorque, ACDrive.DEFAULT_MaximumTorque);
                        break;

                    case ACDriveConfigs.MinimumTorque:

                        this.ConfigureACDrive(
                            ACDrive.DEFAULT_SpeedRampTime, ACDrive.DEFAULT_MaximumCurrent,
                            ACDrive.MINIMUM_MinimumTorque, ACDrive.MINIMUM_MaximumTorque);
                        break;
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StartACDrive()
        {
            const string STRLOG_MethodName = "StartACDrive";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Start AC drive
                //
                acDrive.StartDrive();

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StopACDrive()
        {
            const string STRLOG_MethodName = "StopACDrive";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Stop AC drive and disable drive power
                //
                acDrive.StopDrive();
                acDrive.DisableDrivePower();

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ResetDCDriveMut()
        {
            const string STRLOG_MethodName = "ResetDCDriveMut";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Reset DC drive
                //
                dcDriveMut.ResetDriveFault();
                dcDriveMut.ResetDrive();

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ConfigureDCDriveMut(DCDriveMutConfigs dcDriveMutConfig)
        {
            const string STRLOG_MethodName = "ConfigureDCDriveMut";

            string logMessage = STRLOG_DCDriveMutConfig + dcDriveMutConfig.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Configure the DC drive
                //
                switch (dcDriveMutConfig)
                {
                    case DCDriveMutConfigs.Default:

                        this.ConfigureDCDriveMut(
                            DCDriveMut.DEFAULT_MinSpeedLimit, DCDriveMut.DEFAULT_MaxSpeedLimit,
                            DCDriveMut.DEFAULT_MinTorqueLimit, DCDriveMut.DEFAULT_MaxTorqueLimit,
                            DCDriveMut.DEFAULT_SpeedRampTime);
                        break;

                    case DCDriveMutConfigs.MinimumTorque:

                        this.ConfigureDCDriveMut(
                            DCDriveMut.DEFAULT_MinSpeedLimit, DCDriveMut.DEFAULT_MaxSpeedLimit,
                            DCDriveMut.MINIMUM_MinimumTorque, DCDriveMut.MINIMUM_MaximumTorque,
                            DCDriveMut.DEFAULT_SpeedRampTime);
                        break;
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StartDCDriveMut(DCDriveMutModes dcDriveMutMode)
        {
            const string STRLOG_MethodName = "StartDCDriveMut";

            string logMessage = STRLOG_DCDriveMutMode + dcDriveMutMode.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Start DC drive
                //
                dcDriveMut.SetMainContactorOn();

                switch (dcDriveMutMode)
                {
                    case DCDriveMutModes.EnableOnly:
                        // Don't start the drive
                        break;

                    case DCDriveMutModes.Speed:
                        dcDriveMut.StartDrive();
                        break;

                    case DCDriveMutModes.Torque:
                        dcDriveMut.StartDriveTorque();
                        break;
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StopDCDriveMut()
        {
            const string STRLOG_MethodName = "StopDCDriveMut";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Stop DC drive
                //
                dcDriveMut.ResetDrive();

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetSpeedACDrive(int speed)
        {
            const string STRLOG_MethodName = "SetSpeedACDrive";

            string logMessage = STRLOG_Speed + speed.ToString() + STRLOG_Rpm;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Set AC drive speed
                //
                acDrive.SetSpeed(speed);

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetTorqueDCDriveMut(int percent)
        {
            const string STRLOG_MethodName = "SetTorqueDCDriveMut";

            string logMessage = STRLOG_Torque + percent.ToString() + STRLOG_Percent;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Set DC drive torque
                //
                dcDriveMut.SetTorque(percent);

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetSpeedDCDriveMut(int speed)
        {
            const string STRLOG_MethodName = "SetSpeedDCDriveMut";

            string logMessage = STRLOG_Speed + speed.ToString() + STRLOG_Rpm;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Set DC drive speed
                //
                dcDriveMut.SetSpeed(speed);

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetFieldDCDriveMut(int percent)
        {
            const string STRLOG_MethodName = "SetFieldDCDriveMut";

            string logMessage = STRLOG_Field + percent.ToString() + STRLOG_Percent;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Set DC drive field
                //
                dcDriveMut.SetField(percent);

                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool TakeMeasurement(ref Measurements measurement)
        {
            const string STRLOG_MethodName = "TakeMeasurement";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            this.lastError = null;
            bool success = false;

            try
            {
                // Wait before taking the measurement
                dcDriveMut.WaitDelay(this.measurementDelay);

                //
                // Take the measurement
                //
                measurement.speed = dcDriveMut.ReadDriveSpeed();
                measurement.voltage = dcDriveMut.ReadArmatureVoltage();
                measurement.fieldCurrent = dcDriveMut.ReadFieldCurrent();
                measurement.load = dcDriveMut.ReadTorque();

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_Speed + measurement.speed.ToString() + STRLOG_Rpm +
                Logfile.STRLOG_Spacer + STRLOG_Voltage + measurement.voltage + STRLOG_Volts +
                Logfile.STRLOG_Spacer + STRLOG_FieldCurrent + measurement.fieldCurrent.ToString() + STRLOG_Amps;

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //=================================================================================================//

        private int GetInitialiseTime()
        {
            int intialiseTime = 1;

            intialiseTime += this.GetResetACDriveTime();
            intialiseTime += this.GetResetDCDriveMutTime();
            intialiseTime += this.GetConfigureACDriveTime();
            intialiseTime += this.GetConfigureDCDriveMutTime();
            intialiseTime += ACDrive.DELAY_DisableDrivePower;

            return intialiseTime;
        }

        //-------------------------------------------------------------------------------------------------//

        private void ConfigureACDrive(int speedRampTime, int maxCurrent, int minTorque, int maxTorque)
        {
            acDrive.ConfigureSpeed(0);
            acDrive.ConfigureTorque(0);
            acDrive.ConfigureSpeedRampTime(speedRampTime);
            acDrive.ConfigureMaximumCurrent(maxCurrent);
            acDrive.ConfigureMinimumTorque(minTorque);
            acDrive.ConfigureMaximumTorque(maxTorque);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ConfigureDCDriveMut(int minSpeedLimit, int maxSpeedLimit,
                int minTorqueLimit, int maxTorqueLimit, int speedRampTime)
        {
            dcDriveMut.ConfigureSpeed(0);
            dcDriveMut.ConfigureTorque(0);
            dcDriveMut.ConfigureField(100);
            dcDriveMut.ConfigureMinSpeedLimit(minSpeedLimit);
            dcDriveMut.ConfigureMaxSpeedLimit(maxSpeedLimit);
            dcDriveMut.ConfigureMinTorqueLimit(minTorqueLimit);
            dcDriveMut.ConfigureMaxTorqueLimit(maxTorqueLimit);
            dcDriveMut.ConfigureSpeedRampTime(speedRampTime);
        }
    }
}
