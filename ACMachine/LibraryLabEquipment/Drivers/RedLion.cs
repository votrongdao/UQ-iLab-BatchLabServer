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
        private const string STRLOG_ACDriveMode = "ACDriveMode: ";

        //
        // String constants for error messages
        //
        private const string STRERR_MachineIPNotSpecified = "Machine IP not specified!";
        private const string STRERR_NumberIsNegative = "Number is negative!";
        private const string STRERR_NumberIsInvalid = "Number is invalid!";
        private const string STRERR_FailedToInitialise = "Failed to initialise!";
        private const string STRERR_FailedToResetACDrive = "Failed to reset AC drive!";

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
        private PowerMeter powerMeter;

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
            Default, MaximumCurrent
        }

        //
        // Start AC Drive modes
        //
        public enum ACDriveModes
        {
            NoLoad, FullLoad, LockedRotor, SynchronousSpeed
        }

        //
        // Measurements to take
        //
        public struct Measurements
        {
            public float voltageMut;
            public float voltageVsd;
            public float currentMut;
            public float currentVsd;
            public float powerFactorMut;
            public float powerFactorVsd;
            public int speed;
            public int torque;
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
                            // Configure the AC drive with default values
                            //
                            this.ConfigureACDrive(
                                ACDrive.DEFAULT_SpeedRampTime, ACDrive.DEFAULT_MaximumCurrent,
                                ACDrive.DEFAULT_MaximumTorque, ACDrive.DEFAULT_MinimumTorque);

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

        public int GetStartACDriveTime(ACDriveModes acDriveMode)
        {
            int executionTime = 0;

            executionTime += this.GetConfigureACDriveTime();

            switch (acDriveMode)
            {
                case ACDriveModes.NoLoad:
                    executionTime += ACDrive.DELAY_StartDrive;
                    break;

                case ACDriveModes.FullLoad:
                    executionTime += ACDrive.DELAY_StartDriveFullLoad;
                    break;

                case ACDriveModes.LockedRotor:
                    executionTime += ACDrive.DELAY_StartDrive;
                    break;

                case ACDriveModes.SynchronousSpeed:
                    executionTime += ACDrive.DELAY_StartDrive;
                    break;
            }

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStopACDriveTime(ACDriveModes acDriveMode)
        {
            int executionTime = 0;

            switch (acDriveMode)
            {
                case ACDriveModes.NoLoad:
                    executionTime += ACDrive.DELAY_StopDrive;
                    break;

                case ACDriveModes.FullLoad:
                    executionTime += ACDrive.DELAY_StopDriveFullLoad;
                    break;

                case ACDriveModes.LockedRotor:
                    executionTime += ACDrive.DELAY_StopDrive;
                    break;

                case ACDriveModes.SynchronousSpeed:
                    executionTime += ACDrive.DELAY_StopDrive;
                    break;
            }

            executionTime += this.GetConfigureACDriveTime();
            executionTime += ACDrive.DELAY_DisableDrivePower;

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

        public bool CreateConnection()
        {
            const string STRLOG_MethodName = "CreateConnection";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            try
            {
                this.tcpClient = new TcpClient(this.machineIP, this.machinePort);
                this.master = ModbusIpMaster.CreateTcp(this.tcpClient);
                this.acDrive = new ACDrive(this.master);
                this.powerMeter = new PowerMeter(this.master);

                success = true;
                this.lastError = null;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool CloseConnection()
        {
            const string STRLOG_MethodName = "CloseConnection";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

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

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ResetACDrive()
        {
            const string STRLOG_MethodName = "ResetACDrive";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

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

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ConfigureACDrive(ACDriveConfigs acDriveConfig)
        {
            const string STRLOG_MethodName = "ConfigureACDrive";

            string logMessage = STRLOG_ACDriveConfig + acDriveConfig.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

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
                            ACDrive.DEFAULT_MaximumTorque, ACDrive.DEFAULT_MinimumTorque);
                        break;

                    case ACDriveConfigs.MaximumCurrent:

                        this.ConfigureACDrive(
                            ACDrive.DEFAULT_SpeedRampTime, ACDrive.MAXIMUM_MaximumCurrent,
                            ACDrive.DEFAULT_MaximumTorque, ACDrive.DEFAULT_MinimumTorque);
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

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StartACDrive(ACDriveModes acDriveMode)
        {
            const string STRLOG_MethodName = "StartACDrive";

            string logMessage = STRLOG_ACDriveMode + acDriveMode.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Start the AC drive
                //
                switch (acDriveMode)
                {
                    case ACDriveModes.NoLoad:

                        acDrive.ConfigureSpeed(ACDrive.MAXIMUM_Speed);
                        acDrive.StartDriveNoLoad();
                        break;

                    case ACDriveModes.FullLoad:

                        acDrive.StartDriveFullLoad();
                        break;

                    case ACDriveModes.LockedRotor:

                        acDrive.StartDriveLockedRotor();
                        break;

                    case ACDriveModes.SynchronousSpeed:

                        acDrive.ConfigureSpeed(ACDrive.MAXIMUM_Speed);
                        acDrive.StartDriveSyncSpeed();
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

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StopACDrive(ACDriveModes acDriveMode)
        {
            const string STRLOG_MethodName = "StopACDrive";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            this.lastError = null;
            bool success = false;

            try
            {
                //
                // Stop AC drive
                //
                switch (acDriveMode)
                {
                    case ACDriveModes.NoLoad:
                        acDrive.StopDriveNoLoad();
                        break;

                    case ACDriveModes.FullLoad:
                        acDrive.StopDriveFullLoad();
                        break;

                    case ACDriveModes.LockedRotor:
                        acDrive.StopDriveLockedRotor();
                        break;

                    case ACDriveModes.SynchronousSpeed:
                        acDrive.StopDriveSyncSpeed();
                        break;
                }

                //
                // Disable drive power
                //
                acDrive.DisableDrivePower();

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool TakeMeasurement(ref Measurements measurement)
        {
            const string STRLOG_MethodName = "TakeMeasurement";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            this.lastError = null;
            bool success = false;

            try
            {
                // Wait before taking the measurement
                acDrive.WaitDelay(this.measurementDelay);

                //
                // Take the measurement
                //
                measurement.voltageMut = this.powerMeter.ReadVoltagePhaseToPhaseMut();
                measurement.currentMut = this.powerMeter.ReadCurrentThreePhaseMut();
                measurement.powerFactorMut = this.powerMeter.ReadPowerFactorAverageMut();
                measurement.voltageVsd = this.powerMeter.ReadVoltagePhaseToPhaseVsd();
                measurement.currentVsd = this.powerMeter.ReadCurrentThreePhaseVsd();
                measurement.powerFactorVsd = this.powerMeter.ReadPowerFactorAverageVsd();
                measurement.speed = this.acDrive.ReadDriveSpeed();
                measurement.torque = this.acDrive.ReadDriveTorque();

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //=================================================================================================//

        private int GetInitialiseTime()
        {
            int intialiseTime = 1;

            intialiseTime += this.GetResetACDriveTime();
            intialiseTime += this.GetConfigureACDriveTime();
            intialiseTime += ACDrive.DELAY_DisableDrivePower;

            return intialiseTime;
        }

        //-------------------------------------------------------------------------------------------------//

        private void ConfigureACDrive(int speedRampTime, int maxCurrent, int maxTorque, int minTorque)
        {
            acDrive.ConfigureSpeed(0);
            acDrive.ConfigureTorque(0);
            acDrive.ConfigureSpeedRampTime(speedRampTime);
            acDrive.ConfigureMaximumCurrent(maxCurrent);
            acDrive.ConfigureMinimumTorque(minTorque);
            acDrive.ConfigureMaximumTorque(maxTorque);
        }

    }
}
