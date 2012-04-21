MSSQL-Backup-Utility
====================

A MSSQL database backup utility  that backs up full and partial to your server hard drive and dropbox account.

Parameters
----------

?, h, help, -help					- Displays the help file

-d, -directory						- (Required) Set the directory where the backup files will be saved

-t, -type							- (Required) Set the type of backup, must be either Full or Diffrential

-c, -connection						- (Optional) Sets the connection string to the SQL Server (you may include username and password here) Defaults to: \r\n Server=(local)\SQLEXPRESS; Integrated Security=SSPI

-i, -ignore							- (Optional) Adds databases that you do not wish to backup at this point, if not specified all databases on the server excluding the system databases will be backed up. You must input this parameter separated by commas such as DB1,DB2,DB3

