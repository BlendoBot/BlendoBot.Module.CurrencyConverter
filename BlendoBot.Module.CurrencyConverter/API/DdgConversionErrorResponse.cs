using System.Text.Json.Serialization;

namespace BlendoBot.Module.CurrencyConverter.API;

public record DdgConversionErrorResponse {
	[JsonPropertyName("code")]
	public int Code;

	[JsonPropertyName("message")]
	public string Message = string.Empty;
}
