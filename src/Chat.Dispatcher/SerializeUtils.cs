using Newtonsoft.Json;

namespace Chat.Dispatcher
{
    public static class SerializeUtils
    {
        public static bool SerializeToFile<T>(T obj, string filePath)
        {
            string json = JsonConvert.SerializeObject(obj);
            try
            {
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static T? TryDeserializeFile<T>(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var deserializedObj = JsonConvert.DeserializeObject<T>(json);
                return deserializedObj;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}

