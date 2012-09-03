using System;
using System.Runtime.InteropServices;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment.Drivers
{
    public class FlexMotionCntl : FlexMotion
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "FlexMotionCntl";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Initialising = " Initialising...";
        private const string STRLOG_FindingTubeForwardLimit = " Finding tube forward limit...";
        private const string STRLOG_FindingTubeReverseLimit = " Finding tube reverse limit...";
        private const string STRLOG_ResetingTubePosition = " Reseting tube position...";
        private const string STRLOG_SettingTubeDistanceToHomePosition = " Setting tube distance to home position...";
        private const string STRLOG_SettingSourceToHomeLocation = " Setting source to home location...";
        private const string STRLOG_SettingAbsorberToHomeLocation = " Setting absorber to home location...";

        //
        // String constants for error messages
        //
        private const string STRERR_FlexMotionBoardNotPresent = "FlexMotion board is not present!";
        private const string STRERR_InvalidTubeAxisId_arg = "Invalid tube axis Id: {0}";
        private const string STRERR_InvalidSourceAxisId_arg = "Invalid source axis Id: {0}";
        private const string STRERR_InvalidAbsorberAxisId_arg = "Invalid absorber axis Id: {0}";
        private const string STRERR_PowerEnableBreakpointFailed = "Failed to set PowerEnable breakpoint!";
        private const string STRERR_CounterStartBreakpointFailed = "Failed to set CounterStart breakpoint!";
        private const string STRERR_FindTubeReverseLimitFailed = "FindTubeReverseLimit Failed!";
        private const string STRERR_ResetTubePositionFailed = "ResetTubePosition Failed!";
        private const string STRERR_FindTubeForwardLimitFailed = "FindTubeForwardLimit Failed!";
        private const string STRERR_SetTubeDistanceFailed = "SetTubeDistance Failed!";
        private const string STRERR_SetSourceLocationFailed = "SetSourceLocation Failed!";
        private const string STRERR_SetAbsorberLocationFailed = "SetAbsorberLocation Failed!";
        private const string STRERR_InvalidLocation = " Invalid Location: ";

        // Tube axis encoder counts per mm distance moved
        private const int ENCODER_COUNTS_PER_MM = 43000;

        //
        // Local variables
        //
        private bool initialised;
        private bool isPresent;
        private bool powerupReset;
        private byte tubeAxis;
        private byte sourceAxis;
        private byte absorberAxis;
        private byte powerEnableBreakpoint;
        private byte counterStartBreakpoint;

        #endregion

        #region DLL Import

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_initialize_controller(byte boardID, byte[] settings);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_read_csr_rtn(byte boardID, ref ushort csr);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_find_reference(byte boardID, byte axisOrVectorSpace, ushort axisOrVSMap, byte searchType);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_check_reference(byte boardID, byte axisOrVectorSpace, ushort axisOrVSMap, ref ushort found, ref ushort finding);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_reset_pos(byte boardID, byte axis, int position1, int position2, byte inputVector);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_read_pos_rtn(byte boardID, byte axis, ref int position);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_set_op_mode(byte boardID, byte axis, ushort operationMode);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_load_target_pos(byte boardID, byte axis, int targetPosition, byte inputVector);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_start(byte boardID, byte axisOrVectorSpace, ushort axisOrVSMap);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_stop_motion(byte boardID, byte axisOrVectorSpace, ushort stopType, ushort axisOrVSMap);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_read_axis_status_rtn(byte boardID, byte axis, ref ushort axisStatus);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_read_error_msg_rtn(byte boardID, ref ushort commandID, ref ushort resourceID, ref int errorCode);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_get_error_description(ushort descriptionType, int errorCode, ushort commandID, ushort resourceID, char[] charArray, ref int sizeOfArray);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_set_breakpoint_output_momo(byte boardID, byte axisOrEncoder, ushort mustOn, ushort mustOff, byte inputVector);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_enable_breakpoint(byte boardID, byte axisOrEncoder, byte enable);

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public FlexMotionCntl(XmlNode xmlNodeEquipmentConfig)
            : base(xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "FlexMotionCntl";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.initialised = false;
            this.isPresent = false;
            this.powerupReset = false;

            try
            {
                //
                // Can have up to 4 axes
                //
                bool[] axes = new bool[] { false, false, false, false };

                //
                // Get tube axis
                //
                if ((this.tubeAxis = this.GetNimcAxis(this.tubeAxisId)) == 0)
                {
                    throw new Exception(
                        String.Format(STRERR_InvalidTubeAxisId_arg, this.tubeAxisId));
                }
                axes[this.tubeAxisId - 1] = true;

                //
                // Get source axis
                //
                if ((this.sourceAxis = this.GetNimcAxis(this.sourceAxisId)) == 0 ||
                    axes[this.sourceAxisId - 1] == true)
                {
                    throw new Exception(
                        String.Format(STRERR_InvalidSourceAxisId_arg, this.sourceAxisId));
                }
                axes[this.sourceAxisId - 1] = true;

                if (this.hasAbsorberTable == true)
                {
                    //
                    // Get absorber axis
                    //
                    if ((this.absorberAxis = this.GetNimcAxis(this.absorberAxisId)) == 0 ||
                        axes[this.absorberAxisId - 1] == true)
                    {
                        throw new Exception(
                            String.Format(STRERR_InvalidAbsorberAxisId_arg, this.absorberAxisId));
                    }
                    axes[this.absorberAxisId - 1] = true;
                }

                //
                // Set breakpoint axes
                //
                this.powerEnableBreakpoint = this.tubeAxis;
                this.counterStartBreakpoint = this.sourceAxis;

                //
                // Initialise the Flexmotion controller card. Must be done here because a breakpoint
                // on the controller card is used to powerup the equipment and initialisation is
                // carried out after the equipment is powered up.
                //
                if ((this.isPresent = InitialiseController()) == true)
                {
                    //
                    // Initialisation is complete
                    //
                    if (this.powerupReset == true)
                    {
                        this.initialised = false;
                    }
                }
                else
                {
                    this.statusMessage = STRERR_FlexMotionBoardNotPresent;
                    Logfile.WriteError(this.statusMessage);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        public override bool EnablePower()
        {
            const string STRLOG_MethodName = "EnablePower";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                //
                // Make sure FlexMotion controller is present
                //
                if (this.isPresent == false)
                {
                    throw new ArgumentException(STRERR_FlexMotionBoardNotPresent);
                }

                //
                // Ensure power-enable and counter-start signals are inactive
                //
                if (this.SetBreakpoint(this.boardID, this.powerEnableBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_PowerEnableBreakpointFailed + this.GetLastError());
                }
                if (this.SetBreakpoint(this.boardID, this.counterStartBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.GetLastError());
                }

                //
                // Toggle the counter-start signal to enable both signals
                //
                if (this.SetBreakpoint(this.boardID, this.counterStartBreakpoint, true) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.GetLastError());
                }
                if (this.SetBreakpoint(this.boardID, this.counterStartBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.GetLastError());
                }

                //
                // Enable the power
                //
                if (this.SetBreakpoint(this.boardID, this.powerEnableBreakpoint, true) == false)
                {
                    throw new ArgumentException(STRERR_PowerEnableBreakpointFailed + this.GetLastError());
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public override bool DisablePower()
        {
            const string STRLOG_MethodName = "DisablePower";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                //
                // Make sure FlexMotion controller is present
                //
                if (this.isPresent == false)
                {
                    throw new ArgumentException(STRERR_FlexMotionBoardNotPresent);
                }

                //
                // Make the counter-start and power-enable signals inactive
                //
                if (this.SetBreakpoint(this.boardID, this.counterStartBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.GetLastError());
                }
                if (this.SetBreakpoint(this.boardID, this.powerEnableBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_PowerEnableBreakpointFailed + this.GetLastError());
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public override bool PowerInitialise()
        {
            const string STRLOG_MethodName = "PowerInitialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            //
            // Check if this is first-time initialisation
            //
            if (this.initialised == false)
            {
                this.statusMessage = STRLOG_Initialising;

                try
                {
                    //
                    // Make sure FlexMotion controller is present
                    //
                    if (this.isPresent == false)
                    {
                        throw new ArgumentException(STRERR_FlexMotionBoardNotPresent);
                    }

                    this.statusMessage = STRLOG_Initialising;

                    //if ((this.powerupReset == true) || (this.tubeInitAxis == true))
                    //{
                    //    //
                    //    // Find forward limit switch position in encoder counts (4,283,880)
                    //    //
                    //    Logfile.Write(STRLOG_FindingTubeForwardLimit);
                    //    int forwardPosition = 0;
                    //    if (FindTubeForwardLimit(ref forwardPosition) == false)
                    //    {
                    //        throw new ArgumentException(STRERR_FindTubeForwardLimitFailed + this.GetLastError());
                    //    }
                    //}

                    //
                    // Find the reverse limit switch and set tube position to zero
                    //
                    Logfile.Write(STRLOG_FindingTubeReverseLimit);
                    int reversePosition = 0;
                    if (FindTubeReverseLimit(ref reversePosition) == false)
                    {
                        throw new ArgumentException(STRERR_FindTubeReverseLimitFailed + this.GetLastError());
                    }

                    Logfile.Write(STRLOG_ResetingTubePosition);
                    if (ResetTubePosition() == false)
                    {
                        throw new ArgumentException(STRERR_ResetTubePositionFailed + this.GetLastError());
                    }

                    //
                    // Set tube to its home position
                    //
                    Logfile.Write(STRLOG_SettingTubeDistanceToHomePosition);
                    if (SetTubeDistance(this.tubeHomeDistance) == false)
                    {
                        throw new ArgumentException(STRERR_SetTubeDistanceFailed + this.GetLastError());
                    }

                    //
                    // Set source to its home location
                    //
                    Logfile.Write(STRLOG_SettingSourceToHomeLocation);
                    if (SetSourceLocation(this.sourceHomeLocation) == false)
                    {
                        throw new ArgumentException(STRERR_SetSourceLocationFailed + this.GetLastError());
                    }

                    //
                    // Set absorber to its home location
                    //
                    if (this.hasAbsorberTable == true)
                    {
                        Logfile.Write(STRLOG_SettingAbsorberToHomeLocation);
                        if (SetAbsorberLocation(this.absorberHomeLocation) == false)
                        {
                            throw new ArgumentException(STRERR_SetAbsorberLocationFailed + this.GetLastError());
                        }
                    }

                    //
                    // First-time initialisation is complete
                    //
                    this.initialiseDelay = 0;
                    this.initialised = true;
                    this.online = true;
                    this.statusMessage = StatusCodes.Ready.ToString();

                    success = true;
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }
            }

            //
            // Initialisation that must be done each time the equipment is powered up
            //
            try
            {
                //
                // Initialisation is complete
                //
                this.online = true;
                this.statusMessage = string.Empty;

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public override bool StartCounter()
        {
            const string STRLOG_MethodName = "StartCounter";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                if (this.SetBreakpoint(this.boardID, this.counterStartBreakpoint, true) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.GetLastError());
                }
                if (this.SetBreakpoint(this.boardID, this.counterStartBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.GetLastError());
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public override bool SetTubeDistance(int targetDistance)
        {
            // Convert target position in millimetres to encoder counts
            int targetPosition = (targetDistance - this.tubeOffsetDistance) * ENCODER_COUNTS_PER_MM;

            // Move tube to target position
            return SetTubePosition(targetPosition);
        }

        //---------------------------------------------------------------------------------------//

        public override bool SetSourceLocation(char location)
        {
            const string STRLOG_MethodName = "SetSourceLocation";

            string logMessage = STRLOG_Location + location.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            int index = location - this.sourceFirstLocation;
            if (index >= 0 && index < this.sourceAxisInfo.encoderPositions.Length)
            {
                //
                // Move source table to specified location
                //
                if ((success = this.SetSourceEncoderPosition(this.sourceAxisInfo.encoderPositions[index])) == false)
                {
                    Logfile.WriteError(this.lastError);
                }
            }
            else
            {
                Logfile.WriteError(STRERR_InvalidLocation + location.ToString());
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public override bool SetAbsorberLocation(char location)
        {
            const string STRLOG_MethodName = "SetAbsorberLocation";

            string logMessage = STRLOG_Location + location.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            if (this.hasAbsorberTable == true)
            {
                int index = location - this.absorberFirstLocation;
                if (index >= 0 && index < this.absorberAxisInfo.encoderPositions.Length)
                {
                    //
                    // Move absorber table to specified location
                    //
                    if ((success = this.SetAbsorberEncoderPosition(this.absorberAxisInfo.encoderPositions[index])) == false)
                    {
                        Logfile.WriteError(this.lastError);
                    }
                }
                else
                {
                    Logfile.WriteError(STRERR_InvalidLocation + location.ToString());
                }
            }
            else
            {
                success = true;
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //=======================================================================================//

        private byte GetNimcAxis(int axisId)
        {
            switch (axisId)
            {
                case 1: return Nimc.AXIS1;
                case 2: return Nimc.AXIS2;
                case 3: return Nimc.AXIS3;
                case 4: return Nimc.AXIS4;
                default: return 0;
            }
        }

        //---------------------------------------------------------------------------------------//

        private bool InitialiseController()
        {
            int err = 0;
            ushort csr = 0;
            int state = 0;

            try
            {
                ClearErrors();

                for (; ; )
                {
                    switch (state)
                    {
                        case 0:
                            // Get communication status register
                            err = flex_read_csr_rtn(boardID, ref csr);
                            break;

                        case 1:
                            // Check if the board is in power up reset condition
                            if ((csr & Nimc.POWER_UP_RESET) != 0)
                            {
                                err = flex_initialize_controller(boardID, null);

                                // Tube axis must be initialised after powerup reset
                                this.powerupReset = true;
                            }
                            break;

                        case 2:
                            // Inhibit the tube axis motor
                            err = flex_stop_motion(this.boardID, this.tubeAxis, Nimc.KILL_STOP, 0);
                            break;

                        case 3:
                            // Inhibit the source table motor
                            err = flex_stop_motion(this.boardID, this.sourceAxis, Nimc.KILL_STOP, 0);
                            break;

                        case 4:
                            if (this.hasAbsorberTable == true)
                            {
                                // Inhibit the absorber table motor
                                err = flex_stop_motion(this.boardID, this.absorberAxis, Nimc.KILL_STOP, 0);
                            }
                            break;

                        case 5:
                            // Initialisation successful
                            return true;
                    }

                    // Check for errors
                    if (err != 0)
                    {
                        // Initialisation failed
                        ProcessError(boardID, err);
                        return false;
                    }

                    // Next state
                    state++;
                }
            }
            catch
            {
                return false;
            }
        }

        //---------------------------------------------------------------------------------------//

        private bool SetSourceEncoderPosition(int targetPosition)
        {
            //
            // Make sure flexmotion controller is present
            //
            if (this.isPresent == false)
            {
                this.lastError = STRERR_FlexMotionBoardNotPresent;
                return false;
            }

            int err = 0;
            ushort csr = 0;
            ushort found = 0;
            ushort finding = 0;
            int position = 0;
            ushort axisStatus = 0;
            int state = 0;

            while (true)
            {
                switch (state)
                {
                    case 0:
                        err = flex_find_reference(boardID, Nimc.AXIS_CTRL, (ushort)(1 << sourceAxis),
                            Nimc.FIND_HOME_REFERENCE);
                        break;

                    case 1:
                        err = flex_check_reference(boardID, Nimc.AXIS_CTRL, (ushort)(1 << sourceAxis),
                            ref found, ref finding);
                        break;

                    case 2:
                        if (finding != 0)
                        {
                            state = 1;
                            continue;
                        }
                        break;

                    case 3:
                        err = flex_read_pos_rtn(boardID, sourceAxis, ref position);
                        break;

                    case 4:
                        err = flex_reset_pos(boardID, sourceAxis, 0, 0, 0xFF);
                        break;

                    case 5:
                        err = flex_read_pos_rtn(boardID, sourceAxis, ref position);
                        break;

                    case 6:
                        err = flex_load_target_pos(boardID, sourceAxis, targetPosition, 0xFF);
                        break;

                    case 7:
                        err = flex_start(boardID, sourceAxis, 0);
                        break;

                    case 8:
                        err = flex_read_pos_rtn(boardID, sourceAxis, ref position);
                        break;

                    case 9:
                        err = flex_read_axis_status_rtn(boardID, sourceAxis, ref axisStatus);
                        break;

                    case 10:
                        // Check the modal errors
                        err = flex_read_csr_rtn(boardID, ref csr);
                        break;

                    case 11:
                        if ((csr & Nimc.MODAL_ERROR_MSG) != 0)
                        {
                            // Stop the Motion
                            flex_stop_motion(boardID, sourceAxis, Nimc.DECEL_STOP, 0);
                            err = (short)(csr & Nimc.MODAL_ERROR_MSG);
                        }
                        break;

                    case 12:
                        // Test against the move complete bit
                        if ((axisStatus & (Nimc.MOVE_COMPLETE_BIT | Nimc.AXIS_OFF_BIT)) == 0)
                        {
                            // Not finished yet
                            state = 8;
                            continue;
                        }
                        break;

                    case 13:
                        // Inhibit the motor
                        err = flex_stop_motion(this.boardID, this.sourceAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 14:
                        // Successful
                        return (true);
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardID, err);

                    Logfile.WriteError("state: " + state.ToString() + "  err: " + err.ToString());

                    return (false);
                }

                state++;
            }
        }

        //---------------------------------------------------------------------------------------//

        private bool SetAbsorberEncoderPosition(int targetPosition)
        {
            //
            // Make sure flexmotion controller is present
            //
            if (this.isPresent == false)
            {
                this.lastError = STRERR_FlexMotionBoardNotPresent;
                return false;
            }

            int err = 0;
            ushort csr = 0;
            ushort found = 0;
            ushort finding = 0;
            int position = 0;
            ushort axisStatus = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        err = flex_find_reference(boardID, Nimc.AXIS_CTRL, (ushort)(1 << absorberAxis),
                            Nimc.FIND_HOME_REFERENCE);
                        break;

                    case 1:
                        err = flex_check_reference(boardID, Nimc.AXIS_CTRL, (ushort)(1 << absorberAxis),
                            ref found, ref finding);
                        break;

                    case 2:
                        if (finding != 0)
                        {
                            state = 1;
                            continue;
                        }
                        break;

                    case 3:
                        err = flex_read_pos_rtn(boardID, absorberAxis, ref position);
                        break;

                    case 4:
                        err = flex_reset_pos(boardID, absorberAxis, 0, 0, 0xFF);
                        break;

                    case 5:
                        err = flex_read_pos_rtn(boardID, absorberAxis, ref position);
                        break;

                    case 6:
                        err = flex_load_target_pos(boardID, absorberAxis, targetPosition, 0xFF);
                        break;

                    case 7:
                        err = flex_start(boardID, absorberAxis, 0);
                        break;

                    case 8:
                        err = flex_read_pos_rtn(boardID, absorberAxis, ref position);
                        break;

                    case 9:
                        err = flex_read_axis_status_rtn(boardID, absorberAxis, ref axisStatus);
                        break;

                    case 10:
                        // Check the modal errors
                        err = flex_read_csr_rtn(boardID, ref csr);
                        break;

                    case 11:
                        if ((csr & Nimc.MODAL_ERROR_MSG) != 0)
                        {
                            // Stop the Motion
                            flex_stop_motion(boardID, absorberAxis, Nimc.DECEL_STOP, 0);
                            err = (short)(csr & Nimc.MODAL_ERROR_MSG);
                        }
                        break;

                    case 12:
                        // Test against the move complete bit
                        if ((axisStatus & (Nimc.MOVE_COMPLETE_BIT | Nimc.AXIS_OFF_BIT)) == 0)
                        {
                            // Not finished yet
                            state = 8;
                            continue;
                        }
                        break;

                    case 13:
                        // Inhibit the motor
                        err = flex_stop_motion(this.boardID, this.absorberAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 14:
                        // Successful
                        return (true);
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardID, err);
                    return (false);
                }

                state++;
            }
        }

        //---------------------------------------------------------------------------------------//

        private bool FindTubeForwardLimit(ref int position)
        {
            //
            // Make sure flexmotion controller is present
            //
            if (this.isPresent == false)
            {
                this.lastError = STRERR_FlexMotionBoardNotPresent;
                return false;
            }

            int err = 0;
            ushort found = 0;
            ushort finding = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Find the reverse limit switch
                        err = flex_find_reference(boardID, Nimc.AXIS_CTRL, (ushort)(1 << tubeAxis),
                            Nimc.FIND_FORWARD_LIMIT_REFERENCE);
                        break;

                    case 1:
                        err = flex_check_reference(boardID, Nimc.AXIS_CTRL, (ushort)(1 << tubeAxis),
                            ref found, ref finding);
                        break;

                    case 2:
                        if (finding != 0)
                        {
                            state = 1;
                            continue;
                        }
                        break;

                    case 3:
                        // Read the current position of the tube
                        err = flex_read_pos_rtn(boardID, tubeAxis, ref position);
                        break;

                    case 4:
                        // Inhibit the motor
                        err = flex_stop_motion(boardID, tubeAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 5:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardID, err);
                    return false;
                }

                // Next state
                state++;

            }
        }

        //---------------------------------------------------------------------------------------//

        private bool FindTubeReverseLimit(ref int position)
        {
            //
            // Make sure flexmotion controller is present
            //
            if (this.isPresent == false)
            {
                this.lastError = STRERR_FlexMotionBoardNotPresent;
                return false;
            }

            int err = 0;
            ushort found = 0;
            ushort finding = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Find the reverse limit switch
                        err = flex_find_reference(boardID, Nimc.AXIS_CTRL, (ushort)(1 << tubeAxis),
                            Nimc.FIND_REVERSE_LIMIT_REFERENCE);
                        break;

                    case 1:
                        err = flex_check_reference(boardID, Nimc.AXIS_CTRL, (ushort)(1 << tubeAxis),
                            ref found, ref finding);
                        break;

                    case 2:
                        if (finding != 0)
                        {
                            state = 1;
                            continue;
                        }
                        break;

                    case 3:
                        // Read the current position of the tube
                        err = flex_read_pos_rtn(boardID, tubeAxis, ref position);
                        break;

                    case 4:
                        // Inhibit the motor
                        err = flex_stop_motion(boardID, tubeAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 5:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardID, err);
                    return false;
                }

                // Next state
                state++;

            }
        }

        //---------------------------------------------------------------------------------------//

        private bool ResetTubePosition()
        {
            //
            // Make sure flexmotion controller is present
            //
            if (this.isPresent == false)
            {
                this.lastError = STRERR_FlexMotionBoardNotPresent;
                return false;
            }

            int err = 0;
            int position = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Read the position of the reverse limit switch
                        err = flex_read_pos_rtn(boardID, tubeAxis, ref position);
                        break;

                    case 1:
                        // Reset the position to 0
                        err = flex_reset_pos(boardID, tubeAxis, 0, 0, 0xFF);
                        break;

                    case 2:
                        // Read the position again
                        err = flex_read_pos_rtn(boardID, tubeAxis, ref position);
                        break;

                    case 3:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardID, err);
                    return false;
                }

                // Next state
                state++;
            }
        }

        //---------------------------------------------------------------------------------------//

        private bool GetTubePosition(ref int position)
        {
            //
            // Make sure flexmotion controller is present
            //
            if (this.isPresent == false)
            {
                this.lastError = STRERR_FlexMotionBoardNotPresent;
                return false;
            }

            int err = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Read the current position of the tube
                        err = flex_read_pos_rtn(boardID, tubeAxis, ref position);
                        break;

                    case 1:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardID, err);
                    return false;
                }

                // Next state
                state++;
            }
        }

        //---------------------------------------------------------------------------------------//

        private bool SetTubePosition(int targetPosition)
        {
            //
            // Make sure flexmotion controller is present
            //
            if (this.isPresent == false)
            {
                this.lastError = STRERR_FlexMotionBoardNotPresent;
                return false;
            }

            int err = 0;
            ushort csr = 0;
            int position = 0;
            ushort axisStatus = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        err = flex_set_op_mode(boardID, tubeAxis, Nimc.ABSOLUTE_POSITION);
                        break;

                    case 1:
                        err = flex_read_pos_rtn(boardID, tubeAxis, ref position);
                        break;

                    case 2:
                        err = flex_load_target_pos(boardID, tubeAxis, targetPosition, 0xFF);
                        break;

                    case 3:
                        err = flex_start(boardID, tubeAxis, 0);
                        break;

                    case 4:
                        err = flex_read_pos_rtn(boardID, tubeAxis, ref position);
                        break;

                    case 5:
                        err = flex_read_axis_status_rtn(boardID, tubeAxis, ref axisStatus);
                        break;

                    case 6:
                        // Check the modal errors
                        err = flex_read_csr_rtn(boardID, ref csr);
                        break;

                    case 7:
                        if ((csr & Nimc.MODAL_ERROR_MSG) != 0)
                        {
                            // Stop the Motion
                            err = flex_stop_motion(boardID, tubeAxis, Nimc.DECEL_STOP, 0);
                        }
                        break;

                    case 8:
                        // Test against the move complete bit
                        if ((axisStatus & (Nimc.MOVE_COMPLETE_BIT | Nimc.AXIS_OFF_BIT)) == 0)
                        {
                            // Not finished yet
                            state = 4;
                            continue;
                        }

                        // Inhibit the motor
                        err = flex_stop_motion(boardID, tubeAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 9:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardID, err);
                    return false;
                }

                state++;
            }
        }

        //---------------------------------------------------------------------------------------//

        private bool SetBreakpoint(byte boardID, byte axis, bool enable)
        {
            //
            // Make sure flexmotion controller is present
            //
            if (this.isPresent == false)
            {
                this.lastError = STRERR_FlexMotionBoardNotPresent;
                return false;
            }

            int err = 0;
            int state = 0;

            ushort muston = 0, mustoff = 0;

            if (enable)
            {
                mustoff = (ushort)(1 << axis);
            }
            else
            {
                muston = (ushort)(1 << axis);
            }

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Disable breakpoint to allow direct control of I/O
                        err = flex_enable_breakpoint(boardID, axis, 0);
                        break;

                    case 1:
                        err = flex_set_breakpoint_output_momo(boardID, axis, muston, mustoff, 0xFF);
                        break;

                    case 2:
                        err = flex_set_breakpoint_output_momo(boardID, axis, 0, 0, 0xFF);
                        break;

                    case 3:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardID, err);
                    return false;
                }

                // Next state
                state++;
            }
        }

        //---------------------------------------------------------------------------------------//

        private void ClearErrors()
        {
            int err = 0;
            ushort csr = 0;
            ushort commandID = 0;
            ushort resourceID = 0;
            int errorCode = 0;

            try
            {
                for (; ; )
                {
                    err = flex_read_csr_rtn(boardID, ref csr);
                    if ((csr & Nimc.MODAL_ERROR_MSG) == 0)
                    {
                        return;
                    }

                    flex_read_error_msg_rtn(boardID, ref commandID, ref resourceID, ref errorCode);
                }
            }
            catch
            {
                throw new Exception("FlexMotion controller access failed");
            }
        }

        //---------------------------------------------------------------------------------------//

        private void ProcessError(byte boardID, int error)
        {
            int err = 0;
            ushort csr = 0;
            ushort commandID = 0;
            ushort resourceID = 0;
            int errorCode = 0;

            err = flex_read_csr_rtn(boardID, ref csr);
            if ((csr & Nimc.MODAL_ERROR_MSG) != 0)
            {
                do
                {
                    //
                    // Get the command ID, resource and the error code of
                    // the modal error from the error stack on the board.
                    //
                    err = flex_read_error_msg_rtn(boardID, ref commandID, ref resourceID, ref errorCode);
                    this.lastError = GetErrorDescription(errorCode, commandID, resourceID);

                    err = flex_read_csr_rtn(boardID, ref csr);
                } while ((csr & Nimc.MODAL_ERROR_MSG) != 0);
            }
            else
            {
                lastError = GetErrorDescription(error, 0, 0);
            }
        }

        //---------------------------------------------------------------------------------------//

        private string GetErrorDescription(int errorCode, ushort commandID, ushort resourceID)
        {
            char[] errorDescription = null;
            int sizeOfArray = 0;
            ushort descriptionType;

            descriptionType = (commandID == 0) ? Nimc.ERROR_ONLY : Nimc.COMBINED_DESCRIPTION;

            // First, get the size for the error description
            flex_get_error_description(descriptionType, errorCode, commandID, resourceID,
                                errorDescription, ref sizeOfArray);

            // sizeOfArray is size of description + NULL character
            sizeOfArray++;

            // Allocate char array for the description
            errorDescription = new char[sizeOfArray];

            // Get error description
            flex_get_error_description(descriptionType, errorCode, commandID, resourceID,
                                    errorDescription, ref sizeOfArray);

            return new string(errorDescription);
        }

    }
}
