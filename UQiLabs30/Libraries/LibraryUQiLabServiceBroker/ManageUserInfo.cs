using System;
using System.Collections.Generic;
using System.IO;
using Library.Lab;

namespace Library.UQiLabServiceBroker
{
    public class ManageUserInfo
    {
        #region Constants

        private const string STRLOG_ClassName = "ManageUserInfo";
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_Filename_arg = "Filename: {0}";
        private const string STRLOG_Count_arg = "Count: {0}";
        /*
         * String constants for exception messages
         */
        private const string STRERR_FileNotSpecified = "File not specified!";
        private const string STRERR_FileMustBeCsvDocument = "File must be a comma-seperated value (*.csv) document!";
        private const string STRERR_InsufficientUserFields_arg2 = "Insufficient user fields - need {0} but have {1}";
        private const string STRERR_InvalidStudentID_arg = "Invalid student ID ({0})";
        private const string STRERR_InvalidFamilyName_arg = "Invalid family name ({0})";
        private const string STRERR_InvalidGivenNames_arg = "Invalid given names ({0})";
        private const string STRERR_InvalidEmailAddress_arg = "Invalid email address ({0})";
        /*
         * String constants
         */
        private const string STR_Csv = ".csv";
        /*
         * Constants
         */
        private const int CSV_StudentID = 0;
        private const int CSV_FamilyName = 3;
        private const int CSV_GivenNames = 4;
        private const int CSV_EmailAddress = 5;
        private const int LEN_Password = 6;

        #endregion

        #region Variables

        private Random random;
        private int randomMax;

        #endregion

        //---------------------------------------------------------------------------------------//

        public ManageUserInfo()
        {
            /*
             * Create instance of Random for passwords
             */
            this.random = new Random();
        }

        //---------------------------------------------------------------------------------------//

        public UserInfo[] ParseCsvUserFile(string filename)
        {
            const string STRLOG_MethodName = "ParseCsvUserFile";
            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName,
                String.Format(STRLOG_Filename_arg, filename));

            List<UserInfo> userInfoList = new List<UserInfo>();

            try
            {
                using (StreamReader streamReader = new StreamReader(filename))
                {
                    /*
                     * First line contains the course code
                     */
                    string line = streamReader.ReadLine();
                    Logfile.Write(line);

                    /*
                     * Second line is contains course information
                     */
                    line = streamReader.ReadLine();
                    Logfile.Write(line);

                    /*
                     * Third line contains the column headings
                     */
                    line = streamReader.ReadLine();
                    Logfile.Write(line);

                    /*
                     * Read each user from the file and process
                     */
                    do
                    {
                        if ((line = streamReader.ReadLine()) != null)
                        {
                            UserInfo userInfo;
                            if ((userInfo = this.ParseCsvUser(line)) == null)
                            {
                                break;
                            }

                            /*
                             * Add the user info to the list
                             */
                            userInfoList.Add(userInfo);
                        }
                    } while (line != null);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName,
                String.Format(STRLOG_Count_arg, userInfoList.Count));

            return userInfoList.ToArray();
        }

        //---------------------------------------------------------------------------------------//

        public UserInfo ParseCsvUser(string csvUser)
        {
            UserInfo userInfo = null;

            try
            {
                /*
                 * Split the csv string into its parts and check that sufficeient fields exist
                 */
                string[] userSplit = csvUser.Split(new char[] { ',' });
                if (userSplit.Length <= CSV_EmailAddress)
                {
                    throw new Exception(
                        String.Format(STRERR_InsufficientUserFields_arg2, CSV_EmailAddress + 1, userSplit.Length));
                }

                /*
                 * Check the student ID field is a number
                 */
                int studentID = 0;
                try
                {
                    studentID = Int32.Parse(userSplit[CSV_StudentID].Trim());
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        String.Format(STRERR_InvalidStudentID_arg, userSplit[CSV_StudentID]));
                }

                /*
                 * Check the family name exists
                 */
                string familyName = userSplit[CSV_FamilyName].Trim();
                if (familyName.Length == 0)
                {
                    throw new Exception(
                        String.Format(STRERR_InvalidFamilyName_arg, userSplit[CSV_FamilyName]));
                }

                /*
                 * Check the given names exists
                 */
                string givenNames = userSplit[CSV_GivenNames].Trim();
                if (givenNames.Length == 0)
                {
                    throw new Exception(
                        String.Format(STRERR_InvalidGivenNames_arg, userSplit[CSV_GivenNames]));
                }

                /*
                 * Check the email address exists
                 */
                string emailAddress = userSplit[CSV_EmailAddress].Trim();
                if (emailAddress.Length == 0)
                {
                    throw new Exception(
                        String.Format(STRERR_InvalidEmailAddress_arg, userSplit[CSV_EmailAddress]));
                }
                if (emailAddress.Contains("@") == false)
                {
                    throw new Exception(
                        String.Format(STRERR_InvalidEmailAddress_arg, userSplit[CSV_EmailAddress]));
                }

                /*
                 * Create instance of UserInfo and fill in
                 */
                userInfo = new UserInfo();
                userInfo.studentID = studentID;
                userInfo.familyName = familyName;
                userInfo.givenNames = givenNames;
                userInfo.emailAddress = emailAddress;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return userInfo;
        }

        //---------------------------------------------------------------------------------------//

        public string GeneratePassword()
        {
            return this.GeneratePassword(LEN_Password);
        }

        //---------------------------------------------------------------------------------------//

        public string GeneratePassword(int length)
        {
            string password = String.Empty;

            /*
             * Generate a random  password containing only alphanumeric characters
             */
            for (int i = 0; i < length; i++)
            {
                char value;
                do
                {
                    value = (char)this.random.Next((int)'0', (int)'z' + 1);
                } while (Char.IsLetterOrDigit(value) == false);

                password += value;
            }

            return password;
        }
    }
}
