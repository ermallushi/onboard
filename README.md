# onboard

Modular .NET 10 MVC onboarding platform scaffold with a central orchestration service.

## Solution layout

- `/src/Onboard.Web` - MVC host with channel/API endpoints
- `/src/Onboard.Web/Application` - orchestration contracts and workflow engine
- `/src/Onboard.Web/Domain` - core onboarding entities and explicit case statuses
- `/src/Onboard.Web/Infrastructure` - in-memory persistence, provider adapters, callback security
- `/tests/Onboard.Web.Tests` - focused orchestration tests

## Implemented architecture modules

- Customer Channel Layer and Store Staff Portal endpoints
- Onboarding Orchestrator with explicit lifecycle transitions and audit events
- Identity, Biometric, Risk, Review, Signature, Reporting, Integration, Webhook API groups
- Provider adapter interfaces to isolate vendor-specific implementations
- Callback idempotency and signature validation hooks for async processing
- Cross-channel continuation with transition history records

## Run

```bash
dotnet test Onboard.slnx
dotnet run --project /tmp/workspace/ermallushi/onboard/src/Onboard.Web/Onboard.Web.csproj
```
