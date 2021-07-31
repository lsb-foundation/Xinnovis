using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MFCSoftwareForCUP.Models
{
    public class AppJsonModel
    {
        public string PortName { get; set; }
        public int DeviceMaxCount { get; set; }
        public int AddressToAdd { get; set; }
        public List<DeviceExtras> Devices { get; set; }

        public async void Save()
        {
            string jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"models\app.json");
            string path = Path.GetDirectoryName(jsonFile);
            if (!Directory.Exists(path))
            {
                _ = Directory.CreateDirectory(path);
            }
            if (File.Exists(jsonFile))
            {
                File.Delete(jsonFile);
            }
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            using (var stream = new FileStream(jsonFile, FileMode.OpenOrCreate, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync(json);
            }
        }

        public static async Task<AppJsonModel> ReadFromFileAsync()
        {
            string jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"models\app.json");
            if (!File.Exists(jsonFile))
            {
                return null;
            }
            using (var stream = new FileStream(jsonFile, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                string json = await reader.ReadToEndAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<AppJsonModel>(json);
            }
        }
    }
}