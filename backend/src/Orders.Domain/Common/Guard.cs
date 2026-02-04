
namespace Orders.Domain.Common;

public static class Guard
{
    public static void AgainstNull(object? value, string name)
    {
        if (value is null) throw new DomainException($"{name} não pode ser nulo.");
    }

    public static void AgainstEmpty(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException($"{name} não pode ser vazio.");
    }

    public static void AgainstNonPositive(int value, string name)
    {
        if (value <= 0) throw new DomainException($"{name} deve ser maior que zero.");
    }

    public static void AgainstNegative(decimal value, string name)
    {
        if (value < 0) throw new DomainException($"{name} não pode ser negativo.");
    }
}
