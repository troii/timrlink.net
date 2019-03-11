# timrlink.net
timr SOAP API client implemented in .Net Core

including sample implementation for csv import of project times which creates the required task tree too

## Packaging

### Self Contained Package including dotnet runtime ###

For creating a self contained package run the following command:

#### Windows x64

```
dotnet publish timrlink.net.SampleCSVDotNetCore/ -c Release -r win7-x64 --self-contained
```

The artifacts can then be found at `tirmlink.net.SampleCSVDotNetCore/bin/Release/netcoreapp2.0/win7-x64/publish`

#### MacOS

```
dotnet publish timrlink.net.SampleCSVDotNetCore/ -c Release -r osx-x64 --self-contained
```

The artifacts can then be found at `timrlink.net.SampleCSVDotNetCore/bin/Release/netcoreapp2.0/osx-x64/publish`


#### Ubuntu

```
dotnet publish timrlink.net.SampleCSVDotNetCore/ -c Release -r ubuntu.18.04-x64 --self-contained
```

The artifacts can then be found at `timrlink.net.SampleCSVDotNetCore/bin/Release/netcoreapp2.0/ubuntu.18.04-x64/publish`


More on publishing dotnet core apps:
<https://github.com/dotnet/docs/blob/master/docs/core/tools/dotnet-publish.md>
