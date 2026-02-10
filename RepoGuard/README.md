# RepoGuard

## Build & Run

To start the application locally using the CLI:
```
dotnet build
dotnet run
```
## Tunneling
For local webhook testing, I personally used Cloudflare Tunnel.  
Use port 5193:
```
cloudflared tunnel --url http://localhost:5193
```

## Security & Secrets Configuration
The application implements HMAC SHA-256 signature validation to ensure request integrity.  
Development/Debug: **If no secret is configured, the application skips validation to facilitate easy local debugging**.  
Secure Mode: To enable validation, configure the secret using .NET User Secrets and set the same secret in the GitHub Webhook settings.  
Configuration command:
```
dotnet user-secrets init
dotnet user-secrets set "Github:WebhookSecret" "YOUR_SECRET_HERE"
```

## Architecture & Design Decisions    
1. I used Producer-Consumer Pattern for optional scalability.
   To handle high throughput and prevent GitHub timeouts, the system decouples ingestion from processing:
   The Endpoint: Acts as a Producer. It accepts the payload, pushes it to an in-memory Channel<WebhookEvent>, and immediately returns 202 Accepted.
   The Worker: The WebhookProcessorWorker acts as a Consumer. It runs in the background, processing events asynchronously from the queue.
    
2. Strategy Pattern & Extensibility
   The anomaly detection engine is built using the Strategy Pattern within the RepoGuard.Logic layer:  
   Open/Closed Principle: The system is open for extension but closed for modification.  
   Implementation: New detection rules can be added simply by creating a new class that implements the IAnomalyDetector interface. The Dependency Injection container automatically discovers and registers these rules at startup.  
    
3. Clean Composition Root
   To maintain a clean and readable Program.cs, service registration is encapsulated within extension methods:
```
builder.Services
    .AddNotificationService()
    .AddAnomalyDetectors();
```