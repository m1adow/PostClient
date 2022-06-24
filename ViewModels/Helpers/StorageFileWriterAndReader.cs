using Windows.Storage;
using System;
using System.Threading.Tasks;

namespace PostClient.ViewModels.Helpers
{
    internal static class StorageFileWriterAndReader
    {
        public static async void Write(string name, string data)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await storageFolder.CreateFileAsync(name,
                    CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, data);
        }

        public static async Task<string> Read(string name)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await storageFolder.GetFileAsync(name);

            string data = await FileIO.ReadTextAsync(file);

            return data;
        }
    }
}
