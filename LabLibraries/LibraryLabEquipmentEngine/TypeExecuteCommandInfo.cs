using System;

namespace Library.LabEquipment.Engine
{
    public class ExecuteCommandInfo
    {
        public object command;
        public int timeout;
        public object[] parameters;
        public object[] results;
        public bool success;
        public string errorMessage;

        public ExecuteCommandInfo(object command)
        {
            this.command = command;
            this.timeout = 60;
            this.parameters = null;
            this.results = null;
            this.success = false;
            this.errorMessage = null;
        }
    }

    public enum NonExecuteCommands
    {
        GetTime
    }

    public enum ExecuteCommands
    {
        SetTime
    }


}
