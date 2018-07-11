using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Windows.Storage;

namespace FastMD
{
    public class Settings : ObservableSettings
    {
        private static Settings settings = new Settings();
        public static Settings Default
        {
            get { return settings; }
        }

        public Settings()
            : base(ApplicationData.Current.LocalSettings)
        {
        }

        [DefaultSettingValue(Value = true)]
        public bool ThemeDefault
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = false)]
        public bool ThemeDark
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = false)]
        public bool ThemeLight
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = "")]
        public string MDDocument
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = "FastMD Export")]
        public string DefaultExportName
        {
            get { return Get<string>(); }
            set { Set(value); }
        }
    }
}
