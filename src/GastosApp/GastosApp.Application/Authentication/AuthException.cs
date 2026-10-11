using GastosApp.Domain.Exceptions;

namespace GastosApp.Application.Authentication;

/// <summary>Errores especificos del flujo de autenticacion.</summary>
public sealed class AuthException : DomainException
{
    public AuthException(string message) : base(message) { }
}
