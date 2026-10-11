using GastosApp.Domain.Common;
using GastosApp.Domain.Exceptions;

namespace GastosApp.Domain.Entities;

/// <summary>
/// Presupuesto para un periodo dado. Puede ser personal o de grupo.
/// Invariante: debe ser personal O de grupo, nunca ambos ni ninguno.
/// </summary>
public sealed class Budget : AuditableEntity
{
    public Guid? OwnerUserId { get; private set; }
    public Guid? GroupId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public decimal LimitAmount { get; private set; }
    public string Currency { get; private set; } = "MXN";

    /// <summary>Inicio del periodo del presupuesto (UTC).</summary>
    public DateTimeOffset PeriodStart { get; private set; }

    /// <summary>Fin del periodo del presupuesto (UTC).</summary>
    public DateTimeOffset PeriodEnd { get; private set; }

    public bool IsActive { get; private set; } = true;

    public bool IsPersonal => OwnerUserId.HasValue;

    private Budget() { }

    public static Budget Create(Guid id, Guid createdByUserId, Guid? ownerUserId, Guid? groupId,
        string name, decimal limitAmount, string currency,
        DateTimeOffset periodStart, DateTimeOffset periodEnd)
    {
        if (ownerUserId.HasValue == groupId.HasValue)
            throw new DomainException(
                "El presupuesto debe pertenecer a un usuario o a un grupo, pero no a ambos ni a ninguno.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del presupuesto es obligatorio.");

        if (limitAmount <= 0)
            throw new DomainException("El limite del presupuesto debe ser mayor que cero.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("La moneda debe ser un codigo ISO 4217 de tres letras.");

        if (periodEnd <= periodStart)
            throw new DomainException("La fecha de fin debe ser posterior a la fecha de inicio.");

        return new Budget
        {
            Id = id,
            CreatedByUserId = createdByUserId,
            OwnerUserId = ownerUserId,
            GroupId = groupId,
            Name = name.Trim(),
            LimitAmount = decimal.Round(limitAmount, 2, MidpointRounding.AwayFromZero),
            Currency = currency.ToUpperInvariant(),
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateLimit(decimal newLimit, Guid requestingUserId)
    {
        EnsureCanModify(requestingUserId);

        if (newLimit <= 0)
            throw new DomainException("El limite del presupuesto debe ser mayor que cero.");

        LimitAmount = decimal.Round(newLimit, 2, MidpointRounding.AwayFromZero);
        SetUpdated();
    }

    public void Deactivate(Guid requestingUserId)
    {
        EnsureCanModify(requestingUserId);
        IsActive = false;
        SetUpdated();
    }

    private void EnsureCanModify(Guid requestingUserId)
    {
        if (CreatedByUserId != requestingUserId)
            throw new ForbiddenException("Solo el creador puede modificar este presupuesto.");
    }
}
