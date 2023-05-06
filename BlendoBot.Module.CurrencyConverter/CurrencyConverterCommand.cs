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

		DdgConversionResponse response = await module.DdgCurrencyClient.ConvertCurrency(tokenizedInput[0], tokenizedInput[1], tokenizedInput[2]);

		if (response.Headers.Status == "0") {
			await module.DiscordInteractor.Send(this, new SendEventArgs {
				Message = $"{response.Conversion.FromAmount} {response.Conversion.FromCurrencySymbol} ({response.Conversion.FromCurrencyName.Italics()}) = {response.Conversion.ConvertedAmount} {response.Conversion.ToCurrencySymbol} ({response.Conversion.ToCurrencyName.Italics()})",
				Channel = e.Channel,
				Tag = "CurrencySuccess"
			});
		} else {
			await module.DiscordInteractor.Send(this, new SendEventArgs {
				Message = $"Something went wrong!\n{response.Headers.Description}",
				Channel = e.Channel,
				Tag = "CurrencyFailure"
			});
		}

	}
}
