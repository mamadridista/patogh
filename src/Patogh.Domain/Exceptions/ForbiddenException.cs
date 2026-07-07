namespace Patogh.Domain.Exceptions;

/// <summary>
/// Thrown when the caller is authenticated but does not have permission
/// to perform the requested action on the specific resource.
/// Maps to HTTP 403 Forbidden.
///
/// Difference from UnauthorizedDomainException (401):
/// - 401 = "I don't know who you are" (not authenticated)
/// - 403 = "I know who you are, but you can't do this" (not authorized for this resource)
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException()
        : base("شما مجاز به انجام این عملیات نیستید.") { }

    public ForbiddenException(string message)
        : base(message) { }
}