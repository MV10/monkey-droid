using System.Text.Json.Serialization;

namespace monkeydroid.Models;

[JsonSerializable(typeof(AppData))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class AppDataJsonContext : JsonSerializerContext
{
}
