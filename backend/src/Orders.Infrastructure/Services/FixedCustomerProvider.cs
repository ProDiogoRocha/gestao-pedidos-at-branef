
using Microsoft.Extensions.Configuration;
using Orders.Application.Common.Interfaces;

namespace Orders.Infrastructure.Services;

public sealed class FixedCustomerProvider : IFixedCustomerProvider
{
    private readonly IConfiguration _configuration;

    public FixedCustomerProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Guid GetCustomerId()
    {
        var raw = _configuration["FixedCustomer:Id"];
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("FixedCustomer:Id não configurado.");

        return Guid.Parse(raw);
    }
}
