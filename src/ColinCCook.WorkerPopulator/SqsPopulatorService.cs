using Amazon.SQS;
using Amazon.SQS.Model;

namespace ColinCCook.WorkerRepeater;
public class SqsPopulatorService : BackgroundService
{
    private readonly ILogger<SqsPopulatorService> _logger;
    private readonly IAmazonSQS _sqs;
    private readonly string _targetQueueUrl;
    private int count = 0;

    private readonly int targetMessages = 1000;

    public SqsPopulatorService(
        ILogger<SqsPopulatorService> logger,
        IConfiguration configuration,
        IAmazonSQS sqs
    )
    {
        _logger = logger;
        _sqs = sqs;

        _targetQueueUrl = configuration["SQS_TARGET_QUEUE_URL"]
            ?? throw new Exception("Missing input queue URL");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started");

        await EnsureQueueAsync(_targetQueueUrl, stoppingToken);

        while (!stoppingToken.IsCancellationRequested && targetMessages > count)
        {
            await _sqs.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _targetQueueUrl,
                MessageBody = "{}"
            });

            count++;
        }

        _logger.LogInformation("Finished");
    }

    private async Task EnsureQueueAsync(string queueUrl, CancellationToken ct)
    {
        var name = queueUrl.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        await _sqs.CreateQueueAsync(new CreateQueueRequest { QueueName = name }, ct);
    }
}