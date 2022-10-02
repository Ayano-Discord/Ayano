using System.Reflection.Emit;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using Remora.Rest.Core;
using Remora.Results;
using Npgsql;

namespace Ayano.Core.Services;

public class Database
{

    public NpgsqlConnection _conn { get; init; }

    public Database(
        NpgsqlConnection conn
    )
    {
        _conn = conn;
    }

    public async Task<UInt64?> GetCooldown(Snowflake user, string command)
    {
        return 00000000000;
    }

    public async Task<IResult> SetBalance(Snowflake user, UInt64 amount)
    {
        await _conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            $@"INSERT INTO economy (_id, balance)
            VALUES('{user.Value}', '{amount}')
            ON CONFLICT (_id)
            DO UPDATE SET balance ='{amount}';",
            _conn
        );
        var reader = await cmd.ExecuteNonQueryAsync();
        await _conn.CloseAsync();
        return Result.FromSuccess();
    }

    public async Task<UInt64?> GetBalance(Snowflake user)
    {
        // await _conn.OpenAsync();
        // await using var cmd = new NpgsqlCommand($"SELECT balance FROM economy WHERE _id = '{user.Value}'", _conn);
        // await using var reader = await cmd.ExecuteReaderAsync();

        // UInt64? returnValue = null;

        // while (await reader.ReadAsync())
        // {
        //     returnValue = reader.GetInt64(0);
        // }

        // await _conn.CloseAsync();
        var returnValue = await ReadUserValue(user, new[] { "balance" }, "economy", new int()) ?? 0;

        return (UInt64)returnValue;
    }

    public async Task<dynamic> ReadUserValue(Snowflake user, string[] keys, string table, dynamic returnType)
    {
        await _conn.OpenAsync();

        var command = new NpgsqlCommand();

        if (keys.Length == 1)
        {
            command = new NpgsqlCommand($"SELECT {keys[0]} FROM {table} WHERE _id = '{user.Value}'", _conn);
        }
        else
        {
            command = new NpgsqlCommand($"SELECT {string.Join(", ", keys)} FROM {table} WHERE _id = '{user.Value}'", _conn);
        }

        await using var cmd = command;

        await using var reader = await cmd.ExecuteReaderAsync();

        dynamic returnValue = null;

        switch (returnType.GetType())
        {
            case string:
                returnValue = reader.GetString(0);
                break;
            case int:
                while (await reader.ReadAsync())
                {
                    returnValue = reader.GetInt64(0);
                }
                break;
            default:
                while (await reader.ReadAsync())
                {
                    returnValue = reader.GetInt64(0);
                }
                break;
        }
        await _conn.CloseAsync();
        return returnValue;
    }
}