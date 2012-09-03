using System;
using Library.Lab;
using Library.LabServerEngine;

namespace Library.LabServerEngine.Drivers.Equipment
{
    public class EquipmentServiceAPI
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "EquipmentServiceAPI";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_UnitId = " UnitId: ";
        private const string STRLOG_Url = " Url: ";
        private const string STRLOG_NoEquipmentServicesSpecified = " No equipment services specified.";

        //
        // String constants for error messages
        //
        private const string STRERR_EquipmentServiceNotSpecifiedForUnit = "Equipment service not specified for Unit ";
        private const string STRERR_CsvStringFieldCount = "CSV string has an incorrect number of fields";
        private const string STRERR_CsvStringEmptyField = "CSV string field is empty";

        //
        // Local constants
        //
        private enum CsvFields
        {
            EquipmentServiceUrl = 0,
            EquipmentServicePasskey = 1,
            Length = 2
        };

        #endregion

        #region Properties

        private EquipmentService equipmentServiceProxy;

        public EquipmentService EquipmentServiceProxy
        {
            get { return this.equipmentServiceProxy; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        public EquipmentServiceAPI(int unitId)
        {
            const string STRLOG_MethodName = "EquipmentServiceAPI";

            string logMessage = STRLOG_UnitId + unitId.ToString();

            Logfile.WriteCalled(null, STRLOG_MethodName, logMessage);

            try
            {
                //
                // Get the equipment service values from the Application's configuration file
                //
                string[] equipmentServices = null;
                try
                {
                    equipmentServices = Utilities.GetAppSettings(Consts.STRCFG_EquipmentService);
                }
                catch
                {
                }
                if (equipmentServices == null || equipmentServices.Length == 0)
                {
                    //
                    // No equipment services specified, but that's ok because this LabServer may not use any
                    //
                    Logfile.Write(STRLOG_NoEquipmentServicesSpecified);
                }
                else
                {
                    if (unitId >= equipmentServices.Length)
                    {
                        throw new ArgumentException(STRERR_EquipmentServiceNotSpecifiedForUnit + unitId.ToString());
                    }

                    //
                    // Split the equipment service CSV string into its parts
                    //
                    string[] strSplit = equipmentServices[unitId].Split(new char[] { Consts.CHR_CsvSplitterChar });
                    if (strSplit.Length != (int)CsvFields.Length)
                    {
                        throw new FormatException(STRERR_CsvStringFieldCount);
                    }

                    //
                    // Get the equipment service URL
                    //
                    string equipmentServiceUrl = strSplit[(int)CsvFields.EquipmentServiceUrl].Trim();
                    if (equipmentServiceUrl.Length == 0)
                    {
                        throw new ArgumentNullException(CsvFields.EquipmentServiceUrl.ToString(), STRERR_CsvStringEmptyField);
                    }

                    //
                    // Get the equipment service passkey
                    //
                    string equipmentServicePasskey = strSplit[(int)CsvFields.EquipmentServicePasskey].Trim();
                    if (equipmentServicePasskey.Length == 0)
                    {
                        throw new ArgumentNullException(CsvFields.EquipmentServicePasskey.ToString(), STRERR_CsvStringEmptyField);
                    }

                    // Get LabServer identifier
                    string labServerGuid = Utilities.GetAppSetting(Consts.STRCFG_LabServerGuid);

                    //
                    // Create equipment service interface
                    //
                    this.equipmentServiceProxy = new EquipmentService();
                    this.equipmentServiceProxy.Url = equipmentServiceUrl;
                    Logfile.Write(STRLOG_Url + this.equipmentServiceProxy.Url);

                    //
                    // Create and fill in authorisation information
                    //
                    AuthHeader authHeader = new AuthHeader();
                    authHeader.identifier = labServerGuid;
                    authHeader.passKey = equipmentServicePasskey;
                    this.equipmentServiceProxy.AuthHeaderValue = authHeader;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

    }
}
