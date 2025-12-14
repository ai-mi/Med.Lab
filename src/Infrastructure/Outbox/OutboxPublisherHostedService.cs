using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Med.Labs.Infrastructure.Outbox;

public class OutboxPublisherOptions
{
	public int PollIntervalSeconds { get; set; } = 5;
	// Additional options can be added later (batch size, jitter, etc.)
}

public class OutboxPublisherHostedService : BackgroundService
{
	private readonly OutboxPublisher _publisher;
	private readonly ILogger<OutboxPublisherHostedService> _logger;
	private readonly OutboxPublisherOptions _options;

	public OutboxPublisherHostedService(
		OutboxPublisher publisher,
		IOptions<OutboxPublisherOptions> options,
		ILogger<OutboxPublisherHostedService> logger)
	{
		_publisher = publisher;
		_logger = logger;
		_options = options?.Value ?? new OutboxPublisherOptions();
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("OutboxPublisherHostedService starting. Poll interval: {IntervalSeconds}s", _options.PollIntervalSeconds);

		// PeriodicTimer is available on .NET 6+ (OK for .NET 10)
		using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds)));

		try
		{
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				if (stoppingToken.IsCancellationRequested)
					break;

				try
				{
					await _publisher.PublishPending(stoppingToken);
				}
				catch (OperationCanceledException)
				{
					// shutdown requested — break loop to stop service gracefully
					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Outbox publish loop encountered an error. Will continue polling.");
					// swallow and continue; next iteration will retry
				}
			}
		}
		catch (OperationCanceledException)
		{
			// expected on shutdown
		}
		finally
		{
			_logger.LogInformation("OutboxPublisherHostedService stopping.");
		}
	}
}