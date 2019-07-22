# timrlink.net
timr SOAP API client implemented in .Net Core

## Core

`timrlink.net.Core` contains the basic implementation and abstraction of the <timr.com> SOAP API.

## CLI

`timrlink.net.CLI` is a commandline tool for the most common actions which can be performed on the SOAP API.
Mainly this is used for importing data into timr.

Currently supported:

* Project Time import via .csv and .xlsx files

### Execution

```
dotnet run --project timrlink.net.CLI
```

### Packaging

#### Self Contained Package including dotnet runtime ###

For creating a self contained package run the following command:

##### Windows x64

```
dotnet publish timrlink.net.CLI/ -c Release -r win7-x64 --self-contained
```

The artifacts can then be found at `tirmlink.net.CLI/bin/Release/netcoreapp2.0/win7-x64/publish`

##### MacOS

```
dotnet publish timrlink.net.CLI/ -c Release -r osx-x64 --self-contained
```

The artifacts can then be found at `timrlink.net.CLI/bin/Release/netcoreapp2.0/osx-x64/publish`


##### Ubuntu

```
dotnet publish timrlink.net.CLI/ -c Release -r ubuntu.18.04-x64 --self-contained
```

The artifacts can then be found at `timrlink.net.CLI/bin/Release/netcoreapp2.0/ubuntu.18.04-x64/publish`


More on publishing dotnet core apps:
<https://github.com/dotnet/docs/blob/master/docs/core/tools/dotnet-publish.md>
