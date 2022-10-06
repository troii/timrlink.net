# TIMRLINK CLI
​
​
## timrlink.net.CLI.Migrations
​
Enthält das Datenmodell und den vollständigen Datenzugriff über Entity Framework Core.
​
### Vorbereitungen

Zu Beginn müssen die dotnet ef tools installiert werden.

https://learn.microsoft.com/en-gb/ef/core/cli/dotnet

Das hab ich direkt in Rider als Paket (Nu Get) hinzugefügt (War einfacher).

dotnet add package Microsoft.EntityFrameworkCore.Design

### Preperation on M1 Mac

Zu Beginn hatte ich Probleme dotnet ef auszuführen. Nach einiger Recherge bin ich darauf gestossen:

https://stackoverflow.com/questions/70929949/on-mac-m1-machine-not-able-to-run-ef-core-migrations-add-update-in-asp-net-co

Folgender Befehl hat mir geholfen um die dotnet ef ausführen zu können. 

```
export DOTNET_ROLL_FORWARD=LatestMajor
```

### Migration
​
Wenn Model-Änderungen gemacht werden muss eine Migration angelegt werden.
Dazu werden die [Entity Framework Core tools](https://docs.microsoft.com/en-gb/ef/core/miscellaneous/cli/dotnet) verwendet. 
​
Um eine Migration anzulegen muss folgender Befehl ausgeführt werden (Wird nur benötigt um in Zukunft neue Migrationen zu erstellen)
​
```
dotnet ef --project timrlink.net.CLI --startup-project timrlink.net.CLI migrations add AddDeletedColumnToProjectTimes 
```
​
Dabei wird die in TransactionServer.API verwendete DataSource als Referenz vom alten Schema genommen.
​
Um die aktuell konfigurierte Datenbank auf das aktuellste Schema zu aktualisieren:
​
```
dotnet ef --project timrlink.net.CLI --startup-project timrlink.net.CLI database  update AddDeletedColumnToProjectTimes -- "Server=[ADRESSE]; Database=[DATENBANK_NAME]; User Id=[BENUTZER_NAME]; Password=[PASSWORT];"

```