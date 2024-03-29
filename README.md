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
Task;Bookable;Billable;Description;Start;End;DescriptionRequired
Customer A|Project1|Task1;True;False;Awesome;;;True
Customer A|Project1;True;True;;;;False
Customer A|Project2;false;true;;2019-05-16;;true
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

Optionally address information and latitude longitude can be specified in the following format:

```
Task;Bookable;Billable;Description;Start;End;DescriptionRequired;Address;City;ZipCode;State;Country;Latitude;Longitude
Orts basiert;True;True;;;;True;Martinistraße 8/2;Leonding;4060;;AT;48,246461;14,261041
Orts basiert|Poolhall;True;True;;;;True;Wattstraße 6;Linz;4030;;AT;48,24676258791299;14,265460834572343
Orts basiert|Burgerking;false;true;;2019-05-16;;true;Martinistraße 8/2;Leonding;4060;Oberösterreich;AT;48,246955491407704;
```

Optionally budget information can be specified in the following format:

```
Task;Bookable;Billable;Description;Start;End;DescriptionRequired;BudgetPlanningType;BudgetPlanningTypeInherited;HoursPlanned;HourlyRate;BudgetPlanned
Budget;True;True;;;;True;TASK_HOURLY_RATE;False;2,00;8,00;32,00
Budget|Budget Task 1;True;False;Awesome;;;False;NONE;True;;;
Budget|Budget Task 2;false;true;;2019-05-16;;true;USER_HOURLY_RATE;False;3,00;;48,00
Budget|Budget Task 3;false;true;;2019-05-16;;true;FIXED_PRICE;False;4,00;16,00;64,00
```

`BudgetPlanningTypeInherited` specifies if the budget information (if any) should be used from parent task. When `true` no other information needs to be specified.

Custom fields and/or subtasks and/or budget information can also be specified together.

#### Export project times to DB

Currently only Microsoft SQL Server is supported. The connection string for the database has to be specified as argument.

```
timrlink export-projecttime connectionstring <connectionstring> [from] [to]

Options:
from    Specifies Date from which on Project times are exported    
to      Specifies Date until project times are exported (Only working if both parameters are specified in the following format: 'yyyy-MM-dd')
        Works only if to date is after from date. If from and to date are not specified the old behavior comes in where after each run a timestamp is stored. At the next run it starts where the last period ended. Both specified dates are inclusive. So when the date of project time exactly matches it is taken into account.
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
