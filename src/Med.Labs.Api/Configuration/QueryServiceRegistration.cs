using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Med.Labs.Application.Queries;

namespace Med.Labs.Api.Configuration;

public static class QueryServiceRegistration
{
	public static IServiceCollection AddLaboratoryQueryServices(this IServiceCollection services, string connectionString)
	{
		// register a Dapper-backed query runner that opens Npgsql connections on demand
		services.AddSingleton<IQueryRunner>(_ => new DapperQueryRunner(() => new NpgsqlConnection(connectionString)));

		// register the query service for injection
		services.AddTransient<LaboratoryQueryService>();

		return services;
	}
}