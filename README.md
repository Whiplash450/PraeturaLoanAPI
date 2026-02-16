# PraeturaLoanAPI

A loan application REST API built with .NET 8, ASP.NET Core, and EF Core (In-Memory).

It accepts loan applications, validates input, and processes eligibility in the background.

## How to Run

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Run the API
```bash
cd PraeturaLoanAPI
dotnet run
```

The API will start on `http://localhost:5030` (or `https://localhost:7268` with the `https` profile). Swagger UI is available at `/swagger` in development mode.

## Run Tests
```bash
dotnet test
```

## API Endpoints

## `POST /loan-applications`

Creates a new loan application. The application is saved with a `Pending` status and processed in the background.

**Request body:**
```json
{
  "name": "Alice Example",
  "email": "alice@example.com",
  "monthlyIncome": 3500,
  "requestedAmount": 8000,
  "termMonths": 36
}
```

**Response (`201 Created`):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Pending",
  "createdAt": "2025-05-27T10:30:00Z"
}
```
**Response (`500 Internal Server Error`):** If the application encounters an unexpected error.

**Validation rules (400 Bad Request):**
- Name and email are required
- Email must be a valid format
- Monthly income, requested amount, and term months must be greater than zero

**Idempotency:** Supply an `Idempotency-Key` header to prevent duplicate submissions. If a matching key already exists, the original application is returned with a 200 Ok.

## `GET /loan-applications/{id}`

Returns the full application record including decision log entries once processed.

**Response (`200 OK`):** Full `LoanApplication` object with `DecisionLogs`.

**Response (`404 Not Found`):** If the application ID does not exist.

**Response (`500 Internal Server Error`):** If the application encounters an unexpected error.

## Background Processing

A `BackgroundService` runs every 60 seconds to process pending applications against the following eligibility rules:

|      Rule      |                     Criteria                        |
|----------------|-----------------------------------------------------|
| Minimum Income | Monthly income must be at least £2,000              |
| Maximum Amount | Requested amount must not exceed monthly income x 4 |
| Term Range     | Term must be between 12 and 60 months               |

Each rule produces a `DecisionLogEntry` linked to the application. If all rules pass, the status is set to `Approved`; otherwise `Rejected`. A `ReviewedAt` timestamp is recorded.

Applications are saved individually within the processing loop so that a failure on one record does not affect others.

## Optional Feature: Idempotency

The `POST` endpoint supports a client-supplied `Idempotency-Key` header. If a request is submitted with a key that has already been used, the API returns the original application rather than creating a duplicate, via a 200 Ok response.

## Architecture Notes

## Scaling to 5,000,000 applications per day

The current design prioritises simplicity and correctness for a small-scale demonstration. At 5M applications/day, several things would need to change:

1. Database - switching to a persistent production database (e.g. SQL) to correctly handle load/traffic
2. Background processing - the 60s cycle for application processing would be a huge issue at high volume, this would be better with a different implementation perhaps an Azure functions app?
3. Idempotency keys - querying the db for these keys on every application submission would be hard on the db, so offloading these keys to a view or cache table in the db would help spread load. Holding a recent cache in memory or another service could also be an option.
4. API scaling - It would be ideal to run the service behind a load balancer, and perhaps containerise the service to be able to dynamically scale horizontally if load required.
5. Request batching - linking to point 3 in the trade-offs section, batching processing of requests would be very beneficial at high volumes which could help mitigate db load issues, by reducing the number of distinct db calls.

## Shortcuts and Trade-offs

1. Implement a proper database - As the in-memory db is deleted when the API is stopped, a persistent production db is an obvious upgrade to the compromise for simplicity here.
2. Logging - The API implements exception handling but no explicit logging. Logging was available as an optional extension but idempotency was chosen instead. Hooking up structured logging to AppInsights would be a good visibility upgrade.
3. Db Save-per-application-evaluation - This is a small scale trade-off which will not handle large request volume well, would need logic adapting to implement batching to avoid hammering the db under high volume.
4. No authentication - a shortcut for this exercise is omitting any user authentication which would be required for adaptation to a production service.
