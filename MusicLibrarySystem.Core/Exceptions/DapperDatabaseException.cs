namespace MusicLibrarySystem.Core.Exceptions;

public class DapperDatabaseException : Exception
{
    public DapperDatabaseException(string message, Exception inner)
        : base(message, inner) { }
}

public class ForeignKeyViolationException : DapperDatabaseException
{
    public ForeignKeyViolationException(string message, Exception inner)
        : base(message, inner) { }
}

public class UniqueConstraintViolationException : DapperDatabaseException
{
    public UniqueConstraintViolationException(string message, Exception inner)
        : base(message, inner) { }
}