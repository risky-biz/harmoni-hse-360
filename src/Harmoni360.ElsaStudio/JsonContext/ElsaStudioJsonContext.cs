using System.Text.Json.Serialization;
using Elsa.Api.Client.Shared.Models;
using Elsa.Api.Client.Resources.Features.Models;

namespace Harmoni360.ElsaStudio.JsonContext;

// Define source-generated JSON context for Elsa API types
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
[JsonSerializable(typeof(ListResponse<FeatureDescriptor>))]
[JsonSerializable(typeof(FeatureDescriptor))]
[JsonSerializable(typeof(List<FeatureDescriptor>))]
public partial class ElsaStudioJsonContext : JsonSerializerContext
{
}