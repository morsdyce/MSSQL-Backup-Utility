using System;
using System.IO;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.DropBox;

namespace SQLBackupUtility
{
    public static class DropboxHandler
    {
        private const string ConsumerKey = "key";
        private const string ConsumerSecret = "secret";
        private static DropBoxCredentials _credentials;
        private static string _remoteDirectory;

        public static void SetCredientals(string dpUser, string dpPassword)
        {
            if (string.IsNullOrEmpty(dpUser) || string.IsNullOrEmpty(dpPassword))
                return;

            _credentials = new DropBoxCredentials();
            _credentials.ConsumerKey = ConsumerKey;
            _credentials.ConsumerSecret = ConsumerSecret;
            _credentials.UserName = dpUser.Trim();
            _credentials.Password = dpPassword.Trim();
        }

        public static void SetRemoteDirectory(string remoteDirectory)
        {
            _remoteDirectory = remoteDirectory.Trim();
        }

        public static void UploadFile(string filename, string path)
        {
            if (_credentials == null)
                return;
            else if (string.IsNullOrEmpty(_remoteDirectory))
                return;

            Console.WriteLine("Uploading file to dropbox");

   
            // instanciate a cloud storage configuration, e.g. for dropbox
            DropBoxConfiguration configuration = DropBoxConfiguration.GetStandardConfiguration();

            // instanciate the cloudstorage manager
            CloudStorage storage = new CloudStorage();

            // open the connection to the storage
            if (!storage.Open(configuration, _credentials))
            {
                Console.WriteLine("Connection failed");
                return;
            }


            ICloudDirectoryEntry folder = _remoteDirectory.Equals("root", StringComparison.CurrentCultureIgnoreCase)
                                                           ? storage.GetRoot()
                                                           : GetDirectoryPath(storage);

            // create the file
            ICloudFileSystemEntry file = storage.CreateFile(folder, filename);

            // upload the data
            Stream data = file.GetContentStream(FileAccess.Write);

            // build a stream read
            var writer = new BinaryWriter(data);

            var filedata = File.ReadAllBytes(path);
            writer.Write(filedata);

        
            // close the streamreader
            writer.Close();

            // close the stream
            data.Close();


            // close the cloud storage connection
            if (storage.IsOpened)
            {
                storage.Close();
            }

            Console.WriteLine("Upload Complete");
        }

        private static ICloudDirectoryEntry GetDirectoryPath(CloudStorage storage)
        {
            return storage.GetFolder(_remoteDirectory, storage.GetRoot()) 
                ?? storage.CreateFolder(_remoteDirectory, storage.GetRoot());
        }
    }
}
