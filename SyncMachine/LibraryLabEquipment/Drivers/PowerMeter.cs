using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using Library.Lab;
using Library.Unmanaged;
using Modbus.Device;

namespace Library.LabEquipment.Drivers
{
    public class PowerMeter
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "PowerMeter";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Success = " Success: ";

        //
        // String constants for error messages
        //
        //private const string STRERR_ReadAllRegistersFailed = "Read all registers failed!";
        //private const string STRERR_RegisterWriteReadMismatch = "Register write - read mismatch!";

        //
        // Local variables
        //
        private Logfile.LoggingLevels logLevel;
        private string ipaddr;
        private int port;

        #endregion // Class Constants and Variables

        #region Registers

        private const ushort MODBUSREGS_INPUTREGS_BASEADDRESS = 30001;

        private enum ModbusRegs_InputRegs_RO
        {
            Phase1ToNeutralVoltageTHD = 30108, // THD = Total Harmonic Distortion
            Phase2ToNeutralVoltageTHD = 30110,
            Phase3ToNeutralVoltageTHD = 30112,
            Phase1ToPhase2VoltageTHD = 30114,
            Phase2ToPhase3VoltageTHD = 30116,
            Phase3ToPhase1VoltageTHD = 30118,
            Phase1CurrentTHD = 30120,
            Phase2CurrentTHD = 30122,
            Phase3CurrentTHD = 30124,
            FrequencyPhase1 = 30126,
            Phase1ToNeutralVoltageRMS = 30128, // RMS = Root Mean Square
            Phase2ToNeutralVoltageRMS = 30130,
            Phase3ToNeutralVoltageRMS = 30132,
            Phase1ToPhase2VoltageRMS = 30134,
            Phase2ToPhase3VoltageRMS = 30136,
            Phase3ToPhase1VoltageRMS = 30138,
            Phase1CurrentRMS = 30140,
            Phase2CurrentRMS = 30142,
            Phase3CurrentRMS = 30144,
            NeutralCurrentRMS = 30146,
            Phase1ActivePower = 30148,
            Phase2ActivePower = 30150,
            Phase3ActivePower = 30152,
            Phase1ReactivePower = 30154,
            Phase2ReactivePower = 30156,
            Phase3ReactivePower = 30158,
            Phase1ApparentPower = 30160,
            Phase2ApparentPower = 30162,
            Phase3ApparentPower = 30164,
            Phase1PowerFactor = 30166,
            Phase2PowerFactor = 30168,
            Phase3PowerFactor = 30170,
            PhaseToNeutralVoltageMeanTHD = 30172,
            PhaseToPhaseVoltageMeanTHD = 30174,
            PhaseCurrentMeanTHD = 30176,
            PhaseToNeutralVoltageMeanRMS = 30178,
            PhaseToPhaseVoltageMeanRMS = 30180,
            ThreePhaseCurrentRMS = 30182,
            TotalActivePower = 30184,
            TotalReactivePower = 30186,
            TotalApparentPower = 30188,
            TotalPowerFactor = 30190,
        };

        private struct RegisterMapping_InputRegs
        {
            public int register;
            public string units;
            public string comment;

            public RegisterMapping_InputRegs(int register, string units, string comment)
            {
                this.register = register;
                this.units = units;
                this.comment = comment;
            }
        }

        private RegisterMapping_InputRegs[] RegisterMap_InputRegs = new RegisterMapping_InputRegs[] {
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1ToNeutralVoltageTHD, "%", "Phase 1 to Neutral Voltage, THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2ToNeutralVoltageTHD, "%", "Phase 2 to Neutral Voltage, THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3ToNeutralVoltageTHD, "%", "Phase 3 to Neutral Voltage, THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1ToPhase2VoltageTHD, "%", "Phase 1 to Phase Voltage, THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2ToPhase3VoltageTHD, "%", "Phase 2 to Phase Voltage, THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3ToPhase1VoltageTHD, "%", "Phase 3 to Phase Voltage, THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1CurrentTHD, "%", "Phase 1 Current, THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2CurrentTHD, "%", "Phase 2 Current, THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3CurrentTHD, "%", "Phase 3 Current, THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.FrequencyPhase1, "Hz", "Frequency of Phase 1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1ToNeutralVoltageRMS, "Volts", "Phase 1 to Neutral Voltage, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2ToNeutralVoltageRMS, "Volts", "Phase 2 to Neutral Voltage, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3ToNeutralVoltageRMS, "Volts", "Phase 3 to Neutral Voltage, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1ToPhase2VoltageRMS, "Volts", "Phase 1 to Phase 2 Voltage, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2ToPhase3VoltageRMS, "Volts", "Phase 2 to Phase 3 Voltage, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3ToPhase1VoltageRMS, "Volts", "Phase 3 to Phase 1 Voltage, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1CurrentRMS, "Amps", "Phase 1 Current, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2CurrentRMS, "Amps", "Phase 2 Current, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3CurrentRMS, "Amps", "Phase 3 Current, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.NeutralCurrentRMS, "Amps", "Neutral Current, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1ActivePower, "Watts", "Phase 1 Active Power (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2ActivePower, "Watts", "Phase 2 Active Power (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3ActivePower, "Watts", "Phase 3 Active Power (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1ReactivePower, "var", "Phase 1 Reactive Power (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2ReactivePower, "var", "Phase 2 Reactive Power (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3ReactivePower, "var", "Phase 3 Reactive Power (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1ApparentPower, "VA", "Phase 1 Apparent Power"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2ApparentPower, "VA", "Phase 2 Apparent Power"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3ApparentPower, "VA", "Phase 3 Apparent Power"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase1PowerFactor, "", "Phase 1 Power Factor (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase2PowerFactor, "", "Phase 2 Power Factor (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.Phase3PowerFactor, "", "Phase 3 Power Factor (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.PhaseToNeutralVoltageMeanTHD, "%", "Phase to Neutral Voltage, Mean THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.PhaseToPhaseVoltageMeanTHD, "%", "Phase to Phase Voltage, Mean THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.PhaseCurrentMeanTHD, "%", "Phase Current, Mean THD"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.PhaseToNeutralVoltageMeanRMS, "Volts", "Phase to Neutral Voltage, Mean RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.PhaseToPhaseVoltageMeanRMS, "Volts", "Phase to Phase Voltage, Mean RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.ThreePhaseCurrentRMS, "Amps", "Three Phase Current, RMS Amplitude"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.TotalActivePower, "Watts", "Total Active Power (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.TotalReactivePower, "var", "Total Reactive Power (Imp/Exp)"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.TotalApparentPower, "VA", "Total Apparent Power"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.TotalPowerFactor, "", "Total Power Factor (Imp/Exp)"),
        };

        #endregion // Registers

        #region Properties

        private string lastError;
        private int receiveTimeout;
        private int slaveId;
        private ModbusIpMaster modbusIpMaster;

        public string LastError
        {
            get
            {
                string errorMsg = lastError;
                lastError = null;
                return errorMsg;
            }
        }

        public int ReceiveTimeout
        {
            get { return this.receiveTimeout; }
            set { this.receiveTimeout = value; }
        }

        public int SlaveId
        {
            get { return this.slaveId; }
            set { this.slaveId = value; }
        }

        public ModbusIpMaster ModbusIpMaster
        {
            get { return this.modbusIpMaster; }
            set { this.modbusIpMaster = value; }
        }

        #endregion // Properties

        //-------------------------------------------------------------------------------------------------//

        public PowerMeter(string ipaddr, int port)
        {
            const string STRLOG_MethodName = "PowerMeter";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            this.ipaddr = ipaddr;
            this.port = port;

            try
            {
                //
                // Initialise local variables
                //
                this.lastError = null;

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

        public bool ReadRegisters()
        {
            const string STRLOG_MethodName = "ReadAllRegisters";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;
            this.lastError = null;
            string logMessage;

            try
            {
                //
                // The register map is contiguous so they can be read with one command
                //
                float[] values = null;
                if (this.ReadInputRegisters(RegisterMap_InputRegs[0].register, ref values, RegisterMap_InputRegs.Length) == true)
                {
                    for (int i = 0; i < RegisterMap_InputRegs.Length; i++)
                    {
                        RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[i];
                        logMessage = String.Format("[{0,-5:d}]: Value: {1:f04} {2,-5}  {3}", mapping.register, values[i], mapping.units, mapping.comment);
                        Logfile.Write(logMessage);
                        Trace.WriteLine(logMessage);
                    }
                }
                Trace.WriteLine(String.Empty);

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                Trace.WriteLine(ex.Message);
                this.lastError = ex.Message;
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetPhaseToPhaseVoltage(ref double value, ref string units)
        {
            return GetRegisterValue(ModbusRegs_InputRegs_RO.PhaseToPhaseVoltageMeanRMS, ref value, ref units);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetFrequency(ref double value, ref string units)
        {
            return GetRegisterValue(ModbusRegs_InputRegs_RO.FrequencyPhase1, ref value, ref units);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetPhaseCurrent(ref double value, ref string units)
        {
            return GetRegisterValue(ModbusRegs_InputRegs_RO.Phase1CurrentRMS, ref value, ref units);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetRealPower(ref double value, ref string units)
        {
            return GetRegisterValue(ModbusRegs_InputRegs_RO.TotalActivePower, ref value, ref units);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetReactivePower(ref double value, ref string units)
        {
            return GetRegisterValue(ModbusRegs_InputRegs_RO.TotalReactivePower, ref value, ref units);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetPowerFactor(ref double value, ref string units)
        {
            return GetRegisterValue(ModbusRegs_InputRegs_RO.TotalPowerFactor, ref value, ref units);
        }

        //=================================================================================================//

        private bool GetRegisterValue(ModbusRegs_InputRegs_RO register, ref double value, ref string units)
        {
            bool success;

            //
            // Find the register mapping
            //
            int index = this.FindRegisterMapping(register);
            RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[index];

            //
            // The register map is contiguous so they can be read with one command
            //
            float[] values = null;
            if ((success = this.ReadInputRegisters(RegisterMap_InputRegs[0].register, ref values, RegisterMap_InputRegs.Length)) == true)
            {
                //
                // Get the value and the units
                //
                value = values[index];
                units = mapping.units;
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool ReadInputRegister(int register, ref float value)
        {
            float[] values = null;

            bool success = this.ReadInputRegisters(register, ref values, 1);
            value = values[0];

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool ReadInputRegisters(int register, ref float[] values, int count)
        {
            bool success = false;
            this.lastError = null;

            try
            {
                //
                // Convert register to Modbus protocol address
                //
                ushort protAddress = (ushort)(register - MODBUSREGS_INPUTREGS_BASEADDRESS);

                //
                // Read multiple inputs, need two 16-bit registers for each 32-bit float
                //
                ushort[] inregs = this.modbusIpMaster.ReadInputRegisters((byte)this.slaveId, protAddress, (ushort)(count * 2));

                //
                // Convert two registers to a single float value
                //
                values = new float[count];
                for (int i = 0, j = 0; i < inregs.Length; i += 2, j++)
                {
                    int regValue = inregs[i];
                    regValue = (regValue << 16) | inregs[i + 1];
                    values[j] = Conversion.ToFloat(regValue);
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
                Trace.WriteLine(ex.Message);
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private int FindRegisterMapping(ModbusRegs_InputRegs_RO register)
        {
            int index = -1;

            for (int i = 0; i < RegisterMap_InputRegs.Length; i++)
            {
                if ((int)register == RegisterMap_InputRegs[i].register)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

    }
}
