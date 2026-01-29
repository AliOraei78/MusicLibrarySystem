using System.Data;

namespace MusicLibrarySystem.Data.Context; 

public interface IDapperContext
{
    IDbConnection CreateConnection();
}