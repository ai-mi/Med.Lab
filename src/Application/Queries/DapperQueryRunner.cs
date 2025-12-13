using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;

namespace Med.Labs.Application.Queries;

public class DapperQueryRunner : IQueryRunner
{
	private readonly Func<IDbConnection> _connectionFactory;

	public DapperQueryRunner(Func<IDbConnection> connectionFactory)
	{
		_connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
	}

	public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
	{
		using var conn = _connectionFactory();
		if (conn is DbConnection dbConn && dbConn.State != ConnectionState.Open)
		{
			await dbConn.OpenAsync();
		}

		return await conn.QueryAsync<T>(sql, param);
	}
}