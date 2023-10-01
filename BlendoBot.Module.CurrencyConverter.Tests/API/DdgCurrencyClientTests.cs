using BlendoBot.Module.CurrencyConverter.API;
using RichardSzalay.MockHttp;
using System.Net;
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

		Result<DdgConversionResponse, DdgConversionErrorResponse> expected = new(new DdgConversionResponse {
			From = "USD",
			Amount = 1.0m,
			Timestamp = new DateTime(2023, 9, 30, 22, 33, 0, DateTimeKind.Utc),
			To = new() {
				new() {
					QuoteCurrency = "AUD",
					Mid = 1.5541611393m
				}
			}
		});

		Result<DdgConversionResponse, DdgConversionErrorResponse> actual = ddgCurrencyClient.ConvertCurrency("1", "USD", "AUD").Result;

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ConvertCurrencyDeserializesInvalidFromCurrency() {
		httpMessageHandler.When("https://duckduckgo.com/js/spice/currency/1/BAD/AUD").Respond(HttpStatusCode.BadRequest, "application/javascript", File.ReadAllText(CONVERSION_BAD_FROM_CURRENCY_PATH));

		Result<DdgConversionResponse, DdgConversionErrorResponse> expected = new(new DdgConversionErrorResponse {
			Code = 7,
			Message = "No BAD found on 2023-09-30T22:33:00Z"
		});

		Result<DdgConversionResponse, DdgConversionErrorResponse> actual = ddgCurrencyClient.ConvertCurrency("1", "BAD", "AUD").Result;

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ConvertCurrencyDeserializesInvalidToCurrency() {
		httpMessageHandler.When("https://duckduckgo.com/js/spice/currency/1/USD/BAD").Respond("application/javascript", File.ReadAllText(CONVERSION_BAD_TO_CURRENCY_PATH));

		Result<DdgConversionResponse, DdgConversionErrorResponse> expected = new(new DdgConversionResponse {
			From = "USD",
			Amount = 1.0m,
			Timestamp = new DateTime(2023, 9, 30, 22, 33, 0, DateTimeKind.Utc),
			To = new()
		});

		Result<DdgConversionResponse, DdgConversionErrorResponse> actual = ddgCurrencyClient.ConvertCurrency("1", "USD", "BAD").Result;

		Assert.Equal(expected, actual);
	}
}
