# Generation of Code from WSDL

## Prerequisites:

### dotnet core sdk >= 2.0 

https://dotnet.microsoft.com/download

### dotnet svcutil

`dotnet tool install --global dotnet-svcutil`

and add the dotnet global tools directory to your PATH

https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools

## ServiceReference Update

To only update the already existing Service Reference just run:

```dotnet-svcutil --update```

To generate a new Service Reference from scratch run:

```dotnet-svcutil http://timrsync.timr.com/timr/timrsync.wsdl --outputDir "./Connected Services" --outputFile TimrSyncService.cs --namespace "*,timrlink.net.Core.API" --messageContract --collectionType "System.Collections.Generic.List`1" --noStdLib```
