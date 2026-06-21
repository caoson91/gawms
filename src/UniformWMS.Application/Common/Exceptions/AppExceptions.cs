namespace UniformWMS.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity '{name}' with key '{key}' was not found.") { }

    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base("Validation failed.")
    {
        Errors = new Dictionary<string, string[]> { { field, [error] } };
    }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string? message = null)
        : base(message ?? "You do not have permission to perform this action.") { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string? message = null)
        : base(message ?? "Authentication required.") { }
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
