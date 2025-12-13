namespace Med.Labs.Api.Configuration
{
	using Med.Labs.Application.Queries;
	using Med.Labs.Application.Handlers;
	using Med.Labs.Infrastructure.EventStore;
	using Med.Labs.Infrastructure.Projections;
	using Med.Labs.Infrastructure.Snapshots;
	using Med.Labs.Infrastructure.Upcasting;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Npgsql;

	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddApiDependencies(
			this IServiceCollection services,
			IConfiguration config)
		{
			services.AddSingleton<IEventUpcaster, LabEventUpcaster_v1_to_v2>();
			services.AddSingleton<IEventUpcaster, LabEventUpcaster_v2_to_v3>();

			// Connection string
			var connStr = config.GetConnectionString("db")
						  ?? "Host=localhost;Port=5432;Database=med_labs;Username=med;Password=med";

			// Use factory overloads so the IServiceProvider (sp) is available to the PostgresEventStore ctor.
			services.AddSingleton<PostgresEventStore>(sp => new PostgresEventStore(connStr, sp));
			services.AddSingleton<SnapshotStore>(sp => new SnapshotStore(connStr));
			services.AddSingleton<LaboratoryProjectionHandler>(sp => new LaboratoryProjectionHandler(connStr));

			// Register Dapper-backed IQueryRunner that opens Npgsql connections on demand
			services.AddSingleton<IQueryRunner>(_ => new DapperQueryRunner(() => new NpgsqlConnection(connStr)));

			// Register the query service to receive IQueryRunner by DI
			services.AddTransient<LaboratoryQueryService>();

			services.AddTransient<AddLaboratoryResultHandler>();
			services.AddTransient<UpdateLaboratoryResultHandler>();
			services.AddTransient<DeleteLaboratoryResultHandler>();

			return services;
		}
	}
}