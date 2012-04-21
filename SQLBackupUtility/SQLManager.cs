using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SQLBackupUtility
{
    public class SQLManager
    {
        private string _connectionString;
        private List<string> _ignoredDatabases = new List<string> { "master", "model", "msdb", "tempdb" };

        #region SQLManager Public Methods

        public bool IsConnectionStringNull()
        {
            if (string.IsNullOrEmpty(_connectionString))
                return true;

            return false;
        }

        public void SetConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return;

            _connectionString = connectionString;
        }

        public void AddIgnoredDatabases(string ignoredDatabases)
        {
            if (string.IsNullOrEmpty(ignoredDatabases))
                return;

            ignoredDatabases.Trim();
            if (ignoredDatabases.Contains(" "))
            {
                Console.WriteLine("Incorrect syntax: Ignored databases must be formatted as the following example: myShop,myWebsite,myApplication");
                Console.WriteLine("SQLBackup.exe -d parameter -t parameter -i parameter,parameter,parameter");
                Environment.Exit(0);
            }

            var array = ignoredDatabases.Split(',');

            foreach (var dbName in array)
            {
                _ignoredDatabases.Add(dbName);
            }
        }

        public List<String> GetDatabases()
        {
            var sqlCon = new SqlConnection(_connectionString);
            sqlCon.Open();
            var sqlCom = new SqlCommand();
            sqlCom.Connection = sqlCon;
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.CommandText = "sp_databases";

            SqlDataReader sqlDr = sqlCom.ExecuteReader();

            var databaseList = new List<string>();

            while (sqlDr.Read())
            {
                // Get the database name
                string databaseName = sqlDr.GetString(0);

                //Filter the system databases and add the corresponding databases to the list)
                if (!_ignoredDatabases.Contains(databaseName))
                    databaseList.Add(databaseName);
            }

            sqlDr.Close();
            sqlCon.Close();

            return databaseList;
        }

        public void SaveBackups(List<String> databaseList, string backupDirectory, string backupType)
        {
            var jsonHandler = new JsonHandler();

            foreach (var db in databaseList)
            {
                if (!jsonHandler.CheckDatabase(db) && backupType.Equals("differential",StringComparison.CurrentCultureIgnoreCase))
                    SaveDatabase(db,"full",backupDirectory);
                else
                    SaveDatabase(db,backupType,backupDirectory);

                if (!jsonHandler.CheckDatabase(db))
                    jsonHandler.AddDatabase(db);
            }

            jsonHandler.SaveDatabaseList();
        }

        #endregion

        #region SQLManager Private Methods

        private void SaveDatabase(string db, string backupType, string backupDirectory)
        {
            var filePath = GetFilePath(backupDirectory, db, backupType);
            var fileName = GetFileName(filePath, backupDirectory);

            var backupString = CreateBackupString(db, filePath, backupType);

            CreateProcess(backupString);
            DropboxHandler.UploadFile(fileName, filePath);
        }

        private string GetFilePath(string backupDirectory, string db, string backupType)
        {
            return string.Format(@"{0}\{1}-{2}-{3}{4}", backupDirectory, db, backupType,
                                               DateTime.Now.ToString("dd-MM-yyyy"), ".bak");
        }

        private string GetFileName(string filePath, string backupDirectory)
        {
            return filePath.Substring(backupDirectory.Length + 1);
        }

        private string CreateBackupString(string db, string filePath, string backupType)
        {
            string arguments;

            if (!backupType.Equals("Full", StringComparison.CurrentCultureIgnoreCase))
            {
                arguments =
                    string.Format(
                        @"-S (local)\SQLEXPRESS -Q ""BACKUP DATABASE [{0}] TO DISK = N'{1}' WITH DIFFERENTIAL, DESCRIPTION = N'{2}-{3} Database Backup', SKIP, NOREWIND, STATS = 10""",
                        db, filePath, db, backupType);
            }
            else
            {
                arguments = string.Format(
                                        @"-S (local)\SQLEXPRESS -Q ""BACKUP DATABASE [{0}] TO DISK = N'{1}' WITH DESCRIPTION = N'{2}-{3}', NOFORMAT, NOINIT, NAME = N'{4}', SKIP, NOREWIND, NOUNLOAD,  STATS = 10"" ",
                db, filePath, db, backupType, db);
            }

            return arguments;
        }

        private void CreateProcess(string arguments)
        {
            // Create process
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "sqlcmd";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.Start();

            // Wait for process to finish
            process.WaitForExit();
            process.Close();
            process.Dispose();
        }

        #endregion
    }
}
