# Generation of Code from WSDL

## Prerequisites:

### dotnet core runtime >= 2.1, < 3

https://dotnet.microsoft.com/download/dotnet-core/2.1

### dotnet svcutil

`dotnet tool install dotnet-svcutil`

## ServiceReference Update

To only update the already existing Service Reference just run:

```dotnet svcutil --update```

To generate a new Service Reference from scratch run:

```dotnet svcutil http://timrsync.timr.com/timr/timrsync.wsdl --outputDir "./Connected Services" --outputFile TimrSyncService.cs --namespace "*,timrlink.net.Core.API" --messageContract --collectionType "System.Collections.Generic.List`1" --noStdLib```
