using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace GenesisRadioApp
{
    public class Database
    {
        private SQLiteConnection database;
        private string dbPath = Path.Combine(GetFolderPath(SpecialFolder.Personal), "database.db3");

        public Database()
        {
            database = new SQLiteConnection(dbPath);
            database.CreateTable<Message>();
        }

        public List<Message> GetMessages()
        {
            return database.Table<Message>().ToList();
        }

        public int SaveMessage(Message message)
        {
            return database.Insert(message);
        }
    }
}