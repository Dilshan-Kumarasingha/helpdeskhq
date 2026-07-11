namespace HelpDeskHQ.Core.Common.Exceptions
{
    /// <summary>
    /// Thrown when a requested entity does not exist.
    /// Maps to HTTP 404 in the global exception middleware.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when a request conflicts with existing data
    /// (e.g. duplicate name, duplicate email).
    /// Maps to HTTP 409 in the global exception middleware.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when input fails business validation rules
    /// (e.g. invalid enum value, invalid state transition).
    /// Maps to HTTP 400 in the global exception middleware.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}