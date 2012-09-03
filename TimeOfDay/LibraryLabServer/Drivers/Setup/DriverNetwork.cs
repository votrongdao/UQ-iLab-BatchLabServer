using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverNetwork : DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverNetwork";

        #endregion

        //---------------------------------------------------------------------------------------//

        public DriverNetwork(EquipmentService equipmentServiceProxy, Configuration configuration)
            : this(equipmentServiceProxy, configuration, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverNetwork(EquipmentService equipmentServiceProxy, Configuration configuration, CancelExperiment cancelExperiment)
            : base(equipmentServiceProxy, configuration, cancelExperiment)
        {
            const string STRLOG_MethodName = "DriverNetwork";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // YOUR CODE HERE
            //

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

    }
}
