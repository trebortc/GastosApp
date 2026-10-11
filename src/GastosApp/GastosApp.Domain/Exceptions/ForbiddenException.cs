namespace GastosApp.Domain.Exceptions;

/// <summary>Se lanza cuando el usuario no tiene permiso para ejecutar la operación.</summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "No tienes permiso para realizar esta operación.")
        : base(message) { }
}
