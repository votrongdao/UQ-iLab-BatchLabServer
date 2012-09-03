using System;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using Modbus.Device;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class ACDrive
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ACDrive";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Register = " Register: ";
        private const string STRLOG_Writing = " Writing: ";
        private const string STRLOG_Reading = " Reading: ";
        private const string STRLOG_WritingHex = " Writing: 0x";
        private const string STRLOG_ReadingHex = " Reading: 0x";
        private const string STRLOG_FaultCode = " FaultCode: ";
        private const string STRLOG_Rpm = " RPM";
        private const string STRLOG_Milliamps = " milliAmps";
        private const string STRLOG_Percent = " percent";
        private const string STRLOG_Seconds = " seconds";
        private const string STRLOG_ConvertsTo = " => ";

        //
        // Configuration delays
        //
        public const int DELAY_ConfigureSpeed = 0;
        public const int DELAY_ConfigureTorque = 0;
        public const int DELAY_ConfigureSpeedRampTime = 0;
        public const int DELAY_ConfigureMaximumCurrent = 0;
        public const int DELAY_ConfigureMinimumTorque = 0;
        public const int DELAY_ConfigureMaximumTorque = 0;

        //
        // Command execution delay constants
        //
        public const int DELAY_EnableDrivePower = 5;
        public const int DELAY_DisableDrivePower = 5;
        public const int DELAY_ResetDrive = 5;
        public const int DELAY_StartDrive = 5;
        public const int DELAY_StopDrive = 10;
        public const int DELAY_StartDriveFullLoad = DELAY_StartDrive * 2;
        public const int DELAY_StopDriveFullLoad = DELAY_StopDrive * 2;
        public const int DELAY_SetSpeed = DEFAULT_SpeedRampTime + 2;

        //
        // Default values for control registers
        //
        public const int DEFAULT_Speed = 0;
        public const int DEFAULT_Torque = 0;
        public const int DEFAULT_SpeedRampTime = 3;
        public const int DEFAULT_MaximumCurrent = 5500;
        public const int DEFAULT_MinimumTorque = -100;
        public const int DEFAULT_MaximumTorque = 100;

        //
        // Maximum and minimum values for control registers
        //
        public const int MAXIMUM_Speed = 1500;
        public const int MAXIMUM_MaximumCurrent = 10000;

        //
        // AC Drive register addresses (read/write)
        //
        private const ushort REGADDR_RW_AC_ControlWord = 3200;
        private const ushort REGADDR_RW_AC_ControlSpeed = 3202;
        private const ushort REGADDR_RW_AC_ControlTorque = 3204;
        private const ushort REGADDR_RW_AC_SpeedRampTime = 3206;
        private const ushort REGADDR_RW_AC_MaximumCurrent = 3208;
        private const ushort REGADDR_RW_AC_MaximumTorque = 3210;
        private const ushort REGADDR_RW_AC_MinimumTorque = 3212;

        //
        // AC Drive register addresses (read only)
        //
        private const ushort REGADDR_RO_AC_DriveSpeed = 3000;
        private const ushort REGADDR_RO_AC_DriveTorque = 3006;
        private const ushort REGADDR_RO_AC_ActiveFault = 3050;

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

        public ACDrive(ModbusIpMaster master)
        {
            const string STRLOG_MethodName = "ACDrive";

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

        public void ConfigureSpeedRampTime(int time)
        {
            WriteSpeedRampTime(time);
            WaitDelay(DELAY_ConfigureSpeedRampTime);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureMaximumCurrent(int current)
        {
            WriteMaximumCurrent(current);
            WaitDelay(DELAY_ConfigureMaximumCurrent);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureMinimumTorque(int percent)
        {
            WriteMinimumTorque(percent);
            WaitDelay(DELAY_ConfigureMinimumTorque);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ConfigureMaximumTorque(int percent)
        {
            WriteMaximumTorque(percent);
            WaitDelay(DELAY_ConfigureMaximumTorque);
        }

        //-------------------------------------------------------------------------------------------------//

        public void EnableDrivePower()
        {
            const string STRLOG_MethodName = "EnableDrivePower";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[2] { 0x0000, 0x4000 };

                int value = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X8");

                // Write to register
                this.master.WriteMultipleRegisters(0, REGADDR_RW_AC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlWord, (ushort)2);
                value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X8");

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);

                WaitDelay(DELAY_EnableDrivePower);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void DisableDrivePower()
        {
            const string STRLOG_MethodName = "DisableDrivePower";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[2] { 0x0000, 0x0000 };

                int value = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X8");

                // Write to register
                this.master.WriteMultipleRegisters(0, REGADDR_RW_AC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlWord, (ushort)2);
                value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X8");

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);

                WaitDelay(DELAY_DisableDrivePower);
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
                ushort[] regs = new ushort[2] { 0x09a1, 0x4000 };

                int value = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X8");

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_AC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlWord, (ushort)2);
                value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X8");

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

        public void StartDriveNoLoad()
        {
            ushort[] regs = new ushort[2] { 0x08a1, 0x6000 };
            StartDrive(regs);
        }

        public void StartDriveFullLoad()
        {
            //
            // Start the drive with no load before applying full load
            //
            StartDriveNoLoad();

            ushort[] regs = new ushort[2] { 0x08a2, 0x6000 };
            StartDrive(regs);
        }

        public void StartDriveLockedRotor()
        {
            ushort[] regs = new ushort[2] { 0x08a2, 0x5000 };
            StartDrive(regs);
        }

        public void StartDriveSyncSpeed()
        {
            //ushort[] regs = new ushort[2] { 0x08a2, 0x6000 };
            ushort[] regs = new ushort[2] { 0x08a2, 0x4000 };
            StartDrive(regs);
        }

        //-------------------------------------------------------------------------------------------------//

        public void StopDriveNoLoad()
        {
            ushort[] regs = new ushort[2] { 0x08A1, 0x4000 };
            StopDrive(regs);
        }

        public void StopDriveFullLoad()
        {
            //
            // Take load off AC drive before stopping drive
            //
            ushort[] regs = new ushort[2] { 0x0821, 0x6000 };
            StopDrive(regs);

            regs = new ushort[2] { 0x08a1, 0x4000 };
            StopDrive(regs);
        }

        public void StopDriveLockedRotor()
        {
            ushort[] regs = new ushort[2] { 0x08A1, 0x4000 };
            StopDrive(regs);
        }

        public void StopDriveSyncSpeed()
        {
            ushort[] regs = new ushort[2] { 0x08A1, 0x4000 };
            StopDrive(regs);
        }

        //-------------------------------------------------------------------------------------------------//

        public void SetSpeed(int speed)
        {
            WriteSpeed(speed);
            WaitDelay(DELAY_SetSpeed);
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

        public uint ReadControlWord(bool show)
        {
            const string STRLOG_MethodName = "ReadControlWord";

            if (show == true)
            {
                Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
                Trace.WriteLine(STRLOG_MethodName);
            }

            string logMessage = string.Empty;

            uint controlWord = 0;

            try
            {
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlWord.ToString();

                // Read registers
                ushort[] regs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlWord, (ushort)2);

                controlWord = (uint)((regs[1] << 16) | regs[0]);

                logMessage += STRLOG_ReadingHex + regs[1].ToString("x4") + "-" + regs[0].ToString("x4");

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

        public int ReadActiveFault()
        {
            const string STRLOG_MethodName = "ReadActiveFault";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int value = 0;
            try
            {
                logMessage += STRLOG_Register + REGADDR_RO_AC_ActiveFault.ToString();

                // Read register
                ushort[] regs = this.master.ReadHoldingRegisters(REGADDR_RO_AC_ActiveFault, (ushort)2);
                value = (regs[1] << 16) | regs[0];

                logMessage += STRLOG_FaultCode + value.ToString();

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }

            return value;
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
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlSpeed.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlSpeed, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to speed: -200,000,000 to 200,000,000 => -3000 to 3000
                speed = value * 3 / 200000;
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
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlTorque.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlTorque, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to torque: -200,000,000 to 200,000,000 => -2000 to 2000
                percent = value / 100000;
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
                logMessage += STRLOG_Register + REGADDR_RW_AC_SpeedRampTime.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_SpeedRampTime, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to time: 0 to 1,800,000 => 0 to 1,800
                seconds = value / 1000;
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

        public int ReadMaximumCurrent()
        {
            const string STRLOG_MethodName = "ReadMaximumCurrent";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int current = 0;

            try
            {
                logMessage += STRLOG_Register + REGADDR_RW_AC_MaximumCurrent.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_MaximumCurrent, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to current: 0 to 3,000,000 => 0 to 30,000,000
                current = value * 10;
                logMessage += STRLOG_ConvertsTo + current.ToString() + STRLOG_Milliamps;

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

        public int ReadMinimumTorque()
        {
            const string STRLOG_MethodName = "ReadMinimumTorque";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int percent = 0;
            try
            {
                logMessage += STRLOG_Register + REGADDR_RW_AC_MinimumTorque.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_MinimumTorque, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to percent: 49536 to 65535 => -1600 to -0.1
                percent = -(65535 - value) * 1600 / (65536 - 49536);
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

        public int ReadMaximumTorque()
        {
            const string STRLOG_MethodName = "ReadMaximumTorque";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int percent = 0;
            try
            {
                logMessage += STRLOG_Register + REGADDR_RW_AC_MaximumTorque.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_MaximumTorque, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to percent: 0 to 1,000 => 0 to 100
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
                logMessage += STRLOG_Register + REGADDR_RO_AC_DriveSpeed.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RO_AC_DriveSpeed, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to speed: -3,000,000 to 3,000,000 => -30,000 to 30,000
                speed = value / 100;
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

        public int ReadDriveTorque()
        {
            const string STRLOG_MethodName = "ReadDriveTorque";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            int percent = 0;

            try
            {
                logMessage += STRLOG_Register + REGADDR_RO_AC_DriveTorque.ToString();

                // Read register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RO_AC_DriveTorque, (ushort)2);
                int value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_Reading + value.ToString();

                // Convert register values to percent: 0 to 16,000 => 0 to 1,600
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

        //=================================================================================================//

        private void StartDrive(ushort[] regs)
        {
            const string STRLOG_MethodName = "StartDrive";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            try
            {
                int value = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X8");

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_AC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlWord, (ushort)2);
                value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X8");

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

        private void StopDrive(ushort[] regs)
        {
            const string STRLOG_MethodName = "StopDrive";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
            Trace.WriteLine(STRLOG_MethodName);

            string logMessage = string.Empty;

            try
            {
                int value = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlWord.ToString();
                logMessage += STRLOG_WritingHex + value.ToString("X8");

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_AC_ControlWord, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlWord, (ushort)2);
                value = (inregs[1] << 16) | inregs[0];
                logMessage += STRLOG_ReadingHex + value.ToString("X8");

                Logfile.Write(this.logLevel, logMessage);
                Trace.WriteLine(logMessage);

                WaitDelay(DELAY_StopDrive);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void WriteSpeed(int speed)
        {
            const string STRLOG_MethodName = "WriteSpeed";

            string logMessage = speed.ToString() + STRLOG_Rpm;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[2] { 0x0000, 0x0000 };

                // Convert speed to register values: -3000 to 3000 => -200,000,000 to 200,000,000
                int speedValue = (speed * 200000) / 3;
                regs[0] = (ushort)speedValue;
                regs[1] = (ushort)(speedValue >> 16);

                int regValue = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlSpeed.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_AC_ControlSpeed, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlSpeed, (ushort)2);
                regValue = (inregs[1] << 16) | inregs[0];
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
                ushort[] regs = new ushort[2] { 0x0000, 0x0000 };

                // Convert torque to register values: -2000 to 2000 => -200,000,000 to 200,000,000
                int torqueValue = percent * 100000;
                regs[0] = (ushort)torqueValue;
                regs[1] = (ushort)(torqueValue >> 16);

                int regValue = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_ControlTorque.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_AC_ControlTorque, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_ControlTorque, (ushort)2);
                regValue = (inregs[1] << 16) | inregs[0];
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
                ushort[] regs = new ushort[2] { 0x0000, 0x0000 };

                // Convert time to register values: 0 to 1,800 => 0 to 1,800,000
                int timeValue = seconds * 1000;
                regs[0] = (ushort)timeValue;
                regs[1] = (ushort)(timeValue >> 16);

                int regValue = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register +  REGADDR_RW_AC_SpeedRampTime.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_AC_SpeedRampTime, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_SpeedRampTime, (ushort)2);
                regValue = (inregs[1] << 16) | inregs[0];
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

        private void WriteMaximumCurrent(int current)
        {
            const string STRLOG_MethodName = "WriteMaximumCurrent";

            string logMessage = current.ToString() + STRLOG_Milliamps;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[2] { 0x0000, 0x0000 };

                // Convert current to register values: 0 to 30,000,000 => 0 to 3,000,000
                int currentValue = current / 10;
                regs[0] = (ushort)currentValue;
                regs[1] = (ushort)(currentValue >> 16);

                int regValue = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_MaximumCurrent.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_AC_MaximumCurrent, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_MaximumCurrent, (ushort)2);
                regValue = (inregs[1] << 16) | inregs[0];
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

        private void WriteMinimumTorque(int percent)
        {
            const string STRLOG_MethodName = "WriteMinimumTorque";

            string logMessage = percent.ToString() + STRLOG_Percent;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[2] { 0x0000, 0x0000 };

                // Convert percent to register values: -1600 to -0.1 => 49536 to 65535
                int percentValue = 65535 - ((65536 - 49536) * -percent / 1600);
                regs[0] = (ushort)percentValue;
                regs[1] = (ushort)(percentValue >> 16);

                int regValue = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_MinimumTorque.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_AC_MinimumTorque, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_MinimumTorque, (ushort)2);
                regValue = (inregs[1] << 16) | inregs[0];
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

        private void WriteMaximumTorque(int percent)
        {
            const string STRLOG_MethodName = "WriteMaximumTorque";

            string logMessage = percent.ToString() + STRLOG_Percent;

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
            Trace.WriteLine(STRLOG_MethodName + Logfile.STRLOG_Spacer + logMessage);

            logMessage = string.Empty;

            try
            {
                ushort[] regs = new ushort[2] { 0x0000, 0x0000 };

                // Convert percent to register values: 0 to 100 => 0 to 1,000
                int percentValue = percent * 10;
                regs[0] = (ushort)percentValue;
                regs[1] = (ushort)(percentValue >> 16);

                int regValue = (regs[1] << 16) | regs[0];
                logMessage += STRLOG_Register + REGADDR_RW_AC_MaximumTorque.ToString();
                logMessage += STRLOG_Writing + regValue.ToString();

                // Write to register
                this.master.WriteMultipleRegisters(REGADDR_RW_AC_MaximumTorque, regs);

                // Read back register
                ushort[] inregs = this.master.ReadHoldingRegisters(REGADDR_RW_AC_MaximumTorque, (ushort)2);
                regValue = (inregs[1] << 16) | inregs[0];
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
