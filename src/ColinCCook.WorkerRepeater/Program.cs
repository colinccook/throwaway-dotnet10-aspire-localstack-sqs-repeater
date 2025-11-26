using ColinCCook.WorkerRepeater;
using Amazon.SQS;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSQS>();
    
builder.Services.AddSingleton<SqsProcessorService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SqsProcessorService>());

var host = builder.Build();
host.Run();

