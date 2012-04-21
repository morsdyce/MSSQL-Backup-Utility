using System;
using System.IO;
using AppLimit.CloudComputing.SharpBox.DropBox;
using NDesk.Options;

namespace SQLBackupUtility
{
    class Program
    {
        private static string _backupDirectory;
        private static string _backupType;
        private static string _dpUser;
        private static string _dpPassword;
        private static string _remoteDirectory;

        static void Main(string[] args)
        {
            var sqlManager = new SQLManager();

            OptionSet options = new OptionSet()
                .Add("d=|directory=", b => _backupDirectory = b)
                .Add("t=|type=", b => _backupType = b)
                .Add("c=|connection=", sqlManager.SetConnectionString)
                .Add("i=|ignore=", sqlManager.AddIgnoredDatabases)
                .Add("u=|user=",u => _dpUser = u)
                .Add("p=|password=", p => _dpPassword = p)
                .Add("rd=|remotedirectory=", d => _remoteDirectory = d)
                .Add("?|h|help", h => DisplayHelp());

            options.Parse(args);

            if (string.IsNullOrEmpty(_backupDirectory) || string.IsNullOrEmpty(_backupType))
            {
                DisplayHelp();
            }

            if (!_backupType.Equals("full", StringComparison.CurrentCultureIgnoreCase) & !_backupType.Equals("differential", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine("Type must be either Differential or Full");
                Environment.Exit(0);
            }

            if (sqlManager.IsConnectionStringNull())
                sqlManager.SetConnectionString(@"Server=(local)\SQLEXPRESS; Integrated Security=SSPI");

            if (!Directory.Exists(_backupDirectory))
                Directory.CreateDirectory(_backupDirectory);


            // Set Dropbox login data
            if (!string.IsNullOrEmpty(_dpUser) || !string.IsNullOrEmpty(_dpPassword))
                DropboxHandler.SetCredientals(_dpUser,_dpPassword);

            // Set Dropbox remote directory
            if (!string.IsNullOrEmpty(_remoteDirectory))
                DropboxHandler.SetRemoteDirectory(_remoteDirectory);

            // Get the databases from the provided SQL Server
            var dbList = sqlManager.GetDatabases();

            // Save backups of the databases
            sqlManager.SaveBackups(dbList, _backupDirectory, _backupType);
             
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("SQLBackup Utility Help");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Parameter flags:");
            Console.WriteLine();
            Console.Write("?, h, help, -help".PadRight(23));
            Console.WriteLine(" - Displays the help file");
            Console.WriteLine();
            Console.Write("-d, -directory".PadRight(23));
            Console.WriteLine(" - (Required) Set the directory where the backup");
            Console.Write("".PadRight(23));
            Console.WriteLine(" files will be saved.");
            Console.WriteLine();
            Console.Write("-t, -type".PadRight(23));
            Console.WriteLine(" - (Required) Set the type of backup, must be");
            Console.Write("".PadRight(23));
            Console.WriteLine(" either Full or Diffrential");
            Console.WriteLine();
            Console.Write("-c, -connection".PadRight(23));
            Console.WriteLine(@" - (Optional) Sets the connection string to the");
            Console.Write("".PadRight(23));
            Console.WriteLine(" SQL Server(You may include username and password");
            Console.Write("".PadRight(23));
            Console.WriteLine(@" here) Defaults to ");
            Console.Write("".PadRight(23));
            Console.WriteLine(@" Server=(local)\SQLEXPRESS; Integrated Security=SSPI");
            Console.WriteLine();
            Console.Write("-i, -ignore".PadRight(23));
            Console.WriteLine(" - (Optional) Adds databases that you do not wish to");
            Console.Write("".PadRight(23));
            Console.WriteLine(" backup at this point, if not specified all database on");
            Console.Write("".PadRight(23));
            Console.WriteLine(" the server excluding the system databases will be");
            Console.Write("".PadRight(23));
            Console.WriteLine(" backed up. You must input this parameter seperated by");
            Console.Write("".PadRight(23));
            Console.WriteLine(" commas as such: DB1,DB2,DB3");
            Console.WriteLine();
            Console.Write("-u, -user".PadRight(23));
            Console.WriteLine(" - (Upload Required) Sets Username for Dropbox upload");
            Console.WriteLine();
            Console.Write("-p, -password".PadRight(23));
            Console.WriteLine(" - (Upload Required) Sets the Password for Dropbox upload");
            Console.WriteLine();
            Console.Write("-rd, -remotedirectory".PadRight(23));
            Console.WriteLine(" - (Optional) Sets the remote directory on Dropbox where");
            Console.Write("".PadRight(23));
            Console.WriteLine(" backup files will be saved. if no value provided");
            Console.Write("".PadRight(23));
            Console.WriteLine(" defaults to root");
           


            Environment.Exit(0);
        }
    }
}
