namespace GalacticFleetCommand.Api.Application;

public class CommandBackgroundWorker : BackgroundService
{
    private readonly IBackgroundCommandQueue queue;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<CommandBackgroundWorker> logger;

    public CommandBackgroundWorker(
        IBackgroundCommandQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<CommandBackgroundWorker> logger)
    {
        this.queue = queue;
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Command background worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var commandId = await queue.DequeueAsync(stoppingToken);

                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();

                await processor.ProcessAsync(commandId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error while processing queued command");
            }
        }

        logger.LogInformation("Command background worker stopped");
    }
}