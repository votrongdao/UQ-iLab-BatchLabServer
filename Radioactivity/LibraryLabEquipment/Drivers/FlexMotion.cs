using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment.Drivers
{
    public class FlexMotion
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "FlexMotion";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_NotInitialised = "Not Initialised!";
        private const string STRLOG_FlexMotionBoardID = "FlexMotionBoardID: ";
        private const string STRLOG_TubeInfo_arg4 = "Tube - AxisId: {0}  OffsetDistance: {1}  HomeDistance: {2}  MoveRate: {3}";
        private const string STRLOG_SourceInfo_arg4 = "Source - AxisId: {0}  FirstLocation: {1}  LastLocation: {2}  HomeLocation: {3}";
        private const string STRLOG_AbsorberInfo_arg4 = "Absorber - AxisId: {0}  FirstLocation: {1}  LastLocation: {2}  HomeLocation: {3}";
        protected const string STRLOG_InitialiseDelay = "Initialise Delay: ";
        protected const string STRLOG_Distance = "Distance: ";
        protected const string STRLOG_Location = "Location: ";
        protected const string STRLOG_Success = "Success: ";

        //
        // String constants for exception messages
        //
        private const string STRERR_NumberIsNegative = "Number cannot be negative!";
        private const string STRERR_NumberIsInvalid = "Number is invalid!";
        private const string STRERR_InitialiseDelayNotSpecified = "Initialise delay is not specified!";
        protected const string STRERR_FailedToInitialise = "Failed to initialise!";

        //
        // String constants for error messages
        //

        //
        // Local variables
        //
        protected string lastError;
        protected byte boardID;
        protected int tubeAxisId;
        protected int sourceAxisId;
        protected int absorberAxisId;
        protected bool hasAbsorberTable;
        protected int tubeOffsetDistance;
        protected double tubeMoveRate;
        protected bool tubeInitAxis;

        protected struct AxisInfo
        {
            public int[] encoderPositions;
            public double[] selectTimes;
            public double[] returnTimes;
        }

        protected AxisInfo sourceAxisInfo;
        protected AxisInfo absorberAxisInfo;

        #endregion

        #region Properties

        protected bool online;
        protected string statusMessage;
        protected int initialiseDelay;
        protected int tubeHomeDistance;
        protected char sourceFirstLocation;
        protected char sourceLastLocation;
        protected char sourceHomeLocation;
        protected char absorberFirstLocation;
        protected char absorberLastLocation;
        protected char absorberHomeLocation;

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

        /// <summary>
        /// Returns the time (in seconds) that it takes for the equipment to power-up.
        /// </summary>
        //public int PowerupDelay
        //{
        //    get { return this.powerupDelay; }
        //}

        /// <summary>
        /// Returns the time (in seconds) that it takes for the equipment to initialise.
        /// </summary>
        public int InitialiseDelay
        {
            get { return this.initialiseDelay; }
        }

        public int TubeHomeDistance
        {
            get { return this.tubeHomeDistance; }
        }

        public char SourceFirstLocation
        {
            get { return this.sourceFirstLocation; }
        }

        public char SourceLastLocation
        {
            get { return this.sourceLastLocation; }
        }

        public char SourceHomeLocation
        {
            get { return this.sourceHomeLocation; }
        }

        public char AbsorberFirstLocation
        {
            get { return this.absorberFirstLocation; }
        }

        public char AbsorberLastLocation
        {
            get { return this.absorberLastLocation; }
        }

        public char AbsorberHomeLocation
        {
            get { return this.absorberHomeLocation; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        public FlexMotion(XmlNode xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "FlexMotion";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.lastError = string.Empty;

            //
            // Initialise properties
            //
            this.online = false;
            this.statusMessage = STRLOG_NotInitialised;

            //
            // Get NI FlexMotion controller card board ID
            //
            XmlNode xmlNodeFlexMotion = XmlUtilities.GetXmlNode(xmlNodeEquipmentConfig, Consts.STRXML_flexMotion);
            this.boardID = (byte)XmlUtilities.GetIntValue(xmlNodeFlexMotion, Consts.STRXML_boardID);
            Logfile.Write(STRLOG_FlexMotionBoardID + boardID.ToString());

            //
            // Initialise tube settings
            //
            XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodeFlexMotion, Consts.STRXML_tube);
            this.tubeAxisId = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_axisId);
            this.tubeOffsetDistance = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_offsetDistance);
            this.tubeHomeDistance = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_homeDistance);
            this.tubeMoveRate = XmlUtilities.GetRealValue(xmlNode, Consts.STRXML_moveRate);
            this.tubeInitAxis = XmlUtilities.GetBoolValue(xmlNode, Consts.STRXML_initAxis, false);
            Logfile.Write(
                String.Format(STRLOG_TubeInfo_arg4, this.tubeAxisId, this.tubeOffsetDistance, this.tubeHomeDistance, this.tubeMoveRate));

            //
            // Initialise source settings
            //
            xmlNode = XmlUtilities.GetXmlNode(xmlNodeFlexMotion, Consts.STRXML_sources);
            this.sourceAxisId = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_axisId);
            this.sourceFirstLocation = XmlUtilities.GetCharValue(xmlNode, Consts.STRXML_firstLocation);
            this.sourceHomeLocation = XmlUtilities.GetCharValue(xmlNode, Consts.STRXML_homeLocation);

            //
            // Initialise source encoder positions array
            //
            string sourceEncoderPositions = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_encoderPositions, false);
            string[] strSplit = sourceEncoderPositions.Split(new char[] { Engine.Consts.CHR_CsvSplitterChar });
            this.sourceAxisInfo.encoderPositions = new int[strSplit.Length];
            for (int i = 0; i < strSplit.Length; i++)
            {
                this.sourceAxisInfo.encoderPositions[i] = Int32.Parse(strSplit[i]);
            }

            //
            // Initialise source select times array
            //
            string sourceSelectTimes = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_selectTimes, false);
            strSplit = sourceSelectTimes.Split(new char[] { Engine.Consts.CHR_CsvSplitterChar });
            this.sourceAxisInfo.selectTimes = new double[strSplit.Length];
            for (int i = 0; i < strSplit.Length; i++)
            {
                this.sourceAxisInfo.selectTimes[i] = Double.Parse(strSplit[i]);
            }
            this.sourceLastLocation = (char)(this.sourceFirstLocation + this.sourceAxisInfo.selectTimes.Length - 1);

            //
            // Initialise source return times array
            //
            string sourceReturnTimes = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_returnTimes, false);
            strSplit = sourceReturnTimes.Split(new char[] { Engine.Consts.CHR_CsvSplitterChar });
            this.sourceAxisInfo.returnTimes = new double[strSplit.Length];
            for (int i = 0; i < strSplit.Length; i++)
            {
                this.sourceAxisInfo.returnTimes[i] = Double.Parse(strSplit[i]);
            }

            Logfile.Write(
                String.Format(STRLOG_SourceInfo_arg4, this.sourceAxisId, this.sourceFirstLocation, this.sourceLastLocation, this.sourceHomeLocation));

            try
            {
                //
                // Initialise absorber settings
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeFlexMotion, Consts.STRXML_absorbers);
                this.absorberAxisId = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_axisId);
                this.absorberFirstLocation = XmlUtilities.GetCharValue(xmlNode, Consts.STRXML_firstLocation);
                this.absorberHomeLocation = XmlUtilities.GetCharValue(xmlNode, Consts.STRXML_homeLocation);

                //
                // Initialise absorber encoder positions array
                //
                string absorberEncoderPositions = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_encoderPositions, false);
                strSplit = absorberEncoderPositions.Split(new char[] { Engine.Consts.CHR_CsvSplitterChar });
                this.absorberAxisInfo.encoderPositions = new int[strSplit.Length];
                for (int i = 0; i < strSplit.Length; i++)
                {
                    this.absorberAxisInfo.encoderPositions[i] = Int32.Parse(strSplit[i]);
                }

                //
                // Initialise absorber select times array
                //
                string absorberSelectTimes = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_selectTimes, false);
                strSplit = absorberSelectTimes.Split(new char[] { Engine.Consts.CHR_CsvSplitterChar });
                this.absorberAxisInfo.selectTimes = new double[strSplit.Length];
                for (int i = 0; i < strSplit.Length; i++)
                {
                    this.absorberAxisInfo.selectTimes[i] = Double.Parse(strSplit[i]);
                }
                this.absorberLastLocation = (char)(this.absorberFirstLocation + this.absorberAxisInfo.selectTimes.Length - 1);

                //
                // Initialise absorber return times array
                //
                string absorberReturnTimes = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_returnTimes, false);
                strSplit = absorberReturnTimes.Split(new char[] { Engine.Consts.CHR_CsvSplitterChar });
                this.absorberAxisInfo.returnTimes = new double[strSplit.Length];
                for (int i = 0; i < strSplit.Length; i++)
                {
                    this.absorberAxisInfo.returnTimes[i] = Double.Parse(strSplit[i]);
                }

                Logfile.Write(
                    String.Format(STRLOG_AbsorberInfo_arg4, this.absorberAxisId, this.absorberFirstLocation, this.absorberLastLocation, this.absorberHomeLocation));

                //
                // There is an absorber table
                //
                this.hasAbsorberTable = true;
            }
            catch
            {
                // No absorber table
                this.absorberHomeLocation = this.sourceHomeLocation;
            }

            //
            // Get initialisation delay
            //
            try
            {
                this.initialiseDelay = XmlUtilities.GetIntValue(xmlNodeFlexMotion, Consts.STRXML_initialiseDelay);
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

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public string GetLastError()
        {
            string lastError = this.lastError;
            this.lastError = string.Empty;
            return lastError;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool Initialise()
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            //
            // Initialisation is complete
            //
            this.online = true;
            this.statusMessage = StatusCodes.Ready.ToString();
            success = true;

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool EnablePower()
        {
            const string STRLOG_MethodName = "EnablePower";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool DisablePower()
        {
            const string STRLOG_MethodName = "DisablePower";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool PowerInitialise()
        {
            const string STRLOG_MethodName = "PowerInitialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool StartCounter()
        {
            const string STRLOG_MethodName = "StartCounter";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public double GetTubeMoveTime(int startDistance, int endDistance)
        {
            // Get absolute distance
            int distance = endDistance - startDistance;
            if (distance < 0)
            {
                distance = -distance;
            }

            // Tube move rate is in ms per mm
            double seconds = distance * this.tubeMoveRate;

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetSourceSelectTime(char toLocation)
        {
            double seconds = 0;

            int index = toLocation - this.sourceFirstLocation;
            if (index >= 0 && index < this.sourceAxisInfo.selectTimes.Length)
            {
                seconds = this.sourceAxisInfo.selectTimes[index];
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetSourceReturnTime(char fromLocation)
        {
            double seconds = 0;

            int index = fromLocation - this.sourceFirstLocation;
            if (index >= 0 && index < this.sourceAxisInfo.returnTimes.Length)
            {
                seconds = this.sourceAxisInfo.returnTimes[index];
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetAbsorberSelectTime(char toLocation)
        {
            double seconds = 0;

            if (this.hasAbsorberTable == true)
            {
                int index = toLocation - this.absorberFirstLocation;
                if (index >= 0 && index < this.absorberAxisInfo.selectTimes.Length)
                {
                    seconds = this.absorberAxisInfo.selectTimes[index];
                }
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetAbsorberReturnTime(char fromLocation)
        {
            double seconds = 0;

            if (this.hasAbsorberTable == true)
            {
                int index = fromLocation - this.absorberFirstLocation;
                if (index >= 0 && index < this.absorberAxisInfo.returnTimes.Length)
                {
                    if (this.hasAbsorberTable == true)
                    {
                        seconds = this.absorberAxisInfo.returnTimes[index];
                    }
                }
            }

            return seconds;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool SetTubeDistance(int targetDistance)
        {
            const string STRLOG_MethodName = "SetTubeDistance";

            string logMessage = STRLOG_Distance + targetDistance.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool SetSourceLocation(char location)
        {
            const string STRLOG_MethodName = "SetSourceLocation";

            string logMessage = STRLOG_Location + location.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool SetAbsorberLocation(char location)
        {
            const string STRLOG_MethodName = "SetAbsorberLocation";

            string logMessage = STRLOG_Location + location.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

    }
}
