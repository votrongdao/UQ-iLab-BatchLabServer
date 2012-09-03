using System;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class SerialLcdNone : SerialLcd
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "SerialLcdNone";

        //
        // String constants
        //
        private const string STR_NoHardware = "No Hardware!";

        //
        // Local variables
        //

        #endregion

        //---------------------------------------------------------------------------------------//

        public SerialLcdNone(XmlNode xmlNodeEquipmentConfig)
            : base(xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "SerialLcdNone";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise properties
            //
            this.initialiseDelay = 0;

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        public override bool Initialise()
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public override string GetHardwareFirmwareVersion()
        {
            return STR_NoHardware;
        }

        //---------------------------------------------------------------------------------------//

        public override bool WriteLine(int lineno, string message)
        {
            return true;
        }

        //---------------------------------------------------------------------------------------//

        public override bool StartCapture(int seconds)
        {
            this.CaptureData = seconds;
            return true;
        }

        //---------------------------------------------------------------------------------------//

        public override bool StopCapture()
        {
            return true;
        }

    }
}
