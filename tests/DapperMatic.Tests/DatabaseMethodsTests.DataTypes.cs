using System.Collections.ObjectModel;
using DapperMatic.Models;
using DapperMatic.Providers;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("my_app")]
    protected virtual async Task Can_handle_essential_data_types_Async(string? schemaName)
    {
        using var db = await OpenConnectionAsync();
        await InitFreshSchemaAsync(db, schemaName);

        var providerTypeMap = db.GetProviderTypeMap();

        const string tableName = "testForProviderDataTypes";

        Type[] allSupportedTypes =
        [
            .. CommonTypes,
            .. CommonDictionaryTypes,
            .. CommonEnumerableTypes,
            typeof(byte[]),
            typeof(object)
        ];

        Type[] allTestTypes = [.. allSupportedTypes, .. OtherTypes];

        // create columns starting from .NET types
        foreach (Type type in allTestTypes)
        {
            try
            {
                // make sure a column can get created with the supported .NET type
                var column = new DxColumn(
                    schemaName,
                    tableName,
                    $"col_{type.Name.ToAlpha()}",
                    type
                );

                // var created = await db.CreateTableIfNotExistsAsync(schemaName, tableName, [column]);
                // Assert.True(created);

                // // can retrieve the column created using that supported .NET type
                // var actualColumn = await db.GetColumnAsync(
                //     schemaName,
                //     tableName,
                //     column.ColumnName
                // );
                // Assert.NotNull(actualColumn);

                // // drop the table
                // await db.DropTableIfExistsAsync(schemaName, tableName);

                // make sure a provider data type mapping exists for the .NET type
                var providerDataType = providerTypeMap.GetRecommendedDataTypeForDotnetType(type);
                Assert.NotNull(providerDataType);

                // make sure a column can get created with the mapped SQL type
                if (providerDataType.SupportsLength)
                {
                    Assert.NotNull(providerDataType.SqlTypeWithLengthFormat);
                    Assert.NotEmpty(providerDataType.SqlTypeWithLengthFormat);

                    column.Length = 255;
                    column.ProviderDataType = string.Format(
                        providerDataType.SqlTypeWithLengthFormat,
                        column.Length
                    );
                }
                else if (providerDataType.SupportsScale)
                {
                    Assert.True(providerDataType.SupportsPrecision);

                    Assert.NotNull(providerDataType.SqlTypeWithPrecisionFormat);
                    Assert.NotEmpty(providerDataType.SqlTypeWithPrecisionFormat);

                    Assert.NotNull(providerDataType.SqlTypeWithPrecisionAndScaleFormat);
                    Assert.NotEmpty(providerDataType.SqlTypeWithPrecisionAndScaleFormat);

                    column.Precision = 12;
                    column.Scale = 5;
                    column.ProviderDataType = string.Format(
                        providerDataType.SqlTypeWithPrecisionFormat,
                        column.Precision
                    );
                    Assert.NotNull(column.ProviderDataType);
                    Assert.NotEmpty(column.ProviderDataType);
                    column.ProviderDataType = string.Format(
                        providerDataType.SqlTypeWithPrecisionAndScaleFormat,
                        column.Precision,
                        column.Scale
                    );
                }
                else if (providerDataType.SupportsPrecision)
                {
                    Assert.NotNull(providerDataType.SqlTypeWithPrecisionFormat);
                    Assert.NotEmpty(providerDataType.SqlTypeWithPrecisionFormat);

                    column.Precision = 12;
                    column.ProviderDataType = string.Format(
                        providerDataType.SqlTypeWithPrecisionFormat,
                        column.Precision
                    );
                }
                else
                {
                    column.ProviderDataType = providerDataType.SqlTypeFormat;
                }

                Assert.NotNull(column.ProviderDataType);
                Assert.NotEmpty(column.ProviderDataType);

                // map the column back to the same provider data type
                var fetchedProviderDataTypeString = column.ProviderDataType;
                var fetchedProviderDataType = providerTypeMap.GetRecommendedDataTypeForSqlType(
                    fetchedProviderDataTypeString
                );
                Assert.NotNull(fetchedProviderDataType);

                if (!fetchedProviderDataType.SupportedDotnetTypes.Contains(type))
                {
                    // apparently the provider data type doesn't support the .NET type, even though
                    // the provider data type was recommended for the sql type

                    // this can happen when the .NET type is a custom class,
                    // let's make sure the .NET type is a custom class
                    if (allSupportedTypes.Contains(type))
                    {
                        // this is mostly so that we can add a breakpoint to inspect what's going on
                        Assert.True(type.IsClass);
                    }
                    Assert.DoesNotContain(allSupportedTypes, t => t == type);
                }
                else
                {
                    // if the .NET type is NOT the primary dotnettype for the provider data type,
                    // then it should be in the supported dotnet types
                    if (fetchedProviderDataType.PrimaryDotnetType != type)
                    {
                        Assert.Contains(
                            fetchedProviderDataType.SupportedDotnetTypes,
                            t => t == type
                        );
                    }
                }

                var created = await db.CreateTableIfNotExistsAsync(schemaName, tableName, [column]);
                Assert.True(created);

                var columnName = column.ColumnName;
                await ValidateActualColumnAgainstProviderDataTypeUsedToCreateItAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    providerDataType
                );
            }
            finally
            {
                // drop the table
                await db.DropTableIfExistsAsync(schemaName, tableName);
            }
        }

        // create columns starting from provider data types
        var ci = 0;
        foreach (
            var providerDataType in (
                (ProviderTypeMapBase)providerTypeMap
            ).GetDefaultProviderDataTypes()
        )
        {
            try
            {
                var recommendedDotnetType = providerDataType.PrimaryDotnetType;
                Assert.NotNull(recommendedDotnetType);

                var sqlType = providerDataType.SqlTypeFormat;

                // some types are not supported in the same way by all providers
                // e.g. geomcollection is not supported by MySQL v5.7 like it is in MySQL v8.0
                if (IgnoreSqlType(sqlType))
                    continue;

                if (providerDataType.SupportsLength)
                {
                    Assert.NotNull(providerDataType.SqlTypeWithLengthFormat);
                    Assert.NotEmpty(providerDataType.SqlTypeWithLengthFormat);
                    sqlType = string.Format(providerDataType.SqlTypeWithLengthFormat, 255);
                }
                else if (providerDataType.SupportsScale)
                {
                    Assert.True(providerDataType.SupportsPrecision);

                    Assert.NotNull(providerDataType.SqlTypeWithPrecisionFormat);
                    Assert.NotEmpty(providerDataType.SqlTypeWithPrecisionFormat);

                    Assert.NotNull(providerDataType.SqlTypeWithPrecisionAndScaleFormat);
                    Assert.NotEmpty(providerDataType.SqlTypeWithPrecisionAndScaleFormat);

                    sqlType = string.Format(providerDataType.SqlTypeWithPrecisionFormat, 12);
                    Assert.NotEmpty(sqlType);

                    sqlType = string.Format(
                        providerDataType.SqlTypeWithPrecisionAndScaleFormat,
                        12,
                        5
                    );
                }
                else if (providerDataType.SupportsPrecision)
                {
                    Assert.NotNull(providerDataType.SqlTypeWithPrecisionFormat);
                    Assert.NotEmpty(providerDataType.SqlTypeWithPrecisionFormat);

                    sqlType = string.Format(providerDataType.SqlTypeWithPrecisionFormat, 12);
                }
                Assert.NotEmpty(sqlType);

                // make sure a column can get created with the provider data type
                var column = new DxColumn(
                    schemaName,
                    tableName,
                    $"col_{ci++}_{sqlType.ToAlpha()}",
                    recommendedDotnetType,
                    providerDataType: sqlType
                );

                var created = await db.CreateTableIfNotExistsAsync(schemaName, tableName, [column]);
                Assert.True(created);

                var columnName = column.ColumnName;
                await ValidateActualColumnAgainstProviderDataTypeUsedToCreateItAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    providerDataType
                );
            }
            finally
            {
                // drop the table
                await db.DropTableIfExistsAsync(schemaName, tableName);
            }
        }
    }

    private async Task ValidateActualColumnAgainstProviderDataTypeUsedToCreateItAsync(
        System.Data.IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        ProviderDataType providerDataType
    )
    {
        // can retrieve the column created using that provider data type
        var actualColumn = await db.GetColumnAsync(schemaName, tableName, columnName);
        Assert.NotNull(actualColumn);

        // TODO: validate the actual column against the provider data type used to create it
        //       once incorporated into the provider data type map
        // let's now make sure the type assigned to the column is one of the supported types
        // for the data provider
        // Assert.Contains(actualColumn.DotnetType, providerDataType.SupportedDotnetTypes);

        // if the type supports Length, Precision, or Scale,
        // make sure the actualColumn retrieved from the database has the same values
        if (providerDataType.SupportsLength)
        {
            Assert.Equal(255, actualColumn.Length);
        }
        if (providerDataType.SupportsPrecision)
        {
            Assert.Equal(12, actualColumn.Precision);

            if (providerDataType.SupportsScale)
            {
                Assert.Equal(5, actualColumn.Scale);
            }
        }
    }

    protected static readonly Type[] OtherTypes =
    [
        typeof(TestSampleDao),
        typeof(IDictionary<int, string>),
        typeof(IDictionary<Guid, string>),
        typeof(IDictionary<int, object>),
        typeof(IDictionary<string, object>),
        typeof(IEnumerable<Guid>),
        typeof(ICollection<Guid>),
        typeof(Collection<Guid>),
        typeof(IList<Guid>),
    ];

    protected static readonly Type[] CommonTypes =
    [
        typeof(char),
        typeof(string),
        typeof(bool),
        typeof(byte),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(TimeSpan),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(Guid)
    ];

    protected static readonly Type[] CommonDictionaryTypes =
    [
        // dictionary types
        .. (
            CommonTypes
                .Select(t => typeof(Dictionary<,>).MakeGenericType(t, typeof(string)))
                .ToArray()
        ),
        .. (
            CommonTypes
                .Select(t => typeof(Dictionary<,>).MakeGenericType(t, typeof(object)))
                .ToArray()
        )
    ];

    protected static readonly Type[] CommonEnumerableTypes =
    [
        // enumerable types
        .. (CommonTypes.Select(t => typeof(List<>).MakeGenericType(t)).ToArray()),
        .. (CommonTypes.Select(t => t.MakeArrayType()).ToArray())
    ];
}

public class TestSampleDao
{
    public string? Abc { get; set; }
}
