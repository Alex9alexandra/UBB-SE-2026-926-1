using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ChatAndEvents.Data.EventsData.Database;

public class SqlConnectionFactory
{
    private readonly string _connectionString;
     public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
    //just changing so i can push cuz git is being weird and not letting me push
}
