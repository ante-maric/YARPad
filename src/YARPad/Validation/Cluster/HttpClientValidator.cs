using System.Text;
using FluentValidation;

namespace CodingCell.YARPad;

public class HttpClientValidator : MudValidator<HttpClientModel>
{
    public HttpClientValidator(WebProxyValidator proxyValidator)
    {
        RuleFor(x => x.SslProtocols)
            .Must(protocols => protocols.All(x => Enum.IsDefined(x)))
                .WithMessage("SSL protocols must be defined enum values.");

        RuleFor(x => x.MaxConnectionsPerServer)
            .Must(max => max >= 1)
                .When(x => x.MaxConnectionsPerServer != null)
                .WithMessage("Max connections per server must be greater than or equal to 1.");
        
        RuleFor(x => x.WebProxy)
            .SetValidator(proxyValidator);

        RuleFor(x => x.RequestHeaderEncoding)
            .Must(encoding => ValidateEncoding(encoding!))
                .When(x => x.RequestHeaderEncoding != null)
                .WithMessage("Request header encoding is not valid.");

        RuleFor(x => x.ResponseHeaderEncoding)
            .Must(encoding => ValidateEncoding(encoding!))
                .When(x => x.RequestHeaderEncoding != null)
                .WithMessage("Response header encoding is not valid.");
    }

    private bool ValidateEncoding(string name)
    {
        try
        {
            Encoding.GetEncoding(name);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
