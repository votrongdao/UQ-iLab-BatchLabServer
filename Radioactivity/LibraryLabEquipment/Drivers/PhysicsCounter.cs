using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class PhysicsCounter
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "PhysicsCounter";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_StartingRadiationCounter = " Starting radiation counter...";
        private const string STRLOG_Success = " Success: ";
        private const string STRLOG_CaptureTimeout = " Capture timeout!";
        private const string STRLOG_InvalidData = " Invalid data!";

        //
        // String constants for error messages
        //
        private const string STRERR_NumberIsNegative = "Number cannot be negative!";
        private const string STRERR_NumberIsInvalid = "Number is invalid!";
        private const string STRERR_InitialiseDelayNotSpecified = "Initialise delay is not specified!";

        //
        // Local variables
        //
        private int initialiseDelay;
        private SerialLcd serialLcd;
        private FlexMotion flexMotion;

        public const int DELAY_CAPTURE_DATA = 1; // seconds

        #endregion

        #region Properties

        public const int DELAY_INITIALISE = 0;

        private double adjustDuration;

        /// <summary>
        /// Returns the time (in seconds) that it takes for the equipment to initialise.
        /// </summary>
        public int InitialiseDelay
        {
            get { return this.initialiseDelay; }
        }

        public double AdjustDuration
        {
            get { return this.adjustDuration; }
            set { this.adjustDuration = value; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public PhysicsCounter(XmlNode xmlNodeEquipmentConfig, SerialLcd serialLcd, FlexMotion flexMotion)
        {
            const string STRLOG_MethodName = "RadiationCounter";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.serialLcd = serialLcd;
            this.flexMotion = flexMotion;

            //
            // Get initialisation delay
            //
            XmlNode xmlNodePhysicsCounter = XmlUtilities.GetXmlNode(xmlNodeEquipmentConfig, Consts.STRXML_st360Counter);
            try
            {
                this.initialiseDelay = XmlUtilities.GetIntValue(xmlNodePhysicsCounter, Consts.STRXML_initialiseDelay);
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

        //---------------------------------------------------------------------------------------//

        public bool Initialise()
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success;

            //
            // Start the radiation counter
            //
            Logfile.Write(STRLOG_StartingRadiationCounter);
            success = this.flexMotion.StartCounter();

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetCaptureDataTime(int duration)
        {
            double seconds = 0.0;

            seconds = duration + this.adjustDuration;

            return seconds;
        }

        //---------------------------------------------------------------------------------------//

        public int CaptureData(int duration)
        {
            int data = -1;

            if (this.serialLcd != null)
            {
                this.serialLcd.StartCapture(duration);

                //
                // Use a timeout and retries
                //
                int retries = 3;
                for (int i = 0; i < retries; i++)
                {
                    //
                    // Check for data, but use a timeout
                    //
                    int timeout = (duration + 3) * 2;
                    while (true)
                    {
                        // Get capture data from serial LCD
                        data = this.serialLcd.CaptureData;

                        // Check if data received
                        if (data >= 0)
                        {
                            // Data received
                            break;
                        }

                        // Not data yet, check timeout
                        if (--timeout == 0)
                        {
                            // Timed out
                            break;
                        }

                        //
                        // Wait for data
                        //
                        Thread.Sleep(500);
                        Trace.Write(".");
                    }

                    if (timeout == 0)
                    {
                        Logfile.Write(STRLOG_CaptureTimeout);
                    }
                    else if (data < 0)
                    {
                        Logfile.Write(STRLOG_InvalidData);
                    }
                    else
                    {
                        // Data captured successfully
                        break;
                    }
                }

                this.serialLcd.StopCapture();
            }

            return data;
        }

    }
}
