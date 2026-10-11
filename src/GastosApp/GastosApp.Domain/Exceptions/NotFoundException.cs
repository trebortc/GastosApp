namespace GastosApp.Domain.Exceptions;

/// <summary>Se lanza cuando no se encuentra un recurso solicitado.</summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"No se encontró '{entityName}' con identificador '{key}'.") { }
}
