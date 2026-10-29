# Donation integration test

Restore from NuGet.org and run the integration and unit tests with:

```sh
dotnet restore GiftOfTheGivers_web.Tests/GiftOfTheGivers_web.Tests.csproj --source https://api.nuget.org/v3/index.json
dotnet test GiftOfTheGivers_web.Tests/GiftOfTheGivers_web.Tests.csproj --no-restore
```

`DonationFlowIntegrationTests` exercises the MVC donation forms over HTTP. It submits the donation form and completion form with their anti-forgery tokens, stubs the certificate function HTTP response, and verifies the saved donation and generated certificate in an isolated EF Core in-memory database.

The test checks the persisted record and the function request:

```csharp
Assert.Equal(250m, donation.Amount);
Assert.Equal("Monthly", donation.DonationType);
Assert.True(donation.IsAnonymous);
Assert.Equal("GOTG-2026-INT001", donation.CertificateNumber);
Assert.Equal("Anonymous Donor", functionQuery["donorName"]);
```

Successful test run:

![Successful donation integration test run](screenshots/donation-integration-tests.png)

# Unit Tests Commands

dotnet test GiftOfTheGivers_web.sln

