using Amazon.SQS;
using Amazon.SQS.Model;

namespace ColinCCook.WorkerRepeater;

public class SqsProcessorService : BackgroundService
{
    private readonly ILogger<SqsProcessorService> _logger;
    private readonly IAmazonSQS _sqs;
    private readonly string _inputQueueUrl;
    private readonly string _outputQueueUrl;

    public SqsProcessorService(
        ILogger<SqsProcessorService> logger,
        IConfiguration configuration,
        IAmazonSQS sqs
    )
    {
        _logger = logger;
        _sqs = sqs;

        _inputQueueUrl = configuration["SQS_INPUT_QUEUE_URL"]
            ?? throw new Exception("Missing input queue URL");
        _outputQueueUrl = configuration["SQS_OUTPUT_QUEUE_URL"]
            ?? throw new Exception("Missing output queue URL");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started");

        await EnsureQueueAsync(_inputQueueUrl, stoppingToken);
        await EnsureQueueAsync(_outputQueueUrl, stoppingToken);

        var request = new ReceiveMessageRequest
        {
            QueueUrl = _inputQueueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 20
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await _sqs.ReceiveMessageAsync(request, stoppingToken);

            if (response.Messages == null)
                continue;

            foreach (var msg in response.Messages)
            {
                try
                {
                    await ProcessMessage(msg);
                    await _sqs.DeleteMessageAsync(_inputQueueUrl, msg.ReceiptHandle);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed processing message {msg.MessageId}");
                    // This will allow SQS redrive policy to DLQ on failure
                }
            }
        }
    }

    private async Task ProcessMessage(Message msg)
    {
        _logger.LogInformation($"Received message: {msg.Body}");

        var outboundPayload = new
        {
            Original = msg.Body,
            ProcessedAt = DateTime.UtcNow
        };

        string body = System.Text.Json.JsonSerializer.Serialize(outboundPayload);

        await _sqs.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = _outputQueueUrl,
            MessageBody = body
        });
    }

    private async Task EnsureQueueAsync(string queueUrl, CancellationToken ct)
    {
        var name = queueUrl.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        await _sqs.CreateQueueAsync(new CreateQueueRequest { QueueName = name }, ct);
    }
}