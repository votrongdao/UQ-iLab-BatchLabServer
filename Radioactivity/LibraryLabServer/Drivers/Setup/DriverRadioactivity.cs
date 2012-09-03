using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverRadioactivity : DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverRadioactivity";

        #endregion

        //---------------------------------------------------------------------------------------//

        public DriverRadioactivity(EquipmentService equipmentServiceProxy, Configuration configuration)
            : this(equipmentServiceProxy, configuration, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverRadioactivity(EquipmentService equipmentServiceProxy, Configuration configuration, CancelExperiment cancelExperiment)
            : base(equipmentServiceProxy, configuration, cancelExperiment)
        {
            const string STRLOG_MethodName = "DriverRadioactivity";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Nothing to do here
            //

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

    }
}
