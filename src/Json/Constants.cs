using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using SurrealDB.Json.Numbers;
using SurrealDB.Json.Time;

namespace SurrealDB.Json; 

/// <summary>
/// Collection of repeatedly used constants.
/// </summary>
public static class Constants {
    private static readonly Lazy<JsonSerializerOptions> _jsonSerializerOptions = new(CreateJsonOptions);

    /// <summary>
    /// Creates or returns the shared <see cref="JsonSerializerOptions"/> instance for this thread.
    /// </summary>
    public static JsonSerializerOptions JsonOptions => _jsonSerializerOptions.Value;

    /// <summary>
    /// Instantiates a new instance of <see cref="JsonSerializerOptions"/> with default settings.
    /// </summary>
    public static JsonSerializerOptions CreateJsonOptions() {
        return new() {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            // This was throwing an exception when set to JsonIgnoreCondition.Always
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            // TODO: Remove this when the server is fixed, see: https://github.com/surrealdb/surrealdb/issues/137
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IgnoreReadOnlyFields = false,
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
            Converters = { new DecimalConv(), new DoubleConv(), new SingleConv(), new DateTimeConv() , new DateTimeOffsetConv(), new TimeSpanConv(), new TimeOnlyConv(), new DateOnlyConv() },
        };
    }
}