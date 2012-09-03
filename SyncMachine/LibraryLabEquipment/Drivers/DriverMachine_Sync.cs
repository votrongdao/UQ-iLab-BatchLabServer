using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class DriverMachine_Sync : DriverMachine
    {
        #region Constants

        private const string STRLOG_ClassName = "DriverMachine_Sync";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Speed = " Speed: ";
        private const string STRLOG_SpeedTrimLow = " SpeedTrim Low: ";
        private const string STRLOG_SpeedTrimHigh = " SpeedTrim High: ";
        private const string STRLOG_MaxFieldCurrent = " MaxFieldCurrent: ";
        private const string STRLOG_MaxSyncFieldIncreases = " MaxSyncFieldIncreases: ";
        private const string STRLOG_MaxMeasurements = " MaxMeasurements: ";
        private const string STRLOG_SimpleMovingAverage = " SimpleMovingAverage: ";
        private const string STRLOG_PhaseChangeLimitLow = " PhaseChangeLimitLow: ";
        private const string STRLOG_PhaseChangeLimitHigh = " PhaseChangeLimitHigh: ";
        private const string STRLOG_PhaseSynchronismLow = " PhaseSynchronismLow: ";
        private const string STRLOG_PhaseSynchronismHigh = " PhaseSynchronismHigh: ";
        private const string STRLOG_SpeedSynchronism = " SpeedSynchronism: ";
        private const string STRLOG_TorqueSetpoint = " TorqueSetpoint: ";
        private const string STRLOG_MinTorque = " MinTorque: ";
        private const string STRLOG_MaxTorque = " MaxTorque: ";
        private const string STRLOG_TorqueStep = " TorqueStep: ";
        private const string STRLOG_PowerFactorLow = " PowerFactorLow: ";
        private const string STRLOG_PowerFactorHigh = " PowerFactorHigh: ";

        private const string STRLOG_CallerState = " CallerState: ";
        private const string STRLOG_FieldCurrentValueUnits_Fmt = " FieldCurrent: {0:f03} {1}";
        private const string STRLOG_UnableToIncreaseFieldCurrent = "Unable to increase field current";
        private const string STRLOG_SyncVoltageAboveMainsVoltage = " Sync voltage is higher than mains voltage.";
        private const string STRLOG_SyncFrequencyMatchesMainsFrequency = " Sync frequency matches mains frequency.";
        private const string STRLOG_MeasurementsForVoltagesValueUnits_Fmt = " FieldCurrent: {0:f03} {1} - Sync Voltage: {2:f01} {3} - Mains Voltage: {4:f01} {5}";
        private const string STRLOG_MeasurementsForFrequenciesValueUnits_Fmt = " Sync Frequency: {0:f02} {1} - Mains Frequency: {2:f02} {3}";
        private const string STRLOG_MeasurementsForSynchronismValueUnits_Fmt = " SyncToMainsPhase: {0:f01} {1} - Synchronism: {2}";
        private const string STRLOG_MeasurementsForTorqueValueUnits_Fmt = " FieldCurrent: {0:f03} {1} - Sync Voltage: {2:f01} {3} - Sync Frequency: {4:f01} {5}";
        private const string STRLOG_MeasurementsPowerMeterValueUnits_Fmt = " PowerFactor: {0:f03} - Real Power: {1:f0} {2} - Reactive Power: {3:f0} {4} - Phase Current: {5:f02} {6}";
        private const string STRLOG_UnableToDecreaseFieldCurrent = "Unable to decrease field current";
        private const string STRLOG_MaxTorqueReached = "Maximum torque has been reached: ";
        private const string STRLOG_MinTorqueReached = "Minimum torque has been reached: ";

        //
        // String constants for error messages
        //
        private const string STRERR_MaxSyncFieldIncreasesReached = "Maximum sync field increases have been reached: ";
        private const string STRERR_MaxFieldCurrentReached = "Maximum field current has been reached: ";
        private const string STRERR_MinSpeedTrimReached = "Minimum speed trim has been reached: ";
        private const string STRERR_MaxSpeedTrimReached = "Maximum speed trim has been reached: ";
        private const string STRERR_SynchronismFailed = "Synchronism failed after several attempts! ";

        #endregion

        #region Types

        private struct ConfigParameters
        {
            public int speed;
            public int speedTrimLow;
            public int speedTrimHigh;
            public double maxFieldCurrent;
            public int maxSyncFieldIncreases;
            public int maxMeasurements;
            public int simpleMovingAverage;
            public int phaseChangeLimitLow;
            public int phaseChangeLimitHigh;
            public int phaseSynchronismLow;
            public int phaseSynchronismHigh;
            public int speedSynchronism;
            public int torqueMin;
            public int torqueMax;
            public int torqueStep;
            public double powerFactorLow;
            public double powerFactorHigh;
        }

        public struct Measurement
        {
            public double fieldCurrent;
            public double mainsVoltage;
            public double syncVoltage;
            public double syncFrequency;
            public double syncMainsPhase;
            public bool synchronism;
            public double torqueSetpoint;
            public double powerFactor;
            public double realPower;
            public double reactivePower;
            public double phaseCurrent;
        }

        public struct MeasurementUnits
        {
            public string fieldCurrent;
            public string mainsVoltage;
            public string syncVoltage;
            public string syncFrequency;
            public string syncMainsPhase;
            public string torqueSetpoint;
            public string realPower;
            public string reactivePower;
            public string phaseCurrent;
        }

        public class Measurements
        {
            public List<Measurement> valueList;
            public MeasurementUnits units;

            public Measurements()
            {
                this.valueList = new List<Measurement>();
                this.units = new MeasurementUnits();
            }
        }

        private enum States
        {
            // Common
            CheckStatus, Done,

            // ExecuteInitialising
            OpenConnectionDCDrive, OpenConnectionPLC, InitialisePLC, InitialiseDCDrive,

            // ExecuteStarting
            EnableDCDrive, StartDCDrive, CheckDCDriveStatus, EnableSyncField,
            IncreaseSyncField, CheckSyncFieldCurrent, TakeMeasurementsForVoltages, CheckSyncMainsVoltages,
            EnableSyncCheck, TakeMeasurementsForSynchronism, CheckSpeedForSynchronism, TrySynchronism,
            InSynchronism,

            // ExecuteRunning
            IncreaseTorque, TakeMeasurementsForIncreasedTorque, CheckPowerFactorForIncreasedTorque,
            IncreaseSyncFieldForIncreasedTorque, TakeMeasurementsForIncreasedSyncField,
            DecreaseTorque, TakeMeasurementsForDecreasedTorque, CheckPowerFactorForDecreasedTorque,
            DecreaseSyncFieldForDecreasedTorque, TakeMeasurementsForDecreasedSyncField,

            // TakeMeasurements
            MeasureFieldCurrent, MeasureSyncVoltage, MeasureSyncFrequency,
            MeasureMainsVoltage, MeasureMainsFrequency, MeasureSyncToMainsPhase, MeasureSynchronismStatus,
            MeasureTorqueSetpoint, MeasurePowerFactor, MeasureRealPower, MeasureReactivePower, MeasurePhaseCurrent,
        }

        #endregion

        #region Variables

        private ConfigParameters configParameters;
        private XmlNode xmlNodeMeasurementsTemplate;
        private Measurements measurements;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public DriverMachine_Sync(XmlNode xmlNodeEquipmentConfig, Specification specification)
            : base(xmlNodeEquipmentConfig, specification)
        {
            const string STRLOG_MethodName = "DriverMachine_Sync";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.measurements = null;

            try
            {
                //
                // Get parameters for this driver
                //
                XmlNode xmlNodeConfiguration = XmlUtilities.GetXmlNode(xmlNodeEquipmentConfig, Consts.STRXML_configuration, false);
                XmlNode xmlNodeSynchronisation = XmlUtilities.GetXmlNode(xmlNodeConfiguration, Consts.STRXML_synchronisation, false);

                //
                // Execution time
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodeSynchronisation, Consts.STRXML_executionTimes);
                this.executionTimes.initialise = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_initialise);
                this.executionTimes.start = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_start);
                this.executionTimes.run = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_run);
                this.executionTimes.stop = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_stop);
                this.executionTimes.finalise = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_finalise);
                Logfile.Write(String.Format(STRLOG_ExecuteTimes_Fmt, this.executionTimes.initialise,
                    this.executionTimes.start, this.executionTimes.run,
                    this.executionTimes.stop, this.executionTimes.finalise
                    ));

                //
                // Speed and speed trim
                //
                this.configParameters.speed = XmlUtilities.GetIntValue(xmlNodeSynchronisation, Consts.STRXML_speed);
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeSynchronisation, Consts.STRXML_speedTrim);
                this.configParameters.speedTrimLow = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_low);
                this.configParameters.speedTrimHigh = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_high);
                Logfile.Write(STRLOG_Speed + this.configParameters.speed.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_SpeedTrimLow + this.configParameters.speedTrimLow.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_SpeedTrimHigh + this.configParameters.speedTrimHigh.ToString());

                //
                // Maximum field current, sync field increases and measurements
                //
                this.configParameters.maxFieldCurrent = XmlUtilities.GetRealValue(xmlNodeSynchronisation, Consts.STRXML_maxFieldCurrent);
                this.configParameters.maxSyncFieldIncreases = XmlUtilities.GetIntValue(xmlNodeSynchronisation, Consts.STRXML_maxSyncFieldIncreases);
                this.configParameters.maxMeasurements = XmlUtilities.GetIntValue(xmlNodeSynchronisation, Consts.STRXML_maxMeasurements);
                Logfile.Write(STRLOG_MaxFieldCurrent + this.configParameters.maxFieldCurrent.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_MaxSyncFieldIncreases + this.configParameters.maxSyncFieldIncreases.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_MaxMeasurements + this.configParameters.maxMeasurements.ToString());

                //
                // Speed adjustment for phase change between measurements
                //
                this.configParameters.simpleMovingAverage = XmlUtilities.GetIntValue(xmlNodeSynchronisation, Consts.STRXML_simpleMovingAverage);
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeSynchronisation, Consts.STRXML_phaseChangeLimit);
                this.configParameters.phaseChangeLimitLow = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_low);
                this.configParameters.phaseChangeLimitHigh = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_high);
                Logfile.Write(STRLOG_SimpleMovingAverage + this.configParameters.simpleMovingAverage.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_PhaseChangeLimitLow + this.configParameters.phaseChangeLimitLow.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_PhaseChangeLimitHigh + this.configParameters.phaseChangeLimitHigh.ToString());

                //
                // Phase window to try synchronism
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeSynchronisation, Consts.STRXML_phaseSynchronism);
                this.configParameters.phaseSynchronismLow = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_low);
                this.configParameters.phaseSynchronismHigh = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_high);
                Logfile.Write(STRLOG_PhaseSynchronismLow + this.configParameters.phaseSynchronismLow.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_PhaseSynchronismHigh + this.configParameters.phaseSynchronismHigh.ToString());

                //
                // Speed for synchronism
                //
                this.configParameters.speedSynchronism = XmlUtilities.GetIntValue(xmlNodeSynchronisation, Consts.STRXML_speedSynchronism);
                Logfile.Write(STRLOG_SpeedSynchronism + this.configParameters.speedSynchronism.ToString());

                //
                // Torque range
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeSynchronisation, Consts.STRXML_torque);
                this.configParameters.torqueMin = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_min);
                this.configParameters.torqueMax = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_max);
                this.configParameters.torqueStep = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_step);
                Logfile.Write(STRLOG_MinTorque + this.configParameters.torqueMin.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_MaxTorque + this.configParameters.torqueMax.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_TorqueStep + this.configParameters.torqueStep.ToString());

                //
                // Power factor to increase or decrease sync field
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeSynchronisation, Consts.STRXML_powerFactor);
                this.configParameters.powerFactorLow = XmlUtilities.GetRealValue(xmlNode, Consts.STRXML_low);
                this.configParameters.powerFactorHigh = XmlUtilities.GetRealValue(xmlNode, Consts.STRXML_high);
                Logfile.Write(STRLOG_PowerFactorLow + this.configParameters.powerFactorLow.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_PowerFactorHigh + this.configParameters.powerFactorHigh.ToString());

                //
                // Load the XML measurements template and check that all required XML nodes exist
                //
                this.xmlNodeMeasurementsTemplate = XmlUtilities.GetXmlNode(xmlNodeSynchronisation, Consts.STRXML_measurements);

                //
                // Check that all required XML nodes exist
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_count, true);

                //
                // Field current
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_fieldCurrent, true);
                XmlNode xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_fieldCurrent);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Sync voltage
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_syncVoltage, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_syncVoltage);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Sync frequency
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_syncFrequency, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_syncFrequency);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Torque set point
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_torqueSetpoint, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_torqueSetpoint);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Power factor
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_powerFactor, true);

                //
                // Real power
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_realPower, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_realPower);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Reactive power
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_reactivePower, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_reactivePower);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Phase current
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_phaseCurrent, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_phaseCurrent);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);
            }
            catch (Exception ex)
            {
                //
                // Log the message and throw the exception back to the caller
                //
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public override string GetExecutionResults()
        {
            const string STRLOG_MethodName = "GetExecutionResults";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            string xmlExecutionResults = string.Empty;

            //
            // Check that there are measurements to return
            //
            if (this.executionResultStatus == ExecutionStatus.Completed)
            {
                try
                {
                    //
                    // Take a copy of the XML measurements template
                    //
                    XmlNode xmlNodeMeasurements = this.xmlNodeMeasurementsTemplate.Clone();

                    //
                    // Convert measurement list to arrays
                    //
                    float[] fieldCurrent = new float[this.measurements.valueList.Count];
                    float[] syncVoltage = new float[this.measurements.valueList.Count];
                    float[] syncFrequency = new float[this.measurements.valueList.Count];
                    float[] torqueSetpoint = new float[this.measurements.valueList.Count];
                    float[] powerFactor = new float[this.measurements.valueList.Count];
                    float[] realPower = new float[this.measurements.valueList.Count];
                    float[] reactivePower = new float[this.measurements.valueList.Count];
                    float[] phaseCurrent = new float[this.measurements.valueList.Count];
                    for (int i = 0; i < this.measurements.valueList.Count; i++)
                    {
                        fieldCurrent[i] = (float)this.measurements.valueList[i].fieldCurrent;
                        syncVoltage[i] = (float)this.measurements.valueList[i].syncVoltage;
                        syncFrequency[i] = (float)this.measurements.valueList[i].syncFrequency;
                        torqueSetpoint[i] = (float)this.measurements.valueList[i].torqueSetpoint;
                        powerFactor[i] = (float)this.measurements.valueList[i].powerFactor;
                        realPower[i] = (float)this.measurements.valueList[i].realPower;
                        reactivePower[i] = (float)this.measurements.valueList[i].reactivePower;
                        phaseCurrent[i] = (float)this.measurements.valueList[i].phaseCurrent;
                    }

                    //
                    // Fill in the measurements and units
                    //
                    XmlUtilities.SetXmlValue(xmlNodeMeasurements, Consts.STRXML_count, this.measurements.valueList.Count);

                    //
                    // Field current
                    //
                    XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_fieldCurrent);
                    string strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_fieldCurrent, fieldCurrent, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.fieldCurrent, false);

                    //
                    // Sync voltage
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_syncVoltage);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_syncVoltage, syncVoltage, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.syncVoltage, false);

                    //
                    // Sync frequency
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_syncFrequency);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_syncFrequency, syncFrequency, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.syncFrequency, false);

                    //
                    // Torque setpoint
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_torqueSetpoint);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_torqueSetpoint, torqueSetpoint, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.torqueSetpoint, false);

                    //
                    // Power factor
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_powerFactor);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_powerFactor, powerFactor, strFormat, Consts.CHR_Splitter, false);

                    //
                    // Real power
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_realPower);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_realPower, realPower, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.realPower, false);

                    //
                    // Reactive power
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_reactivePower);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_reactivePower, reactivePower, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.reactivePower, false);

                    //
                    // Phase current
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_phaseCurrent);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_phaseCurrent, phaseCurrent, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.phaseCurrent, false);

                    //
                    // Write the XML measurements to a string
                    //
                    xmlExecutionResults = XmlUtilities.ToXmlString(xmlNodeMeasurements);

                    //
                    // The execution results are no longer available once they have been retrieved
                    //
                    this.executionResultStatus = ExecutionStatus.None;
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                    xmlExecutionResults = null;
                }
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return xmlExecutionResults;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteInitialising()
        {
            const string STRLOG_MethodName = "ExecuteInitialising";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            string logMessage;

            //
            // Get the time now to determine how long execution actually takes
            //
            DateTime startDateTime = DateTime.Now;

            //
            // Calculate execution completion time
            //
            int executionTimeRemaining = 0;
            executionTimeRemaining += this.executionTimes.initialise;
            executionTimeRemaining += this.executionTimes.start;
            executionTimeRemaining += this.executionTimes.run;
            executionTimeRemaining += this.executionTimes.stop;
            executionTimeRemaining += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTimeRemaining);
            Logfile.Write(
                String.Format(STRLOG_ExecutionTimeRemaining_arg, executionTimeRemaining));

            //
            // Initialise state machine
            //
            this.lastError = null;
            States state = States.OpenConnectionDCDrive;

            //
            // State machine loop
            //
            while (state != States.Done)
            {
                Trace.WriteLine("state: " + state.ToString());

                switch (state)
                {
                    case States.OpenConnectionPLC:
                        //
                        // Open network connection to the PLC
                        //
                        if ((success = this.plc.OpenConnection()) == true)
                        {
                            state = States.CheckStatus;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.OpenConnectionDCDrive:
                        //
                        // Open network connection to the DC drive
                        //
                        if ((success = this.dcDrive.OpenConnection()) == true)
                        {
                            state = States.OpenConnectionPLC;
                        }
                        else
                        {
                            this.lastError = this.dcDrive.LastError;
                        }
                        break;

                    case States.CheckStatus:
                        //
                        // Check machine status before continuing
                        //
                        if ((success = this.CheckStatus()) == true)
                        {
#if x
                            state = States.InitialisePLC;
#else
                            state = States.Done;
#endif
                        }
                        else
                        {
                            // this.lastError already updated
                        }
                        break;

                    case States.InitialisePLC:
                        //
                        // Initialise the PLC to default settings
                        //
                        if ((success = this.plc.Initialise()) == true)
                        {
                            state = States.InitialiseDCDrive;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.InitialiseDCDrive:
                        //
                        // Initialise the DC drive to default settings
                        //
                        if ((success = this.dcDrive.Initialise()) == true)
                        {
                            state = States.Done;
                        }
                        else
                        {
                            this.lastError = this.dcDrive.LastError;
                        }
                        break;
                }

                //
                // Check if any errors occurred
                //
                if (success == false)
                {
                    if (this.lastError != null)
                    {
                        Logfile.WriteError(this.lastError);
                    }
                    state = States.Done;
                }
            }

            //
            // Determine how long execution actually took
            //
            TimeSpan timeSpan = DateTime.Now - startDateTime;
            double totalTime = timeSpan.TotalSeconds;
            logMessage = String.Format(STRLOG_ExecuteTime_Fmt, totalTime);
            Trace.WriteLine(logMessage);

            logMessage += Logfile.STRLOG_Spacer + STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteStarting()
        {
            const string STRLOG_MethodName = "ExecuteStarting";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            string logMessage;

            //
            // Get the time now to determine how long execution actually takes
            //
            DateTime startDateTime = DateTime.Now;

            //
            // Calculate execution completion time
            //
            int executionTimeRemaining = 0;
            executionTimeRemaining += this.executionTimes.start;
            executionTimeRemaining += this.executionTimes.run;
            executionTimeRemaining += this.executionTimes.stop;
            executionTimeRemaining += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTimeRemaining);
            Logfile.Write(
                String.Format(STRLOG_ExecutionTimeRemaining_arg, executionTimeRemaining));

            //
            // Create the data structure for storing the measurements
            //
            this.measurements = new Measurements();

            //
            // Initialise state machine
            //
            this.lastError = null;
            int increaseSyncFieldCount = 0;
            double currentSpeed = 0;
            int measurementCount = 0;
            Measurement[] measurementArray = new Measurement[this.configParameters.simpleMovingAverage + 1];
            int synchronismRetries = 3;
            States state = States.CheckStatus;

            //
            // State machine loop
            //
            while (state != States.Done)
            {
                bool status = false;
                double value = 0;
                string units = String.Empty;

                Trace.WriteLine("state: " + state.ToString());

                switch (state)
                {
                    case States.CheckStatus:
                        //
                        // Check machine status before continuing
                        //
                        if ((success = this.CheckStatus()) == true)
                        {
                            state = States.EnableDCDrive;
                        }
                        else
                        {
                            // this.lastError already updated
                        }
                        break;

                    case States.EnableDCDrive:
                        //
                        // Enable the DC drive
                        //
                        if ((success = this.plc.EnableDCDrive(true)) == true)
                        {
                            state = States.StartDCDrive;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.StartDCDrive:
                        //
                        // Start the DC drive
                        //
                        int speed = this.configParameters.speed;
                        if ((success = this.dcDrive.StartSpeedMode(speed)) == true)
                        {
                            currentSpeed = this.dcDrive.SpeedSetpoint;
                            state = States.CheckDCDriveStatus;
                        }
                        else
                        {
                            this.lastError = this.dcDrive.LastError;
                        }
                        break;

                    case States.CheckDCDriveStatus:
                        //
                        // Check the DC drive status
                        //
                        if ((success = this.plc.GetDCDriveStatus(ref status)) == true && status == true)
                        {
                            state = States.EnableSyncField;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.EnableSyncField:
                        //
                        // Enable the sync field
                        //
                        if ((success = this.plc.EnableSyncField(true)) == true)
                        {
                            state = States.IncreaseSyncField;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.IncreaseSyncField:
                        //
                        // Check if maximum sync field incease count has been reached - should not happen
                        //
                        if ((success = (++increaseSyncFieldCount < this.configParameters.maxSyncFieldIncreases)) == true)
                        {
                            //
                            // Increase the sync. machine field by one increment
                            //
                            if ((success = (this.plc.IncreaseSyncField(ref status) == true && status == true)) == true)
                            {
                                state = States.CheckSyncFieldCurrent;
                            }
                            else
                            {
                                this.lastError = STRLOG_UnableToIncreaseFieldCurrent + this.plc.LastError;
                            }
                        }
                        else
                        {
                            this.lastError = STRERR_MaxSyncFieldIncreasesReached + increaseSyncFieldCount.ToString();
                        }
                        break;

                    case States.CheckSyncFieldCurrent:
                        //
                        // Get the sync field current
                        //
                        if ((success = this.plc.GetSyncFieldCurrent(ref value, ref units)) == true)
                        {
                            Logfile.Write(String.Format(STRLOG_FieldCurrentValueUnits_Fmt, value, units));

                            //
                            // Check if the maximum field current has been reached
                            //
                            if ((success = (value < this.configParameters.maxFieldCurrent)) == true)
                            {
                                state = States.TakeMeasurementsForVoltages;
                            }
                            else
                            {
                                //
                                // Maximum field current has been reached
                                //
                                this.lastError = STRERR_MaxFieldCurrentReached + value.ToString();
                            }
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.TakeMeasurementsForVoltages:
                        //
                        // Take measurements, only need one
                        //
                        if ((success = this.TakeMeasurements(state, ref measurementArray[0])) == true)
                        {
                            state = States.CheckSyncMainsVoltages;
                        }
                        else
                        {
                            // this.lastError already updated
                        }
                        break;

                    case States.CheckSyncMainsVoltages:
                        //
                        // Check if the sync voltage is at or above the mains voltage
                        //
                        if (measurementArray[0].syncVoltage < measurementArray[0].mainsVoltage)
                        {
                            //
                            // Required sync voltage not reached yet
                            //
                            state = States.IncreaseSyncField;
                        }
                        else
                        {
                            //
                            // Required sync voltage has been reached, all done here
                            //
                            logMessage = STRLOG_SyncVoltageAboveMainsVoltage;
                            Logfile.Write(logMessage);
                            Trace.WriteLine(logMessage);

                            state = States.EnableSyncCheck;
                        }
                        break;

                    case States.EnableSyncCheck:
                        //
                        // Enable the sync check
                        //
                        if ((success = this.plc.EnableSyncCheck(true)) == true)
                        {
                            state = States.TakeMeasurementsForSynchronism;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.TakeMeasurementsForSynchronism:
                        //
                        // Take measurements
                        //
                        if ((success = this.TakeMeasurements(state, ref measurementArray[measurementCount])) == true)
                        {
                            //
                            // Check if the synchronism window is open
                            //
                            if (measurementArray[measurementCount].syncMainsPhase > this.configParameters.phaseSynchronismLow &&
                                measurementArray[measurementCount].syncMainsPhase < this.configParameters.phaseSynchronismHigh)
                            {
                                //
                                // Try to synchronise to mains
                                //
                                measurementCount = 0;
                                state = States.TrySynchronism;
                            }
                            else
                            {
                                //
                                // Check if there are enough measurements to get an average phase change
                                //
                                if (++measurementCount >= this.configParameters.simpleMovingAverage + 1)
                                {
                                    measurementCount = 0;
                                    state = States.CheckSpeedForSynchronism;
                                }
                                else
                                {
                                    //
                                    // Wait before taking measurements again
                                    //
                                    this.WaitDelay(1);
                                }
                            }
                        }
                        else
                        {
                            // this.lastError already updated
                        }
                        break;

                    case States.CheckSpeedForSynchronism:
                        //
                        // Calculate the average phase change from the measurements
                        //
                        double syncMainsPhaseChange = 0;
                        for (int i = 0; i < this.configParameters.simpleMovingAverage; i++)
                        {
                            double currentPhase = measurementArray[i + 1].syncMainsPhase;
                            double previousPhase = measurementArray[i].syncMainsPhase;
                            double phaseDifference = PhaseDifference(previousPhase, currentPhase);
                            syncMainsPhaseChange += phaseDifference;
                            //Trace.WriteLine(string.Format("i: {0} - curr: {1:f01} - prev: {2:f01} - diff: {3:f01}", i, currentPhase, previousPhase, phaseDifference));
                        }
                        syncMainsPhaseChange /= this.configParameters.simpleMovingAverage;
                        Trace.WriteLine("syncMainsPhaseChange: " + syncMainsPhaseChange.ToString("f0"));

                        //
                        // Compare the average phase change with the limits and adjust the speed if necessary
                        //
                        if (syncMainsPhaseChange < this.configParameters.phaseChangeLimitLow)
                        {
                            //
                            // Phase change is too low, need to increase the speed
                            //
                            currentSpeed += 0.5;
                        }
                        else if (syncMainsPhaseChange > this.configParameters.phaseChangeLimitHigh)
                        {
                            //
                            // Phase change is too high, need to decrease the speed
                            // 
                            currentSpeed -= 0.5;
                        }
                        else
                        {
                            //
                            // No change in speed required
                            //
                            state = States.TakeMeasurementsForSynchronism;
                            break;
                        }

                        //
                        // Check if the current speed is out of range
                        //
                        if ((success = currentSpeed <= this.configParameters.speed + this.configParameters.speedTrimHigh) == false)
                        {
                            this.lastError = STRERR_MaxSpeedTrimReached + currentSpeed.ToString();
                        }
                        else if ((success = currentSpeed >= this.configParameters.speed - this.configParameters.speedTrimLow) == false)
                        {
                            this.lastError = STRERR_MinSpeedTrimReached + currentSpeed.ToString();
                        }
                        else
                        {
                            //
                            // Change the DC drive speed
                            //
                            if ((success = this.dcDrive.ChangeSpeed(currentSpeed)) == true)
                            {
                                state = States.TakeMeasurementsForSynchronism;
                            }
                            else
                            {
                                this.lastError = this.dcDrive.LastError;
                            }
                        }
                        break;

                    case States.TrySynchronism:
                        //
                        // Get synchronism status
                        //
                        if ((success = this.plc.GetSynchronismStatus(ref status)) == true)
                        {
                            if (status == true)
                            {
                                //
                                // We are now in synchronism, attempt to close the contactor
                                //
                                if ((success = this.plc.CloseContactorA(ref status)) == true)
                                {
                                    if (status == true)
                                    {
                                        //
                                        // Synchronism successful
                                        //
                                        state = States.InSynchronism;
                                    }
                                    else
                                    {
                                        //
                                        // Retry synchronism
                                        //
                                        if (--synchronismRetries > 0)
                                        {
                                            state = States.TakeMeasurementsForSynchronism;
                                        }
                                        else
                                        {
                                            this.lastError = STRERR_SynchronismFailed;
                                            success = false;
                                        }
                                    }
                                }
                                else
                                {
                                    this.lastError = this.plc.LastError;
                                }
                            }
                            else
                            {
                                //
                                // Not in synchronism yet - wait a very short time before taking measurements
                                //
                                Thread.Sleep(200);
                                state = States.TakeMeasurementsForSynchronism;
                            }
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.InSynchronism:
                        //
                        // We are now in synchronism - set the torque limits
                        //
                        if ((success = this.dcDrive.SetMaxTorqueLimit(DCDrive.DEFAULT_Motor1TorqueMaxLimit_Synchronism)) == true)
                        {
                            if ((success = this.dcDrive.SetMinTorqueLimit(DCDrive.DEFAULT_Motor1TorqueMinLimit_Synchronism)) == true)
                            {
                                //
                                // Load up the machine by making it drive harder
                                //
                                if ((success = this.dcDrive.ChangeSpeed(this.configParameters.speedSynchronism, false)) == true)
                                {
                                    this.WaitDelay(5);
                                    state = States.Done;
                                }
                                else
                                {
                                    this.lastError = this.dcDrive.LastError;
                                }
                            }
                            else
                            {
                                this.lastError = this.dcDrive.LastError;
                            }
                        }
                        else
                        {
                            this.lastError = this.dcDrive.LastError;
                        }
                        break;
                }

                //
                // Check if any errors occurred
                //
                if (success == false)
                {
                    if (this.lastError != null)
                    {
                        Logfile.WriteError(this.lastError);
                    }
                    state = States.Done;
                }
            }

            //
            // Determine how long execution actually took
            //
            TimeSpan timeSpan = DateTime.Now - startDateTime;
            double totalTime = timeSpan.TotalSeconds;
            logMessage = String.Format(STRLOG_ExecuteTime_Fmt, totalTime);
            Trace.WriteLine(logMessage);

            logMessage += Logfile.STRLOG_Spacer + STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteRunning()
        {
            const string STRLOG_MethodName = "ExecuteRunning";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            string logMessage;

            //
            // Get the time now to determine how long execution actually takes
            //
            DateTime startDateTime = DateTime.Now;

            //
            // Calculate execution completion time
            //
            int executionTimeRemaining = 0;
            executionTimeRemaining += this.executionTimes.run;
            executionTimeRemaining += this.executionTimes.stop;
            executionTimeRemaining += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTimeRemaining);
            Logfile.Write(
                String.Format(STRLOG_ExecutionTimeRemaining_arg, executionTimeRemaining));

            //
            // Create the data structure for storing the measurements
            //
            this.measurements = new Measurements();

            //
            // Initialise state machine
            //
            this.lastError = null;
            int torque = this.configParameters.torqueMin;
            int measurementCount = 0;
            Measurement[] measurementArray = new Measurement[this.configParameters.simpleMovingAverage];
            Measurement measurementAverage = new Measurement();
            States state = States.CheckStatus;

            //
            // State machine loop
            //
            while (state != States.Done)
            {
                bool status = false;
                string units = String.Empty;

                Trace.WriteLine("state: " + state.ToString());

                switch (state)
                {
                    case States.CheckStatus:
                        //
                        // Check machine status before continuing
                        //
                        if ((success = this.CheckStatus()) == true)
                        {
                            state = States.TakeMeasurementsForIncreasedSyncField;
                        }
                        else
                        {
                            // this.lastError already updated
                        }
                        break;

                    case States.TakeMeasurementsForIncreasedTorque:
                    case States.TakeMeasurementsForIncreasedSyncField:
                    case States.TakeMeasurementsForDecreasedTorque:
                    case States.TakeMeasurementsForDecreasedSyncField:
                        //
                        // Wait before taking measurements
                        //
                        this.WaitDelay(1);

                        //
                        // Take measurements
                        //
                        if ((success = this.TakeMeasurements(state, ref measurementArray[measurementCount])) == true)
                        {
                            //
                            // Check if there are enough measurements to get an average of the power factor
                            //
                            if (++measurementCount >= this.configParameters.simpleMovingAverage)
                            {
                                //
                                // Average the measurements
                                //
                                measurementAverage = new Measurement();

                                for (int i = 0; i < this.configParameters.simpleMovingAverage; i++)
                                {
                                    measurementAverage.fieldCurrent += measurementArray[i].fieldCurrent;
                                    measurementAverage.syncVoltage += measurementArray[i].syncVoltage;
                                    measurementAverage.syncFrequency += measurementArray[i].syncFrequency;
                                    measurementAverage.torqueSetpoint += measurementArray[i].torqueSetpoint;
                                    measurementAverage.realPower += measurementArray[i].realPower;
                                    measurementAverage.reactivePower += measurementArray[i].reactivePower;
                                    measurementAverage.phaseCurrent += measurementArray[i].phaseCurrent;
                                    if (measurementArray[i].powerFactor < 0)
                                    {
                                        measurementArray[i].powerFactor += 2;
                                    }
                                    measurementAverage.powerFactor += measurementArray[i].powerFactor;
                                }
                                measurementAverage.fieldCurrent /= this.configParameters.simpleMovingAverage;
                                measurementAverage.syncVoltage /= this.configParameters.simpleMovingAverage;
                                measurementAverage.syncFrequency /= this.configParameters.simpleMovingAverage;
                                measurementAverage.torqueSetpoint /= this.configParameters.simpleMovingAverage;
                                measurementAverage.powerFactor /= this.configParameters.simpleMovingAverage;
                                measurementAverage.realPower /= this.configParameters.simpleMovingAverage;
                                measurementAverage.reactivePower /= this.configParameters.simpleMovingAverage;
                                measurementAverage.phaseCurrent /= this.configParameters.simpleMovingAverage;

                                //
                                // Add the averaged measurement to the list
                                //
                                this.measurements.valueList.Add(measurementAverage);
                                measurementCount = 0;

                                //
                                // Determine next state
                                //
                                switch (state)
                                {
                                    case States.TakeMeasurementsForIncreasedTorque:
                                        state = States.CheckPowerFactorForIncreasedTorque;
                                        break;

                                    case States.TakeMeasurementsForIncreasedSyncField:
                                        state = States.IncreaseTorque;
                                        break;

                                    case States.TakeMeasurementsForDecreasedTorque:
                                        state = (torque > this.configParameters.torqueMin) ? States.CheckPowerFactorForDecreasedTorque : States.Done;
                                        break;

                                    case States.TakeMeasurementsForDecreasedSyncField:
                                        state = States.DecreaseTorque;
                                        break;
                                }
                            }
                            else
                            {
                                //
                                // Take another measurement
                                //
                            }
                        }
                        else
                        {
                            // this.lastError already updated
                        }
                        break;

                    case States.IncreaseTorque:
                        //
                        // Check if the desired maximum torque has been reached
                        //
                        if (torque < this.configParameters.torqueMax)
                        {
                            //
                            // Increase the torque by one step
                            //
                            torque += this.configParameters.torqueStep;
                            logMessage = STRLOG_TorqueSetpoint + torque.ToString();
                            Logfile.Write(logMessage);
                            Trace.WriteLine(logMessage);

                            //
                            // Change the DC drive torque maximum limit setpoint
                            //
                            if ((success = (this.dcDrive.SetMaxTorqueLimit(torque))) == true)
                            {
                                //
                                // Wait for increased torque to settle
                                //
                                this.WaitDelay(3);

                                state = States.TakeMeasurementsForIncreasedTorque;
                            }
                            else
                            {
                                this.lastError = this.dcDrive.LastError;
                            }
                        }
                        else
                        {
                            //
                            // Maximum torque has been reached, all done here
                            //
                            Logfile.Write(STRLOG_MaxTorqueReached + this.configParameters.torqueMax.ToString());
                            state = States.DecreaseTorque;
                        }
                        break;

                    case States.CheckPowerFactorForIncreasedTorque:
                        //
                        // Check if the average power factor is too high
                        //
                        Trace.WriteLine(string.Format("Average PowerFactor: {0:f03}", measurementAverage.powerFactor));
                        if (measurementAverage.powerFactor > this.configParameters.powerFactorHigh)
                        {
                            //
                            // Need to increase the field current to decrease the power factor
                            //
                            state = States.IncreaseSyncFieldForIncreasedTorque;
                        }
                        else
                        {
                            state = States.IncreaseTorque;
                        }
                        break;

                    case States.IncreaseSyncFieldForIncreasedTorque:
                        //
                        // Increase the sync. machine field by one increment
                        //
                        if ((success = (this.plc.IncreaseSyncField(ref status) == true && status == true)) == true)
                        {
                            //
                            // Wait for increased sync field to settle
                            //
                            this.WaitDelay(3);

                            state = States.TakeMeasurementsForIncreasedSyncField;
                        }
                        else
                        {
                            //
                            // Unable to increase field current because of machine limitations, not an error
                            //
                            logMessage = STRLOG_UnableToIncreaseFieldCurrent;
                            Logfile.Write(logMessage);
                            Trace.WriteLine(logMessage);

                            success = true;

                            state = States.IncreaseTorque;
                        }
                        break;

                    case States.DecreaseTorque:
                        //
                        // Check if the desired minimum torque has been reached
                        //
                        if (torque > this.configParameters.torqueMin)
                        {
                            //
                            // Decrease the torque by one step
                            //
                            torque -= this.configParameters.torqueStep;
                            logMessage = STRLOG_TorqueSetpoint + torque.ToString();
                            Logfile.Write(logMessage);
                            Trace.WriteLine(logMessage);

                            //
                            // Change the DC drive torque maximum limit setpoint
                            //
                            if ((success = (this.dcDrive.SetMaxTorqueLimit(torque))) == true)
                            {
                                state = States.TakeMeasurementsForDecreasedTorque;
                            }
                            else
                            {
                                this.lastError = this.dcDrive.LastError;
                            }
                        }
                        else
                        {
                            //
                            // Minimum torque has been reached, all done here
                            //
                            Logfile.Write(STRLOG_MinTorqueReached + this.configParameters.torqueMax.ToString());
                            state = States.Done;
                        }
                        break;

                    case States.CheckPowerFactorForDecreasedTorque:
                        //
                        // Check if the average power factor is too high
                        //
                        Trace.WriteLine(string.Format("Average PowerFactor: {0:f03}", measurementAverage.powerFactor));
                        if (measurementAverage.powerFactor < this.configParameters.powerFactorLow)
                        {
                            //
                            // Need to decrease the field current to increase the power factor
                            //
                            state = States.DecreaseSyncFieldForDecreasedTorque;
                        }
                        else
                        {
                            state = States.DecreaseTorque;
                        }
                        break;

                    case States.DecreaseSyncFieldForDecreasedTorque:
                        //
                        // Decrease the sync. machine field by one increment
                        //
                        if ((success = (this.plc.DecreaseSyncField(ref status) == true && status == true)) == true)
                        {
                            state = States.TakeMeasurementsForDecreasedSyncField;
                        }
                        else
                        {
                            this.lastError = STRLOG_UnableToDecreaseFieldCurrent + this.plc.LastError;
                        }
                        break;
                }

                //
                // Check if any errors occurred
                //
                if (success == false)
                {
                    if (this.lastError != null)
                    {
                        Logfile.WriteError(this.lastError);
                    }
                    state = States.Done;
                }
            }

            //
            // Determine how long execution actually took
            //
            TimeSpan timeSpan = DateTime.Now - startDateTime;
            double totalTime = timeSpan.TotalSeconds;
            logMessage = String.Format(STRLOG_ExecuteTime_Fmt, totalTime);
            Trace.WriteLine(logMessage);

            logMessage += Logfile.STRLOG_Spacer + STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteStopping()
        {
            const string STRLOG_MethodName = "ExecuteStopping";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            string logMessage;

            //
            // Get the time now to determine how long execution actually takes
            //
            DateTime startDateTime = DateTime.Now;

            //
            // Calculate execution completion time
            //
            int executionTimeRemaining = 0;
            executionTimeRemaining += this.executionTimes.stop;
            executionTimeRemaining += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTimeRemaining);
            Logfile.Write(
                String.Format(STRLOG_ExecutionTimeRemaining_arg, executionTimeRemaining));

            //
            // Restore machine state to post-synchronism
            //
            bool error = false;

            if (this.dcDrive.SetMaxTorqueLimit(DCDrive.DEFAULT_Motor1TorqueMaxLimit_Synchronism) == false)
            {
                this.lastError = this.dcDrive.LastError;
                error = true;
            }
            if (this.dcDrive.ChangeSpeed(this.configParameters.speed, false) == false)
            {
                this.lastError = this.dcDrive.LastError;
                error = true;
            }

            //
            // Restore machine state to pre-synchronism
            //
            if (this.plc.OpenContactorA() == false)
            {
                this.lastError = this.plc.LastError;
                error = true;
            }
            //if (this.dcDrive.SetMaxTorqueLimit(DCDrive.DEFAULT_Motor1TorqueMaxLimit) == false)
            //{
            //    this.lastError = this.dcDrive.LastError;
            //    error = true;
            //}
            if (this.plc.EnableSyncCheck(false) == false)
            {
                this.lastError = this.plc.LastError;
                error = true;
            }

            //
            // Stop the machine
            //
            if (this.plc.ResetSyncField() == false)
            {
                this.lastError = this.plc.LastError;
                error = true;
            }
            if (this.plc.EnableSyncField(false) == false)
            {
                this.lastError = this.plc.LastError;
                error = true;
            }
            if (this.dcDrive.Stop() == false)
            {
                this.lastError = this.dcDrive.LastError;
                error = true;
            }
            if (this.plc.EnableDCDrive(false) == false)
            {
                this.lastError = this.plc.LastError;
                error = true;
            }
            success = !error;

            //
            // Determine how long execution actually took
            //
            TimeSpan timeSpan = DateTime.Now - startDateTime;
            double totalTime = timeSpan.TotalSeconds;
            logMessage = String.Format(STRLOG_ExecuteTime_Fmt, totalTime);
            Trace.WriteLine(logMessage);

            logMessage += Logfile.STRLOG_Spacer + STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteFinalising()
        {
            const string STRLOG_MethodName = "ExecuteFinalising";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            string logMessage;

            //
            // Get the time now to determine how long execution actually takes
            //
            DateTime startDateTime = DateTime.Now;

            //
            // Calculate execution completion time
            //
            int executionTimeRemaining = 0;
            executionTimeRemaining += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTimeRemaining);
            Logfile.Write(
                String.Format(STRLOG_ExecutionTimeRemaining_arg, executionTimeRemaining));

            //
            // Close the network connections
            //
            success = (
                this.dcDrive.CloseConnection() == true &&
                this.plc.CloseConnection() == true
                );

            //
            // Determine how long execution actually took
            //
            TimeSpan timeSpan = DateTime.Now - startDateTime;
            double totalTime = timeSpan.TotalSeconds;
            logMessage = String.Format(STRLOG_ExecuteTime_Fmt, totalTime);
            Trace.WriteLine(logMessage);

            logMessage += Logfile.STRLOG_Spacer + STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //=================================================================================================//

        private bool TakeMeasurements(States callerState, ref Measurement measurement)
        {
            const string STRLOG_MethodName = "TakeMeasurements";

            string logMessage = STRLOG_CallerState + callerState.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            //
            // Create the data structure for storing the measurements
            //
            measurement = new Measurement();

            //
            // Initialise state machine
            //
            this.lastError = null;
            States state = States.MeasureFieldCurrent;

            //
            // State machine loop
            //
            while (state != States.Done)
            {
                double value = 0;
                string units = String.Empty;

                //Trace.WriteLine("state: " + state.ToString());

                switch (state)
                {
                    case States.MeasureFieldCurrent:
                        //
                        // Measure the field current
                        //
                        if ((success = this.plc.GetSyncFieldCurrent(ref value, ref units)) == true)
                        {
                            //
                            // Save the field current measurement
                            //
                            measurement.fieldCurrent = value;
                            if (this.measurements.units.fieldCurrent == null)
                            {
                                this.measurements.units.fieldCurrent = units;
                            }

                            state = States.MeasureSyncVoltage;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.MeasureSyncVoltage:
                        //
                        // Measure the sync voltage
                        //
                        if ((success = this.plc.SyncMonitor.GetSyncVoltage(ref value, ref units)) == true)
                        {
                            //
                            // Save the sync voltage measurement
                            //
                            measurement.syncVoltage = value;
                            if (this.measurements.units.syncVoltage == null)
                            {
                                this.measurements.units.syncVoltage = units;
                            }

                            //
                            // Determine next state
                            //
                            if (callerState == States.TakeMeasurementsForVoltages)
                            {
                                state = States.MeasureMainsVoltage;
                            }
                            else if (callerState == States.TakeMeasurementsForSynchronism)
                            {
                                state = States.MeasureSyncToMainsPhase;
                            }
                            else
                            {
                                state = States.MeasureSyncFrequency;
                            }
                        }
                        else
                        {
                            this.lastError = this.plc.SyncMonitor.LastError;
                        }
                        break;

                    case States.MeasureMainsVoltage:
                        //
                        // Measure the mains voltage
                        //
                        if ((success = this.plc.SyncMonitor.GetMainsVoltage(ref value, ref units)) == true)
                        {
                            //
                            // Save the mains voltage measurement
                            //
                            measurement.mainsVoltage = value;
                            if (this.measurements.units.mainsVoltage == null)
                            {
                                this.measurements.units.mainsVoltage = units;
                            }

                            state = States.Done;
                        }
                        else
                        {
                            this.lastError = this.plc.SyncMonitor.LastError;
                        }
                        break;

                    case States.MeasureSyncToMainsPhase:
                        //
                        // Measure the sync to mains phase difference
                        //
                        if ((success = this.plc.SyncMonitor.GetSyncMainsPhase(ref value, ref units)) == true)
                        {
                            //
                            // Save the sync to mains phase difference measurement
                            //
                            measurement.syncMainsPhase = value;
                            if (this.measurements.units.syncMainsPhase == null)
                            {
                                this.measurements.units.syncMainsPhase = units;
                            }

                            state = States.MeasureSynchronismStatus;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.MeasureSynchronismStatus:
                        //
                        // Measure synchronism status
                        //
                        bool status = false;
                        if ((success = this.plc.GetSynchronismStatus(ref status)) == true)
                        {
                            measurement.synchronism = status;

                            //
                            // Determine next state
                            //
                            if (callerState == States.TakeMeasurementsForSynchronism)
                            {
                                state = States.Done;
                            }
                            else
                            {
                                state = States.MeasureSyncFrequency;
                            }
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.MeasureSyncFrequency:
                        //
                        // Measure the sync frequency
                        //
                        if ((success = this.plc.SyncMonitor.GetSyncFrequency(ref value, ref units)) == true)
                        {
                            //
                            // Save the sync frequency measurement
                            //
                            measurement.syncFrequency = value;
                            if (this.measurements.units.syncFrequency == null)
                            {
                                this.measurements.units.syncFrequency = units;
                            }

                            state = States.MeasureTorqueSetpoint;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.MeasureTorqueSetpoint:
                        //
                        // Measure the torque to get the units
                        //
                        if ((success = this.dcDrive.GetTorque(ref value, ref units)) == true)
                        {
                            //
                            // Save the torque setpoint
                            //
                            measurement.torqueSetpoint = this.dcDrive.TorqueMaxLimitSetpoint;
                            if (this.measurements.units.torqueSetpoint == null)
                            {
                                this.measurements.units.torqueSetpoint = units;
                            }

                            state = States.MeasurePowerFactor;
                        }
                        else
                        {
                            this.lastError = this.dcDrive.LastError;
                        }
                        break;

                    case States.MeasurePowerFactor:
                        //
                        // Measure the power factor
                        //
                        if ((success = this.plc.PowerMeter.GetPowerFactor(ref value, ref units)) == true)
                        {
                            //
                            // Save the sync power factor measurement
                            //
                            measurement.powerFactor = value;

                            state = States.MeasureRealPower;
                        }
                        else
                        {
                            this.lastError = this.plc.PowerMeter.LastError;
                        }
                        break;

                    case States.MeasureRealPower:
                        //
                        // Measure the real power
                        //
                        if ((success = this.plc.PowerMeter.GetRealPower(ref value, ref units)) == true)
                        {
                            //
                            // Save the sync power factor measurement
                            //
                            measurement.realPower = value;
                            if (this.measurements.units.realPower == null)
                            {
                                this.measurements.units.realPower = units;
                            }

                            state = States.MeasureReactivePower;
                        }
                        else
                        {
                            this.lastError = this.plc.PowerMeter.LastError;
                        }
                        break;

                    case States.MeasureReactivePower:
                        //
                        // Measure the machine reactive power
                        //
                        if ((success = this.plc.PowerMeter.GetReactivePower(ref value, ref units)) == true)
                        {
                            //
                            // Save the reactive factor measurement
                            //
                            measurement.reactivePower = value;
                            if (this.measurements.units.reactivePower == null)
                            {
                                this.measurements.units.reactivePower = units;
                            }

                            state = States.MeasurePhaseCurrent;
                        }
                        else
                        {
                            this.lastError = this.plc.PowerMeter.LastError;
                        }
                        break;

                    case States.MeasurePhaseCurrent:
                        //
                        // Measure the phase current
                        //
                        if ((success = this.plc.PowerMeter.GetPhaseCurrent(ref value, ref units)) == true)
                        {
                            //
                            // Save the phase current measurement
                            //
                            measurement.phaseCurrent = value;
                            if (this.measurements.units.phaseCurrent == null)
                            {
                                this.measurements.units.phaseCurrent = units;
                            }

                            state = States.Done;
                        }
                        else
                        {
                            this.lastError = this.plc.PowerMeter.LastError;
                        }
                        break;

                }

                //
                // Check if any errors occurred
                //
                if (success == false)
                {
                    if (this.lastError != null)
                    {
                        Logfile.WriteError(this.lastError);
                    }
                    state = States.Done;
                }
            }

            if (success == true)
            {
                //
                // Log the measurements
                //
                if (callerState == States.TakeMeasurementsForVoltages)
                {
                    logMessage = String.Format(STRLOG_MeasurementsForVoltagesValueUnits_Fmt,
                        measurement.fieldCurrent, this.measurements.units.fieldCurrent,
                        measurement.syncVoltage, this.measurements.units.syncVoltage,
                        measurement.mainsVoltage, this.measurements.units.mainsVoltage);
                    Logfile.Write(logMessage);
                    Trace.WriteLine(logMessage);
                }
                else if (callerState == States.TakeMeasurementsForSynchronism)
                {
                    logMessage = String.Format(STRLOG_MeasurementsForSynchronismValueUnits_Fmt,
                        measurement.syncMainsPhase, this.measurements.units.syncMainsPhase,
                        measurement.synchronism);
                    Logfile.Write(logMessage);
                    Trace.WriteLine(logMessage);
                }
                else
                {
                    logMessage = String.Format(STRLOG_MeasurementsForTorqueValueUnits_Fmt,
                        measurement.fieldCurrent, this.measurements.units.fieldCurrent,
                        measurement.syncVoltage, this.measurements.units.syncVoltage,
                        measurement.syncFrequency, this.measurements.units.syncFrequency);
                    Logfile.Write(logMessage);
                    Trace.WriteLine(logMessage);

                    logMessage = String.Format(STRLOG_MeasurementsPowerMeterValueUnits_Fmt,
                        measurement.powerFactor,
                        measurement.realPower, this.measurements.units.realPower,
                        measurement.reactivePower, this.measurements.units.reactivePower,
                        measurement.phaseCurrent, this.measurements.units.phaseCurrent);
                    Logfile.Write(logMessage);
                    Trace.WriteLine(logMessage);
                }
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Calculate the phase difference between the previous and current phase values.
        /// The wraparound at -180/+180 degrees is taken care of.
        /// </summary>
        /// <param name="previous">The previous phase value in degrees.</param>
        /// <param name="current">The current phase value in degrees.</param>
        /// <returns>The change in phase in degrees.</returns>
        private double PhaseDifference(double previous, double current)
        {
            double difference = current - previous;

            if (difference > 180)
            {
                difference -= 360;
            }
            else if (difference < -180)
            {
                difference += 360;
            }

            return difference;
        }

    }
}
