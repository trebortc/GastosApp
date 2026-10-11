namespace GastosApp.Domain.Enums;

/// <summary>Estado de una invitación a un grupo de gastos.</summary>
public enum GroupInvitationStatus
{
    /// <summary>Enviada y esperando respuesta del destinatario.</summary>
    Pending = 1,

    /// <summary>Aceptada por el destinatario.</summary>
    Accepted = 2,

    /// <summary>Rechazada por el destinatario.</summary>
    Declined = 3,

    /// <summary>Cancelada por el propietario o administrador del grupo.</summary>
    Cancelled = 4,

    /// <summary>Expiró sin respuesta.</summary>
    Expired = 5
}
