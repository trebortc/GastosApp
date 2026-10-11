using GastosApp.Domain.Exceptions;

namespace GastosApp.Domain.ValueObjects;

/// <summary>
/// Objeto de valor que encapsula un importe monetario y su moneda ISO 4217.
/// Inmutable. Dos instancias con mismo importe y moneda son iguales.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }

    /// <summary>Código de moneda ISO 4217 en mayúsculas, p.ej. "MXN", "USD".</summary>
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("El importe no puede ser negativo.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("La moneda debe indicarse con un código ISO 4217 de tres letras.");

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException(
                $"No se pueden operar monedas distintas: '{Currency}' y '{other.Currency}'.");
    }

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => obj is Money m && Equals(m);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount:F2} {Currency}";

    public static bool operator ==(Money? left, Money? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Money? left, Money? right) => !(left == right);
}
