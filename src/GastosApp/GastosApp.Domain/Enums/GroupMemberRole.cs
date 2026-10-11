namespace GastosApp.Domain.Enums;

/// <summary>Rol de un miembro dentro de un grupo de gastos compartidos.</summary>
public enum GroupMemberRole
{
    /// <summary>Creador del grupo. Puede administrarlo y eliminarlo.</summary>
    Owner = 1,

    /// <summary>Puede gestionar movimientos y miembros según permisos asignados.</summary>
    Admin = 2,

    /// <summary>Puede consultar y registrar movimientos según permisos asignados.</summary>
    Member = 3
}
