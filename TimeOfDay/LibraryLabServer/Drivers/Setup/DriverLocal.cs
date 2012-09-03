using System;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServer.Drivers.Module;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverLocal : DriverModuleGeneric
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverLocal";

        //
        // Local variables
        //
        private TimeOfDay timeOfDay;

        #endregion

        //---------------------------------------------------------------------------------------//

        public DriverLocal(Configuration configuration)
            : this(configuration, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverLocal(Configuration configuration, CancelExperiment cancelExperiment)
            : base(configuration, cancelExperiment)
        {
            //
            // Create an instance of the TimeOfDay class
            //
            this.timeOfDay = new TimeOfDay(configuration);
        }

    }
}
