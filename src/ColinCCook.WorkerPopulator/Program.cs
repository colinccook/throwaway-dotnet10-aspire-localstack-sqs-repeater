using ColinCCook.WorkerRepeater;
using Amazon.SQS;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSQS>();
    
builder.Services.AddSingleton<SqsPopulatorService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SqsPopulatorService>());

var host = builder.Build();
host.Run();