# Azure Deployment Plan

> **Status:** Deployed

Generated: 2026-10-29

## 1. Project Overview

**Goal:** Deploy the prebuilt ASP.NET Core MVC application ZIP to the user-selected existing Azure App Service.

**Path:** Update existing application

## 2. Requirements

| Attribute | Value |
|-----------|-------|
| Classification | Existing app deployment; no new infrastructure |
| Scale | Existing App Service plan; unchanged |
| Budget | No infrastructure or SKU changes |
| Subscription | sub-ter-rcecpe-rosebankcollege-studrsg-dts-001 (`1e9c617f-6871-42f1-90ac-37d4b80e445c`) |
| Location | South Africa North (existing App Service location) |

## 3. Components Detected

| Component | Type | Technology | Path |
|-----------|------|------------|------|
| GiftOfTheGivers web app | Web application | ASP.NET Core MVC, .NET 8 | `GiftOfTheGivers_web.csproj` |

## 4. Recipe Selection

**Selected:** Azure CLI, prebuilt ZIP deployment

**Rationale:** The user selected an existing Linux App Service with the .NET 8 runtime. Deploying published output avoids the failing Oryx source-build path and does not provision infrastructure.

## 5. Architecture

**Stack:** Existing Linux App Service

| Component | Azure Service | SKU |
|-----------|---------------|-----|
| GiftOfTheGivers web app | Existing App Service `app-260827090139` in `RSG-RCECPE-ST10365776-SANORTH` | Existing plan; unchanged |

The existing database connection and certificate-function settings are retained. The app will run the published `GiftOfTheGivers_web.dll`; remote build is disabled for this ZIP deployment.

## 6. Provisioning Limit Checklist

No Azure resources will be created or resized by this deployment. The existing App Service plan and its capacity are unchanged; resource quota checks are not applicable.

| Resource Type | Number to Deploy | Total After Deployment | Limit/Quota | Notes |
|---------------|------------------|------------------------|-------------|-------|
| New or resized Azure resources | 0 | 0 | Not applicable | Existing App Service receives application files only |

**Status:** No quota-consuming provisioning planned.

## 7. Execution Checklist

### Phase 1: Planning
- [x] Analyze workspace and identify .NET 8 web app
- [x] Confirm target subscription, App Service, resource group, and existing region
- [x] Select prebuilt ZIP deployment; no infrastructure provisioning
- [x] User requested deployment and confirmed the target

### Phase 2: Execution
- [x] Publish web project to `publish/`
- [x] Create and inspect `GiftOfTheGivers_web-azure.zip`
- [ ] Validate deployment target and package
- [ ] Deploy ZIP and verify endpoint

### Validation checks
- [x] Confirm Azure CLI subscription matches the user-confirmed subscription
- [x] Confirm selected App Service is running Linux with the .NET 8 runtime
- [x] Confirm ZIP archive integrity and presence of the published app entry point
- [x] Confirm required application setting names are present without displaying values
- [x] Static RBAC/IaC review: not applicable; no infrastructure templates or role assignments are part of this package deployment

## 8. Validation Proof

| Check | Command Run | Result | Timestamp |
|-------|-------------|--------|-----------|
| Publish | `dotnet publish GiftOfTheGivers_web.csproj --configuration Release --output publish` | Pass; .NET 8 publish output produced | 2026-10-29 |
| Subscription and target runtime | `az account show --query id -o tsv`; `az webapp show --name app-260827090139 --resource-group RSG-RCECPE-ST10365776-SANORTH` | Pass; confirmed subscription; running Linux App Service, `DOTNETCORE|8.0` | 2026-10-29 |
| ZIP integrity and entry point | `unzip -tq GiftOfTheGivers_web-azure.zip`; `unzip -Z1 GiftOfTheGivers_web-azure.zip` | Pass; valid archive contains `GiftOfTheGivers_web.dll` | 2026-10-29 |
| Required app settings | `az webapp config appsettings list` filtered to setting names | Pass; database and certificate-function setting names exist; values not displayed | 2026-10-29 |
| Static role verification | Review deployment scope for infrastructure templates/RBAC | Not applicable; ZIP deployment changes no role assignments | 2026-10-29 |
| Dashboard date fix publish | `dotnet publish GiftOfTheGivers_web.csproj --configuration Release --output publish` | Pass; updated Razor view compiled into published application | 2026-10-29 |
| Azure deployment | `az webapp deploy --name app-260827090139 --resource-group RSG-RCECPE-ST10365776-SANORTH --src-path GiftOfTheGivers_web-azure.zip --type zip` | Pass; OneDeploy completed with status 4 | 2026-10-29 |
| App response | `curl -m 15 https://app-260827090139.azurewebsites.net/` | Pass; HTTP 200 | 2026-10-29 |

**Validated by:** azure-validate workflow

## 9. Next Steps

> Current: Deployment complete.
