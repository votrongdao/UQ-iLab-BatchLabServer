using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServer.Drivers.Module;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverSimActivity : DriverModuleGeneric
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverSimActivity";

        //
        // Local variables
        //
        private SimActivity simActivity;

        #endregion

        //---------------------------------------------------------------------------------------//

        public DriverSimActivity(Configuration configuration)
            : this(configuration, null, true)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverSimActivity(Configuration configuration, bool simulateDelays)
            : this(configuration, null, simulateDelays)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverSimActivity(Configuration configuration, CancelExperiment cancelExperiment)
            : this(configuration, cancelExperiment, true)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverSimActivity(Configuration configuration, CancelExperiment cancelExperiment, bool simulateDelays)
            : base(configuration, cancelExperiment)
        {
            //
            // Create an instance of the SimActivity class
            //
            this.simActivity = new SimActivity(configuration, simulateDelays);
        }

    }
}
