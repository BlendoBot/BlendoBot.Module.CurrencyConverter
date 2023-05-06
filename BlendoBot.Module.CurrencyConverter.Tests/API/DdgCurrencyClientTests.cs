using BlendoBot.Module.CurrencyConverter.API;
using RichardSzalay.MockHttp;
using Xunit;

namespace BlendoBot.Module.CurrencyConverter.Tests.API;

public class DdgCurrencyClientTests : IDisposable {

	private const string CONVERSION_SUCCESS_PATH = "Files/currency-1-usd-aud-response.txt";
	private const string CONVERSION_BAD_FROM_CURRENCY_PATH = "Files/currency-1-bad-aud-response.txt";
	private const string CONVERSION_BAD_TO_CURRENCY_PATH = "Files/currency-1-usd-bad-response.txt";

	private DdgCurrencyClient ddgCurrencyClient;
	private MockHttpMessageHandler httpMessageHandler;

	public DdgCurrencyClientTests() {
		httpMessageHandler = new();
		ddgCurrencyClient = new(httpMessageHandler.ToHttpClient());
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			ddgCurrencyClient?.Dispose();
		}
	}

	[Fact]
	public void ConvertCurrencyDeserializesSuccess() {
		httpMessageHandler.When("https://duckduckgo.com/js/spice/currency/1/USD/AUD").Respond("application/javascript", File.ReadAllText(CONVERSION_SUCCESS_PATH));

		DdgConversionResponse expected = new() {
			Headers = new DdgConversionResponse.HeadersModel {
				Status = "0",
				Description = ""
			},
			Conversion = new DdgConversionResponse.ConversionModel {
				RateUtcTimestamp = new DateTime(2023, 5, 5, 16, 0, 0, DateTimeKind.Utc),
				RateFrequency = "daily rates",
				FromAmount = "1",
				FromCurrencySymbol = "USD",
				FromCurrencyName = "United States Dollars",
				ConvertedAmount = "1.48241",
				ToCurrencySymbol = "AUD",
				ToCurrencyName = "Australia Dollars",
				ConversionRate = "1 USD = 1.48241 AUD",
				ConversionInverse = "1 AUD = 0.674579 USD"
			}
		};

		DdgConversionResponse actual = ddgCurrencyClient.ConvertCurrency("1", "USD", "AUD").Result;

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ConvertCurrencyDeserializesInvalidFromCurrency() {
		httpMessageHandler.When("https://duckduckgo.com/js/spice/currency/1/BAD/AUD").Respond("application/javascript", File.ReadAllText(CONVERSION_BAD_FROM_CURRENCY_PATH));

		DdgConversionResponse expected = new() {
			Headers = new DdgConversionResponse.HeadersModel {
				Status = "4",
				Description = "ERROR: Invalid from_currency_symbol."
			},
			Conversion = new DdgConversionResponse.ConversionModel {
				RateUtcTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				RateFrequency = "daily rates",
				FromAmount = "1",
				FromCurrencySymbol = "BAD",
				FromCurrencyName = "",
				ConvertedAmount = "",
				ToCurrencySymbol = "AUD",
				ToCurrencyName = "",
				ConversionRate = "",
				ConversionInverse = ""
			}
		};

		DdgConversionResponse actual = ddgCurrencyClient.ConvertCurrency("1", "BAD", "AUD").Result;

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ConvertCurrencyDeserializesInvalidToCurrency() {
		httpMessageHandler.When("https://duckduckgo.com/js/spice/currency/1/USD/BAD").Respond("application/javascript", File.ReadAllText(CONVERSION_BAD_TO_CURRENCY_PATH));

		DdgConversionResponse expected = new() {
			Headers = new DdgConversionResponse.HeadersModel {
				Status = "5",
				Description = "ERROR: Invalid to_currency_symbol."
			},
			Conversion = new DdgConversionResponse.ConversionModel {
				RateUtcTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				RateFrequency = "daily rates",
				FromAmount = "1",
				FromCurrencySymbol = "USD",
				FromCurrencyName = "United States Dollars",
				ConvertedAmount = "",
				ToCurrencySymbol = "BAD",
				ToCurrencyName = "",
				ConversionRate = "",
				ConversionInverse = ""
			}
		};

		DdgConversionResponse actual = ddgCurrencyClient.ConvertCurrency("1", "USD", "BAD").Result;

		Assert.Equal(expected, actual);
	}
}
