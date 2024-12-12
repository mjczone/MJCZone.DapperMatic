# Installation

To get started, add the github nuget source for the package to your nuget.config.

```sh
# create the nuget.config file if it doesn't exist
dotnet new nuget.config

# add the package source
dotnet nuget add source https://nuget.pkg.github.com/mjczone/index.json -n mjczone@nuget.pkg.github
```

Add the nuget package to your project.

```sh
dotnet add package MJCZone.DapperMatic
```

Explore the extension methods you can use in your project.

- [Extension Methods](#/usage/extension-methods)
