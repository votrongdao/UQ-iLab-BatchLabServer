using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverSynchronousSpeed : DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverSynchronousSpeed";

        //
        // Local variables
        //
        private int measurementCount;

        #endregion

        //---------------------------------------------------------------------------------------//

        public DriverSynchronousSpeed(EquipmentService equipmentServiceProxy, Configuration configuration)
            : this(equipmentServiceProxy, configuration, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverSynchronousSpeed(EquipmentService equipmentServiceProxy, Configuration configuration, CancelExperiment cancelExperiment)
            : base(equipmentServiceProxy, configuration, cancelExperiment)
        {
            const string STRLOG_MethodName = "DriverSynchronousSpeed";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Save for use by this driver
            //
            this.measurementCount = configuration.MeasurementCount;

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

    }
}
