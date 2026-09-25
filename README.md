# Gift of the Givers

Gift of the Givers is an ASP.NET Core MVC application developed for the
**APPR6312 Part 2 POE**. It provides a donation and volunteering experience
for the public, together with an employee-facing area for managing operational
information. The solution also contains an Azure Functions project that creates
donation certificate identifiers and a reusable helper library packaged for
NuGet consumption.

## Implemented capabilities

### Public users and authentication

- Account registration, login, logout, and profile updates.
- Cookie-based authentication with 30-day persistent-session support when
  requested.
- Password hashing through `IPasswordHasher<User>`, including a migration path
  for legacy stored passwords after a successful login.
- Authenticated users can view their profile and donation history.

### Donations and certificates

- Donation entry, review, and completion flows for signed-in and anonymous
  donors.
- Donation records with amount, currency, payment method, donation type,
  status, date, and anonymity state.
- Authenticated users can view certificates belonging to their own donations.
- The web application calls the certificate Azure Function after saving a
  donation. A certificate-generation failure does not discard the saved
  donation; the user is informed that the certificate is pending.

### Volunteers and employees

- Authenticated users can submit one volunteer application with skills,
  availability, and location details.
- Employee and administrator sign-in is separate from public-user sign-in.
- Employee/admin users can access the dashboard, donations, volunteers,
  relief operations, reports, contact messages, and account settings.
- Employee/admin users can update donation and volunteer statuses and manage
  relief-operation records.

## Architecture and project structure

The solution targets **.NET 8** and follows an MVC structure. Entity Framework
Core uses the SQL Server provider, and migrations define the database schema.

```text
GiftOfTheGivers_web.sln
├── GiftOfTheGivers_web.csproj        # ASP.NET Core MVC web application
├── Controllers/                      # Account, donation, employee, home, volunteer flows
├── Data/                             # ApplicationDbContext and database setup types
├── Models/                           # Entities and view models
├── Migrations/                       # EF Core migrations
├── Services/                         # Application services, including employee password handling
├── Views/                            # Razor views
├── wwwroot/                          # Static CSS, JavaScript, and images
├── GenerateDonationCertificate/      # .NET isolated Azure Functions application
└── DonationsFunctions/               # GiftOfTheGivers.Helpers NuGet helper library
```

The web project references `GiftOfTheGivers.Helpers` version `1.0.0`. The
helper library currently provides certificate-number normalization used after
the Azure Function returns a certificate response.

## Prerequisites

- .NET 8 SDK
- A SQL Server instance or Azure SQL Database
- An IDE such as Visual Studio 2022, Visual Studio Code, or JetBrains Rider
- Azure Functions Core Tools v4 for running the certificate function locally
- Azure CLI and an authenticated Azure account for Azure deployment
- Access to the team Azure DevOps project/feed when restoring or publishing
  private packages

Verify the SDK installation:

```bash
dotnet --info
dotnet --list-sdks
```

## Local setup

1. Clone the repository and enter its directory.
2. Restore the complete solution:

   ```bash
   dotnet restore GiftOfTheGivers_web.sln
   ```

3. Configure the web application's database connection as described in
   [Azure SQL setup](#azure-sql-setup).
4. Apply the included migrations:

   ```bash
   dotnet ef database update --project GiftOfTheGivers_web.csproj
   ```

5. Configure the certificate function endpoint as described in
   [Certificate Azure Function](#certificate-azure-function).
6. Run the web application:

   ```bash
   dotnet run --project GiftOfTheGivers_web.csproj
   ```

The launch profile and application output identify the local URL. Do not assume
a fixed port.

## Azure SQL setup

The web application reads the `DefaultConnection` connection string through
the standard `ConnectionStrings` configuration section. Keep database
credentials and connection strings out of source control.

For local development, initialise user secrets for the web project and set the
connection string in the local user-secret store:

```bash
dotnet user-secrets init --project GiftOfTheGivers_web.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<AZURE_SQL_CONNECTION_STRING>" --project GiftOfTheGivers_web.csproj
```

Use a connection string obtained securely from the Azure Portal or your team's
approved secret store. Replace the placeholder locally; do not paste an actual
connection string into this README, a commit, or a pull-request description.

For deployed environments, configure the app setting
`ConnectionStrings__DefaultConnection` in the hosting environment or its
backing secret store. Double underscores map to nested .NET configuration
keys. Ensure the Azure SQL firewall/network configuration permits the
application to connect, then apply migrations from an approved development or
deployment workflow:

```bash
dotnet ef database update --project GiftOfTheGivers_web.csproj
```

## Roles and authentication

Authentication uses ASP.NET Core cookie authentication. Authorization is
enforced on controller actions with role policies.

| Role | Sign-in area | Access implemented in the application |
| --- | --- | --- |
| `User` | `/Account/Login` | Profile, donation history/certificates, and volunteer application |
| `Employee` | `/Employee/Login` | Employee dashboard and operational management |
| `Admin` | `/Employee/Login` | The same employee-management routes currently authorized for `Employee,Admin` |

Public account registration creates a `User` role. Employee and administrator
records are stored separately and only active employee records can sign in.

## Certificate Azure Function

`GenerateDonationCertificate` is a .NET 8 isolated Azure Functions v4
application. Its HTTP endpoint accepts `donorName` and `donationAmount` query
parameters, validates them, then returns a certificate number and UTC issue
time. The web application reads its endpoint from
`CertificateFunction:Endpoint`.

### Run and test locally

Create `GenerateDonationCertificate/local.settings.json`; it is ignored by
Git. Do not commit this file. Its shape is:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

Start the function:

```bash
cd GenerateDonationCertificate
func start
```

Test the endpoint using the local URL printed by Functions Core Tools:

```bash
curl "<LOCAL_FUNCTION_URL>?donorName=Example%20Donor&donationAmount=125.50"
```

Set the web application's local endpoint without committing it:

```bash
dotnet user-secrets set "CertificateFunction:Endpoint" "<LOCAL_OR_DEPLOYED_FUNCTION_URL>" --project GiftOfTheGivers_web.csproj
```

### Deploy

Build and publish the function project before deployment:

```bash
dotnet publish GenerateDonationCertificate/GenerateDonationCertificate.csproj --configuration Release
cd GenerateDonationCertificate
func azure functionapp publish <FUNCTION_APP_NAME>
```

The function currently declares an anonymous HTTP trigger. Restrict access
appropriately before exposing it publicly, then set
`CertificateFunction__Endpoint` in the web application's deployed
configuration to the deployed endpoint. Do not document or commit function
keys, host names intended to remain private, or other deployment secrets.

## Azure Repos workflow

Use short-lived branches and a pull-request review process:

1. Update the local `main` branch and create a descriptive branch, for example
   `feature/volunteer-status`.
2. Make focused changes, run the relevant build/test commands, and commit
   clear messages.
3. Push the branch to Azure Repos and open a pull request targeting `main`.
4. Request review, resolve comments, and ensure the configured pipeline
   succeeds before completing the pull request.
5. Delete the merged remote branch when it is no longer needed.

Protect `main` with pull-request review and build-validation policies in Azure
DevOps. Do not commit directly to protected branches.

## Azure Pipelines CI

No Azure Pipelines YAML file is checked into this repository. Configure a
pipeline in Azure DevOps to run against the solution and use the following
commands as its build-validation steps:

```bash
dotnet restore GiftOfTheGivers_web.sln
dotnet build GiftOfTheGivers_web.sln --configuration Release --no-restore
dotnet test GiftOfTheGivers_web.sln --configuration Release --no-build
```

Use a .NET 8-capable hosted or self-hosted agent. Grant the pipeline identity
read access to the Azure Artifacts feed used by `NuGet.Config`; publishing
jobs additionally need contributor permission. Add a separate deployment stage
only after configuring the target environment, approvals, and secret handling
in Azure DevOps.

## Azure Artifacts helper package

The `DonationsFunctions` project packages the
`GiftOfTheGivers.Helpers` library. The repository's `NuGet.Config` registers
NuGet.org and a team Azure Artifacts source by name; use the configured source
name rather than copying a feed URL into documentation.

Pack the helper:

```bash
dotnet pack DonationsFunctions/DonationsFunctions.csproj --configuration Release --output ./artifacts
```

Authenticate to the team's Azure Artifacts feed using the approved Azure DevOps
credential flow, then publish with a secret supplied by the environment:

```bash
dotnet nuget push "./artifacts/GiftOfTheGivers.Helpers.1.0.0.nupkg" --source GiftOfTheGiversFeed --api-key "<AZURE_DEVOPS_PAT>"
```

Consumers restore normally once the feed is configured and authenticated:

```bash
dotnet restore GiftOfTheGivers_web.sln
```

The web project already consumes `GiftOfTheGivers.Helpers` version `1.0.0`.
Update the package reference deliberately when publishing a new version.

## Build and test

```bash
# Restore dependencies
dotnet restore GiftOfTheGivers_web.sln

# Build the complete solution
dotnet build GiftOfTheGivers_web.sln --configuration Release --no-restore

# Run test projects when they are added to the solution
dotnet test GiftOfTheGivers_web.sln --configuration Release --no-build

# Build the certificate function alone
dotnet build GenerateDonationCertificate/GenerateDonationCertificate.csproj --configuration Release
```

The current solution does not include a dedicated test project. `dotnet test`
is retained as the standard validation command and will discover tests when
they are added.

## Troubleshooting

| Symptom | Checks |
| --- | --- |
| Package restore cannot find `GiftOfTheGivers.Helpers` | Confirm Azure Artifacts access, the configured `GiftOfTheGiversFeed` source, and the referenced package version. |
| Database connection fails | Confirm `ConnectionStrings:DefaultConnection` is present in user secrets or the deployment environment, validate Azure SQL firewall access, and verify that migrations have been applied. |
| `dotnet ef` is unavailable | Install the EF Core CLI tool if required: `dotnet tool install --global dotnet-ef`; then run restore again. |
| Donation completes without a certificate | Confirm `CertificateFunction:Endpoint` is configured, the function is running/reachable, and its response is successful. The donation remains saved by design. |
| Function does not start locally | Install Azure Functions Core Tools v4, verify `local.settings.json` is present, and check the target framework is .NET 8. |
| Employee cannot sign in | Verify the employee record is active and that the stored password hash matches the password-handling flow. |

## Team contributions

Complete this section with the final POE team allocation before submission.

| Team member | Student number | Primary contribution | Azure DevOps responsibility |
| --- | --- | --- | --- |
| _Name_ | _Student number_ | _Feature/module_ | _Boards, Repos, Pipelines, or Artifacts responsibility_ |
| _Name_ | _Student number_ | _Feature/module_ | _Boards, Repos, Pipelines, or Artifacts responsibility_ |
| _Name_ | _Student number_ | _Feature/module_ | _Boards, Repos, Pipelines, or Artifacts responsibility_ |

## Security and configuration note

Treat every password, personal access token, SQL connection string, function
key, and private endpoint as a secret. Store secrets in user secrets for local
development and in approved Azure DevOps/Azure secret-management facilities
for deployed environments. Rotate any secret that is accidentally exposed.
