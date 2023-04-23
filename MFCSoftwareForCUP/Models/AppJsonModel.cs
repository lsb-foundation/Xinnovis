using MFCSoftware.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MFCSoftwareForCUP.Models
{
    public class AppJsonModel
    {
        public string PortName { get; set; }
        public int DeviceMaxCount { get; set; }
        public int AddressToAdd { get; set; }
        public List<DeviceExtras> Devices { get; set; }

        static async Task<AppJsonModel> ReadFromFileAsync()
        {
            string jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"models\app.json");
            if (!File.Exists(jsonFile))
            {
                return null;
            }
            using var stream = new FileStream(jsonFile, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AppJsonModel>(json);
        }

        //json反序列化经常出问题，直接将数据存储至sqlite数据库
        public async Task SaveAsync()
        {
            //Save device extras
            var deviceExtras = Devices.Select(e => new CupDeviceExtras
            {
                Address = e.Address,
                Floor = e.Floor,
                Room = e.Room,
                GasType = e.GasType
            }).ToList();

            await SqliteHelper.UpdateCupDeviceExtras(deviceExtras);

            //Save settings
            await SqliteHelper.UpdateSettings(nameof(PortName), PortName);
            await SqliteHelper.UpdateSettings(nameof(AddressToAdd), AddressToAdd.ToString());
            await SqliteHelper.UpdateSettings(nameof(DeviceMaxCount), DeviceMaxCount.ToString());
        }

        static async Task<AppJsonModel> TryLoadFromJsonFileAsync()
        {
            try
            {
                var validStr = await SqliteHelper.GetSettings("JsonFileValid");
                var jsonFileValid = string.IsNullOrEmpty(validStr) || bool.Parse(validStr);

                //先尝试从json文件中加载，实现无缝切换
                if (jsonFileValid && await ReadFromFileAsync() is AppJsonModel jsonModel)
                {
                    await SqliteHelper.UpdateSettings("JsonFileValid", "false");
                    return jsonModel;
                }
            }
            catch { }

            return null;
        }

        public static async Task<AppJsonModel> LoadAsync()
        {
            var model = await TryLoadFromJsonFileAsync();
            if (model is not null) return model;

            var portName = await SqliteHelper.GetSettings(nameof(PortName));
            if (string.IsNullOrEmpty(portName)) return null;

            int.TryParse(await SqliteHelper.GetSettings(nameof(DeviceMaxCount)), out int deviceMaxCount);
            int.TryParse(await SqliteHelper.GetSettings(nameof(AddressToAdd)), out int addressToAdd);

            model = new AppJsonModel
            {
                PortName = portName,
                DeviceMaxCount = deviceMaxCount,
                AddressToAdd = addressToAdd
            };

            var deviceExtras = await SqliteHelper.GetDeviceExtrasAsync();
            model.Devices = deviceExtras.Select(e => new DeviceExtras
            {
                Address = e.Address,
                Floor = e.Floor,
                Room = e.Room,
                GasType = e.GasType
            }).ToList();

            return model;
        }
    }
}