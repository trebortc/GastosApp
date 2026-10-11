namespace GastosApp.Domain.Enums;

/// <summary>Estado de una operación de sincronización en la cola local del dispositivo.</summary>
public enum SyncOperationStatus
{
    /// <summary>Pendiente de enviar al servidor.</summary>
    Pending = 1,

    /// <summary>En proceso de sincronización.</summary>
    Syncing = 2,

    /// <summary>Confirmada por el servidor.</summary>
    Synced = 3,

    /// <summary>Falló el envío al servidor.</summary>
    Failed = 4,

    /// <summary>El servidor detectó un conflicto con la versión del recurso.</summary>
    Conflict = 5
}
