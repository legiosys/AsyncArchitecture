using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace EventSchemaRegistry;

public static class SchemeValidator
{
    public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions() {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
    public static bool IsValid<T>(KafkaEvent<T> value, out IList<string> errors) where T : class
    {
        var eventNameArr = value.EventName.Split('_');
        var schema = JSchema.Parse(new StreamReader(Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"EventSchemaRegistry.Schemas.{eventNameArr[0]}.{eventNameArr[1]}.{value.Version}.json"))
            .ReadToEnd());

        var result = JObject.Parse(JsonSerializer.Serialize(value, SerializerOptions))
            .IsValid(schema, out errors);
        return result;
    }

    public static void ValidateOrThrow<T>(KafkaEvent<T> value) where T : class
    {
        if(!IsValid(value, out var errors))
            throw new ValidationException(string.Join(',', errors));
    }
}