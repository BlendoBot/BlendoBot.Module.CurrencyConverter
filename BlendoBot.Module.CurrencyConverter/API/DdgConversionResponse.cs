using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlendoBot.Module.CurrencyConverter.API;

public record DdgConversionResponse {
	[JsonPropertyName("from")]
	public string From = string.Empty;

	[JsonPropertyName("amount")]
	public decimal Amount;

	[JsonPropertyName("timestamp")]
	[JsonConverter(typeof(DateTimeConverter))]
	public DateTime Timestamp;

	[JsonPropertyName("to")]
	public List<ToModel> To = new();

	public record ToModel {
		[JsonPropertyName("quotecurrency")]
		public string QuoteCurrency = string.Empty;

		[JsonPropertyName("mid")]
		public decimal Mid;
	}

	public virtual bool Equals(DdgConversionResponse? other) {
		return other is not null &&
			From == other.From &&
			Amount == other.Amount &&
			Timestamp == other.Timestamp &&
			To.SequenceEqual(other.To);
	}

	public override int GetHashCode() {
		int hash = 1128120 + 5 * From.GetHashCode() + 2 * Amount.GetHashCode() + 17 * Timestamp.GetHashCode();
		foreach (ToModel to in To) {
			hash += 7 * to.QuoteCurrency.GetHashCode() + 11 * to.Mid.GetHashCode();
		}
		return hash;
	}
}

internal class DateTimeConverter : JsonConverter<DateTime> {
	private const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss";

	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		string? s = reader.GetString();
		if (string.IsNullOrEmpty(s)) {
			return DateTime.SpecifyKind(DateTime.UnixEpoch, DateTimeKind.Utc);
		} else {
			return DateTime.SpecifyKind(DateTime.ParseExact(string.Join("", s.SkipLast(1)), DATETIME_FORMAT, null), DateTimeKind.Utc);
		}
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(DATETIME_FORMAT));
}
