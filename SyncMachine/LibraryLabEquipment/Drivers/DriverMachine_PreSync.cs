using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class DriverMachine_PreSync : DriverMachine
    {
        #region Constants

        private const string STRLOG_ClassName = "DriverMachine_PreSync";

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

        private const string STRLOG_CallerState = " CallerState: ";
        private const string STRLOG_FieldCurrentValueUnits_Fmt = " FieldCurrent: {0:f03} {1}";
        private const string STRLOG_MaxFieldCurrentReached = "Maximum field current has been reached: ";
        private const string STRLOG_UnableToIncreaseFieldCurrent = "Unable to increase field current";
        private const string STRLOG_SyncVoltageAboveMainsVoltage = " Sync voltage is higher than mains voltage.";
        private const string STRLOG_SyncFrequencyMatchesMainsFrequency = " Sync frequency matches mains frequency.";
        private const string STRLOG_MeasurementsForVoltagesValueUnits_Fmt = " FieldCurrent: {0:f03} {1} - Sync Voltage: {2:f01} {3} - Mains Voltage: {4:f01} {5}";
        private const string STRLOG_MeasurementsForFrequenciesValueUnits_Fmt = " Sync Frequency: {0:f02} {1} - Mains Frequency: {2:f02} {3}";
        private const string STRLOG_MeasurementsForSynchronismValueUnits_Fmt = " SyncToMainsPhase: {0:f01} {1} - Synchronism: {2}";

        //
        // String constants for error messages
        //
        private const string STRERR_MaxSyncFieldIncreasesReached = "Maximum sync field increases have been reached: ";
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
        }

        public struct Measurement
        {
            public double fieldCurrent;
            public double speedSetpoint;
            public double mainsVoltage;
            public double mainsFrequency;
            public double syncVoltage;
            public double syncFrequency;
            public double syncMainsPhase;
            public bool synchronism;
        }

        public struct MeasurementUnits
        {
            public string fieldCurrent;
            public string speedSetpoint;
            public string mainsVoltage;
            public string mainsFrequency;
            public string syncVoltage;
            public string syncFrequency;
            public string syncMainsPhase;
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

            // ExecuteRunning
            IncreaseSyncField, CheckSyncFieldCurrent, TakeMeasurementsForVoltages, CheckSyncMainsVoltages,
            TakeMeasurementsForFrequencies, CheckSpeedForFrequencies, EnableSyncCheck,
            TakeMeasurementsForSynchronism, CheckSpeedForSynchronism, TrySynchronism,
            InSynchronism, TakeMeasurementsInSynchronism, EndSynchronism,

            // TakeMeasurements
            MeasureFieldCurrent, MeasureSpeedSetpoint, MeasureSyncVoltage, MeasureMainsVoltage,
            MeasureSyncFrequency, MeasureMainsFrequency,
            MeasureSyncToMainsPhase, MeasureSynchronismStatus
        }

        #endregion

        #region Variables

        private ConfigParameters configParameters;
        private XmlNode xmlNodeMeasurementsTemplate;
        private Measurements measurements;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public DriverMachine_PreSync(XmlNode xmlNodeEquipmentConfig, Specification specification)
            : base(xmlNodeEquipmentConfig, specification)
        {
            const string STRLOG_MethodName = "DriverMachine_PreSync";

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
                XmlNode xmlNodePreSynchronisation = XmlUtilities.GetXmlNode(xmlNodeConfiguration, Consts.STRXML_preSynchronisation, false);

                //
                // Execution time
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodePreSynchronisation, Consts.STRXML_executionTimes);
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
                this.configParameters.speed = XmlUtilities.GetIntValue(xmlNodePreSynchronisation, Consts.STRXML_speed);
                xmlNode = XmlUtilities.GetXmlNode(xmlNodePreSynchronisation, Consts.STRXML_speedTrim);
                this.configParameters.speedTrimLow = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_low);
                this.configParameters.speedTrimHigh = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_high);
                Logfile.Write(STRLOG_Speed + this.configParameters.speed.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_SpeedTrimLow + this.configParameters.speedTrimLow.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_SpeedTrimHigh + this.configParameters.speedTrimHigh.ToString());

                //
                // Maximum field current, sync field increases and measurements
                //
                this.configParameters.maxFieldCurrent = XmlUtilities.GetRealValue(xmlNodePreSynchronisation, Consts.STRXML_maxFieldCurrent);
                this.configParameters.maxSyncFieldIncreases = XmlUtilities.GetIntValue(xmlNodePreSynchronisation, Consts.STRXML_maxSyncFieldIncreases);
                this.configParameters.maxMeasurements = XmlUtilities.GetIntValue(xmlNodePreSynchronisation, Consts.STRXML_maxMeasurements);
                Logfile.Write(STRLOG_MaxFieldCurrent + this.configParameters.maxFieldCurrent.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_MaxSyncFieldIncreases + this.configParameters.maxSyncFieldIncreases.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_MaxMeasurements + this.configParameters.maxMeasurements.ToString());

                //
                // Speed adjustment for phase change between measurements
                //
                this.configParameters.simpleMovingAverage = XmlUtilities.GetIntValue(xmlNodePreSynchronisation, Consts.STRXML_simpleMovingAverage);
                xmlNode = XmlUtilities.GetXmlNode(xmlNodePreSynchronisation, Consts.STRXML_phaseChangeLimit);
                this.configParameters.phaseChangeLimitLow = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_low);
                this.configParameters.phaseChangeLimitHigh = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_high);
                Logfile.Write(STRLOG_SimpleMovingAverage + this.configParameters.simpleMovingAverage.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_PhaseChangeLimitLow + this.configParameters.phaseChangeLimitLow.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_PhaseChangeLimitHigh + this.configParameters.phaseChangeLimitHigh.ToString());

                //
                // Phase window to try synchronism
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodePreSynchronisation, Consts.STRXML_phaseSynchronism);
                this.configParameters.phaseSynchronismLow = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_low);
                this.configParameters.phaseSynchronismHigh = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_high);
                Logfile.Write(STRLOG_PhaseSynchronismLow + this.configParameters.phaseSynchronismLow.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_PhaseSynchronismHigh + this.configParameters.phaseSynchronismHigh.ToString());

                //
                // Load the XML measurements template and check that all required XML nodes exist
                //
                this.xmlNodeMeasurementsTemplate = XmlUtilities.GetXmlNode(xmlNodePreSynchronisation, Consts.STRXML_measurements);

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
                // Speed set point
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_speedSetpoint, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_speedSetpoint);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Mains voltage
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_mainsVoltage, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_mainsVoltage);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Mains frequency
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_mainsFrequency, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_mainsFrequency);
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
                // Sync to mains phase
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_syncMainsPhase, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_syncMainsPhase);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Synchronism status
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_synchronism, true);
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
                    float[] speedSetpoint = new float[this.measurements.valueList.Count];
                    float[] mainsVoltage = new float[this.measurements.valueList.Count];
                    float[] mainsFrequency = new float[this.measurements.valueList.Count];
                    float[] syncVoltage = new float[this.measurements.valueList.Count];
                    float[] syncFrequency = new float[this.measurements.valueList.Count];
                    float[] syncMainsPhase = new float[this.measurements.valueList.Count];
                    int[] synchronism = new int[this.measurements.valueList.Count];
                    for (int i = 0; i < this.measurements.valueList.Count; i++)
                    {
                        fieldCurrent[i] = (float)this.measurements.valueList[i].fieldCurrent;
                        speedSetpoint[i] = (float)this.measurements.valueList[i].speedSetpoint;
                        mainsVoltage[i] = (float)this.measurements.valueList[i].mainsVoltage;
                        mainsFrequency[i] = (float)this.measurements.valueList[i].mainsFrequency;
                        syncVoltage[i] = (float)this.measurements.valueList[i].syncVoltage;
                        syncFrequency[i] = (float)this.measurements.valueList[i].syncFrequency;
                        syncMainsPhase[i] = (float)this.measurements.valueList[i].syncMainsPhase;
                        synchronism[i] = (this.measurements.valueList[i].synchronism == true) ? 1 : 0;
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
                    // Speed setpoint
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_speedSetpoint);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_speedSetpoint, speedSetpoint, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.speedSetpoint, false);

                    //
                    // Mains voltage
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_mainsVoltage);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_mainsVoltage, mainsVoltage, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.mainsVoltage, false);

                    //
                    // Mains frequency
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_mainsFrequency);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_mainsFrequency, mainsFrequency, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.mainsFrequency, false);

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
                    // Sync to mains phase
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_syncMainsPhase);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_syncMainsPhase, syncMainsPhase, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.syncMainsPhase, false);

                    //
                    // Synchronism status
                    //
                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_synchronism);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_synchronism, synchronism, Consts.CHR_Splitter, false);

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
                            state = States.InitialisePLC;
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
            // Initialise state machine
            //
            this.lastError = null;
            States state = States.CheckStatus;

            //
            // State machine loop
            //
            while (state != States.Done)
            {
                bool status = false;

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
                        int speed = this.configParameters.speed - this.configParameters.speedTrimLow;
                        if ((success = this.dcDrive.StartSpeedMode(speed)) == true)
                        {
                            Trace.WriteLine("SpeedSetpoint: " + this.dcDrive.SpeedSetpoint.ToString());
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
                            state = States.Done;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
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
            int increaseSyncFieldCount = 0;
            double currentSpeed = this.dcDrive.SpeedSetpoint;
            int syncFrequencyCount = 0;
            int syncMainsPhaseCount = 0;
            int synchronismRetries = 3;
            int inSynchronismCount = 0;
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
                            state = States.IncreaseSyncField;
                        }
                        else
                        {
                            // this.lastError already updated
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
                            if (value < this.configParameters.maxFieldCurrent)
                            {
                                state = States.TakeMeasurementsForVoltages;
                            }
                            else
                            {
                                //
                                // Maximum field current has been reached, all done here
                                //
                                Logfile.Write(STRLOG_MaxFieldCurrentReached + this.configParameters.maxFieldCurrent.ToString());
                                state = States.TakeMeasurementsForFrequencies;
                            }
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.TakeMeasurementsForVoltages:
                        //
                        // Take measurements
                        //
                        if ((success = this.TakeMeasurements(state)) == true)
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
                        // Get the sync and mains voltages from the last measurement
                        //
                        Measurement lastMeasurement = this.measurements.valueList[this.measurements.valueList.Count - 1];

                        //
                        // Check if the sync voltage is at or above the mains voltage
                        //
                        if (lastMeasurement.syncVoltage < lastMeasurement.mainsVoltage)
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

                            state = States.TakeMeasurementsForFrequencies;
                        }
                        break;

                    case States.TakeMeasurementsForFrequencies:
                        //
                        // Take measurements
                        //
                        if ((success = this.TakeMeasurements(state)) == true)
                        {
                            //
                            // Check if there are enough measurements to get an average frequency
                            //
                            if (++syncFrequencyCount >= this.configParameters.simpleMovingAverage)
                            {
                                syncFrequencyCount = 0;
                                state = States.CheckSpeedForFrequencies;
                            }
                            else
                            {
                                //
                                // Wait before taking measurements again
                                //
                                this.WaitDelay(1);
                            }
                        }
                        else
                        {
                            // this.lastError already updated
                        }
                        break;

                    case States.CheckSpeedForFrequencies:
                        //
                        // Calculate the average frequencies from the measurements
                        //
                        double syncFrequency = 0;
                        double mainsFrequency = 0;
                        for (int i = 0; i < this.configParameters.simpleMovingAverage; i++)
                        {
                            Measurement measurement = this.measurements.valueList[this.measurements.valueList.Count - 1 - i];
                            syncFrequency += measurement.syncFrequency;
                            mainsFrequency += measurement.mainsFrequency;
                        }
                        syncFrequency /= this.configParameters.simpleMovingAverage;
                        mainsFrequency /= this.configParameters.simpleMovingAverage;
                        Trace.WriteLine(string.Format("Average Frequencies >> Sync: {0:f02} - Mains: {1:f02}", syncFrequency, mainsFrequency));

                        //
                        // Check if the sync frequency is above the mains frequency yet
                        //
                        if (syncFrequency < mainsFrequency)
                        {
                            //
                            // Sync machine speed is too low, need to increase the speed
                            //
                            currentSpeed++;
                        }
                        else
                        {
                            //
                            // Required sync frequency has been reached
                            //
                            logMessage = STRLOG_SyncFrequencyMatchesMainsFrequency;
                            Logfile.Write(logMessage);
                            Trace.WriteLine(logMessage);

                            state = States.EnableSyncCheck;
                            break;
                        }

                        //
                        // Check if the current speed is out of range
                        //
                        if ((success = currentSpeed <= this.configParameters.speed + this.configParameters.speedTrimHigh) == false)
                        {
                            this.lastError = STRERR_MaxSpeedTrimReached + currentSpeed.ToString();
                        }
                        else
                        {
                            //
                            // Change the DC drive speed
                            //
                            if ((success = this.dcDrive.ChangeSpeed(currentSpeed)) == true)
                            {
                                state = States.TakeMeasurementsForFrequencies;
                            }
                            else
                            {
                                this.lastError = this.dcDrive.LastError;
                            }
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
                        if ((success = this.TakeMeasurements(state)) == true)
                        {
                            //
                            // Check if the synchronism window is open
                            //
                            Measurement measurement = this.measurements.valueList[this.measurements.valueList.Count - 1];
                            if (measurement.syncMainsPhase > this.configParameters.phaseSynchronismLow &&
                                measurement.syncMainsPhase < this.configParameters.phaseSynchronismHigh)
                            {
                                //
                                // Try to synchronise to mains
                                //
                                syncMainsPhaseCount = 0;
                                state = States.TrySynchronism;
                            }
                            else
                            {
                                //
                                // Check if there are enough measurements to get an average phase change
                                //
                                if (++syncMainsPhaseCount >= this.configParameters.simpleMovingAverage + 1)
                                {
                                    syncMainsPhaseCount = 0;
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
                            double currentPhase = this.measurements.valueList[this.measurements.valueList.Count - i - 1].syncMainsPhase;
                            double previousPhase = this.measurements.valueList[this.measurements.valueList.Count - i - 2].syncMainsPhase;
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
                                state = States.TakeMeasurementsInSynchronism;
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

                    case States.TakeMeasurementsInSynchronism:
                         //
                         // Wait before taking measurements
                         //
                         this.WaitDelay(1);

                        //
                        // Take measurements
                        //
                        if ((success = this.TakeMeasurements(state)) == true)
                        {
                            //
                            // Check if enough measurements have been taken
                            //
                            if (++inSynchronismCount >= this.configParameters.maxMeasurements)
                            {
                                inSynchronismCount = 0;
                                state = States.EndSynchronism;
                            }
                        }
                        else
                        {
                            // this.lastError already updated
                        }
                        break;

                    case States.EndSynchronism:
                        //
                        // Restore machine state to pre-synchronism
                        //
                        bool error = false;
                        if (this.plc.OpenContactorA() == false)
                        {
                            this.lastError = this.plc.LastError;
                            error = true;
                        }
                        if (this.dcDrive.SetMaxTorqueLimit(DCDrive.DEFAULT_Motor1TorqueMaxLimit) == false)
                        {
                            this.lastError = this.dcDrive.LastError;
                            error = true;
                        }
                        if (this.plc.EnableSyncCheck(false) == false)
                        {
                            this.lastError = this.plc.LastError;
                            error = true;
                        }
                        success = !error;

                        state = States.Done;
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
            // All of these need to be executed whether or not an error occurs
            //
            success = (
                this.plc.OpenContactorA() == true &&
                this.plc.EnableSyncCheck(false) == true &&
                this.plc.ResetSyncField() == true &&
                this.plc.EnableSyncField(false) == true &&
                this.dcDrive.Stop() == true &&
                this.plc.EnableDCDrive(false) == true
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

        private bool TakeMeasurements(States callerState)
        {
            const string STRLOG_MethodName = "TakeMeasurements";

            string logMessage = STRLOG_CallerState + callerState.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            //
            // Create the data structure for storing the measurements
            //
            Measurement measurement = new Measurement();

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
                            state = States.MeasureSpeedSetpoint;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.MeasureSpeedSetpoint:
                        //
                        // Measure the speed
                        //
                        if ((success = this.dcDrive.GetSpeed(ref value, ref units)) == true)
                        {
                            //
                            // Save the speed setpoint
                            //
                            measurement.speedSetpoint = this.dcDrive.SpeedSetpoint;
                            if (this.measurements.units.speedSetpoint == null)
                            {
                                this.measurements.units.speedSetpoint = units;
                            }
                            state = States.MeasureSyncVoltage;
                        }
                        else
                        {
                            this.lastError = this.dcDrive.LastError;
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
                            state = States.MeasureMainsVoltage;
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

                            //
                            // Determine next state
                            //
                            if (callerState == States.TakeMeasurementsForVoltages)
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
                            this.lastError = this.plc.SyncMonitor.LastError;
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
                            state = States.MeasureMainsFrequency;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.MeasureMainsFrequency:
                        //
                        // Measure the mains frequency
                        //
                        if ((success = this.plc.SyncMonitor.GetMainsFrequency(ref value, ref units)) == true)
                        {
                            //
                            // Save the symainsnc frequency measurement
                            //
                            measurement.mainsFrequency = value;
                            if (this.measurements.units.mainsFrequency == null)
                            {
                                this.measurements.units.mainsFrequency = units;
                            }

                            //
                            // Determine next state
                            //
                            if (callerState == States.TakeMeasurementsForFrequencies)
                            {
                                state = States.Done;
                            }
                            else
                            {
                                state = States.MeasureSyncToMainsPhase;
                            }
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
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
                            state = States.Done;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
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
                // Add the measurements to the list
                //
                this.measurements.valueList.Add(measurement);

                logMessage = String.Format(STRLOG_MeasurementsForVoltagesValueUnits_Fmt,
                    measurement.fieldCurrent, this.measurements.units.fieldCurrent,
                    measurement.syncVoltage, this.measurements.units.syncVoltage,
                    measurement.mainsVoltage, this.measurements.units.mainsVoltage);
                Logfile.Write(logMessage);
                Trace.WriteLine(logMessage);

                if (callerState != States.TakeMeasurementsForVoltages)
                {
                    logMessage = String.Format(STRLOG_MeasurementsForFrequenciesValueUnits_Fmt,
                        measurement.syncFrequency, this.measurements.units.syncFrequency,
                        measurement.mainsFrequency, this.measurements.units.mainsFrequency);
                    Logfile.Write(logMessage);
                    Trace.WriteLine(logMessage);

                    if (callerState != States.TakeMeasurementsForFrequencies)
                    {
                        logMessage = String.Format(STRLOG_MeasurementsForSynchronismValueUnits_Fmt,
                            measurement.syncMainsPhase, this.measurements.units.syncMainsPhase,
                            measurement.synchronism);
                        Logfile.Write(logMessage);
                        Trace.WriteLine(logMessage);
                    }
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
