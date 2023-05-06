using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlendoBot.Module.CurrencyConverter.API;

public record DdgConversionResponse {
	[JsonPropertyName("headers")]
	public HeadersModel Headers;

	[JsonPropertyName("conversion")]
	public ConversionModel Conversion;

	public record HeadersModel {
		[JsonPropertyName("status")]
		public string Status;

		[JsonPropertyName("description")]
		public string Description;

	}

	public record ConversionModel {
		[JsonPropertyName("rate-utc-timestamp")]
		[JsonConverter(typeof(DateTimeConverter))]
		public DateTime RateUtcTimestamp;

		[JsonPropertyName("rate-frequency")]
		public string RateFrequency;

		[JsonPropertyName("from-amount")]
		public string FromAmount;

		[JsonPropertyName("from-currency-symbol")]
		public string FromCurrencySymbol;

		[JsonPropertyName("from-currency-name")]
		public string FromCurrencyName;

		[JsonPropertyName("converted-amount")]
		public string ConvertedAmount;

		[JsonPropertyName("to-currency-symbol")]
		public string ToCurrencySymbol;

		[JsonPropertyName("to-currency-name")]
		public string ToCurrencyName;

		[JsonPropertyName("conversion-rate")]
		public string ConversionRate;

		[JsonPropertyName("conversion-inverse")]
		public string ConversionInverse;
	}
}

internal class DateTimeConverter : JsonConverter<DateTime> {
	private const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm";

	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		string s = reader.GetString();
		if (string.IsNullOrEmpty(s)) {
			return DateTime.SpecifyKind(DateTime.UnixEpoch, DateTimeKind.Utc);
		} else {
			return DateTime.SpecifyKind(DateTime.ParseExact(string.Join("", reader.GetString().SkipLast(4)), DATETIME_FORMAT, null), DateTimeKind.Utc);
		}
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(DATETIME_FORMAT));
}
