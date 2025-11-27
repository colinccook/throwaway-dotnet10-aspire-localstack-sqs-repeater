# throwaway-dotnet10-aspire-localstack-sqs-repeater
A throwaway project showing a couple of dotnet10 worker services interacting with SQS locally

## In Summary

This is a solution that has an Aspire project. I wanted to experiment with a locally running local worker service that does stuff with SQS queues.

## Architecture

<img width="1597" height="1054" alt="image" src="https://github.com/user-attachments/assets/0571b707-28a6-4d10-958d-97b4f429670f" />

1) Aspire runs 10 instances of WorkerPopulator. WorkerPopulator publishes to an SQS-based "InputQueue"
2) WorkerRepeater is subscribed to "InputQueue", validates the JSON payload
3) WorkerRepeater then publishes to SQS-based "OutputQueue"

## Improvements I want to make
- WorkerRepeater creates SQS topics if they don't exist. In prod, we want our cloud landscape to have been already created. I want to research alternative ways to set up these queues
- WorkerRepeater isn't efficiently polling the InputQueue
- Run some sort of local Observability
- I'd love to write an integration test that proves the functionality
  - One test to prove the happy path (one input = one output)
  - One test with a bad payload to go into a dead letter queue
