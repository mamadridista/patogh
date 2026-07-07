namespace Patogh.Domain.Exceptions;

// Thrown when a business rule is violated (not a missing entity, not unauthorized —
// just a rule: "restaurant is closed", "capacity exceeded", "wrong status").
// Maps to HTTP 422 Unprocessable Entity in ExceptionHandlingMiddleware.
public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) { }
}