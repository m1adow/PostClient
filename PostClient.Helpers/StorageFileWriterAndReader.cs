using Windows.Storage;
using System;
using System.Threading.Tasks;

namespace PostClient.Helpers
{
    public static class StorageFileWriterAndReader
    {
        public static async Task Write(string name, string data)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await storageFolder.CreateFileAsync(name,
                    CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, data);
        }

        public static async Task<string> Read(string name)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file;

            try
            {
                file = await storageFolder.GetFileAsync(name);
            }
            catch
            {
                file = await storageFolder.CreateFileAsync(name);
                await FileIO.WriteTextAsync(file, "[]");
            }

            string data = await FileIO.ReadTextAsync(file);

            return data;
        }
    }
}
