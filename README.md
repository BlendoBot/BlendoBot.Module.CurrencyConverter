# BlendoBot.Module.CurrencyConverter
## Displays conversion rates for many currencies.
![GitHub Workflow Status](https://img.shields.io/github/workflow/status/BlendoBot/BlendoBot.Module.CurrencyConverter/Tests)

The currency converter command allows users to get a live conversion rate between two or more currencies.

## Discord Usage
- `?currency [value] [from] [to ...]`
  - Converts `value` amount of `from` currency to all the `to` currencies (multiple can be listed).

Currencies are converted using [AlphaVantage](https://www.alphavantage.co/). A full list of available currencies to compare against are linked via the [AlphaVantage API Docs](https://www.alphavantage.co/documentation/#crypto-exchange).

## Config
This module requires an AlphaVantage API key. The key should be in the config as:
```cfg
[Currency Converter]
ApiKey=YOUR_API_KEY
```