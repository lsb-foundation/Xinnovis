using System.Collections.Generic;
using System.Configuration;

namespace CommonLib.Models
{
    /// <summary>
    /// 气体类型代码
    /// </summary>
    public class GasTypeCode
    {
        public string GasName { get; set; }
        public int Code { get; set; }

        public static List<GasTypeCode> GetGasTypeCodes()
        {
            return new List<GasTypeCode>()
            {
                new GasTypeCode(){ GasName="空气(Air)", Code = 8},
                new GasTypeCode(){ GasName="氩气(Ar)", Code = 4},
                new GasTypeCode(){ GasName="二氧化碳(CO2)", Code = 25},
                new GasTypeCode(){ GasName="氦气(He)", Code = 1},
                new GasTypeCode(){ GasName="氢气(H2)", Code = 7},
                new GasTypeCode(){ GasName="甲烷(CH4)", Code = 28},
                new GasTypeCode(){ GasName="氮气(N2)", Code = 13},
                new GasTypeCode(){ GasName="氧气(O2)", Code = 15}
            };
        }

        public static List<GasTypeCode> GetGasTypeCodesFromConfiguration()
        {
            var section = GasTypeConfigurationSection.GetConfig();
            var gasTypeCodes = new List<GasTypeCode>();
            foreach(GasTypeConfigurationElement element in section.GasTypeCollection)
            {
                gasTypeCodes.Add(new GasTypeCode
                {
                    GasName = element.Name,
                    Code = element.Code
                });
            }
            return gasTypeCodes;
        }
    }

    #region config配置
    public class GasTypeConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get => base["name"] as string;
            set => base["name"] = value;
        }

        [ConfigurationProperty("code", IsRequired = true)]
        public int Code
        {
            get => (int)base["code"];
            set => base["code"] = value;
        }
    }

    public class GasTypeConfigurationElementCollection : ConfigurationElementCollection
    {
        public GasTypeConfigurationElement this[int index]
        {
            get => BaseGet(index) as GasTypeConfigurationElement;
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(value);
            }
        }

        public new GasTypeConfigurationElement this[string name]
        {
            get => BaseGet(name) as GasTypeConfigurationElement;
            set
            {
                if (BaseGet(name) != null)
                    BaseRemoveAt(BaseIndexOf(BaseGet(name)));
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new GasTypeConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as GasTypeConfigurationElement).Name;
        }
    }

    public class GasTypeConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty(nameof(GasTypeCollection))]
        [ConfigurationCollection(typeof(GasTypeConfigurationElementCollection), AddItemName = "GasType")]
        public GasTypeConfigurationElementCollection GasTypeCollection
        {
            get => this[nameof(GasTypeCollection)] as GasTypeConfigurationElementCollection;
        }

        public static GasTypeConfigurationSection GetConfig()
        {
            return ConfigurationManager.GetSection("GasTypeConfiguration") as GasTypeConfigurationSection;
        }
    }
    #endregion
}
