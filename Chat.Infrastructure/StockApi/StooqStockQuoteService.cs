using System.Globalization;
using Chat.Application.DTOs;
using Chat.Application.Interfaces;

namespace Chat.Infrastructure.StockApi
{
    internal sealed class StooqStockQuoteService : IStockQuoteService
    {
        private readonly HttpClient _httpClient;

        public StooqStockQuoteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<StockQuoteDto> GetQuoteAsync(
            string stockCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(stockCode))
            {
                throw new ArgumentException("Stock code is required.", nameof(stockCode));
            }

            var normalizedStockCode = stockCode.Trim().ToLowerInvariant();
            var path = $"/q/d/l/?s={Uri.EscapeDataString(normalizedStockCode)}&i=d";
            var csv = await _httpClient.GetStringAsync(path, cancellationToken);

            return ParseQuote(csv, normalizedStockCode);
        }

        internal static StockQuoteDto ParseQuote(string csv, string requestedStockCode)
        {
            var lines = csv
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (lines.Length < 2)
            {
                throw new InvalidOperationException($"Stock quote for {requestedStockCode} was not found.");
            }

            var headers = lines[0].Split(',', StringSplitOptions.TrimEntries);
            var values = lines[^1].Split(',', StringSplitOptions.TrimEntries);

            // Supports both the challenge quote CSV shape and Stooq's historical CSV download shape.
            if (headers.Length != values.Length)
            {
                throw new InvalidOperationException($"Stock quote for {requestedStockCode} was not in the expected format.");
            }

            var closeIndex = Array.FindIndex(
                headers,
                header => string.Equals(header, "Close", StringComparison.OrdinalIgnoreCase));

            if (closeIndex < 0)
            {
                throw new InvalidOperationException($"Stock quote for {requestedStockCode} was not in the expected format.");
            }

            var symbolIndex = Array.FindIndex(
                headers,
                header => string.Equals(header, "Symbol", StringComparison.OrdinalIgnoreCase));
            var symbol = symbolIndex >= 0
                ? values[symbolIndex]
                : requestedStockCode;
            var close = values[closeIndex];

            if (string.IsNullOrWhiteSpace(symbol)
                || LooksLikeHtml(csv)
                || string.Equals(symbol, "N/D", StringComparison.OrdinalIgnoreCase)
                || string.Equals(close, "N/D", StringComparison.OrdinalIgnoreCase)
                || !decimal.TryParse(close, NumberStyles.Number, CultureInfo.InvariantCulture, out var closePrice))
            {
                throw new InvalidOperationException($"Stock quote for {requestedStockCode} was not found.");
            }

            return new StockQuoteDto(symbol.ToUpperInvariant(), closePrice);
        }

        private static bool LooksLikeHtml(string value)
        {
            return value.TrimStart().StartsWith("<", StringComparison.Ordinal);
        }
    }
}
