using System.Collections.Generic;
using System.Data;

using Microsoft.Data.Sqlite;

namespace NQuery.Tests.Oracle;

// Test oracle for "what is the correct result of a query". Each differential test runs a query
// through the live NQuery engine and, separately, through SQLite over the same Northwind data;
// the result rows must match (as a multiset -- see RowSet). SQLite is a mature, in-process,
// in-memory engine whose relational semantics (joins, set operators, three-valued logic,
// grouping) we treat as ground truth for the subset NQuery and SQLite share.
//
// This mirrors OldEngine (the baseline-engine differential harness): callers pass a SQL string
// and get back normalized rows; the SqliteConnection never leaks out. Two things differ from
// the baseline harness, because the oracle is a *different* engine rather than another build of
// NQuery:
//
//   * Dialect. NQuery is T-SQL-flavored (e.g. SELECT TOP n); SQLite uses LIMIT. When the two
//     spellings differ, the test supplies a SQLite-specific query text. For the common subset
//     (projection, equality, joins, EXISTS/IN, UNION/INTERSECT/EXCEPT, GROUP BY) the same string
//     runs on both.
//   * Value shape. SQLite hands back long/double/string/byte[]/DBNull; NQuery hands back the CLR
//     types it bound to (Int32, ...). RowSet.Normalize canonicalizes both sides so the
//     comparison is about *relational* correctness, not representation.
internal static class SqliteOracle
{
    // A :memory: database lives exactly as long as its connection is open and is private to that
    // connection, so we hold one open connection for the lifetime of the test run and seed it once.
    private static readonly SqliteConnection Connection = CreateAndSeed();

    public static List<object[]> RunQuery(string text)
    {
        using var command = Connection.CreateCommand();
        command.CommandText = text;

        using var reader = command.ExecuteReader();

        var rows = new List<object[]>();
        while (reader.Read())
        {
            var row = new object[reader.FieldCount];
            for (var i = 0; i < row.Length; i++)
                row[i] = RowSet.Normalize(reader.GetValue(i))!;
            rows.Add(row);
        }

        return rows;
    }

    private static SqliteConnection CreateAndSeed()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        Seed(connection);
        return connection;
    }

    // Seeds the Northwind schema and data into the oracle.
    //
    // The data is derived from the very same engine-neutral DataSet the live engine queries
    // (NorthwindCatalog.CreateDataSet), so the two engines provably see identical rows -- the
    // property an oracle needs. If you'd rather drive seeding from a hand-authored .sql script,
    // this is the single seam to replace: execute your DDL/DML against `connection` here instead.
    private static void Seed(SqliteConnection connection)
    {
        var dataSet = NorthwindCatalog.CreateDataSet();

        foreach (DataTable table in dataSet.Tables)
            CreateTable(connection, table);

        foreach (DataTable table in dataSet.Tables)
            InsertRows(connection, table);
    }

    private static void CreateTable(SqliteConnection connection, DataTable table)
    {
        var columns = new List<string>();
        foreach (DataColumn column in table.Columns)
            columns.Add($"{Quote(column.ColumnName)} {SqliteDeclaredType(column.DataType)}");

        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE TABLE {Quote(table.TableName)} ({string.Join(", ", columns)})";
        command.ExecuteNonQuery();
    }

    private static void InsertRows(SqliteConnection connection, DataTable table)
    {
        var columnNames = new List<string>();
        var parameterNames = new List<string>();
        for (var i = 0; i < table.Columns.Count; i++)
        {
            columnNames.Add(Quote(table.Columns[i].ColumnName));
            parameterNames.Add($"$p{i}");
        }

        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.CommandText =
            $"INSERT INTO {Quote(table.TableName)} ({string.Join(", ", columnNames)}) " +
            $"VALUES ({string.Join(", ", parameterNames)})";

        var parameters = new SqliteParameter[table.Columns.Count];
        for (var i = 0; i < parameters.Length; i++)
            parameters[i] = command.Parameters.Add(parameterNames[i], SqliteType.Text);

        foreach (DataRow row in table.Rows)
        {
            for (var i = 0; i < parameters.Length; i++)
                parameters[i].Value = row[i] is DBNull ? DBNull.Value : row[i];
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static string SqliteDeclaredType(Type type)
    {
        if (type == typeof(byte[]))
            return "BLOB";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean or
            TypeCode.SByte or TypeCode.Byte or
            TypeCode.Int16 or TypeCode.UInt16 or
            TypeCode.Int32 or TypeCode.UInt32 or
            TypeCode.Int64 or TypeCode.UInt64 => "INTEGER",
            TypeCode.Single or TypeCode.Double => "REAL",
            TypeCode.Decimal => "NUMERIC",
            _ => "TEXT",
        };
    }

    private static string Quote(string identifier)
    {
        return $"[{identifier}]";
    }
}
