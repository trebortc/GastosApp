namespace GastosApp.Domain.Enums;

/// <summary>Tipo de operación representada en la cola de sincronización.</summary>
public enum SyncOperationType
{
    CreateExpense = 1,
    UpdateExpense = 2,
    DeleteExpense = 3,
    CreateCategory = 4,
    UpdateCategory = 5,
    DeleteCategory = 6
}
