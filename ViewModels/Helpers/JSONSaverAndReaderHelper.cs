using System.Text.Json;
using System.Threading.Tasks;

namespace PostClient.ViewModels.Helpers
{
    internal static class JSONSaverAndReaderHelper
    {
        public static void Save<T>(T objectForSerialization, string name)
        {
            string json = JsonSerializer.Serialize((object)objectForSerialization, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            StorageFileWriterAndReader.Write(name, json);
        }

        public static async Task<T> Read<T>(string name)
        {
            string json = await StorageFileWriterAndReader.Read(name);

            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
