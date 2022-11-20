# TIMRLINK CLI Database Migrations

Contains the data model and full access via Entity Framework Core.

## Preperations

First you have to install dotnet ef tools.

https://learn.microsoft.com/en-gb/ef/core/cli/dotnet

## Preperations for Apple Silicon Macs

This command is needed so we can finally run `dotnet ef tools`

Explanation can be found [here](https://stackoverflow.com/questions/70929949/on-mac-m1-machine-not-able-to-run-ef-core-migrations-add-update-in-asp-net-co). 

```
export DOTNET_ROLL_FORWARD=LatestMajor
```

## Migrations

With this command your database will be updated to match the current datamodel defined in .Net

```
dotnet ef --project timrlink.net.CLI --startup-project timrlink.net.CLI database update [migration_name] -- "Server=[ADRESSE]; Database=[DATENBANK_NAME]; User Id=[BENUTZER_NAME]; Password='[PASSWORT]';"

```

### Create new Migration during development

**IMPORTANT:** This step is only needed during development

To create a new migration add new properties in DatabaseContext.cs. Then run the following command and a migration is created.

```
dotnet ef --project timrlink.net.CLI --startup-project timrlink.net.CLI migrations add [migration_name] -- "Server=[ADRESSE]; Database=[DATENBANK_NAME]; User Id=[BENUTZER_NAME]; Password='[PASSWORT]';"
```