using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class ST360Counter : IDisposable
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ST360Counter";

        //
        // Value ranges and defaults
        //
        public const int MIN_HighVoltage = 0;
        public const int MAX_HighVoltage = 450;
        public const int DEFAULT_HighVoltage = 400;

        public const int MIN_SpeakerVolume = 0;
        public const int MAX_SpeakerVolume = 5;
        public const int DEFAULT_SpeakerVolume = 0;

        public const int MIN_PresetTime = 1;
        public const int MAX_PresetTime = 10;

        //
        // Delays are in millisecs
        //
        private const int DELAY_DISPLAY_MS = 1000;
        private const int DELAY_ISCOUNTING_MS = 500;
        private const int MAX_RESPONSE_TIME = 2000;

        //
        // Display selections
        //
        public enum Display
        {
            Counts, Time, Rate, HighVoltage, AlarmPoint, SpeakerVolume
        }

        //
        // String constants for logfile messages
        //
        protected const string STRLOG_NotInitialised = " Not Initialised!";
        protected const string STRLOG_Initialising = " Initialising...";
        protected const string STRLOG_Online = " Online: ";
        protected const string STRLOG_disposing = " disposing: ";
        protected const string STRLOG_disposed = " disposed: ";
        protected const string STRLOG_Success = " Success: ";
        private const string STRLOG_ReportHandlerThreadIsStarting = " ReportHandler thread is starting...";
        private const string STRLOG_ReportHandlerThreadIsRunning = " ReportHandler thread is running.";
        private const string STRLOG_InterfaceMode = " InterfaceMode: ";
        private const string STRLOG_Selection = " Selection: ";
        private const string STRLOG_HighVoltage = " HighVoltage: ";
        private const string STRLOG_SpeakerVolume = " SpeakerVolume: ";
        private const string STRLOG_PresetTime = " PresetTime: ";
        private const string STRLOG_Duration = " Duration: ";
        private const string STRLOG_CaptureDataTime = " CaptureDataTime: ";
        private const string STRLOG_IsCounting = " Is Counting: ";
        private const string STRLOG_Counts = " Counts: ";

        //
        // String constants for error messages
        //
        protected const string STRERR_FailedToInitialise = "Failed to initialise!";
        private const string STRERR_NumberIsNegative = "Number cannot be negative!";
        private const string STRERR_NumberIsInvalid = "Number is invalid!";
        private const string STRERR_InitialiseDelayNotSpecified = "Initialise delay is not specified!";
        private const string STRERR_ReportHandlerThreadFailedToStart = "ReportHandler thread failed to start!";
        private const string STRERR_InvalidCommand = "Invalid command!";
        private const string STRERR_FailedToSetInterfaceMode = "Failed to set Interface Mode: ";
        private const string STRERR_FailedToSetDisplaySelection = "Failed to set Display Selection: ";
        private const string STRERR_FailedToSetHighVoltage = "Failed to set High Voltage: ";
        private const string STRERR_FailedToSetSpeakerVolume = "Failed to set Speaker Volume: ";
        private const string STRERR_FailedToSetPresetTime = "Failed to set Preset Time: ";
        private const string STRERR_FailedToPushDisplaySelectSwitch = "Failed to push Display Select switch";
        private const string STRERR_FailedToPushStartSwitch = "Failed to push Start switch";
        private const string STRERR_FailedToPushStopSwitch = "Failed to push Stop switch";
        private const string STRERR_FailedToPushResetSwitch = "Failed to push Reset switch";
        private const string STRERR_FailedToReadCountingStatus = "Failed to read Counting Status";
        private const string STRERR_FailedToReadCounts = "Failed to read Counts";
        private const string STRERR_CaptureTimedOut = "Capture timed out!";

        //
        // Single byte commands that return 5 bytes which is an echo of the command
        // followed by 4 data bytes
        //
        private const byte CMD_ReadCounts = 0x40;
        private const byte CMD_ReadPresetCounts = 0x41;
        private const byte CMD_ReadElapsedTime = 0x42;
        private const byte CMD_ReadPresetTime = 0x43;
        private const byte CMD_ReadCountsPerSec = 0x44;
        private const byte CMD_ReadCountsPerMin = 0x45;
        private const byte CMD_ReadHighVoltage = 0x46;
        private const byte CMD_ReadAlarmSetPoint = 0x47;
        private const byte CMD_ReadIsCounting = 0x48;
        private const byte CMD_ReadSpeakerVolume = 0x49;
        private const byte CMD_ReadDisplaySelection = 0x4A;
        private const byte CMD_ReadAnalyserInfo = 0x4B;

        private const int DATALEN_CMD_Read = 5;

        //
        // Single byte commands that return 1 byte which is an echo of the command
        //
        private const byte CMD_InterfaceNone = 0x50;
        private const byte CMD_InterfaceSerial = 0x51;
        private const byte CMD_InterfaceUsb = 0x52;

        public enum Commands
        {
            InterfaceNone = 0x50, InterfaceSerial = 0x51, InterfaceUsb = 0x52
        }

        private const int DATALEN_CMD_Interface = 1;

        //
        // Two byte commands that return 2 bytes which is an echo of the command.
        // Write and read the first byte before writing and reading the second byte
        //
        private const byte CMD_SetHighVoltage = 0x80;
        private const byte CMD_SetAlarmRate = 0x82;
        private const byte CMD_SetPresetTime = 0x83;
        private const byte CMD_SetSpeakerVolume = 0x84;
        private const byte CMD_SetCPMRateDisplay = 0x86;
        private const byte CMD_SetCPSRateDisplay = 0x87;

        private const int DATALEN_CMD_Set = 1;

        //
        // Single byte commands that return 1 byte which is an echo of the command
        //
        private const byte CMD_PushDisplaySelectSwitch = 0xDF;
        private const byte CMD_PushDownSwitch = 0xEF;
        private const byte CMD_PushUpSwitch = 0xF7;
        private const byte CMD_PushResetSwitch = 0xFB;
        private const byte CMD_PushStopSwitch = 0xFD;
        private const byte CMD_PushStartSwitch = 0xFE;

        private const int DATALEN_CMD_Push = 1;

        //
        // Local variables
        //
        protected Logfile.LoggingLevels logLevel;
        protected bool initialised;
        protected bool configured;
        private string lastError;
        private bool disposed;
        private int geigerTubeVoltage;
        private int speakerVolume;
        private byte[] responsePacket;
        private Object responseSignal;

        private class ReceiveDataInfo
        {
            public const int BUFFER_SIZE = 128;
            public byte[] receiveBuffer = new byte[BUFFER_SIZE];
            public int numBytesToRead = BUFFER_SIZE;
            public int bytesRead = 0;
            public int bufferIndex = 0;
            public int expectedPacketLength = -1;
        }
        private ReceiveDataInfo receiveDataInfo;

        #endregion

        #region Public Properties

        //
        // Minimum power-up and initialise delays in seconds
        //
        public const int DELAY_POWERUP = 5;

        protected int initialiseDelay;
        protected bool online;
        protected string statusMessage;
        private double[] timeAdjustmentCapture;

        /// <summary>
        /// Returns the time (in seconds) that it takes for the equipment to initialise.
        /// </summary>
        public int InitialiseDelay
        {
            get { return this.initialiseDelay; }
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

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ST360Counter(XmlNode xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "ST360Counter";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.disposed = true;
            this.initialised = false;
            this.lastError = null;
            this.configured = false;

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
            // Get initialisation delay
            //
            XmlNode xmlNodeST360Counter = XmlUtilities.GetXmlNode(xmlNodeEquipmentConfig, Consts.STRXML_st360Counter);
            try
            {
                this.initialiseDelay = XmlUtilities.GetIntValue(xmlNodeST360Counter, Consts.STRXML_initialiseDelay);
                if (this.initialiseDelay < 0)
                {
                    throw new ArgumentException(STRERR_NumberIsNegative);
                }
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentException(STRERR_InitialiseDelayNotSpecified);
            }
            catch (FormatException)
            {
                // Value cannot be converted
                throw new ArgumentException(STRERR_NumberIsInvalid, Consts.STRXML_initialiseDelay);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw new ArgumentException(ex.Message, Consts.STRXML_initialiseDelay);
            }

            //
            // Get Geiger tube voltage from application's configuration file
            //
            this.geigerTubeVoltage = XmlUtilities.GetIntValue(xmlNodeST360Counter, Consts.STRXML_voltage, DEFAULT_HighVoltage);

            //
            // Make sure that the high voltage is within range
            //
            if (this.geigerTubeVoltage < MIN_HighVoltage)
            {
                this.geigerTubeVoltage = MIN_HighVoltage;
            }
            else if (this.geigerTubeVoltage > MAX_HighVoltage)
            {
                this.geigerTubeVoltage = MAX_HighVoltage;
            }
            Logfile.Write(STRLOG_HighVoltage + this.geigerTubeVoltage.ToString());

            //
            // Get speaker volume from application's configuration file
            //
            this.speakerVolume = XmlUtilities.GetIntValue(xmlNodeST360Counter, Consts.STRXML_volume, MIN_SpeakerVolume);

            //
            // Make sure that the speaker volume is within range
            //
            if (this.speakerVolume < MIN_SpeakerVolume)
            {
                this.speakerVolume = MIN_SpeakerVolume;
            }
            else if (this.speakerVolume > MAX_SpeakerVolume)
            {
                this.speakerVolume = MAX_SpeakerVolume;
            }
            Logfile.Write(STRLOG_SpeakerVolume + this.speakerVolume.ToString());

            //
            // Get capture time adjustment: y = Mx + C
            //
            XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodeST360Counter, Consts.STRXML_timeAdjustment, false);
            string capture = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_capture, false);
            string[] strSplit = capture.Split(new char[] { Engine.Consts.CHR_CsvSplitterChar });
            this.timeAdjustmentCapture = new double[strSplit.Length];
            for (int i = 0; i < strSplit.Length; i++)
            {
                this.timeAdjustmentCapture[i] = Double.Parse(strSplit[i]);
            }

            //
            // Create the receive objects
            //
            this.receiveDataInfo = new ReceiveDataInfo();
            this.responseSignal = new Object();

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

        public virtual bool Initialise(bool configure)
        {
            //
            // There is now some disposing to do
            //
            this.disposed = false;

            return true;
        }

        //-------------------------------------------------------------------------------------------------//

        protected bool Configure()
        {
            const string STRLOG_MethodName = "Configure";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                //
                // Display the high voltage and set it
                //
                if (this.SetDisplay(ST360Counter.Display.HighVoltage) == false)
                {
                    throw new Exception(this.GetLastError());
                }
                if (this.SetHighVoltage(this.geigerTubeVoltage) == false)
                {
                    throw new Exception(this.GetLastError());
                }
                Thread.Sleep(DELAY_DISPLAY_MS);

                //
                // Display the speaker volume and set it
                //
                if (this.SetDisplay(ST360Counter.Display.SpeakerVolume) == false)
                {
                    throw new Exception(this.GetLastError());
                }
                if (this.SetSpeakerVolume(this.speakerVolume) == false)
                {
                    throw new Exception(this.GetLastError());
                }
                Thread.Sleep(DELAY_DISPLAY_MS);

                //
                // Set display to counts and clear time and counts
                //
                if (this.SetDisplay(Display.Counts) == false)
                {
                    throw new Exception(this.GetLastError());
                }
                if (this.PushResetSwitch() == false)
                {
                    throw new Exception(this.GetLastError());
                }

                this.configured = true;
				success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetCaptureDataTime(int duration)
        {
            double seconds = duration;

            seconds += (double)(DELAY_DISPLAY_MS) / 1000.0;

            //
            // y = Mx + C
            // 
            seconds = seconds * this.timeAdjustmentCapture[0] + this.timeAdjustmentCapture[1];

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetInterfaceMode(Commands command)
        {
            const string STRLOG_MethodName = "SetInterfaceMode";

            string logMessage = STRLOG_InterfaceMode + command.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            try
            {
                //
                // Check for valid interface command
                //
                if (command != Commands.InterfaceNone &&
                    command != Commands.InterfaceSerial)
                {
                    throw new ArgumentException(STRERR_InvalidCommand, command.ToString());
                }

                //
                // Write the command and get the received data, should be the command echoed back
                //
                byte[] readData = WriteReadData(new byte[] { (byte)command }, 1, DATALEN_CMD_Interface);
                if (readData == null || readData.Length != DATALEN_CMD_Interface || readData[0] != (byte)command)
                {
                    throw new Exception(STRERR_FailedToSetInterfaceMode + command.ToString());
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool CaptureData(int duration, int[] counts)
        {
            const string STRLOG_MethodName = "CaptureData";

            string logMessage = STRLOG_Duration + duration.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            try
            {
                //
                // Use timeout with retries
                //
                int retries = 3;
                for (int i = 0; i < retries; i++)
                {
                    //
                    // Set the duration
                    //
                    if (this.SetPresetTime(duration) == false)
                    {
                        throw new Exception(this.GetLastError());
                    }

                    //
                    // Reset the time and count before starting the counter
                    //
                    if (this.PushResetSwitch() == false)
                    {
                        throw new Exception(this.GetLastError());
                    }

                    //
                    // Set the display to time so that we can see the progress
                    //
                    //if (this.SetDisplay(ST360Counter.Display.Time) == false)
                    if (this.SetDisplay(ST360Counter.Display.Counts) == false)
                    {
                        throw new Exception(this.GetLastError());
                    }

                    //
                    // Start the counter
                    //
                    if (this.StartCounter() == false)
                    {
                        throw new Exception(this.GetLastError());
                    }

                    //
                    // Set a timeout so that we don't wait forever if something goes wrong
                    //
                    int timeout = (duration + 5) * 1000 / DELAY_ISCOUNTING_MS;
                    while (--timeout > 0)
                    {
                        //
                        // Wait a bit and then check if still counting
                        //
                        Thread.Sleep(DELAY_ISCOUNTING_MS);

                        bool[] isCounting = new bool[1];
                        if (this.IsCounting(isCounting) == false)
                        {
                            throw new Exception(this.GetLastError());
                        }
                        if (isCounting[0] == false)
                        {
                            break;
                        }
                    }
                    if (timeout == 0)
                    {
                        Logfile.WriteError(STRERR_CaptureTimedOut);

                        //
                        // Stop the counter
                        //
                        if (this.StopCounter() == false)
                        {
                            throw new Exception(this.GetLastError());
                        }

                        //
                        // Retry
                        //
                        continue;
                    }

                    //
                    // Display the counts for a moment
                    //
                    Thread.Sleep(DELAY_DISPLAY_MS);

                    //
                    // Get the counts and check for error
                    //
                    if (this.GetCounts(counts) == false)
                    {
                        throw new Exception(this.GetLastError());
                    }

                    //
                    // Data captured successfully
                    //
                    break;
                }
            }
            catch (Exception ex)
            {
                success = false;
                Logfile.WriteError(ex.Message);
            }

            logMessage = STRLOG_Counts + counts[0].ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetDisplay(Display selection)
        {
            const string STRLOG_MethodName = "SetDisplay";

            string logMessage = STRLOG_Selection + selection.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            try
            {
                while (true)
                {
                    //
                    // Get the current display selection
                    //
                    byte[] readData = WriteReadData(new byte[] { CMD_ReadDisplaySelection }, 1, DATALEN_CMD_Read);
                    if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadDisplaySelection)
                    {
                        throw new Exception(STRERR_FailedToSetDisplaySelection + selection.ToString());
                    }
                    Display currentSelection = (Display)readData[DATALEN_CMD_Read - 1];

                    //
                    // Check if this is the desired selection
                    //
                    if (currentSelection == selection)
                    {
                        break;
                    }

                    //
                    // Move the display selection down by one
                    //
                    readData = WriteReadData(new byte[] { CMD_PushDisplaySelectSwitch }, 1, DATALEN_CMD_Push);
                    if (readData == null || readData.Length != DATALEN_CMD_Push || readData[0] != CMD_PushDisplaySelectSwitch)
                    {
                        throw new Exception(STRERR_FailedToPushDisplaySelectSwitch);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetHighVoltage(int highVoltage)
        {
            const string STRLOG_MethodName = "SetHighVoltage";

            string logMessage = STRLOG_HighVoltage + highVoltage.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            try
            {
                //
                // Make sure high voltage is within range
                //
                if (highVoltage < MIN_HighVoltage && highVoltage > MAX_HighVoltage)
                {
                    throw new ArgumentOutOfRangeException("SetHighVoltage", "Not in range");
                }

                //
                // Determine value to write for desired high voltage 
                //
                byte highVoltageValue = (byte)(highVoltage / 5);

                //
                // Write the command
                //
                byte[] readData = WriteReadData(new byte[] { CMD_SetHighVoltage }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != CMD_SetHighVoltage)
                {
                    throw new Exception(STRERR_FailedToSetHighVoltage + highVoltage.ToString());
                }

                //
                // Write the high voltage value
                //
                readData = WriteReadData(new byte[] { highVoltageValue }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != highVoltageValue)
                {
                    throw new Exception(STRERR_FailedToSetHighVoltage + highVoltage.ToString());
                }

                //
                // Read the high voltage back
                //
                readData = WriteReadData(new byte[] { CMD_ReadHighVoltage }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadHighVoltage)
                {
                    throw new Exception(STRERR_FailedToSetHighVoltage + highVoltage.ToString());
                }

                //
                // Extract high voltage value from byte array and compare
                //
                int readHighVoltage = 0;
                for (int i = 1; i < DATALEN_CMD_Read; i++)
                {
                    readHighVoltage = readHighVoltage * 256 + (int)readData[i];
                }
                if (readHighVoltage != highVoltage)
                {
                    throw new Exception(STRERR_FailedToSetHighVoltage + highVoltage.ToString());
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetSpeakerVolume(int speakerVolume)
        {
            const string STRLOG_MethodName = "SetSpeakerVolume";

            string logMessage = STRLOG_SpeakerVolume + speakerVolume.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            try
            {
                //
                //
                // Make sure speaker volume is within range
                //
                if (speakerVolume < MIN_SpeakerVolume && speakerVolume > MAX_SpeakerVolume)
                {
                    throw new ArgumentOutOfRangeException("SetSpeakerVolume", "Not in range");
                }

                //
                // Determine value to write for desired speaker volume
                //
                byte speakerVolumeValue = (byte)speakerVolume;

                //
                // Write the command
                //
                byte[] readData = WriteReadData(new byte[] { CMD_SetSpeakerVolume }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != CMD_SetSpeakerVolume)
                {
                    throw new Exception(STRERR_FailedToSetSpeakerVolume + speakerVolumeValue.ToString());
                }

                //
                // Write the speaker volume value
                //
                readData = WriteReadData(new byte[] { speakerVolumeValue }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != speakerVolumeValue)
                {
                    throw new Exception(STRERR_FailedToSetSpeakerVolume + speakerVolumeValue.ToString());
                }

                //
                // Read the speaker volume back
                //
                readData = WriteReadData(new byte[] { CMD_ReadSpeakerVolume }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadSpeakerVolume ||
                    readData[DATALEN_CMD_Read - 1] != speakerVolumeValue)
                {
                    throw new Exception(STRERR_FailedToSetSpeakerVolume + speakerVolumeValue.ToString());
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetPresetTime(int seconds)
        {
            const string STRLOG_MethodName = "SetPresetTime";

            string logMessage = STRLOG_PresetTime + seconds.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            try
            {
                //
                // Make sure preset time is within range
                //
                if (seconds < MIN_PresetTime || seconds > MAX_PresetTime)
                {
                    throw new ArgumentOutOfRangeException("SetPresetTime", "Not in range");
                }

                //
                // Determine value to write for desired preset time
                //
                byte secondsValue = (byte)seconds;

                //
                // Write the command
                //
                byte[] readData = WriteReadData(new byte[] { CMD_SetPresetTime }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != CMD_SetPresetTime)
                {
                    throw new Exception(STRERR_FailedToSetPresetTime + secondsValue.ToString());
                }

                //
                // Write the preset time value
                //
                readData = WriteReadData(new byte[] { secondsValue }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != secondsValue)
                {
                    throw new Exception(STRERR_FailedToSetPresetTime + secondsValue.ToString());
                }

                //
                // Read the preset time back
                //
                readData = WriteReadData(new byte[] { CMD_ReadPresetTime }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadPresetTime ||
                    readData[DATALEN_CMD_Read - 1] != secondsValue)
                {
                    throw new Exception(STRERR_FailedToSetPresetTime + secondsValue.ToString());
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StartCounter()
        {
            const string STRLOG_MethodName = "StartCounter";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            try
            {
                //
                // Write the command
                //
                byte[] readData = WriteReadData(new byte[] { CMD_PushStartSwitch }, 1, DATALEN_CMD_Push);
                if (readData == null || readData.Length != DATALEN_CMD_Push || readData[0] != CMD_PushStartSwitch)
                {
                    throw new Exception(STRERR_FailedToPushStartSwitch);
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StopCounter()
        {
            const string STRLOG_MethodName = "StopCounter";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            try
            {
                //
                // Write the command
                //
                byte[] readData = WriteReadData(new byte[] { CMD_PushStopSwitch }, 1, DATALEN_CMD_Push);
                if (readData == null || readData.Length != DATALEN_CMD_Push || readData[0] != CMD_PushStopSwitch)
                {
                    throw new Exception(STRERR_FailedToPushStopSwitch);
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool IsCounting(bool[] isCounting)
        {
            //const string STRLOG_MethodName = "IsCounting";

            //Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            try
            {
                //
                // Write the command
                //
                byte[] readData = WriteReadData(new byte[] { CMD_ReadIsCounting }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadIsCounting)
                {
                    throw new Exception(STRERR_FailedToReadCountingStatus);
                }

                isCounting[0] = (readData[DATALEN_CMD_Read - 1] != 0);
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            //string logMessage = STRLOG_Success + success.ToString() +
            //    Logfile.STRLOG_Spacer + STRLOG_IsCounting + isCounting[0].ToString();

            //Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetCounts(int[] counts)
        {
            const string STRLOG_MethodName = "GetCounts";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            try
            {
                //
                // Write the command
                //
                byte[] readData = WriteReadData(new byte[] { CMD_ReadCounts }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadCounts)
                {
                    throw new Exception(STRERR_FailedToReadCounts);
                }

                //
                // Extract count value from byte array
                //
                for (int i = 1; i < DATALEN_CMD_Read; i++)
                {
                    counts[0] = counts[0] * 256 + (int)readData[i];
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            string logMessage = counts.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool PushResetSwitch()
        {
            const string STRLOG_MethodName = "PushResetSwitch";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            try
            {
                //
                // Write the command
                //
                byte[] readData = WriteReadData(new byte[] { CMD_PushResetSwitch }, 1, DATALEN_CMD_Push);
                if (readData == null || readData.Length != DATALEN_CMD_Push || readData[0] != CMD_PushResetSwitch)
                {
                    throw new Exception(STRERR_FailedToPushResetSwitch);
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.lastError = ex.Message;
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        #region Close and Dispose

        /// <summary>
        /// Do not make this method virtual. A derived class should not be allowed to override this method.
        /// </summary>
        public void Close()
        {
            const string STRLOG_MethodName = "Close";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            // Calls the Dispose method without parameters
            Dispose();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Implement IDisposable. Do not make this method virtual. A derived class should not be able
        /// to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Take yourself off the Finalization queue to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Use C# destructor syntax for finalization code. This destructor will run only if the Dispose
        /// method does not get called. It gives your base class the opportunity to finalize. Do not provide
        /// destructors in types derived from this class.
        /// </summary>
        ~ST360Counter()
        {
            Trace.WriteLine("~ST360Counter():");

            //
            // Do not re-create Dispose clean-up code here. Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            //
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios:
        /// 1. If disposing equals true, the method has been called directly or indirectly by a user's code.
        ///    Managed and unmanaged resources can be disposed.
        /// 2. If disposing equals false, the method has been called by the runtime from inside the finalizer
        ///    and you should not reference other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            const string STRLOG_MethodName = "Dispose";

            string logMessage = STRLOG_disposing + disposing.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_disposed + this.disposed.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Check to see if Dispose has already been called
            //
            if (this.disposed == false)
            {
                //
                // If disposing equals true, dispose all managed and unmanaged resources.
                //
                if (disposing == true)
                {
                    // Dispose managed resources here. Anything that has a Dispose() method.
                }

                //
                // Release unmanaged resources here. If disposing is false, only the following
                // code is executed.
                //
                if (this.configured == true)
                {
                    //
                    // Reset everything
                    //
                    bool[] isCounting = new bool[1];
                    if (this.IsCounting(isCounting) == false)
                    {
                        Logfile.WriteError(this.GetLastError());
                    }
                    else
                    {
                        if (isCounting[0] == true)
                        {
                            if (this.StopCounter() == false)
                            {
                                Logfile.WriteError(this.GetLastError());
                            }
                        }
                    }
                    if (this.SetHighVoltage(0) == false)
                    {
                        Logfile.WriteError(this.GetLastError());
                    }
                    if (this.SetSpeakerVolume(MIN_SpeakerVolume) == false)
                    {
                        Logfile.WriteError(this.GetLastError());
                    }
                    if (this.SetDisplay(Display.Counts) == false)
                    {
                        Logfile.WriteError(this.GetLastError());
                    }
                    if (this.PushResetSwitch() == false)
                    {
                        Logfile.WriteError(this.GetLastError());
                    }
                }

                if (this.initialised == true)
                {
                    //
                    // Set interface type back to none
                    //
                    if (this.SetInterfaceMode(Commands.InterfaceNone) == false)
                    {
                        Logfile.WriteError(this.GetLastError());
                    }
                }

                this.disposed = true;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        #endregion

        //=================================================================================================//

        private byte[] WriteReadData(byte[] writeData, int writeCount, int readCount)
        {
            //
            // Send the request and return the response
            //
            lock (responseSignal)
            {
                responsePacket = null;

                //
                // Initialise the receive data info
                //
                ReceiveDataInfo rdi = this.receiveDataInfo;
                rdi.bytesRead = 0;
                rdi.bufferIndex = 0;
                rdi.expectedPacketLength = readCount;

                // Write the data to the serial LCD
                this.SendData(writeData, writeCount);

                // Wait for the response packet
                if (Monitor.Wait(responseSignal, MAX_RESPONSE_TIME))
                {
                    return responsePacket;
                }
            }

            return null;
        }

        //-------------------------------------------------------------------------------------------------//

        protected virtual bool SendData(byte[] data, int dataLength)
        {
            return true;
        }

        //---------------------------------------------------------------------------------------//

        protected void ReceiveData(byte[] data, int dataLength)
        {
            ReceiveDataInfo rdi = this.receiveDataInfo;

            try
            {
                //
                // Copy all of the data to the receive buffer
                //
                Array.Copy(data, 0, rdi.receiveBuffer, rdi.bufferIndex, dataLength);
                rdi.bytesRead += dataLength;
                rdi.bufferIndex += dataLength;

                //
                // Check if all of the expected data has been read
                //
                if (rdi.bytesRead >= rdi.expectedPacketLength)
                {
                    //
                    // The buffer has at least as many bytes for this packet
                    //
                    this.responsePacket = new byte[rdi.expectedPacketLength];
                    Array.Copy(rdi.receiveBuffer, 0, this.responsePacket, 0, rdi.expectedPacketLength);

                    //
                    // Signal the waiting thread that the data has been received
                    //
                    lock (responseSignal)
                    {
                        Monitor.Pulse(responseSignal);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }
        }

    }
}
