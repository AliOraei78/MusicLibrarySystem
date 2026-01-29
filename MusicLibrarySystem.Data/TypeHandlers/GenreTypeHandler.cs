using Dapper;
using System;
using System.Data;

namespace MusicLibrarySystem.Data.TypeHandlers;

public class GenreTypeHandler : SqlMapper.TypeHandler<GenreType>
{
    public override GenreType Parse(object value)
    {
        if (value == null || value == DBNull.Value)
            throw new ArgumentNullException(nameof(value), "GenreType cannot be null");

        return Enum.Parse<GenreType>(value.ToString()!, ignoreCase: true);
    }

    public override void SetValue(IDbDataParameter parameter, GenreType value)
    {
        parameter.Value = value.ToString();
    }
}