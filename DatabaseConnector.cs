using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector; // Tuodaan MySqlConnector -kirjasto

namespace Mokki_softa
{
    public class DatabaseConnector
    {
        private readonly AppSettings _appSettings;

        public DatabaseConnector(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public MySqlConnection GetConnection()
        {
            string connectionString =
                $"Server={_appSettings.Server};Port={_appSettings.Port};uid={_appSettings.UserId};" +
                $"password={_appSettings.Password};database={_appSettings.Database}";

            MySqlConnection connection = new MySqlConnection(connectionString);
            return connection;
        }
    }
}