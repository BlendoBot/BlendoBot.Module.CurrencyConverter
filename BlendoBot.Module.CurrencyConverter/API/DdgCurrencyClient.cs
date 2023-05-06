using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlendoBot.Module.CurrencyConverter.API;

public class DdgCurrencyClient : IDisposable {
	public DdgCurrencyClient(HttpClient httpClient) {
		this.httpClient = httpClient;
	}

	private readonly HttpClient httpClient;

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			httpClient?.Dispose();
		}
	}

	public async Task<DdgConversionResponse> ConvertCurrency(string amount, string fromCurrency, string toCurrency) {
		return await Get<DdgConversionResponse>($"https://duckduckgo.com/js/spice/currency/{amount}/{fromCurrency}/{toCurrency}");
	}

	private async Task<T> Get<T>(string url) {
		string response = await httpClient.GetStringAsync(url);
		T responseObject = JsonSerializer.Deserialize<T>(StripExtraContent(response), new JsonSerializerOptions() { IncludeFields = true }) ?? throw new JsonException($"Could not deserialize {nameof(T)}");
		return responseObject;
	}

	private static string StripExtraContent(string content) => string.Join("\n", content.Split("\n").Skip(1).SkipLast(2));
}
