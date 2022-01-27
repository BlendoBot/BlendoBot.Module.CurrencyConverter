using BlendoBot.Core.Command;
using BlendoBot.Core.Entities;
using BlendoBot.Core.Module;
using BlendoBot.Core.Utility;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlendoBot.Module.CurrencyConverter;

internal class CurrencyConverterCommand : ICommand {
	public CurrencyConverterCommand(CurrencyConverter module) {
		this.module = module;
	}

	private readonly CurrencyConverter module;
	public IModule Module => module;

	public string Guid => "currencyconverter.command";
	public string DesiredTerm => "currency";
	public string Description => "Returns the conversion rate between two currencies";
	public Dictionary<string, string> Usage => new() {
		{ "[value] [from] [to]", $"Shows how much {"value".Code()} in currency {"from".Code()} is in {"to".Code()}" },
	};
		
	public async Task OnMessage(MessageCreateEventArgs e, string[] tokenizedInput) {
		if (tokenizedInput.Length < 3) {
			await module.DiscordInteractor.Send(this, new SendEventArgs {
				Message = $"Too few arguments specified to {module.ModuleManager.GetHelpTermForCommand(this).Code()}",
				Channel = e.Channel,
				Tag = "CurrencyErrorTooFewArgs"
			});
			return;
		}

		if (!double.TryParse(tokenizedInput[0], out double amount)) {
			await module.DiscordInteractor.Send(this, new SendEventArgs {
				Message = $"Incorrect input: the currency value supplied was not a number!",
				Channel = e.Channel,
				Tag = "CurrencyErrorNonNumericValue"
			});
			return;
		}

		if (amount < 0.0) {
			await module.DiscordInteractor.Send(this, new SendEventArgs {
				Message = $"Incorrect input: the currency value supplied was less than 0!",
				Channel = e.Channel,
				Tag = "CurrencyErrorNegativeValue"
			});
			return;
		}

		string fromCurrency = tokenizedInput[1];
		int foundMatches = 0;
		List<string> failedMatches = new();

		StringBuilder sb = new();

		for (int i = 2; i < tokenizedInput.Length; ++i) {
			using HttpClient wc = new();
			string convertJsonString = await wc.GetStringAsync($"https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency={fromCurrency}&to_currency={tokenizedInput[i]}&apikey={module.ApiKey}");
			dynamic convertJson = JsonConvert.DeserializeObject(convertJsonString);
			try {
				double rate = convertJson["Realtime Currency Exchange Rate"]["5. Exchange Rate"];
				if (foundMatches == 0) {
					sb.AppendLine($"{amount.ToString("0.00000")[..7].Code()} - {convertJson["Realtime Currency Exchange Rate"]["1. From_Currency Code"]} ({((string)convertJson["Realtime Currency Exchange Rate"]["2. From_Currency Name"]).Italics()})");
				}
				sb.AppendLine($"{(amount * rate).ToString("0.00000")[..7].Code()} - {convertJson["Realtime Currency Exchange Rate"]["3. To_Currency Code"]} ({((string)convertJson["Realtime Currency Exchange Rate"]["4. To_Currency Name"]).Italics()})");
				++foundMatches;
			} catch (Exception) {
				// Unsuccessful, next one.
				failedMatches.Add(tokenizedInput[i]);
			}
		}

		if (failedMatches.Count > 0) {
			sb.Append("Failed to match the currency codes: ");
			foreach (string failedCode in failedMatches) {
				sb.Append($"{failedCode.Code()} ");
			}
		}

		await module.DiscordInteractor.Send(this, new SendEventArgs {
			Message = sb.ToString(),
			Channel = e.Channel,
			Tag = "CurrencySuccess"
		});
	}
}
