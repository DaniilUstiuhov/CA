namespace CulinaryAssistant.Domain.Exceptions;

/// <summary>
/// Исключение доменного уровня
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
