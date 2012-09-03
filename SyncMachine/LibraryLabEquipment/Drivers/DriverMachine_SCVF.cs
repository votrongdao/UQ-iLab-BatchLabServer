using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class DriverMachine_SCVF : DriverMachine
    {
        #region Constants

        private const string STRLOG_ClassName = "DriverMachine_SCVF";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Speed = " Speed: ";
        private const string STRLOG_MaxFieldCurrent = " MaxFieldCurrent: ";
        private const string STRLOG_MaxStatorCurrent = " MaxStatorCurrent: ";
        private const string STRLOG_MaxSyncFieldIncreases = " MaxSyncFieldIncreases: ";

        private const string STRLOG_StatorCurrentValueUnits_Fmt = " StatorCurrent: {0:f02} {1}";
        private const string STRLOG_FieldCurrentValueUnits_Fmt = " FieldCurrent: {0:f03} {1}";
        private const string STRLOG_MaxStatorCurrentReached = " Maximum stator current has been reached: ";
        private const string STRLOG_MaxFieldCurrentReached = " Maximum field current has been reached: ";
        private const string STRLOG_UnableToIncreaseFieldCurrent = " Unable to increase field current";
        private const string STRLOG_MeasurementsValueUnits_Fmt = " FieldCurrent: {0:f03} {1} - Speed: {2:f0} {3} - StatorCurrent: {4:f02} {5}";

        //
        // String constants for error messages
        //
        private const string STRERR_MaxSyncFieldIncreasesReached = "Maximum sync field increases have been reached: ";

        #endregion

        #region Types

        private struct ConfigParameters
        {
            public int speed;
            public double maxFieldCurrent;
            public double maxStatorCurrent;
            public int maxSyncFieldIncreases;
        }

        public struct Measurement
        {
            public double fieldCurrent;
            public double speed;
            public double statorCurrent;
        }

        public struct MeasurementUnits
        {
            public string fieldCurrent;
            public string speed;
            public string statorCurrent;
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
            OpenConnectionDCDrive, OpenConnectionPLC, InitialiseDCDrive,

            // ExecuteStarting
            EnableDCDrive, StartDCDrive, CheckDCDriveStatus, EnableSyncField, CloseContactorB,

            // ExecuteRunning
            IncreaseSyncField, CheckStatorCurrent, CheckSyncFieldCurrent, CheckMaxSyncFieldCount, TakeMeasurements,

            // TakeMeasurements
            MeasureSpeed, MeasureFieldCurrent, MeasureStatorCurrent
        }

        #endregion

        #region Variables

        private ConfigParameters configParameters;
        private XmlNode xmlNodeMeasurementsTemplate;
        private Measurements measurements;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public DriverMachine_SCVF(XmlNode xmlNodeEquipmentConfig, Specification specification)
            : base(xmlNodeEquipmentConfig, specification)
        {
            const string STRLOG_MethodName = "DriverMachine_SCVF";

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
                XmlNode xmlNodeShortCircuitVaryField = XmlUtilities.GetXmlNode(xmlNodeConfiguration, Consts.STRXML_shortCircuitVaryField, false);

                //
                // Execution times
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodeShortCircuitVaryField, Consts.STRXML_executionTimes);
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
                // Speed
                //
                this.configParameters.speed = XmlUtilities.GetIntValue(xmlNodeShortCircuitVaryField, Consts.STRXML_speed);
                Logfile.Write(STRLOG_Speed + this.configParameters.speed.ToString());

                //
                // Maximum field current, sync field increases and stator current
                //
                this.configParameters.maxFieldCurrent = XmlUtilities.GetRealValue(xmlNodeShortCircuitVaryField, Consts.STRXML_maxFieldCurrent);
                this.configParameters.maxSyncFieldIncreases = XmlUtilities.GetIntValue(xmlNodeShortCircuitVaryField, Consts.STRXML_maxSyncFieldIncreases);
                this.configParameters.maxStatorCurrent = XmlUtilities.GetRealValue(xmlNodeShortCircuitVaryField, Consts.STRXML_maxStatorCurrent);
                Logfile.Write(STRLOG_MaxFieldCurrent + this.configParameters.maxFieldCurrent.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_MaxSyncFieldIncreases + this.configParameters.maxSyncFieldIncreases.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_MaxStatorCurrent + this.configParameters.maxStatorCurrent.ToString());

                //
                // Load the XML measurements template and check that all required XML nodes exist
                //
                this.xmlNodeMeasurementsTemplate = XmlUtilities.GetXmlNode(xmlNodeShortCircuitVaryField, Consts.STRXML_measurements);

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
                // Speed
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_speed, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_speed);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_units, true);
                XmlUtilities.GetXmlValue(xmlNodeParam, Consts.STRXMLPARAM_format, false);

                //
                // Stator current
                //
                XmlUtilities.GetXmlValue(this.xmlNodeMeasurementsTemplate, Consts.STRXML_statorCurrent, true);
                xmlNodeParam = XmlUtilities.GetXmlNode(this.xmlNodeMeasurementsTemplate, Consts.STRXML_statorCurrent);
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
                    float[] speed = new float[this.measurements.valueList.Count];
                    float[] statorCurrent = new float[this.measurements.valueList.Count];
                    for (int i = 0; i < this.measurements.valueList.Count; i++)
                    {
                        fieldCurrent[i] = (float)this.measurements.valueList[i].fieldCurrent;
                        speed[i] = (float)this.measurements.valueList[i].speed;
                        statorCurrent[i] = (float)this.measurements.valueList[i].statorCurrent;
                    }

                    //
                    // Fill in the measurements and units
                    //
                    XmlUtilities.SetXmlValue(xmlNodeMeasurements, Consts.STRXML_count, this.measurements.valueList.Count);

                    XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_fieldCurrent);
                    string strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_fieldCurrent, fieldCurrent, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.fieldCurrent, false);

                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_speed);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_speed, speed, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.speed, false);

                    xmlNode = XmlUtilities.GetXmlNode(xmlNodeMeasurements, Consts.STRXML_statorCurrent);
                    strFormat = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXMLPARAM_format, true);
                    XmlUtilities.SetXmlValues(xmlNodeMeasurements, Consts.STRXML_statorCurrent, statorCurrent, strFormat, Consts.CHR_Splitter, false);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXMLPARAM_units, this.measurements.units.statorCurrent, false);

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

                    case States.CheckStatus:
                        //
                        // Check machine status before continuing
                        //
                        if ((success = this.CheckStatus()) == true)
                        {
                            state = States.InitialiseDCDrive;
                        }
                        else
                        {
                            // this.lastError already updated
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
                        if ((success = this.dcDrive.StartSpeedMode(this.configParameters.speed)) == true)
                        {
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
                            state = States.CloseContactorB;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.CloseContactorB:
                        //
                        // Close contactor B which shorts the stator terminals
                        //
                        if ((success = this.plc.CloseContactorB()) == true)
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
            int increaseSyncFieldCount = 0;
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
                        // Increase the sync. machine field by one increment
                        //
                        if ((success = this.plc.IncreaseSyncField(ref status)) == true)
                        {
                            //
                            // Check if the sync field current was increased successfully
                            //
                            if (status == false)
                            {
                                //
                                // Unable to increase field current any further, this is not an error
                                //
                                logMessage = STRLOG_UnableToIncreaseFieldCurrent;
                                Logfile.Write(logMessage);
                                Trace.WriteLine(logMessage);
                            }
                            else
                            {
                                state = States.CheckStatorCurrent;
                            }
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.CheckStatorCurrent:
                        //
                        // Get stator phase current and store
                        //
                        if ((success = this.plc.PowerMeter.GetPhaseCurrent(ref value, ref units)) == true)
                        {
                            logMessage = String.Format(STRLOG_StatorCurrentValueUnits_Fmt, value, units);
                            Logfile.Write(logMessage);
                            Trace.WriteLine(logMessage);
                        }

                        //
                        // Check if the maximum stator current has been reached
                        //
                        if (value < this.configParameters.maxStatorCurrent)
                        {
                            state = States.CheckSyncFieldCurrent;
                        }
                        else
                        {
                            //
                            // Maximum stator current has been reached, all done here
                            //
                            logMessage = STRLOG_MaxStatorCurrentReached + this.configParameters.maxStatorCurrent.ToString();
                            Logfile.Write(logMessage);
                            Trace.WriteLine(logMessage);
                            state = States.Done;
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
                                state = States.CheckMaxSyncFieldCount;
                            }
                            else
                            {
                                //
                                // Maximum field current has been reached, all done here
                                //
                                logMessage = STRLOG_MaxFieldCurrentReached + this.configParameters.maxFieldCurrent.ToString();
                                Logfile.Write(logMessage);
                                Trace.WriteLine(logMessage);
                                state = States.Done;
                            }
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.CheckMaxSyncFieldCount:
                        //
                        // Check if maximum sync field incease count has been reached - should not happen
                        //
                        if ((success = (increaseSyncFieldCount++ < this.configParameters.maxSyncFieldIncreases)) == true)
                        {
                            Trace.WriteLine("increaseSyncFieldCount: " + increaseSyncFieldCount.ToString());
                            state = States.TakeMeasurements;
                        }
                        else
                        {
                            this.lastError = STRERR_MaxSyncFieldIncreasesReached + increaseSyncFieldCount.ToString();
                        }
                        break;

                    case States.TakeMeasurements:
                        //
                        // Wait a moment before taking the measurements
                        //
                        this.WaitDelay(1);

                        //
                        // Take measurements
                        //
                        if ((success = this.TakeMeasurements()) == true)
                        {
                            state = States.CheckStatus;
                        }
                        else
                        {
                            // this.lastError already updated
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
            // All of these need to be executed whether or not an error occurs
            //
            success = (
                this.plc.ResetSyncField() == true &&
                this.plc.OpenContactorB() == true &&
                this.plc.EnableSyncField(false) == true &&
                this.dcDrive.Stop() == true &&
                this.plc.EnableDCDrive(false) == true &&
                this.dcDrive.SetMaxTorqueLimit(DCDrive.DEFAULT_Motor1TorqueMaxLimit) == true
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

        private bool TakeMeasurements()
        {
            const string STRLOG_MethodName = "TakeMeasurements";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            string logMessage;

            //
            // Create the data structure for storing the measurements
            //
            Measurement measurement = new Measurement();

            //
            // Initialise state machine
            //
            this.lastError = null;
            States state = States.MeasureSpeed;

            //
            // State machine loop
            //
            while (state != States.Done)
            {
                double value = 0;
                string units = String.Empty;

                Trace.WriteLine("state: " + state.ToString());

                switch (state)
                {
                    case States.MeasureSpeed:
                        //
                        // Measure the speed
                        //
                        if ((success = this.dcDrive.GetSpeed(ref value, ref units)) == true)
                        {
                            //
                            // Save the speed measurement
                            //
                            measurement.speed = value;
                            if (this.measurements.units.speed == null)
                            {
                                this.measurements.units.speed = units;
                            }
                            state = States.MeasureFieldCurrent;
                        }
                        else
                        {
                            this.lastError = this.dcDrive.LastError;
                        }
                        break;

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
                            state = States.MeasureStatorCurrent;
                        }
                        else
                        {
                            this.lastError = this.plc.LastError;
                        }
                        break;

                    case States.MeasureStatorCurrent:
                        //
                        // Measure the stator phase current
                        //
                        if ((success = this.plc.PowerMeter.GetPhaseCurrent(ref value, ref units)) == true)
                        {
                            //
                            // Save the stator phase current measurement
                            //
                            measurement.statorCurrent = value;
                            if (this.measurements.units.statorCurrent == null)
                            {
                                this.measurements.units.statorCurrent = units;
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
                // Add the measurements to the list
                //
                this.measurements.valueList.Add(measurement);

                logMessage = String.Format(STRLOG_MeasurementsValueUnits_Fmt,
                    measurement.fieldCurrent, this.measurements.units.fieldCurrent,
                    measurement.speed, this.measurements.units.speed,
                    measurement.statorCurrent, this.measurements.units.statorCurrent);
                Logfile.Write(logMessage);
                Trace.WriteLine(logMessage);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

    }
}
