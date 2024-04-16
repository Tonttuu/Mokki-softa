using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector; // Tuodaan MySqlConnector -kirjasto

namespace Mokki_softa
{
    public class DatabaseConnector
    {   // piilota nämä, oma file tms?? kuten conn.php toisessa projektissa
        private readonly string server = "localhost";
        private readonly string port = "3307";
        private readonly string uid = "root";
        private readonly string pwd = "Ruutti";
        private readonly string database = "vn";
        public DatabaseConnector()
        {
        }
        public MySqlConnection _getConnection()
        {
            string connectionString =
           $"Server={server};Port={port};uid={uid};password={pwd};database={database}";

            MySqlConnection connection = new MySqlConnection(connectionString);
            return connection;
        }
    }
}