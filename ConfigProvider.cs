using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mokki_softa
{
    public static class ConfigurationProvider
    {
        public static AppSettings GetAppSettings()
        {
            return new AppSettings
            {
                Server = "localhost",
                Port = "3307",
                UserId = "root",
                Password = "Ruutti",
                Database = "vn"
            };
        }
    }
}
