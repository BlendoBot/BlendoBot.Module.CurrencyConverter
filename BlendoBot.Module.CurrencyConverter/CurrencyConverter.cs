using BlendoBot.Core.Module;
using BlendoBot.Core.Services;
using BlendoBot.Module.CurrencyConverter.API;
using System;
using System.Threading.Tasks;

namespace BlendoBot.Module.CurrencyConverter;

[Module(Guid = "com.biendeo.blendobot.module.currencyconverter", Name = "Currency Converter", Author = "Biendeo", Version = "3.1.0", Url = "https://github.com/BlendoBot/BlendoBot.Module.CurrencyConverter")]
public class CurrencyConverter : IModule, IDisposable {
	public CurrencyConverter(IDiscordInteractor discordInteractor, IModuleManager moduleManager, ILogger logger) {
		DiscordInteractor = discordInteractor;
		ModuleManager = moduleManager;
		Logger = logger;

		DdgCurrencyClient = new(new());
		CurrencyConverterCommand = new(this);
	}

	internal ulong GuildId { get; private set; }

	internal readonly DdgCurrencyClient DdgCurrencyClient;
	internal readonly CurrencyConverterCommand CurrencyConverterCommand;

	internal readonly IDiscordInteractor DiscordInteractor;
	internal readonly IModuleManager ModuleManager;
	internal readonly ILogger Logger;

	public Task<bool> Startup(ulong guildId) {
		GuildId = guildId;
		return Task.FromResult(ModuleManager.RegisterCommand(this, CurrencyConverterCommand, out _));
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			DdgCurrencyClient?.Dispose();
		}
	}
}
