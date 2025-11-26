var builder = DistributedApplication.CreateBuilder(args);

var sqsInputQueueUrl = builder.AddParameter("SQSInputQueueUrl");
var sqsOutputQueueUrl = builder.AddParameter("SQSOutputQueueUrl");

var localstack = builder
    .AddContainer("localstack", "localstack/localstack")
    .WithEndpoint(port: 4566, targetPort: 4566);

builder
    .AddProject<Projects.ColinCCook_WorkerRepeater>("worker-repeater")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "test")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "test")
    .WithEnvironment("AWS_DEFAULT_REGION", "us-east-1")
    .WithEnvironment("SQS_INPUT_QUEUE_URL", sqsInputQueueUrl)
    .WithEnvironment("SQS_OUTPUT_QUEUE_URL", sqsOutputQueueUrl)
    .WithEnvironment("AWS__ServiceURL", "http://localhost:4566")
    .WaitFor(localstack);

builder
    .AddProject<Projects.ColinCCook_WorkerPopulator>("worker-populator")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "test")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "test")
    .WithEnvironment("AWS_DEFAULT_REGION", "us-east-1")
    .WithEnvironment("SQS_TARGET_QUEUE_URL", sqsInputQueueUrl)
    .WithEnvironment("AWS__ServiceURL", "http://localhost:4566")
    .WaitFor(localstack)
    .WithReplicas(10);

builder.Build().Run();
