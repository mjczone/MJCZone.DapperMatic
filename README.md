# MJCZone.DapperMatic

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.github/workflows/build-and-test.yml](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/build-and-test.yml)
[![.github/workflows/release.yml](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/release.yml/badge.svg)](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/release.yml)

Additional `IDbConnection` extensions for DDL operations across multiple database providers.

## Dependencies

This library uses [Dapper](https://github.com/DapperLib/Dapper) for data access operations. We use a version range (`[2.1.35,3.0.0)`) to ensure compatibility with your application's Dapper version while maintaining functionality.

**Supported Dapper Versions**: 2.1.35 through 2.1.66+ (any 2.1.x version)

**Version Conflict Resolution**: If your application uses a different Dapper 2.1.x version, NuGet will automatically resolve to your version, preventing conflicts.

Refer to [documentation](https://mjczone.github.io/MJCZone.DapperMatic/).
