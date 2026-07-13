using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Atelier.Web.Services;

public sealed class ZarinpalOptions
{
    public const string Section = "Payment:Zarinpal";
    public string MerchantId { get; set; } = string.Empty;
    public string RequestUrl { get; set; } = "https://payment.zarinpal.com/pg/v4/payment/request.json";
    public string VerifyUrl { get; set; } = "https://payment.zarinpal.com/pg/v4/payment/verify.json";
    public string StartPayUrl { get; set; } = "https://www.zarinpal.com/pg/StartPay/";
}

public interface IPaymentGateway
{
    Task<PaymentRequestResult> RequestAsync(decimal amountToman, string description, string callbackUrl, string phone, string orderNumber, CancellationToken cancellationToken);
    Task<PaymentVerifyResult> VerifyAsync(decimal amountToman, string authority, CancellationToken cancellationToken);
}

public sealed record PaymentRequestResult(bool Succeeded, int Code, string? Authority, string? RedirectUrl, string? Error);
public sealed record PaymentVerifyResult(bool Succeeded, bool AlreadyVerified, int Code, string? ReferenceId, string? Error);

public sealed class ZarinpalPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _http;
    private readonly ZarinpalOptions _options;
    public ZarinpalPaymentGateway(HttpClient http, Microsoft.Extensions.Options.IOptions<ZarinpalOptions> options) { _http = http; _options = options.Value; }

    public async Task<PaymentRequestResult> RequestAsync(decimal amountToman, string description, string callbackUrl, string phone, string orderNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.MerchantId)) return new(false, -1, null, null, "درگاه پرداخت هنوز پیکربندی نشده است.");
        var payload = new { merchant_id = _options.MerchantId, currency = "IRT", amount = decimal.ToInt64(amountToman), callback_url = callbackUrl, description, metadata = new { mobile = phone, order_id = orderNumber } };
        try
        {
            using var response = await _http.PostAsJsonAsync(_options.RequestUrl, payload, cancellationToken);
            var body = await response.Content.ReadFromJsonAsync<ZarinpalResponse>(cancellationToken: cancellationToken);
            var error = ReadError(body?.Errors);
            var data = ReadData(body?.Data);
            var code = data.Code ?? error.Code ?? (int)response.StatusCode;
            var authority = data.Authority;
            return code == 100 && !string.IsNullOrWhiteSpace(authority)
                ? new(true, code, authority, _options.StartPayUrl.TrimEnd('/') + "/" + authority, null)
                : new(false, code, null, null, error.Message ?? "درگاه پرداخت درخواست را نپذیرفت.");
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or TaskCanceledException) { return new(false, -2, null, null, "ارتباط امن با درگاه پرداخت برقرار نشد."); }
    }

    public async Task<PaymentVerifyResult> VerifyAsync(decimal amountToman, string authority, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.MerchantId)) return new(false, false, -1, null, "درگاه پرداخت پیکربندی نشده است.");
        try
        {
            using var response = await _http.PostAsJsonAsync(_options.VerifyUrl, new { merchant_id = _options.MerchantId, amount = decimal.ToInt64(amountToman), authority }, cancellationToken);
            var body = await response.Content.ReadFromJsonAsync<ZarinpalResponse>(cancellationToken: cancellationToken);
            var error = ReadError(body?.Errors);
            var data = ReadData(body?.Data);
            var code = data.Code ?? error.Code ?? (int)response.StatusCode;
            var success = code is 100 or 101;
            return new(success, code == 101, code, data.ReferenceId, success ? null : error.Message ?? "تأیید پرداخت ناموفق بود.");
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or TaskCanceledException) { return new(false, false, -2, null, "ارتباط امن با درگاه پرداخت برقرار نشد."); }
    }

    private sealed class ZarinpalResponse
    {
        [JsonPropertyName("data")] public JsonElement Data { get; set; }
        [JsonPropertyName("errors")] public JsonElement Errors { get; set; }
    }

    private static (int? Code, string? Authority, string? ReferenceId) ReadData(JsonElement? data)
    {
        if (!data.HasValue || data.Value.ValueKind != JsonValueKind.Object) return (null, null, null);
        var value = data.Value;
        var code = value.TryGetProperty("code", out var codeElement) && codeElement.TryGetInt32(out var parsed) ? parsed : null;
        var authority = value.TryGetProperty("authority", out var authorityElement) ? authorityElement.GetString() : null;
        var referenceId = value.TryGetProperty("ref_id", out var refElement) ? refElement.ToString() : null;
        return (code, authority, referenceId);
    }
    private static (int? Code, string? Message) ReadError(JsonElement? errors)
    {
        if (!errors.HasValue || errors.Value.ValueKind != JsonValueKind.Object) return (null, null);
        var value = errors.Value;
        var code = value.TryGetProperty("code", out var codeElement) && codeElement.TryGetInt32(out var parsed) ? parsed : null;
        var message = value.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : null;
        return (code, message);
    }
}
