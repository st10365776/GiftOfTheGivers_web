# Selenium UI tests

The test project contains two browser-driven scenarios:

- The employee sign-in page displays its email and password fields.
- A guest selects a donation amount and payment method, then verifies the review page marks the donation anonymous.

The tests use Selenium WebDriver with headless Chrome. Install Google Chrome, start the web app with a database connection configured in `appsettings.json` or user secrets, then set `SELENIUM_BASE_URL` to the running app URL. For example:

```sh
dotnet run --project GiftOfTheGivers_web.csproj --no-launch-profile --urls http://localhost:5000
```

In Visual Studio, create a `.runsettings` file with this setting, select it under **Test > Configure Run Settings > Select Solution Wide runsettings File**, then run `SeleniumUiTests` from Test Explorer:

```xml
<RunSettings>
  <RunConfiguration>
    <EnvironmentVariables>
      <SELENIUM_BASE_URL>http://localhost:5000</SELENIUM_BASE_URL>
    </EnvironmentVariables>
  </RunConfiguration>
</RunSettings>
```

Selenium Manager resolves the matching ChromeDriver automatically. The tests stop at the donation review page, so they do not submit a real donation or call the certificate service.

To run from the command line:

```sh
SELENIUM_BASE_URL=http://localhost:5000 dotnet test GiftOfTheGivers_web.Tests/GiftOfTheGivers_web.Tests.csproj --filter FullyQualifiedName~SeleniumUiTests
```

When `SELENIUM_BASE_URL` is not set, these UI tests are skipped; the remaining unit and integration tests remain runnable without a browser or local app.
