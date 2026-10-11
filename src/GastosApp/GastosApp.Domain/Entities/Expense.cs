using GastosApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GastosApp.Domain.Entities
{
    public sealed class Expense
    {
        public Guid Id { get; private set; }

        // Solo uno de estos dos campos tendrá valor:
        // PersonalOwnerUserId o GroupId.
        public Guid? PersonalOwnerUserId { get; private set; }
        public Guid? GroupId { get; private set; }

        public Guid CategoryId { get; private set; }
        public Guid CreatedByUserId { get; private set; }

        public ExpenseType Type { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "USD";
        public string Description { get; private set; } = string.Empty;

        public DateTimeOffset OccurredAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        // Control básico de concurrencia.
        public long Version { get; private set; }

        public bool IsDeleted { get; private set; }

        private Expense() { }

        public static Expense Create(
            Guid? personalOwnerUserId,
            Guid? groupId,
            Guid categoryId,
            Guid createdByUserId,
            ExpenseType type,
            decimal amount,
            string description,
            DateTimeOffset occurredAt,
            string currency = "USD")
        {
            // Un movimiento debe ser personal o grupal,
            // pero nunca ambas cosas ni ninguna.
            if (personalOwnerUserId.HasValue == groupId.HasValue)
                throw new ArgumentException(
                    "El movimiento debe pertenecer a un usuario o a un grupo.");

            if (categoryId == Guid.Empty || createdByUserId == Guid.Empty)
                throw new ArgumentException(
                    "La categoría y el creador son obligatorios.");

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(amount), "El importe debe ser mayor que cero.");

            if (!Enum.IsDefined(type))
                throw new ArgumentException("Tipo de movimiento inválido.");

            if (string.IsNullOrWhiteSpace(currency) ||
                currency.Length != 3)
                throw new ArgumentException(
                    "La moneda debe indicarse con un código de tres letras.");

            var now = DateTimeOffset.UtcNow;

            return new Expense
            {
                Id = Guid.NewGuid(),
                PersonalOwnerUserId = personalOwnerUserId,
                GroupId = groupId,
                CategoryId = categoryId,
                CreatedByUserId = createdByUserId,
                Type = type,
                Amount = decimal.Round(amount, 2),
                Currency = currency.ToUpperInvariant(),
                Description = description?.Trim() ?? string.Empty,
                OccurredAt = occurredAt,
                CreatedAt = now,
                UpdatedAt = now,
                Version = 1
            };
        }
    }
}
