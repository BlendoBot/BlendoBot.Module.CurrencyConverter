using BlendoBot.Core.Entities;
using BlendoBot.Core.Module;
using BlendoBot.Core.Services;
using System.Threading.Tasks;

namespace BlendoBot.Module.CurrencyConverter;

[Module(Guid = "com.biendeo.blendobot.module.currencyconverter", Name = "Currency Converter", Author = "Biendeo", Version = "2.0.0", Url = "https://github.com/BlendoBot/BlendoBot.Module.CurrencyConverter")]
public class CurrencyConverter : IModule {
	public CurrencyConverter(IConfig config, IDiscordInteractor discordInteractor, IModuleManager moduleManager, ILogger logger) {
		Config = config;
		DiscordInteractor = discordInteractor;
		ModuleManager = moduleManager;
		Logger = logger;

		CurrencyConverterCommand = new(this);
	}

	internal ulong GuildId { get; private set; }

	internal readonly CurrencyConverterCommand CurrencyConverterCommand;

	internal readonly IConfig Config;
	internal readonly IDiscordInteractor DiscordInteractor;
	internal readonly IModuleManager ModuleManager;
	internal readonly ILogger Logger;

	internal string ApiKey { get; private set; }

	public Task<bool> Startup(ulong guildId) {
		GuildId = guildId;
		ApiKey = Config.ReadConfig(this, "Currency Converter", "ApiKey");
		if (ApiKey == null) {
			Config.WriteConfig(this, "Currency Converter", "ApiKey", "PLEASE ADD API KEY");
			Logger.Log(this, new LogEventArgs {
				Type = LogType.Error,
				Message = $"BlendoBot Currency Converter has not been supplied a valid API key! Please acquire a key from https://www.alphavantage.co/ and add it in the config under the [Currency Converter] section."
			});
			return Task.FromResult(false);
		}
		return Task.FromResult(ModuleManager.RegisterCommand(this, CurrencyConverterCommand, out _));
	}
}
