using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServer.Drivers.Module;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverSimulation : DriverModuleGeneric
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverSimulation";

        //
        // Local variables
        //
        private Simulation simulation;

        #endregion

        //---------------------------------------------------------------------------------------//

        public DriverSimulation(Configuration configuration)
            : this(configuration, null, true)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverSimulation(Configuration configuration, bool simulateDelays)
            : this(configuration, null, simulateDelays)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverSimulation(Configuration configuration, CancelExperiment cancelExperiment)
            : this(configuration, cancelExperiment, true)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverSimulation(Configuration configuration, CancelExperiment cancelExperiment, bool simulateDelays)
            : base(configuration, cancelExperiment)
        {
            //
            // Create an instance of the Simulation class
            //
            this.simulation = new Simulation(configuration, simulateDelays);
        }

    }
}
