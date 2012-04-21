using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SQLBackupUtility
{
    public class JsonHandler
    {
        private IList<string> _databaseList;

        public JsonHandler()
        {
            _databaseList = LoadDatabaseList();
            if (_databaseList == null)
                _databaseList = new List<string>();
        }

        public bool CheckDatabase(string databaseName)
        {
            return _databaseList.Contains(databaseName);
        }

        public void AddDatabase(string databaseName)
        {
            _databaseList.Add(databaseName);
        }

        private List<string> LoadDatabaseList()
        {
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),@"SQLBackup\list.json")))
            {
                var streamReader = File.OpenText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                           @"SQLBackup\list.json"));

                string json = streamReader.ReadToEnd();

                streamReader.Close();

               return JsonConvert.DeserializeObject<List<string>>(json);
            }

            return null;
        }

        public void SaveDatabaseList()
        {

            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SQLBackup")))
            {
                Directory.CreateDirectory(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SQLBackup"));
            }

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SQLBackup\list.json");

            var stream = File.CreateText(filePath);

            stream.WriteLine(JsonConvert.SerializeObject(_databaseList));

            stream.Flush();
            stream.Close();
        }
    }
}
