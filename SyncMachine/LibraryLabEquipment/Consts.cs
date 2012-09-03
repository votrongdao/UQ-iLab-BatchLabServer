using System;

namespace Library.LabEquipment
{
    public class Consts
    {
        //
        // XML elements in the EquipmentConfig.xml file
        //
        public const string STRXML_dcDrive = "dcDrive";
        public const string STRXML_plc = "plc";
        public const string STRXML_network = "network";
        public const string STRXML_ipAddr = "ipAddr";
        public const string STRXML_ipPort = "ipPort";
        public const string STRXML_timeouts = "timeouts";
        public const string STRXML_receive = "receive";
        public const string STRXML_modbus = "modbus";
        public const string STRXML_slaveId = "slaveId";
        public const string STRXML_doInitialise = "doInitialise";
        public const string STRXML_initialiseDelay = "initialiseDelay";
        public const string STRXML_configuration = "configuration";
        public const string STRXML_openCircuitVaryField = "openCircuitVaryField";
        public const string STRXML_openCircuitVarySpeed = "openCircuitVarySpeed";
        public const string STRXML_shortCircuitVaryField = "shortCircuitVaryField";
        public const string STRXML_preSynchronisation = "preSynchronisation";
        public const string STRXML_synchronisation = "synchronisation";

        public const string STRXML_executionTimes = "executionTimes";
        public const string STRXML_initialise = "initialise";
        public const string STRXML_start = "start";
        public const string STRXML_run = "run";
        public const string STRXML_stop = "stop";
        public const string STRXML_finalise = "finalise";

        public const string STRXML_maxSyncFieldIncreases = "maxSyncFieldIncreases";
        public const string STRXML_maxMeasurements = "maxMeasurements";
        public const string STRXML_maxFieldCurrent = "maxFieldCurrent";
        public const string STRXML_maxStatorCurrent = "maxStatorCurrent";
        public const string STRXML_speed = "speed";
        public const string STRXML_speedTrim = "speedTrim";
        public const string STRXML_torque = "torque";
        public const string STRXML_low = "low";
        public const string STRXML_high = "high";
        public const string STRXML_min = "min";
        public const string STRXML_max = "max";
        public const string STRXML_step = "step";
        public const string STRXML_simpleMovingAverage = "simpleMovingAverage";
        public const string STRXML_phaseChangeLimit = "phaseChangeLimit";
        public const string STRXML_phaseSynchronism = "phaseSynchronism";
        public const string STRXML_speedSynchronism = "speedSynchronism";
        public const string STRXML_measurementCount = "measurementCount";

        public const string STRXML_measurements = "measurements";
        public const string STRXMLPARAM_units = "@units";
        public const string STRXMLPARAM_format = "@format";
        public const string STRXML_fieldCurrent = "fieldCurrent";
        public const string STRXML_count = "count";
        public const string STRXML_voltage = "voltage";
        public const string STRXML_frequency = "frequency";
        public const string STRXML_statorCurrent = "statorCurrent";
        public const string STRXML_speedSetpoint = "speedSetpoint";
        public const string STRXML_mainsVoltage = "mainsVoltage";
        public const string STRXML_mainsFrequency = "mainsFrequency";
        public const string STRXML_syncVoltage = "syncVoltage";
        public const string STRXML_syncFrequency = "syncFrequency";
        public const string STRXML_syncMainsPhase = "syncMainsPhase";
        public const string STRXML_synchronism = "synchronism";
        public const string STRXML_torqueSetpoint = "torqueSetpoint";
        public const string STRXML_powerFactor = "powerFactor";
        public const string STRXML_realPower = "realPower";
        public const string STRXML_reactivePower = "reactivePower";
        public const string STRXML_phaseCurrent = "phaseCurrent";

        public const char CHR_Splitter = ',';

        //
        // XML elements in the equipment request strings
        //
        public const string STRXML_ReqSpecification = "Specification";
        public const string STRXML_ExperimentSpecification = "experimentSpecification";
        public const string STRXML_SetupId = "setupId";
        public const string STRXML_SetupId_OpenCircuitVaryField = "OpenCircuitVaryField";
        public const string STRXML_SetupId_OpenCircuitVarySpeed = "OpenCircuitVarySpeed";
        public const string STRXML_SetupId_ShortCircuitVaryField = "ShortCircuitVaryField";
        public const string STRXML_SetupId_PreSynchronisation = "PreSynchronisation";
        public const string STRXML_SetupId_Synchronisation = "Synchronisation";

        //
        // XML elements in the equipment response strings
        //
        public const string STRXML_RspExecutionTime = "ExecutionTime";
        public const string STRXML_RspExecutionStatus = "ExecutionStatus";
        public const string STRXML_RspExecutionTimeRemaining = "ExecutionTimeRemaining";
        public const string STRXML_RspExecutionResultStatus = "ExecutionResultStatus";
        public const string STRXML_RspExecutionResults = "ExecutionResults";

        //
        // String constants
        //
        public const string STR_FieldCurrent = "FieldCurrent";
        public const string STR_Speed = "Speed";
        public const string STR_Voltage = "Voltage";
        public const string STR_StatorCurrent = "StatorCurrent";

    }
}
