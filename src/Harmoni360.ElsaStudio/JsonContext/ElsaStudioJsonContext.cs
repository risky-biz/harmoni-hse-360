using System.Text.Json.Serialization;

namespace Harmoni360.ElsaStudio.JsonContext;

// Define source-generated JSON context for common Elsa types
// This avoids NullabilityInfoContext_NotSupported errors in WebAssembly
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(List<object>))]
[JsonSerializable(typeof(object[]))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(Guid))]
public partial class ElsaStudioJsonContext : JsonSerializerContext
{
}