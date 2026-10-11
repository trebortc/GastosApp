using GastosApp.Domain.Common;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;
using GastosApp.Domain.ValueObjects;

namespace GastosApp.Domain.Entities;

public sealed class Expense : AuditableEntity
{
    public Guid? PersonalOwnerUserId { get; private set; }
    public Guid? GroupId { get; private set; }
    public Guid CategoryId { get; private set; }
    public ExpenseType Type { get; private set; }
    public Money Money { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool IsPersonal => PersonalOwnerUserId.HasValue;
    public bool IsGroupExpense => GroupId.HasValue;
    private Expense() { }
    public static Expense Create(Guid id, Guid? personalOwnerUserId, Guid? groupId, Guid categoryId, Guid createdByUserId, ExpenseType type, decimal amount, string currency, string description, DateTimeOffset occurredAt)
    {
        if (personalOwnerUserId.HasValue == groupId.HasValue)
            throw new DomainException("El movimiento debe pertenecer a un usuario personal o a un grupo, pero no a ambos ni a ninguno.");
        if (categoryId == Guid.Empty) throw new DomainException("La categoria es obligatoria.");
        if (createdByUserId == Guid.Empty) throw new DomainException("El creador es obligatorio.");
        if (!Enum.IsDefined(type)) throw new DomainException("Tipo invalido.");
        if (amount <= 0) throw new DomainException("El importe debe ser mayor que cero.");
        return new Expense { Id = id, PersonalOwnerUserId = personalOwnerUserId, GroupId = groupId, CategoryId = categoryId, CreatedByUserId = createdByUserId, Type = type, Money = new Money(amount, currency), Description = description?.Trim() ?? string.Empty, OccurredAt = occurredAt, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
    }
    public void Update(Guid categoryId, ExpenseType type, decimal amount, string currency, string description, DateTimeOffset occurredAt, Guid requestingUserId)
    {
        EnsureNotDeleted(); EnsureCanModify(requestingUserId);
        if (categoryId == Guid.Empty) throw new DomainException("La categoria es obligatoria.");
        if (!Enum.IsDefined(type)) throw new DomainException("Tipo invalido.");
        if (amount <= 0) throw new DomainException("El importe debe ser mayor que cero.");
        CategoryId = categoryId; Type = type; Money = new Money(amount, currency);
        Description = description?.Trim() ?? string.Empty; OccurredAt = occurredAt; SetUpdated();
    }
    public void Delete(Guid requestingUserId) { EnsureNotDeleted(); EnsureCanModify(requestingUserId); IsDeleted = true; SetUpdated(); }
    private void EnsureNotDeleted() { if (IsDeleted) throw new DomainException("El movimiento ya fue eliminado."); }
    private void EnsureCanModify(Guid requestingUserId) { if (CreatedByUserId != requestingUserId) throw new ForbiddenException("Solo el creador puede modificar este movimiento."); }
}
