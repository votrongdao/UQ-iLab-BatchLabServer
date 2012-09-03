using System;

namespace Library.LabEquipment.Engine
{
    public class Consts
    {
        //
        // Application configuration file key strings
        //
        public const string STRCFG_LogFilesPath = "LogFilesPath";
        public const string STRCFG_LoggingLevel = "LoggingLevel";
        public const string STRCFG_AllowedCaller = "AllowedCaller";
        public const string STRCFG_AuthenticateCaller = "AuthenticateCaller";
        public const string STRCFG_LogCallerIdPasskey = "LogCallerIdPasskey";
        public const string STRCFG_XmlEquipmentConfigFilename = "XmlEquipmentConfigFilename";

        // Comma-seperated-value string splitter character
        public const char CHR_CsvSplitterChar = ',';

        //
        // XML elements in the experiment specification string
        //
        public const string STRXML_ExperimentSpecification = "experimentSpecification";
        public const string STRXML_SetupId = "setupId";
        public const string STRXML_SetupId_DriverGeneric = "DriverGeneric";

        //
        // XML elements in the EquipmentConfig.xml file
        //
        public const string STRXML_equipmentConfig = "equipmentConfig";
        public const string STRXMLPARAM_title = "@title";
        public const string STRXMLPARAM_version = "@version";
        public const string STRXML_powerupDelay = "powerupDelay";
        public const string STRXML_powerdownTimeout = "powerdownTimeout";

        //
        // XML elements in the equipment request string
        //
        public const string STRXML_Request = "Request";
        public const string STRXML_Command = "Command";

        //
        // XML elements in the equipment response string
        //
        public const string STRXML_Response = "Response";
        public const string STRXML_RspSuccess = "Success";
        public const string STRXML_RspErrorMessage = "ErrorMessage";
    }
}
