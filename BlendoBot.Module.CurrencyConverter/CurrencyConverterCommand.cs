using BlendoBot.Core.Command;
using BlendoBot.Core.Entities;
using BlendoBot.Core.Module;
using BlendoBot.Core.Utility;
using BlendoBot.Module.CurrencyConverter.API;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
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
		{ "[value] [from] [to]", $"Converts {"value".Code()} amount of currency {"from".Code()} into currency {"to".Code()}" },
		{ "Note", "The currently supported currencies are listed at https://www.xe.com/currencyconverter/" }
	};
		
	public async Task OnMessage(MessageCreateEventArgs e, string[] tokenizedInput) {
		if (tokenizedInput.Length < 3) {
			await module.DiscordInteractor.Send(this, new SendEventArgs {
				Message = $"Too few arguments specified! See {module.ModuleManager.GetHelpTermForCommand(this).Code()} for help!",
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

		Result<DdgConversionResponse, DdgConversionErrorResponse> response = await module.DdgCurrencyClient.ConvertCurrency(tokenizedInput[0], tokenizedInput[1], tokenizedInput[2]);

		if (response.Success is not null) {
			if (response.Success.To.Count > 0) {
				await module.DiscordInteractor.Send(this, new SendEventArgs {
					Message = $"{response.Success.Amount} {response.Success.From} = {response.Success.To[0].Mid} {response.Success.To[0].QuoteCurrency}",
					Channel = e.Channel,
					Tag = "CurrencySuccess"
				});
			} else {
				await module.DiscordInteractor.Send(this, new SendEventArgs {
					Message = $"Something went wrong!\nError: Invalid to currency {tokenizedInput[2]}.",
					Channel = e.Channel,
					Tag = "CurrencyInvalidToCurrency"
				});
			}
		} else {
			if (response.Error is not null) {
				if (response.Error.Message.StartsWith("No ") && response.Error.Message.Contains(" found on ")) {
					await module.DiscordInteractor.Send(this, new SendEventArgs {
						Message = $"Something went wrong!\nError: Invalid from currency {tokenizedInput[1]}.",
						Channel = e.Channel,
						Tag = "CurrencyInvalidFromCurrency"
					});
				} else {
					await module.DiscordInteractor.Send(this, new SendEventArgs {
						Message = $"Something went wrong!\n{response.Error.Message}",
						Channel = e.Channel,
						Tag = "CurrencyInvalidGeneric"
					});
				}
			} else {
				await module.DiscordInteractor.Send(this, new SendEventArgs {
					Message = $"Something went wrong!\nThe API returned something unhandleable!",
					Channel = e.Channel,
					Tag = "CurrencyFailure"
				});
			}
		}
	}
}
