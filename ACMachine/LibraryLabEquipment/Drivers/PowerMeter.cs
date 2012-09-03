using System;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using Library.Unmanaged;
using Modbus.Device;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    class PowerMeter
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "PowerMeter";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Register = " Register: ";
        private const string STRLOG_Writing = " Writing: ";
        private const string STRLOG_Reading = " Reading: ";
        //private const string STRLOG_WritingHex = " Writing: 0x";
        //private const string STRLOG_ReadingHex = " Reading: 0x";
        private const string STRLOG_Amps = " amps";
        private const string STRLOG_Volts = " volts";
        private const string STRLOG_ConvertsTo = " => ";

        //
        // Power meter register addresses (read only)
        //
        private const ushort REGADDR_RW_PM_VoltagePhaseToPhaseVsd = 1072;
        private const ushort REGADDR_RW_PM_CurrentThreePhaseVsd = 1074;
        private const ushort REGADDR_RW_PM_PowerFactorAverageVsd = 1082;
        private const ushort REGADDR_RW_PM_VoltagePhaseToPhaseMut = 2072;
        private const ushort REGADDR_RW_PM_CurrentThreePhaseMut = 2074;
        private const ushort REGADDR_RW_PM_PowerFactorAverageMut = 2082;

        // Local variables
        private Logfile.LoggingLevels logLevel;
        private ModbusIpMaster master;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public PowerMeter(ModbusIpMaster master)
        {
            const string STRLOG_MethodName = "PowerMeter";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            this.master = master;

            //
            // Determine the logging level for this class
            //
            try
            {
                this.logLevel = (Logfile.LoggingLevels)Utilities.GetIntAppSetting(STRLOG_ClassName);
            }
            catch
            {
                this.logLevel = Logfile.LoggingLevels.Minimum;
            }
            Logfile.Write(Logfile.STRLOG_LogLevel + this.logLevel.ToString());

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public float ReadVoltagePhaseToPhaseMut()
        {
            const string STRLOG_MethodName = "ReadVoltagePhaseToPhaseMut";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            return ReadVoltagePhaseToPhase(REGADDR_RW_PM_VoltagePhaseToPhaseMut);
        }

        //-------------------------------------------------------------------------------------------------//

        public float ReadVoltagePhaseToPhaseVsd()
        {
            const string STRLOG_MethodName = "ReadVoltagePhaseToPhaseVsd";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            return ReadVoltagePhaseToPhase(REGADDR_RW_PM_VoltagePhaseToPhaseVsd);
        }

        //-------------------------------------------------------------------------------------------------//

        public float ReadCurrentThreePhaseMut()
        {
            const string STRLOG_MethodName = "ReadCurrentThreePhaseMut";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            return ReadCurrentThreePhase(REGADDR_RW_PM_CurrentThreePhaseMut);
        }

        //-------------------------------------------------------------------------------------------------//

        public float ReadCurrentThreePhaseVsd()
        {
            const string STRLOG_MethodName = "ReadCurrentThreePhaseVsd";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            return ReadCurrentThreePhase(REGADDR_RW_PM_CurrentThreePhaseVsd);
        }

        //-------------------------------------------------------------------------------------------------//

        public float ReadPowerFactorAverageMut()
        {
            const string STRLOG_MethodName = "ReadPowerFactorAverageMut";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            return ReadPowerFactorAverage(REGADDR_RW_PM_PowerFactorAverageMut);
        }

        //-------------------------------------------------------------------------------------------------//

        public float ReadPowerFactorAverageVsd()
        {
            const string STRLOG_MethodName = "ReadPowerFactorAverageVsd";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            return ReadPowerFactorAverage(REGADDR_RW_PM_PowerFactorAverageVsd);
        }

        //=================================================================================================//

        private float ReadVoltagePhaseToPhase(ushort regAddress)
        {
            string logMessage = string.Empty;

            float voltage;
            try
            {
                logMessage += STRLOG_Register + regAddress.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(regAddress, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to voltage
                voltage = Conversion.ToFloat(value);
                logMessage += STRLOG_ConvertsTo + voltage.ToString("F04") + STRLOG_Volts;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return voltage;
        }

        //-------------------------------------------------------------------------------------------------//

        private float ReadCurrentThreePhase(ushort regAddress)
        {
            string logMessage = string.Empty;

            float current;
            try
            {
                logMessage += STRLOG_Register + regAddress.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(regAddress, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to current
                current = Conversion.ToFloat(value);
                logMessage += STRLOG_ConvertsTo + current.ToString("F04") + STRLOG_Amps;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return current;
        }

        //-------------------------------------------------------------------------------------------------//

        private float ReadPowerFactorAverage(ushort regAddress)
        {
            string logMessage = string.Empty;

            float powerFactor;
            try
            {
                logMessage += STRLOG_Register + regAddress.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(regAddress, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to powerfactor
                powerFactor = Conversion.ToFloat(value);
                logMessage += STRLOG_ConvertsTo + powerFactor.ToString("F04");

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return powerFactor;
        }

    }
}
