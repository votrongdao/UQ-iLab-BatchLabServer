using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverVoltageVsSpeed : DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverVoltageVsSpeed";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_SpeedMin = " Speed Min: ";
        private const string STRLOG_SpeedMax = " Speed Max: ";
        private const string STRLOG_SpeedStep = " Speed Step: ";

        //
        // String constants for error messages
        //

        //
        // Local variables
        //
        private int measurementCount;

        #endregion

        //---------------------------------------------------------------------------------------//

        public DriverVoltageVsSpeed(EquipmentService equipmentServiceProxy, Configuration configuration)
            : this(equipmentServiceProxy, configuration, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverVoltageVsSpeed(EquipmentService equipmentServiceProxy, Configuration configuration, CancelExperiment cancelExperiment)
            : base(equipmentServiceProxy, configuration, cancelExperiment)
        {
            const string STRLOG_MethodName = "DriverVoltageVsSpeed";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Save for use by this driver
            //
            this.measurementCount = configuration.MeasurementCount;

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

    }
}
