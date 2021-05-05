# timrlink.net
[timr](timr.com) SOAP API client implemented in .Net Core

## Core

`timrlink.net.Core` contains the basic implementation and abstraction of the [timr.com](timr.com) SOAP API.

## CLI

`timrlink.net.CLI` is a commandline tool for the most common actions which can be performed on the SOAP API.
Mainly this is used for importing data into timr.

Currently supported:

* Project Time import via .csv and .xlsx files
* Task import via .csv files

### Setup

Specify your timr identifier (\<identifier>.timr.com) in the `config.json` file and enter your `Authentication Token` (found at `Administration -> Settings -> timr API`).
For full support with excel imports also set the `Show external ID` to `Yes`, so task assignment and creation works as expected.

```
{
  "timrSync": {
    "identifier": "<identifier>",
    "token": "<Authentication Token>"
  }
}
```

### Usage

#### Import project times

```
timrlink pt <file>
timrlink projecttime <file>
```

CSV file has to be in the following format:

```
User;Task;StartDateTime;EndDateTime;Break;Notes;Billable
John Dow;INTERNAL|Holiday;01.12.15 8:00;01.12.15 16:30;0:30;;false
```

Excel files are supported in the form of excel exports from timr.

#### Import tasks

```
timrlink t <file> [--u]
timrlink task <file> [--update]

Options:
  -u, --update    Update existing tasks with same externalId, default: true
```

CSV file has to be in the following format:

```
Task;Bookable;Billable;Description;Start;End
Customer A|Project1|Task1;True;False;Awesome;;
Customer A|Project1;True;True;;;
Customer A|Project2;false;true;;2019-05-16;
```

Optionally Custom Fields can be included, which then requires the following format:

```
Task;Bookable;Billable;Description;Start;End;CustomField1;CustomField2;CustomField3
Customer A|Project1|Task1;True;False;Awesome;;Field1;Field2;Field3
Customer A|Project1;True;True;;;;;;
Customer A|Project2;false;true;;2019-05-16;;;;
```

Optionally Subtasks can be included. They need to be seperated by ','. Subtasks are always bookable and inherit the specified billable:

```
Task;Bookable;Billable;Description;Start;End;Subtasks
Customer A|Project1|Task1;True;False;Awesome;;;Support,Sales
Customer A|Project1;True;True;;;;Support
Customer A|Project2;false;true;;2019-05-16;;Development,Testing
```

Custom Fields and Subtasks can also be specified together.

#### Export project times to DB

Currently only Microsoft SQL Server is supported. The connection string for the database has to be specified as argument.

```
timrlink export-projecttime connectionstring <connectionstring>
```

Project times will get exported to the specified database. Required tables get created automatically.
Project times will get updated when changed in timr too. 

### Building

```
dotnet build
```

### Execution

Execution from source

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

The artifacts can then be found at `timrlink.net.CLI/bin/Release/netcoreapp3.1/win7-x64/publish`

##### MacOS

```
dotnet publish timrlink.net.CLI/ -c Release -r osx-x64 --self-contained
```

The artifacts can then be found at `timrlink.net.CLI/bin/Release/netcoreapp3.1/osx-x64/publish`


##### Ubuntu

```
dotnet publish timrlink.net.CLI/ -c Release -r ubuntu.18.04-x64 --self-contained
```

The artifacts can then be found at `timrlink.net.CLI/bin/Release/netcoreapp3.1/ubuntu.18.04-x64/publish`


More on publishing dotnet core apps:
<https://github.com/dotnet/docs/blob/master/docs/core/tools/dotnet-publish.md>
