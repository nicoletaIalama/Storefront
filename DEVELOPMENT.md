# Storefront — local development

## `TypeLoadException: The signature is incorrect`

Usually **mixed DLL versions** or a locked API output folder. **Stop** the running API, then:

```bash
dotnet clean src/Storefront.Api/Storefront.Api.csproj
dotnet build src/Storefront.Api/Storefront.Api.csproj
```

## Backend unit tests

```bash
dotnet test tests/Storefront.Application.Tests/Storefront.Application.Tests.csproj
```

## API integration tests

```bash
dotnet test tests/Storefront.Api.IntegrationTests/Storefront.Api.IntegrationTests.csproj
```

If the build fails copying `Storefront.Api.exe`, stop any running API/debug session so the file is not locked.
