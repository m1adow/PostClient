using PostClient.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace PostClient.ViewModels.Helpers
{
    internal static class JSONSaverAndReaderHelper
    {    
        private static readonly string _name = "AccountCredentials.txt";

        public static async Task<Account> Read()
        {
            string json = await StorageFileWriterAndReader.Read(_name);

            Account account = JsonSerializer.Deserialize<Account>(json);

            return account;
        }

        public static void Save(Account account)
        {
            string json = JsonSerializer.Serialize((object)account, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            StorageFileWriterAndReader.Write(_name, json); 
        }
    }
}
