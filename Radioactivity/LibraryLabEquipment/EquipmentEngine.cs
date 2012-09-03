using System;
using System.Web;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Drivers;

namespace Library.LabEquipment
{
    public class EquipmentEngine : LabEquipmentEngine
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "EquipmentEngine";

        //
        // Constants
        //

        //
        // String constants
        //

        //
        // String constants for logfile messages
        //

        //
        // String constants for error messages
        //
        private const string STRERR_SerialLcdCommsTypeNotSpecified = "Serial LCD communications type not specified!";
        private const string STRERR_ST360CounterCommsTypeNotSpecified = "ST360 Counter communications type not specified!";
        private const string STRERR_RadiationCounterTypeNotSpecified = "Radiation counter type not specified!";
        private const string STRERR_FailedToInitialiseSerialLcd = "Failed to initialise Serial LCD!";
        private const string STRERR_FailedToInitialiseST360Counter = "Failed to initialise ST360 Counter!";
        private const string STRERR_FailedToInitialisePhysicsCounter = "Failed to initialise Physics Counter!";
        private const string STRERR_FailedToInitialiseFlexMotion = "Failed to initialise FlexMotion controller!";

        //
        // Serial LCD types - the type being used is specified in the application's configuration file
        //
        private enum CommunicationTypes
        {
            Network, Serial, None
        }

        //
        // Radiation counter types - the type being used is specified in the application's configuration file
        //
        private enum RadiationCounterTypes
        {
            ST360, Physics
        }

        //
        // Local variables
        //
        private FlexMotion flexMotion;
        private CommunicationTypes serialLcdCommType;
        private SerialLcdSer serialLcdSer;
        private SerialLcdTcp serialLcdTcp;
        private SerialLcdNone serialLcdNone;
        private SerialLcd serialLcd;
        private RadiationCounterTypes radiationCounterType;
        private CommunicationTypes st360CounterCommType;
        private ST360CounterSer st360CounterSer;
        private ST360CounterTcp st360CounterTcp;
        private ST360CounterNone st360CounterNone;
        private ST360Counter st360Counter;
        private PhysicsCounter physicsCounter;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public EquipmentEngine(string rootFilePath)
            : base(rootFilePath)
        {
            const string STRLOG_MethodName = "EquipmentEngine";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            int initialiseDelay = 0;
            bool hardwarePresent = true;

            try
            {
                //
                // Determine if hardware is to be used or whether this is running without hardware
                // for testing and debugging
                //
                try
                {
                    hardwarePresent = XmlUtilities.GetBoolValue(this.xmlNodeEquipmentConfig, Consts.STRXML_hardwarePresent, false);
                }
                catch
                {
                    // Key not found or invalid so assume hardware is present
                }

                //
                // Create an instance of the FlexMotion class, must be done before creating the Radiation Counter class
                //
                if (hardwarePresent == true)
                {
                    this.flexMotion = new FlexMotionCntl(this.xmlNodeEquipmentConfig);
                }
                else
                {
                    this.flexMotion = new FlexMotionNone(this.xmlNodeEquipmentConfig);
                }
                initialiseDelay += this.flexMotion.InitialiseDelay;

                //
                // Get the serial LCD communication type
                //
                XmlNode xmlNodeSerialLcd = XmlUtilities.GetXmlNode(this.xmlNodeEquipmentConfig, Consts.STRXML_serialLcd);
                string strSerialLcdCommType = XmlUtilities.GetXmlValue(xmlNodeSerialLcd, Consts.STRXML_type, false);
                this.serialLcdCommType = (CommunicationTypes)Enum.Parse(typeof(CommunicationTypes), strSerialLcdCommType);

                //
                // Create an instance of the SerialLcd class, must be done before creating the Radiation Counter class
                //
                if (hardwarePresent == true)
                {
                    if (this.serialLcdCommType == CommunicationTypes.Network)
                    {
                        this.serialLcdTcp = new SerialLcdTcp(this.xmlNodeEquipmentConfig);
                        this.serialLcd = this.serialLcdTcp;
                    }
                    else if (this.serialLcdCommType == CommunicationTypes.Serial)
                    {
                        this.serialLcdSer = new SerialLcdSer(this.xmlNodeEquipmentConfig);
                        this.serialLcd = this.serialLcdSer;
                    }
                    else if (this.serialLcdCommType == CommunicationTypes.None)
                    {
                        this.serialLcdNone = new SerialLcdNone(this.xmlNodeEquipmentConfig);
                        this.serialLcd = this.serialLcdNone;
                    }
                    else
                    {
                        throw new ArgumentException(STRERR_SerialLcdCommsTypeNotSpecified);
                    }
                }
                else
                {
                    this.serialLcdNone = new SerialLcdNone(this.xmlNodeEquipmentConfig);
                    this.serialLcd = this.serialLcdNone;
                }
                initialiseDelay += this.serialLcd.InitialiseDelay;

                //
                // Get the radiation counter type
                //
                XmlNode xmlNodeRadiationCounter = XmlUtilities.GetXmlNode(this.xmlNodeEquipmentConfig, Consts.STRXML_radiationCounter, false);
                string strCounterType = XmlUtilities.GetXmlValue(xmlNodeRadiationCounter, Consts.STRXML_type, false);
                this.radiationCounterType = (RadiationCounterTypes)Enum.Parse(typeof(RadiationCounterTypes), strCounterType);

                //
                // Create an instance of the radiation counter class
                //
                if (this.radiationCounterType == RadiationCounterTypes.ST360)
                {
                    //
                    // Get the ST360 counter communication type
                    //
                    XmlNode xmlNodeST360Counter = XmlUtilities.GetXmlNode(this.xmlNodeEquipmentConfig, Consts.STRXML_st360Counter);
                    string strST360CounterCommType = XmlUtilities.GetXmlValue(xmlNodeST360Counter, Consts.STRXML_type, false);
                    this.st360CounterCommType = (CommunicationTypes)Enum.Parse(typeof(CommunicationTypes), strST360CounterCommType);

                    //
                    // Create an instance of the ST360 counter class depending on the communication type
                    //
                    if (hardwarePresent == true)
                    {
                        if (this.st360CounterCommType == CommunicationTypes.Network)
                        {
                            this.st360CounterTcp = new ST360CounterTcp(this.xmlNodeEquipmentConfig);
                            this.st360Counter = st360CounterTcp;
                        }
                        else if (this.st360CounterCommType == CommunicationTypes.Serial)
                        {
                            this.st360CounterSer = new ST360CounterSer(this.xmlNodeEquipmentConfig);
                            this.st360Counter = st360CounterSer;
                        }
                        else
                        {
                            throw new ArgumentException(STRERR_ST360CounterCommsTypeNotSpecified);
                        }
                    }
                    else
                    {
                        this.st360CounterNone = new ST360CounterNone(this.xmlNodeEquipmentConfig);
                        this.st360Counter = st360CounterNone;
                    }
                    initialiseDelay += this.st360Counter.InitialiseDelay;
                }
                else if (this.radiationCounterType == RadiationCounterTypes.Physics)
                {
                    if (hardwarePresent == true)
                    {
                        if (this.serialLcdCommType == CommunicationTypes.Network)
                        {
                            this.physicsCounter = new PhysicsCounter(this.xmlNodeEquipmentConfig, this.serialLcdTcp, this.flexMotion);
                        }
                        else if (this.serialLcdCommType == CommunicationTypes.Serial)
                        {
                            this.physicsCounter = new PhysicsCounter(this.xmlNodeEquipmentConfig, this.serialLcdSer, this.flexMotion);
                        }
                        else
                        {
                            throw new ArgumentException(STRERR_SerialLcdCommsTypeNotSpecified);
                        }
                    }
                    else
                    {
                        this.physicsCounter = new PhysicsCounter(this.xmlNodeEquipmentConfig, this.serialLcdNone, this.flexMotion);
                    }
                    initialiseDelay += this.physicsCounter.InitialiseDelay;
                }
                else
                {
                    throw new ArgumentException(STRERR_RadiationCounterTypeNotSpecified);
                }

                //
                // Update the power initialisation delay
                //
                this.powerupInitialiseDelay = initialiseDelay;
                Logfile.Write(STRLOG_InitialiseDelay + initialiseDelay.ToString() + STRLOG_Seconds);
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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Enable power to the external equipment.
        /// </summary>
        /// <returns>True if successful.</returns>
        public override bool PowerupEquipment()
        {
            const string STRLOG_MethodName = "PowerupEquipment";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            success = this.flexMotion.EnablePower();

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Initialise the equipment after it has been powered up.
        /// </summary>
        /// <returns>True if successful.</returns>
        public override bool InitialiseEquipment()
        {
            const string STRLOG_MethodName = "InitialiseEquipment";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                //
                // The initialisation delay may change once the equipment has been initialised
                // for the first time, so recalculate it
                //
                int initialiseDelay = 0;

                //
                // Initialise the FlexMotion controller
                //
                if (this.flexMotion.PowerInitialise() == false)
                {
                    throw new Exception(STRERR_FailedToInitialiseFlexMotion);
                }
                initialiseDelay += this.flexMotion.InitialiseDelay;

                //
                // Initialise the SerialLcd
                //
                if (this.serialLcd.Initialise() == false)
                {
                    throw new Exception(STRERR_FailedToInitialiseSerialLcd);
                }
                initialiseDelay += this.serialLcd.InitialiseDelay;

                //
                // Display LabEquipment service title on the Serial LCD
                //
                this.serialLcd.WriteLine(1, this.Title);
                this.serialLcd.WriteLine(2, string.Empty);

                //
                // Initialise the radiation counter class being used
                //
                if (this.radiationCounterType == RadiationCounterTypes.ST360)
                {
                    this.physicsCounter = null;
                    if (this.st360Counter.Initialise(true) == false)
                    {
                        throw new Exception(STRERR_FailedToInitialiseST360Counter);
                    }
                    initialiseDelay += this.st360Counter.InitialiseDelay;
                }
                else if (this.radiationCounterType == RadiationCounterTypes.Physics)
                {
                    this.st360Counter = null;
                    if (this.physicsCounter.Initialise() == false)
                    {
                        throw new Exception(STRERR_FailedToInitialisePhysicsCounter);
                    }
                    initialiseDelay += this.physicsCounter.InitialiseDelay;
                }

                //
                // Update the power initialisation delay for TimeUntilReady()
                //
                this.powerupInitialiseDelay = initialiseDelay;
                Logfile.Write(STRLOG_InitialiseDelay + initialiseDelay.ToString() + STRLOG_Seconds);

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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Disable power to the external equipment.
        /// </summary>
        /// <returns>True if successful.</returns>
        public override bool PowerdownEquipment()
        {
            const string STRLOG_MethodName = "PowerdownEquipment";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Close the Serial LCD and ST360 counter before powering down
            //
            if (this.serialLcd != null)
            {
                this.serialLcd.Close();
            }
            if (this.st360Counter != null)
            {
                this.st360Counter.Close();
            }
#if NO_POWERDOWN
#else
            success = this.flexMotion.DisablePower();
#endif

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public override LabStatus GetLabEquipmentStatus()
        {
            const string STRLOG_MethodName = "GetLabEquipmentStatus";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            LabStatus labStatus = new LabStatus(this.flexMotion.Online, this.flexMotion.StatusMessage);
            if (labStatus.online == true)
            {
                labStatus.online = this.slOnline;
                labStatus.labStatusMessage = this.slStatusMessage;
            }

            string logMessage = STRLOG_Online + labStatus.online.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_StatusMessage + Logfile.STRLOG_Quote + labStatus.labStatusMessage + Logfile.STRLOG_Quote;

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return labStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public override ExecuteCommandInfo ProcessCommand(ExecuteCommandInfo executeCommandInfo)
        {
            const string STRLOG_MethodName = "ProcessCommand";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            CommandInfo commandInfo = (CommandInfo)executeCommandInfo;

            bool success = true;
            string errorMessage = null;

            try
            {
                //
                // Process the execute command
                //
                ExecuteCommands executeCommand = (ExecuteCommands)commandInfo.command;

                switch (executeCommand)
                {
                    case ExecuteCommands.SetTubeDistance:

                        //
                        // Set tube distance
                        //
                        int tubeDistance = (int)commandInfo.parameters[0];
                        if ((success = this.flexMotion.SetTubeDistance(tubeDistance)) == false)
                        {
                            errorMessage = this.flexMotion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.SetSourceLocation:

                        //
                        // Set source location
                        //
                        char sourceLocation = (char)commandInfo.parameters[0];
                        if ((success = this.flexMotion.SetSourceLocation(sourceLocation)) == false)
                        {
                            errorMessage = this.flexMotion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.SetAbsorberLocation:

                        //
                        // Set absorber location
                        //
                        char absorberLocation = (char)commandInfo.parameters[0];
                        if ((success = this.flexMotion.SetAbsorberLocation(absorberLocation)) == false)
                        {
                            errorMessage = this.flexMotion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.GetCaptureData:

                        //
                        // Get duration from parameters
                        //
                        int duration = (int)commandInfo.parameters[0];

                        //
                        // Get radiation count
                        //
                        int count = -1;
                        if (this.radiationCounterType == RadiationCounterTypes.ST360)
                        {
                            int[] counts = new int[1];
                            if (this.st360Counter.CaptureData(duration, counts) == true)
                            {
                                count = counts[0];
                            }
                        }
                        else if (this.radiationCounterType == RadiationCounterTypes.Physics)
                        {
                            count = this.physicsCounter.CaptureData(duration);
                        }

                        //
                        // Add radiation count to results
                        //
                        commandInfo.results = new object[] { count };
                        break;

                    case ExecuteCommands.WriteLcdLine:

                        //
                        // Get LCD line number from request
                        //
                        int lcdLineNo = (int)commandInfo.parameters[0];

                        //
                        // Get LCD message from request and 'URL Decode' to preserve spaces
                        //
                        string lcdMessage = (string)commandInfo.parameters[1];
                        lcdMessage = HttpUtility.UrlDecode(lcdMessage);

                        //
                        // Write message to LCD
                        //
                        if ((success = this.serialLcd.WriteLine(lcdLineNo, lcdMessage)) == false)
                        {
                            errorMessage = this.serialLcd.GetLastError();
                        }
                        break;

                    default:

                        //
                        // Unknown command
                        //
                        errorMessage = STRERR_UnknownCommand + executeCommand.ToString();
                        success = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
            }

            //
            // Update success of command execution
            //
            executeCommandInfo.success = success;

            string logMessage = STRLOG_Success + success.ToString();
            if (success == false)
            {
                executeCommandInfo.errorMessage = errorMessage;
                logMessage += Logfile.STRLOG_Spacer + STRLOG_ErrorMessage + errorMessage;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return executeCommandInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetTubeHomeDistance()
        {
            return this.flexMotion.TubeHomeDistance;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetTubeMoveTime(int startDistance, int endDistance)
        {
            return this.flexMotion.GetTubeMoveTime(startDistance, endDistance);
        }

        //-------------------------------------------------------------------------------------------------//

        public char GetSourceHomeLocation()
        {
            return this.flexMotion.SourceHomeLocation;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetSourceSelectTime(char toLocation)
        {
            return this.flexMotion.GetSourceSelectTime(toLocation);
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetSourceReturnTime(char fromLocation)
        {
            return this.flexMotion.GetSourceReturnTime(fromLocation);
        }

        //-------------------------------------------------------------------------------------------------//

        public char GetAbsorberHomeLocation()
        {
            return this.flexMotion.AbsorberHomeLocation;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetAbsorberSelectTime(char toLocation)
        {
            return this.flexMotion.GetAbsorberSelectTime(toLocation);
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetAbsorberReturnTime(char fromLocation)
        {
            return this.flexMotion.GetAbsorberReturnTime(fromLocation);
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetLcdWriteLineTime()
        {
            return this.serialLcd.GetWriteLineTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetCaptureDataTime(int duration)
        {
            double captureDataTime = 0;
            if (this.radiationCounterType == RadiationCounterTypes.ST360)
            {
                captureDataTime = this.st360Counter.GetCaptureDataTime(duration);
            }
            else if (this.radiationCounterType == RadiationCounterTypes.Physics)
            {
                captureDataTime = this.physicsCounter.GetCaptureDataTime(duration);
            }
            return captureDataTime;
        }

    }
}
