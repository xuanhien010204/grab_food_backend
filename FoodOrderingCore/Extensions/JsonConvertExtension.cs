using Newtonsoft.Json;

namespace FoodOrderingCore.Extensions
{
    public static class JsonConvertExtension
    {
        public static string ToJsonString(this object o)
        {
            if (o == null) return null;

            return JsonConvert.SerializeObject(o);
        }

        public static T ToObject<T>(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(s); 
        }
    }
}
