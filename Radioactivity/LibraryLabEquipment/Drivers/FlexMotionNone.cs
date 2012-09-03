using System;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class FlexMotionNone : FlexMotion
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "FlexMotionNone";

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public FlexMotionNone(XmlNode xmlNodeEquipmentConfig)
            : base(xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "FlexMotionNone";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise properties
            //
            //this.powerupDelay = 0;
            this.initialiseDelay = 0;

            //
            // Initialise the flexmotion controller
            //
            if (this.Initialise() == false)
            {
                throw new Exception(STRERR_FailedToInitialise);
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        public override bool Initialise()
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

    }
}
