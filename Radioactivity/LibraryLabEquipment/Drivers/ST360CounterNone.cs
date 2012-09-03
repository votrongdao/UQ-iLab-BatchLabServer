using System;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class ST360CounterNone : ST360Counter
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ST360CounterNone";

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ST360CounterNone(XmlNode xmlNodeEquipmentConfig)
            : base(xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "ST360CounterNone";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise properties
            //
            this.initialiseDelay = 0;

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public override bool Initialise(bool configure)
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            string logMessage = STRLOG_Online + this.online.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected new bool Configure()
        {
            const string STRLOG_MethodName = "Configure";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

    }
}
