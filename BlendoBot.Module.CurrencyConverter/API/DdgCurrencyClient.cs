using System;
using System.Linq;
using System.Net;
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

	public async Task<Result<DdgConversionResponse, DdgConversionErrorResponse>> ConvertCurrency(string amount, string fromCurrency, string toCurrency) {
		return await Get<DdgConversionResponse, DdgConversionErrorResponse>($"https://duckduckgo.com/js/spice/currency/{amount}/{fromCurrency}/{toCurrency}");
	}

	private async Task<Result<T, E>> Get<T, E>(string url) {
		HttpResponseMessage response = await httpClient.GetAsync(url);
		if (response.StatusCode == HttpStatusCode.BadRequest) {
			E errorResponse = JsonSerializer.Deserialize<E>(StripExtraContent(await response.Content.ReadAsStringAsync()), new JsonSerializerOptions() { IncludeFields = true }) ?? throw new JsonException($"Could not deserialize {nameof(E)}");
			return new(errorResponse);
		}
		response.EnsureSuccessStatusCode();
		T responseObject = JsonSerializer.Deserialize<T>(StripExtraContent(await response.Content.ReadAsStringAsync()), new JsonSerializerOptions() { IncludeFields = true }) ?? throw new JsonException($"Could not deserialize {nameof(T)}");
		return new(responseObject);
	}

	private async Task<T> Get<T>(string url) {
		string response = await httpClient.GetStringAsync(url);
		T responseObject = JsonSerializer.Deserialize<T>(StripExtraContent(response), new JsonSerializerOptions() { IncludeFields = true }) ?? throw new JsonException($"Could not deserialize {nameof(T)}");
		return responseObject;
	}

	private static string StripExtraContent(string content) => content.Split("\n").Skip(1).First()[..^2];
}
