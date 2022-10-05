# POS Transaktions Server
​
​
## TransactionServer.Data
​
Enthält das Datenmodell und den vollständigen Datenzugriff über Entity Framework Core.
​
### Migration
​
Wenn Model-Änderungen gemacht werden muss eine Migration angelegt werden.
Dazu werden die [Entity Framework Core tools](https://docs.microsoft.com/en-gb/ef/core/miscellaneous/cli/dotnet) verwendet. 
​
Um eine Migration anzulegen muss folgender Command ausgeführt werden:
​
```
dotnet dotnet-ef --project TransactionServer.Data --startup-project TransactionServer.API migrations add <migration_name>
```
​
Dabei wird die in TransactionServer.API verwendete DataSource als Referenz vom alten Schema genommen.
​
Um die aktuell konfigurierte Datenbank auf das aktuellste Schema upzudaten:
​
```
dotnet dotnet-ef --project TransactionServer.Data --startup-project TransactionServer.API database update
```
​
Das SQL Script zum manuellen migrieren einer DB kann folgendermaßen generiert werden:
​
```
dotnet dotnet-ef --project TransactionServer.Data --startup-project TransactionServer.API migrations script
```
​
Um ein manulles Migrations-Skript zu erzeugen welches unabhängig von den bereits angewendeten Migration ist kann folgender Aufruf verwendet werden:
​
```
dotnet dotnet-ef --project TransactionServer.Data --startup-project TransactionServer.API migrations script --idempotent --output migration.sql   
```
​
Um die Datenbank völlig zurückzusetzen (ohne Löschen zu müssen) können alle Migrationen mit folgendem Command reverted werden:
​
```
dotnet dotnet-ef --project TransactionServer.Data --startup-project TransactionServer.API database update 0    
```
​
## TransactionServer.API
​
ASP.NET Core WebApi (REST API) zum übermitteln der Transactions aus SignPosServer
​
## TransactionServer.Frontend
​
ASP.NET Core WebApi (REST API) + Angular Frontend.