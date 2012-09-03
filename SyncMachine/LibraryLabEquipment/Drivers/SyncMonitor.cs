using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Library.Lab;
using Modbus.Device;

namespace Library.LabEquipment.Drivers
{
    public class SyncMonitor
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "SyncMonitor";

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
        private int ipport;

        #region Registers

        private const ushort MODBUSREGS_INPUTREGS_BASEADDRESS = 30001;

        private enum ModbusRegs_InputRegs_RO
        {
            GenVoltageL1L2 = 30208,
            GenVoltageL2L3 = 30209,
            GenVoltageL3L1 = 30210,
            GenVoltageL1N = 30211,
            GenVoltageL2N = 30212,
            GenVoltageL3N = 30213,
            GenVoltageFreqL1 = 30214,
            GenVoltageFreqL2 = 30215,
            GenVoltageFreqL3 = 30216,
            UGenPhaseL1L2 = 30217,
            UGenPhaseL2L3 = 30218,
            UGenPhaseL3L1 = 30219,
            GenCurrentL1 = 30220,
            GenCurrentL2 = 30221,
            GenCurrentL3 = 30222,
            GenPowerL1 = 30223,
            GenPowerL2 = 30224,
            GenPowerL3 = 30225,
            GenPower = 30226,
            GenReactivePowerL1 = 30227,
            GenReactivePowerL2 = 30228,
            GenReactivePowerL3 = 30229,
            GenReactivePower = 30230,
            GenApparentPowerL1 = 30231,
            GenApparentPowerL2 = 30232,
            GenApparentPowerL3 = 30233,
            GenApparentPower = 30234,
            CounterHiReactiveEnergy = 30235,
            CounterLoReactiveEnergy = 30236,
            CounterHiActiveEnergyDay = 30237,
            CounterLoActiveEnergyDay = 30238,
            CounterHiActiveEnergyWeek = 30239,
            CounterLoActiveEnergyWeek = 30240,
            CounterHiActiveEnergyMonth = 30241,
            CounterLoActiveEnergyMonth = 30242,
            CounterHiActiveEnergyTotal = 30243,
            CounterLoActiveEnergyTotal = 30244,
            GenPowerFactor = 30245,
            BusbarVoltageL1L2 = 30246,
            BusbarVoltageL2L3 = 30247,
            BusbarVoltageL3L1 = 30248,
            BusbarVoltageL1N = 30249,
            BusbarVoltageL2N = 30250,
            BusbarVoltageL3N = 30251,
            BusbarVoltageFreqL1 = 30252,
            BusbarVoltageFreqL2 = 30253,
            BusbarVoltageFreqL3 = 30254,
            UBusbarPhaseL1L2 = 30255,
            UBusbarPhaseL2L3 = 30256,
            UBusbarPhaseL3L1 = 30257,
            UBusBarUGenPhaseL1 = 30258,
            UBusBarUGenPhaseL2 = 30259,
            UBusBarUGenPhaseL3 = 30260,
        };

        private struct RegisterMapping_InputRegs
        {
            public const int RAW_ZERO = 0;
            public const int RAW_FULL = 1;
            public const int RAW_OFFSET = 2;
            public const int RAW_LENGTH = 3;
            public const int ENG_ZERO = 0;
            public const int ENG_FULL = 1;
            public const int ENG_LENGTH = 2;

            public int register;
            public short[] raw;
            public double[] eng;
            public string units;
            public string comment;

            public RegisterMapping_InputRegs(int register, short raw_zero, short raw_full, short raw_offset, double eng_zero, double eng_full, string units, string comment)
            {
                this.register = register;
                this.raw = new short[RAW_LENGTH] { raw_zero, raw_full, raw_offset };
                this.eng = new double[ENG_LENGTH] { eng_zero, eng_full };
                this.units = units;
                this.comment = comment;
            }
        }

        private RegisterMapping_InputRegs[] RegisterMap_InputRegs = new RegisterMapping_InputRegs[] {
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenVoltageL1L2, -32768, 32767, 0, -3276.8, 3276.7, "V", "Generator Voltage Line1-Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenVoltageL2L3, -32768, 32767, 0, -3276.8, 3276.7, "V", "Generator Voltage Line2-Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenVoltageL3L1, -32768, 32767, 0, -3276.8, 3276.7, "V", "Generator Voltage Line3-Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenVoltageL1N, -32768, 32767, 0, -3276.8, 3276.7, "V", "Generator Voltage Line1-Neutral"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenVoltageL2N, -32768, 32767, 0, -3276.8, 3276.7, "V", "Generator Voltage Line2-Neutral"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenVoltageL3N, -32768, 32767, 0, -3276.8, 3276.7, "V", "Generator Voltage Line3-Neutral"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenVoltageFreqL1, -32768, 32767, 0, -327.68, 327.67, "Hz", "Generator Voltage Frequency Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenVoltageFreqL2, -32768, 32767, 0, -327.68, 327.67, "Hz", "Generator Voltage Frequency Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenVoltageFreqL3, -32768, 32767, 0, -327.68, 327.67, "Hz", "Generator Voltage Frequency Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.UGenPhaseL1L2, -32768, 32767, 0, -3276.8, 3276.7, "Deg", "Generator Phase Angle Line1-Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.UGenPhaseL2L3, -32768, 32767, 0, -3276.8, 3276.7, "Deg", "Generator Phase Angle Line2-Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.UGenPhaseL3L1, -32768, 32767, 0, -3276.8, 3276.7, "Deg", "Generator Phase Angle Line3-Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenCurrentL1, 0, 0, 0, 0, 0, "A", "Generator Current Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenCurrentL2, 0, 0, 0, 0, 0, "A", "Generator Current Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenCurrentL3, 0, 0, 0, 0, 0, "A", "Generator Current Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenPowerL1, -32768, 32767, 0, -3276.8, 3276.7, "kW", "Generator Power Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenPowerL2, -32768, 32767, 0, -3276.8, 3276.7, "kW", "Generator Power Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenPowerL3, -32768, 32767, 0, -3276.8, 3276.7, "kW", "Generator Power Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenPower, -32768, 32767, 0, -3276.8, 3276.7, "kW", "Generator Power"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenReactivePowerL1, -32768, 32767, 0, -3276.8, 3276.7, "kVar", "Generator Reactive Power Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenReactivePowerL2, -32768, 32767, 0, -3276.8, 3276.7, "kVar", "Generator Reactive Power Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenReactivePowerL3, -32768, 32767, 0, -3276.8, 3276.7, "kVar", "Generator Reactive Power Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenReactivePower, -32768, 32767, 0, -3276.8, 3276.7, "kVar", "Generator Reactive Power"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenApparentPowerL1, -32768, 32767, 0, -3276.8, 3276.7, "kVA", "Generator Apparent Power Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenApparentPowerL2, -32768, 32767, 0, -3276.8, 3276.7, "kVA", "Generator Apparent Power Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenApparentPowerL3, -32768, 32767, 0, -3276.8, 3276.7, "kVA", "Generator Apparent Power Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenApparentPower, -32768, 32767, 0, -3276.8, 3276.7, "kVA", "Generator Apparent Power"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterHiReactiveEnergy, 0, 0, 0, 0, 0, "kVarh", "Reactive Energy counter high"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterLoReactiveEnergy, 0, 0, 0, 0, 0, "kVarh", "Reactive Energy counter low"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterHiActiveEnergyDay, 0, 0, 0, 0, 0, "kWh", "Active Energy counter day high"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterLoActiveEnergyDay, 0, 0, 0, 0, 0, "kWh", "Active Energy counter day low"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterHiActiveEnergyWeek, 0, 0, 0, 0, 0, "kWh", "Active Energy counter week high"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterLoActiveEnergyWeek, 0, 0, 0, 0, 0, "kWh", "Active Energy counter week low"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterHiActiveEnergyMonth, 0, 0, 0, 0, 0, "kWh", "Active Energy counter month high"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterLoActiveEnergyMonth, 0, 0, 0, 0, 0, "kWh", "Active Energy counter month low"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterHiActiveEnergyTotal, 0, 0, 0, 0, 0, "kWh", "Active Energy counter total high"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.CounterLoActiveEnergyTotal, 0, 0, 0, 0, 0, "kWh", "Active Energy counter total low"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.GenPowerFactor, -32768, 32767, 0, -327.68, 327.67, "", "Generator Voltage Frequency Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.BusbarVoltageL1L2, -32768, 32767, 0, -3276.8, 3276.7, "V", "Busbar Voltage Line1-Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.BusbarVoltageL2L3, -32768, 32767, 0, -3276.8, 3276.7, "V", "Busbar Voltage Line2-Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.BusbarVoltageL3L1, -32768, 32767, 0, -3276.8, 3276.7, "V", "Busbar Voltage Line3-Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.BusbarVoltageL1N, -32768, 32767, 0, -3276.8, 3276.7, "V", "Busbar Voltage Line1-Neutral"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.BusbarVoltageL2N, -32768, 32767, 0, -3276.8, 3276.7, "V", "Busbar Voltage Line2-Neutral"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.BusbarVoltageL3N, -32768, 32767, 0, -3276.8, 3276.7, "V", "Busbar Voltage Line3-Neutral"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.BusbarVoltageFreqL1, -32768, 32767, 0, -327.68, 327.67, "Hz", "Busbar Voltage Frequency Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.BusbarVoltageFreqL2, -32768, 32767, 0, -327.68, 327.67, "Hz", "Busbar Voltage Frequency Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.BusbarVoltageFreqL3, -32768, 32767, 0, -327.68, 327.67, "Hz", "Busbar Voltage Frequency Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.UBusbarPhaseL1L2, -32768, 32767, 0, -3276.8, 3276.7, "Deg", "Busbar Phase Line1-Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.UBusbarPhaseL2L3, -32768, 32767, 0, -3276.8, 3276.7, "Deg", "Busbar Phase Line2-Line3"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.UBusbarPhaseL3L1, -32768, 32767, 0, -3276.8, 3276.7, "Deg", "Busbar Phase Line3-Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.UBusBarUGenPhaseL1, -32768, 32767, 0, -3276.8, 3276.7, "Deg", "Busbar-Generator Phase Angle Line1"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.UBusBarUGenPhaseL2, -32768, 32767, 0, -3276.8, 3276.7, "Deg", "Busbar-Generator Phase Angle Line2"),
            new RegisterMapping_InputRegs((int)ModbusRegs_InputRegs_RO.UBusBarUGenPhaseL3, -32768, 32767, 0, -3276.8, 3276.7, "Deg", "Busbar-Generator Phase Angle Line3"),
        };

        #endregion // Registers

        #endregion // Class Constants and Variables

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

        public SyncMonitor(string ipaddr, int ipport)
        {
            const string STRLOG_MethodName = "SyncMonitor";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            this.ipaddr = ipaddr;
            this.ipport = ipport;

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
                ushort[] values = null;
                if (this.ReadInputRegisters(RegisterMap_InputRegs[0].register, ref values, RegisterMap_InputRegs.Length) == true)
                {
                    for (int i = 0; i < RegisterMap_InputRegs.Length; i++)
                    {
                        RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[i];
                        logMessage = String.Format("[{0,-5:d}]: ", mapping.register);

                        //
                        // Convert the register value to eng. units
                        //
                        double engValue = this.ConvertRawToEng(values[i], mapping.raw, mapping.eng);
                        logMessage += String.Format("Value: {0,-6}  Eng: {1,6:f01} {2,-5} {3}", values[i], engValue, mapping.units, mapping.comment);
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

        public bool GetMainsVoltage(ref double voltage, ref string units)
        {
            bool success;

            //
            // Find the register mapping
            //
            int index = this.FindRegisterMapping(ModbusRegs_InputRegs_RO.BusbarVoltageL1L2);
            RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[index];

            //
            // Read the register
            //
            ushort value = 0;
            if ((success = this.ReadInputRegister(mapping.register, ref value)) == true)
            {
                // Convert the register value to eng. units
                voltage = this.ConvertRawToEng(value, mapping.raw, mapping.eng);
                units = mapping.units;
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetMainsFrequency(ref double frequency, ref string units)
        {
            bool success;

            //
            // Find the register mapping
            //
            int index = this.FindRegisterMapping(ModbusRegs_InputRegs_RO.BusbarVoltageFreqL1);
            RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[index];

            //
            // Read the register
            //
            ushort value = 0;
            if ((success = this.ReadInputRegister(mapping.register, ref value)) == true)
            {
                // Convert the register value to eng. units
                frequency = this.ConvertRawToEng(value, mapping.raw, mapping.eng);
                units = mapping.units;
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetSyncVoltage(ref double voltage, ref string units)
        {
            bool success;

            //
            // Find the register mapping
            //
            int index = this.FindRegisterMapping(ModbusRegs_InputRegs_RO.GenVoltageL1L2);
            RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[index];

            //
            // Read the register
            //
            ushort value = 0;
            if ((success = this.ReadInputRegister(mapping.register, ref value)) == true)
            {
                // Convert the register value to eng. units
                voltage = this.ConvertRawToEng(value, mapping.raw, mapping.eng);
                units = mapping.units;
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetSyncFrequency(ref double frequency, ref string units)
        {
            bool success;

            //
            // Find the register mapping
            //
            int index = this.FindRegisterMapping(ModbusRegs_InputRegs_RO.GenVoltageFreqL1);
            RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[index];

            //
            // Read the register
            //
            ushort value = 0;
            if ((success = this.ReadInputRegister(mapping.register, ref value)) == true)
            {
                // Convert the register value to eng. units
                frequency = this.ConvertRawToEng(value, mapping.raw, mapping.eng);
                units = mapping.units;
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetSyncMainsPhase(ref double degrees, ref string units)
        {
            bool success;

            //
            // Find the register mapping
            //
            int index = this.FindRegisterMapping(ModbusRegs_InputRegs_RO.UBusBarUGenPhaseL1);
            RegisterMapping_InputRegs mapping = RegisterMap_InputRegs[index];

            //
            // Read the register
            //
            ushort value = 0;
            if ((success = this.ReadInputRegister(mapping.register, ref value)) == true)
            {
                // Convert the register value to eng. units
                degrees = this.ConvertRawToEng(value, mapping.raw, mapping.eng);
                units = mapping.units;
            }

            return success;
        }

        //=================================================================================================//

        private bool ReadInputRegister(int register, ref ushort value)
        {
            ushort[] values = null;

            bool success = this.ReadInputRegisters(register, ref values, 1);
            value = values[0];

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool ReadInputRegisters(int register, ref ushort[] value, int count)
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
                // Read multiple inputs
                //
                value = this.modbusIpMaster.ReadInputRegisters((byte)this.slaveId, protAddress, (ushort)count);

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

        private double ConvertRawToEng(ushort rawValue, short[] raw, double[] eng)
        {
            //
            // Convert from unsigned short to signed double
            //
            double engValue = (double)((short)rawValue);

            //
            // Add the offset to the value and scale
            //
            if (eng[RegisterMapping_InputRegs.ENG_FULL] - eng[RegisterMapping_InputRegs.ENG_ZERO] > 0)
            {
                engValue = (engValue + raw[RegisterMapping_InputRegs.RAW_OFFSET])
                    * (eng[RegisterMapping_InputRegs.ENG_FULL] - eng[RegisterMapping_InputRegs.ENG_ZERO])
                    / (raw[RegisterMapping_InputRegs.RAW_FULL] - raw[RegisterMapping_InputRegs.RAW_ZERO]);
            }

            return engValue;
        }

        //-------------------------------------------------------------------------------------------------//

        private ushort ConvertEngToRaw(double engValue, double[] eng, short[] raw)
        {
            double rawValue = engValue;

            //
            // Unscale the value and subtract the offset
            //
            if (eng[RegisterMapping_InputRegs.ENG_FULL] - eng[RegisterMapping_InputRegs.ENG_ZERO] > 0)
            {
                rawValue = rawValue
                    * (raw[RegisterMapping_InputRegs.RAW_FULL] - raw[RegisterMapping_InputRegs.RAW_ZERO])
                    / (eng[RegisterMapping_InputRegs.ENG_FULL] - eng[RegisterMapping_InputRegs.ENG_ZERO])
                    - raw[RegisterMapping_InputRegs.RAW_OFFSET];
            }

            //
            // Convert from signed double to unsigned short
            //
            return (ushort)((short)rawValue);
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
