using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Microsoft.Data.SqlClient.DataClassification;
using MJCZone.DapperMatic.DataAnnotations;
using MJCZone.DapperMatic.Models;
using MJCZone.DapperMatic.Providers;

namespace MJCZone.DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Theory]
    [InlineData(typeof(TestDao1))]
    [InlineData(typeof(TestDao2))]
    [InlineData(typeof(TestDao3))]
    [InlineData(typeof(TestTable4))]
    protected virtual async Task Can_create_tables_from_model_classes_async(Type type)
    {
        var tableDef = DmTableFactory.GetTable(type);

        using var db = await OpenConnectionAsync();

        if (!string.IsNullOrWhiteSpace(tableDef.SchemaName))
        {
            await db.CreateSchemaIfNotExistsAsync(tableDef.SchemaName);
        }

        await db.CreateTableIfNotExistsAsync(tableDef);

        var tableExists = await db.DoesTableExistAsync(tableDef.SchemaName, tableDef.TableName);
        Assert.True(tableExists);

        var dropped = await db.DropTableIfExistsAsync(tableDef.SchemaName, tableDef.TableName);
        Assert.True(dropped);
    }
}

[Table("TestTable1")]
public class TestDao1
{
    [Key]
    public Guid Id { get; set; }
}

[Table("TestTable2", Schema = "my_app")]
public class TestDao2
{
    [Key]
    public Guid Id { get; set; }
}

[DmTable("TestTable3")]
public class TestDao3
{
    [DmPrimaryKeyConstraint]
    public Guid Id { get; set; }
}

[DmPrimaryKeyConstraint([nameof(TestTable4.Id)])]
public class TestTable4
{
    public Guid Id { get; set; }

    // create column of all supported types
    public string StringColumn { get; set; } = null!;
    public int IntColumn { get; set; }
    public long LongColumn { get; set; }
    public short ShortColumn { get; set; }
    public byte ByteColumn { get; set; }
    public decimal DecimalColumn { get; set; }
    public double DoubleColumn { get; set; }
    public float FloatColumn { get; set; }
    public bool BoolColumn { get; set; }
    public DateTime DateTimeColumn { get; set; }
    public DateTimeOffset DateTimeOffsetColumn { get; set; }
    public TimeSpan TimeSpanColumn { get; set; }
    public byte[] ByteArrayColumn { get; set; } = null!;
    public Guid GuidColumn { get; set; }
    public char CharColumn { get; set; }
    public char[] CharArrayColumn { get; set; } = null!;
    public object ObjectColumn { get; set; } = null!;

    // create column of all supported nullable types
    public string? NullableStringColumn { get; set; }
    public int? NullableIntColumn { get; set; }
    public long? NullableLongColumn { get; set; }
    public short? NullableShortColumn { get; set; }
    public byte? NullableByteColumn { get; set; }
    public decimal? NullableDecimalColumn { get; set; }
    public double? NullableDoubleColumn { get; set; }
    public float? NullableFloatColumn { get; set; }
    public bool? NullableBoolColumn { get; set; }
    public DateTime? NullableDateTimeColumn { get; set; }
    public DateTimeOffset? NullableDateTimeOffsetColumn { get; set; }
    public TimeSpan? NullableTimeSpanColumn { get; set; }
    public byte[]? NullableByteArrayColumn { get; set; }
    public Guid? NullableGuidColumn { get; set; }
    public char? NullableCharColumn { get; set; }
    public char[]? NullableCharArrayColumn { get; set; }
    public object? NullableObjectColumn { get; set; }

    // create columns of all enumerable types
    public IDictionary<string, string> IDictionaryColumn { get; set; } = null!;
    public IDictionary<string, string>? NullableIDictionaryColumn { get; set; }
    public Dictionary<string, string> DictionaryColumn { get; set; } = null!;
    public Dictionary<string, string>? NullableDictionaryColumn { get; set; }
    public IDictionary<string, object> IObjectDictionaryColumn { get; set; } = null!;
    public IDictionary<string, object>? NullableIObjectDictionaryColumn { get; set; }
    public Dictionary<string, object> ObjectDictionaryColumn { get; set; } = null!;
    public Dictionary<string, object>? NullableObjectDictionaryColumn { get; set; }
    public IList<string> IListColumn { get; set; } = null!;
    public IList<string>? NullableIListColumn { get; set; }
    public List<string> ListColumn { get; set; } = null!;
    public List<string>? NullableListColumn { get; set; }
    public ICollection<string> ICollectionColumn { get; set; } = null!;
    public ICollection<string>? NullableICollectionColumn { get; set; }
    public Collection<string> CollectionColumn { get; set; } = null!;
    public Collection<string>? NullableCollectionColumn { get; set; }
    public IEnumerable<string> IEnumerableColumn { get; set; } = null!;
    public IEnumerable<string>? NullableIEnumerableColumn { get; set; }

    // create columns of arrays
    public string[] StringArrayColumn { get; set; } = null!;
    public string[]? NullableStringArrayColumn { get; set; }
    public int[] IntArrayColumn { get; set; } = null!;
    public int[]? NullableIntArrayColumn { get; set; }
    public long[] LongArrayColumn { get; set; } = null!;
    public long[]? NullableLongArrayColumn { get; set; }
    public Guid[] GuidArrayColumn { get; set; } = null!;
    public Guid[]? NullableGuidArrayColumn { get; set; }

    // create columns of enums, structs and classes
    public TestEnum EnumColumn { get; set; }
    public TestEnum? NullableEnumColumn { get; set; }
    public TestStruct StructColumn { get; set; }
    public TestStruct? NullableStructColumn { get; set; }
    public TestClass ClassColumn { get; set; } = null!;
    public TestClass? NullableClassColumn { get; set; }
    public TestInterface InterfaceColumn { get; set; } = null!;
    public TestInterface? NullableInterfaceColumn { get; set; }
    public TestAbstractClass AbstractClassColumn { get; set; } = null!;
    public TestAbstractClass? NullableAbstractClassColumn { get; set; }
    public TestConcreteClass ConcreteClass { get; set; } = null!;
    public TestConcreteClass? NullableConcreteClass { get; set; }
}

public enum TestEnum
{
    Value1,
    Value2,
    Value3
}

public struct TestStruct
{
    public int Value { get; set; }
}

public class TestClass
{
    public int Value { get; set; }
}

public interface TestInterface
{
    int Value { get; set; }
}

public abstract class TestAbstractClass
{
    public int Value { get; set; }
}

public class TestConcreteClass : TestAbstractClass
{
    public int Value2 { get; set; }
}
