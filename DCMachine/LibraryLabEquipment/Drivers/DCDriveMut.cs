using System;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using Modbus.Device;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class DCDriveMut
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DCDriveMut";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Register = " Register: ";
        private const string STRLOG_Writing = " Writing: ";
        private const string STRLOG_Reading = " Reading: ";
        private const string STRLOG_WritingHex = " Writing: 0x";
        private const string STRLOG_ReadingHex = " Reading: 0x";
        private const string STRLOG_Rpm = " RPM";
        private const string STRLOG_Volts = " volts";
        private const string STRLOG_Amps = " amps";
        private const string STRLOG_Percent = " percent";
        private const string STRLOG_Seconds = " seconds";
        private const string STRLOG_ConvertsTo = " => ";

        //
        // Configuration delays
        //
        public const int DELAY_ConfigureSpeed = 0;
        public const int DELAY_ConfigureTorque = 0;
        public const int DELAY_ConfigureMinSpeedLimit = 0;
        public const int DELAY_ConfigureMaxSpeedLimit = 0;
        public const int DELAY_ConfigureMinTorqueLimit = 0;
        public const int DELAY_ConfigureMaxTorqueLimit = 0;
        public const int DELAY_ConfigureSpeedRampTime = 0;
        public const int DELAY_ConfigureField = 0;
        public const int DELAY_ConfigureFieldTrim = 0;

        //
        // Command execution delay constants
        //
        public const int DELAY_ResetDriveFault = 5;
        public const int DELAY_ResetDrive = 5;
        public const int DELAY_SetMainContactorOn = 5;
        public const int DELAY_StartDrive = 5;
        public const int DELAY_StartDriveTorque = 5;
        public const int DELAY_SetSpeed = DEFAULT_SpeedRampTime + 2;
        public const int DELAY_SetTorque = 5;
        public const int DELAY_SetField = 5;

        //
        // Default values for control registers
        //
        public const int DEFAULT_Speed = 0;
        public const int DEFAULT_Torque = 0;
        public const int DEFAULT_MinSpeedLimit = -1500;
        public const int DEFAULT_MaxSpeedLimit = 1500;
        public const int DEFAULT_MinTorqueLimit = -100;
        public const int DEFAULT_MaxTorqueLimit = 100;
        public const int DEFAULT_SpeedRampTime = 5;
        public const int DEFAULT_Field = 100;
        public const int DEFAULT_FieldTrim = 0;

        //
        // Minimum and maximum values for control registers
        //
        public const int MINIMUM_Speed = -1500;
        public const int MAXIMUM_Speed = 1500;
        public const int MAXIMUM_MaximumCurrent = 10000;
        public const int MINIMUM_MinimumTorque = -50;
        public const int MINIMUM_MaximumTorque = 50;

        //
        // DC drive register addresses (read/write)
        //
        private const ushort REGADDR_RW_DC_ControlWord = 4200;
        private const ushort REGADDR_RW_DC_ControlSpeed = 4201;
        private const ushort REGADDR_RW_DC_ControlTorque = 4202;
        private const ushort REGADDR_RW_DC_MinSpeedLimit = 4203;
        private const ushort REGADDR_RW_DC_MaxSpeedLimit = 4204;
        private const ushort REGADDR_RW_DC_MaxTorqueLimit = 4205;
        private const ushort REGADDR_RW_DC_MinTorqueLimit = 4206;
        private const ushort REGADDR_RW_DC_SpeedRampTime = 4207;
        private const ushort REGADDR_RW_DC_FieldLimit = 4208;
        private const ushort REGADDR_RW_DC_FieldTrim = 4209;

        //
        // DC drive register addresses (read only)
        //
        private const ushort REGADDR_RO_DC_SpeedEncoder = 4002;
        private const ushort REGADDR_RO_DC_Torque = 4007;
        private const ushort REGADDR_RO_DC_ArmatureVoltage = 4013;
        private const ushort REGADDR_RO_DC_FieldCurrent = 4024;

        //
        // Local variables
        //
        private Logfile.LoggingLevels logLevel;
        private ModbusIpMaster master;

        #endregion

        #region Properties

        private string lastError;

        public string LastError
        {
            get
            {
                string errorMsg = lastError;
                lastError = null;
                return errorMsg;
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public DCDriveMut(ModbusIpMaster master)
        {
            const string STRLOG_MethodName = "DCDriveMut";

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

        public void ConfigureSpeed(int speed)
        {
            WriteSpeed(speed);
            WaitDelay(DELAY_ConfigureSpeed);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureTorque(int percent)
        {
            WriteTorque(percent);
            WaitDelay(DELAY_ConfigureTorque);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureMinSpeedLimit(int speed)
        {
            WriteMinSpeedLimit(speed);
            WaitDelay(DELAY_ConfigureMinSpeedLimit);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureMaxSpeedLimit(int speed)
        {
            WriteMaxSpeedLimit(speed);
            WaitDelay(DELAY_ConfigureMaxSpeedLimit);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureMinTorqueLimit(int percent)
        {
            WriteMinTorqueLimit(percent);
            WaitDelay(DELAY_ConfigureMinTorqueLimit);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureMaxTorqueLimit(int percent)
        {
            WriteMaxTorqueLimit(percent);
            WaitDelay(DELAY_ConfigureMaxTorqueLimit);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureSpeedRampTime(int time)
        {
            WriteSpeedRampTime(time);
            WaitDelay(DELAY_ConfigureSpeedRampTime);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureField(int percent)
        {
            WriteField(percent);
            WaitDelay(DELAY_ConfigureField);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ResetDriveFault()
        {
            const string STRLOG_MethodName = "ResetDriveFault";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            try
            {
                // Write multiple holding registers
                ushort[] regs = new ushort[1] { 0x04f6 };

                int value = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X4");

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlWord, (ushort)1);
                value = inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X4");

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);

                WaitDelay(DELAY_ResetDriveFault);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ResetDrive()
        {
            const string STRLOG_MethodName = "ResetDrive";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            try
            {
                // Write multiple holding registers
                ushort[] regs = new ushort[1] { 0x0476 };

                int value = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X4");

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlWord, (ushort)1);
                value = inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X4");

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);

                WaitDelay(DELAY_ResetDrive);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void SetMainContactorOn()
        {
            const string STRLOG_MethodName = "SetMainContactorOn";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0x0477 };

                int value = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X4");

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlWord, (ushort)1);
                value = inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X4");

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);

                WaitDelay(DELAY_SetMainContactorOn);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void StartDrive()
        {
            const string STRLOG_MethodName = "StartDrive";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0x047F };

                int value = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X4");

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlWord, (ushort)1);
                value = inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X4");

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);

                WaitDelay(DELAY_StartDrive);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void StartDriveTorque()
        {
            const string STRLOG_MethodName = "StartDriveTorque";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0x147F };

                int value = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X4");

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlWord, (ushort)1);
                value = inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X4");

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);

                WaitDelay(DELAY_StartDriveTorque);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void SetSpeed(int speed)
        {
            WriteSpeed(speed);
            WaitDelay(DELAY_SetSpeed);
        }

        //-------------------------------------------------------------------------------------------------//

        public void SetTorque(int percent)
        {
            WriteTorque(percent);
            WaitDelay(DELAY_SetTorque);
        }

        //-------------------------------------------------------------------------------------------------//

        public void SetField(int percent)
        {
            WriteField(percent);
            WaitDelay(DELAY_SetField);
        }

        //-------------------------------------------------------------------------------------------------//

        public void WaitDelay(int seconds)
        {
            try
            {
                for (int i = 0; i < seconds; i++)
                {
                    ReadControlWord(false);
                    Trace.Write(".");
                    Thread.Sleep(1000);
                }
                Trace.WriteLine("");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public ushort ReadControlWord(bool show)
        {
            const string STRLOG_MethodName = "ReadControlWord";

            if (show == true)
            {
                Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
                Trace.WriteLine(STRLOG_MethodName);
            }

            string logMessage = string.Empty;

            ushort controlWord = 0;

            try
            {
                logMessage += STRLOG_Register + REGADDR_RW_DC_ControlWord.ToString();

                // Read register
                ushort[] regs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlWord, (ushort)1);

                controlWord = regs[0];

                logMessage += STRLOG_ReadingHex + controlWord.ToString("x4");

                if (show == true)
                {
                    Logfile.Write(this.logLevel, logMessage);
                    Trace.WriteLine(logMessage);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return controlWord;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadControlSpeed()
        {
            const string STRLOG_MethodName = "ReadControlSpeed";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int speed = 0;

            try
            {
                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlSpeed, (ushort)1);
                int value = inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register value to speed: -20,000 to 20,000 => -1500 to 1500
                speed = (value * 1500) / 20000;
                logMessage += STRLOG_ConvertsTo + speed.ToString() + STRLOG_Rpm;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return speed;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadControlTorque()
        {
            const string STRLOG_MethodName = "ReadControlTorque";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int percent = 0;

            try
            {
                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlTorque, (ushort)1);
                int value = inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register value to percent: -32,768 to 32,767 => -327.68 to 327.67 
                percent = value / 100;
                logMessage += STRLOG_ConvertsTo + percent.ToString() + STRLOG_Percent;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return percent;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadMinSpeedLimit()
        {
            const string STRLOG_MethodName = "ReadMinSpeedLimit";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int speed = 0;

            try
            {
                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_MinSpeedLimit, (ushort)1);
                int value = inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert speed to register values: -10,000 to 0 => -10,000 to 0
                speed = value;
                speed = (speed > 32767) ? speed - 65536 : speed;
                logMessage += STRLOG_ConvertsTo + speed.ToString() + STRLOG_Rpm;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return speed;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadMaxSpeedLimit()
        {
            const string STRLOG_MethodName = "ReadMaxSpeedLimit";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int speed = 0;

            try
            {
                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_MaxSpeedLimit, (ushort)1);
                int value = inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert speed to register values: 0 to 10,000 => 0 to 10,000
                speed = value;
                logMessage += STRLOG_ConvertsTo + speed.ToString() + STRLOG_Rpm;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return speed;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadMinTorqueLimit()
        {
            const string STRLOG_MethodName = "ReadMinTorqueLimit";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int percent = 0;

            try
            {
                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_MinTorqueLimit, (ushort)1);
                int value = inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register value to percent: -32500 to 0 => -325 to 0
                percent = value;
                percent = (percent > 32767) ? percent - 65536 : percent;
                percent = percent / 100;
                logMessage += STRLOG_ConvertsTo + percent.ToString() + STRLOG_Percent;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return percent;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadMaxTorqueLimit()
        {
            const string STRLOG_MethodName = "ReadMaxTorqueLimit";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int percent = 0;

            try
            {
                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_MaxTorqueLimit, (ushort)1);
                int value = inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register value to percent: 0 to 32500 => 0 to 325
                percent = value / 100;
                logMessage += STRLOG_ConvertsTo + percent.ToString() + STRLOG_Percent;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return percent;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadSpeedRampTime()
        {
            const string STRLOG_MethodName = "ReadSpeedRampTime";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int seconds = 0;

            try
            {
                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_SpeedRampTime, (ushort)1);
                int value = inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register value to time: 0 to 30,000 => 0 to 300
                seconds = value / 100;
                logMessage += STRLOG_ConvertsTo + seconds.ToString() + STRLOG_Seconds;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadFieldLimit()
        {
            const string STRLOG_MethodName = "ReadFieldLimit";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int percent = 0;

            try
            {
                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_FieldLimit, (ushort)1);
                int value = inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register value to percent: 0 to 10,000 => 0 to 100
                percent = value / 100;
                logMessage += STRLOG_ConvertsTo + percent.ToString() + STRLOG_Percent;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return percent;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadFieldTrim()
        {
            const string STRLOG_MethodName = "ReadFieldTrim";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int percent = 0;

            try
            {
                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_FieldTrim, (ushort)1);
                int value = inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register value to percent: -200 to 200 => -20 to 20
                percent = value / 10;
                logMessage += STRLOG_ConvertsTo + percent.ToString() + STRLOG_Percent;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return percent;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadDriveSpeed()
        {
            const string STRLOG_MethodName = "ReadDriveSpeed";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int speed = 0;

            try
            {
                logMessage += STRLOG_Register + REGADDR_RO_DC_SpeedEncoder.ToString();

                // Read register
                ushort[] regs = this.master.ReadHoldingRegisters(REGADDR_RO_DC_SpeedEncoder, (ushort)1);

                // Convert the speed to an integer value
                speed = regs[0];
                speed = (speed > 32767) ? speed - 65536 : speed;
                logMessage += STRLOG_ConvertsTo + speed.ToString() + STRLOG_Rpm;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return speed;
        }

        //-------------------------------------------------------------------------------------------------//

        public int ReadArmatureVoltage()
        {
            const string STRLOG_MethodName = "ReadArmatureVoltage";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int voltage = 0;

            try
            {
                logMessage += STRLOG_Register + REGADDR_RO_DC_ArmatureVoltage.ToString();

                // Read register
                ushort[] regs = this.master.ReadHoldingRegisters(REGADDR_RO_DC_ArmatureVoltage, (ushort)1);

                // Convert the voltage to an integer value
                voltage = regs[0];
                voltage = (voltage > 32767) ? voltage - 65536 : voltage;
                logMessage += STRLOG_ConvertsTo + voltage.ToString() + STRLOG_Volts;

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

        public float ReadFieldCurrent()
        {
            const string STRLOG_MethodName = "ReadFieldCurrent";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            float current = 0;

            try
            {
                logMessage += STRLOG_Register + REGADDR_RO_DC_FieldCurrent.ToString();

                // Read register
                ushort[] regs = this.master.ReadHoldingRegisters(REGADDR_RO_DC_FieldCurrent, (ushort)1);

                // Convert current to an integer value
                current = regs[0];
                current /= 100;
                logMessage += STRLOG_ConvertsTo + current.ToString() + STRLOG_Amps;

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

        public int ReadTorque()
        {
            const string STRLOG_MethodName = "ReadTorque";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int percent = 0;

            try
            {
                logMessage += STRLOG_Register + REGADDR_RO_DC_Torque.ToString();

                // Read register
                ushort[] regs = this.master.ReadHoldingRegisters(REGADDR_RO_DC_Torque, (ushort)1);

                // Convert percentage to an integer value
                percent = regs[0];
                percent /= 100;
                logMessage += STRLOG_ConvertsTo + percent.ToString() + STRLOG_Percent;

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return percent;
        }

        //=================================================================================================//

        private void WriteSpeed(int speed)
        {
            const string STRLOG_MethodName = "WriteSpeed";

            string logMessage = speed.ToString() + STRLOG_Rpm;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0 };

                // Convert speed to register values: -1500 to 1500 => -20,000 to 20,000
                int speedValue = (speed * 20000) / 1500;
                regs[0] = (ushort)speedValue;

                int regValue = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_ControlSpeed.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_ControlSpeed, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlSpeed, (ushort)1);
                regValue = inregs[0];
                logMessage += STRLOG_Reading + regValue.ToString();

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void WriteTorque(int percent)
        {
            const string STRLOG_MethodName = "WriteTorque";

            string logMessage = percent.ToString() + STRLOG_Percent;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0 };

                // Convert percent to register values: -327 to 327 => -32700 to 32700
                int percentValue = percent * 100;
                regs[0] = (ushort)percentValue;

                int regValue = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_ControlTorque.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_ControlTorque, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_ControlTorque, (ushort)1);
                regValue = inregs[0];
                logMessage += STRLOG_Reading + regValue.ToString();

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void WriteMinSpeedLimit(int speed)
        {
            const string STRLOG_MethodName = "WriteMinSpeedLimit";

            string logMessage = speed.ToString() + STRLOG_Rpm;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0 };

                // Convert speed to register values: -10,000 to 0 => -10,000 to 0
                int speedValue = speed;
                regs[0] = (ushort)speedValue;

                int regValue = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_MinSpeedLimit.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_MinSpeedLimit, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_MinSpeedLimit, (ushort)1);
                regValue = inregs[0];
                logMessage += STRLOG_Reading + regValue.ToString();

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void WriteMaxSpeedLimit(int speed)
        {
            const string STRLOG_MethodName = "WriteMaxSpeedLimit";

            string logMessage = speed.ToString() + STRLOG_Rpm;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0 };

                // Convert speed to register values: 0 to 10,000 => 0 to 10,000
                int speedValue = speed;
                regs[0] = (ushort)speedValue;

                int regValue = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_MaxSpeedLimit.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_MaxSpeedLimit, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_MaxSpeedLimit, (ushort)1);
                regValue = inregs[0];
                logMessage += STRLOG_Reading + regValue.ToString();

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void WriteMinTorqueLimit(int percent)
        {
            const string STRLOG_MethodName = "WriteMinTorqueLimit";

            string logMessage = percent.ToString() + STRLOG_Percent;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0 };

                // Convert percent to register values: -325 to 0 => -32500 to 0
                int percentValue = percent * 100;
                regs[0] = (ushort)percentValue;

                int regValue = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_MinTorqueLimit.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_MinTorqueLimit, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_MinTorqueLimit, (ushort)1);
                regValue = inregs[0];
                logMessage += STRLOG_Reading + regValue.ToString();

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void WriteMaxTorqueLimit(int percent)
        {
            const string STRLOG_MethodName = "WriteMaxTorqueLimit";

            string logMessage = percent.ToString() + STRLOG_Percent;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0 };

                // Convert percent to register values: 0 to 325 => 0 to 32500
                int percentValue = percent * 100;
                regs[0] = (ushort)percentValue;

                int regValue = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_MaxTorqueLimit.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_MaxTorqueLimit, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_MaxTorqueLimit, (ushort)1);
                regValue = inregs[0];
                logMessage += STRLOG_Reading + regValue.ToString();

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void WriteSpeedRampTime(int seconds)
        {
            const string STRLOG_MethodName = "WriteSpeedRampTime";

            string logMessage = seconds.ToString() + STRLOG_Seconds;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0 };

                // Convert time to register values: 0 to 300 => 0 to 30,000
                int timeValue = seconds * 100;
                regs[0] = (ushort)timeValue;

                int regValue = regs[0];
                logMessage += STRLOG_Register +REGADDR_RW_DC_SpeedRampTime.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_SpeedRampTime, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_SpeedRampTime, (ushort)1);
                regValue = inregs[0];
                logMessage += STRLOG_Reading + regValue.ToString();

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void WriteField(int percent)
        {
            const string STRLOG_MethodName = "WriteField";

            string logMessage = percent.ToString() + STRLOG_Percent;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[1] { 0 };

                // Convert percent to register values: 0 to 100 => 0 to 10,000
                int percentValue = percent * 100;
                regs[0] = (ushort)percentValue;

                int regValue = regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_DC_FieldLimit.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_DC_FieldLimit, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_DC_FieldLimit, (ushort)1);
                regValue = inregs[0];
                logMessage += STRLOG_Reading + regValue.ToString();

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

    }
}
