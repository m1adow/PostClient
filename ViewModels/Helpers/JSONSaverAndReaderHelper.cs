using Newtonsoft.Json;
using PostClient.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace PostClient.ViewModels.Helpers
{
    public static class JSONSaverAndReaderHelper
    {
        private static StorageFolder _storageFolder = ApplicationData.Current.LocalFolder;
        private static string _name = "AccountCredentials.txt";

        public static async Task<Account> Read()
        {
            StorageFile file = await _storageFolder.GetFileAsync(_name);

            string json = await FileIO.ReadTextAsync(file);

            Account account = JsonConvert.DeserializeObject<Account>(json);

            return account;
        }

        public static async void Save(Account account)
        {
            string json = JsonConvert.SerializeObject(account);

            StorageFile file = await _storageFolder.CreateFileAsync(_name,
                    CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, json);  
        }
    }
}
