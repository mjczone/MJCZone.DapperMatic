using System.Text;

namespace DapperMatic;

/// <summary>
/// Describes a .NET type with its SQL type properties.
/// </summary>
public class DotnetTypeDescriptor
{
    public DotnetTypeDescriptor(
        Type dotnetType,
        int? length = null,
        int? precision = null,
        int? scale = null,
        bool? isAutoIncrementing = null,
        bool? isUnicode = null,
        bool? isFixedLength = null
    )
    {
        DotnetType =
            dotnetType?.OrUnderlyingTypeIfNullable()
            ?? throw new ArgumentNullException(nameof(dotnetType));
        Length = length;
        Precision = precision;
        Scale = scale;
        IsAutoIncrementing = isAutoIncrementing;
        IsUnicode = isUnicode;
        IsFixedLength = isFixedLength;
    }

    public Type DotnetType { get; set; }
    public int? Length { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool? IsAutoIncrementing { get; set; }
    public bool? IsUnicode { get; set; }
    public bool? IsFixedLength { get; set; }

    /// <summary>
    /// Describes the object as a string
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(DotnetType.GetFriendlyName());
        if (Length.GetValueOrDefault(0) > 0)
        {
            sb.Append($" length({Length})");
        }
        if (Precision.GetValueOrDefault(0) > 0)
        {
            if (Scale.GetValueOrDefault(0) > 0)
            {
                sb.Append($" precision({Precision},{Scale})");
            }
            else
            {
                sb.Append($" precision({Precision})");
            }
        }
        if (IsAutoIncrementing.GetValueOrDefault(false) == true)
        {
            sb.Append(" auto_increment");
        }
        if (IsUnicode.GetValueOrDefault(false) == true)
        {
            sb.Append(" unicode");
        }
        return sb.ToString();
    }
}

/// <summary>
/// A descriptor for a SQL type that breaks up the SQL type name into its useful parts,
/// including the base type name, the complete SQL type name, and the numbers extracted from the SQL type name.
/// </summary>
/// <param name="BaseTypeName"> The base type name of the SQL type.
/// For example, the base type name of "varchar(255)" is "varchar",
/// and, the base type name of "time(7) with time zone" is "time with time zone". </param>
/// <param name="SqlTypeName"> The complete SQL type name with length, precision and/or scale. </param>
/// <param name="SqlTypeNumbers"> The numbers extracted from the SQL type name.
/// /// For example, the numbers extracted from "varchar(255)" are [255]. </param>
/// <param name="Length"></param>
/// <param name="Precision"></param>
/// <param name="Scale"></param>
/// <param name="IsAutoIncrementing"></param>
/// <param name="IsUnicode"></param>
/// <param name="IsFixedLength"></param>
public class SqlTypeDescriptor
{
    public SqlTypeDescriptor(string sqlTypeName)
    {
        if (string.IsNullOrWhiteSpace(sqlTypeName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(sqlTypeName));
        }

        BaseTypeName = sqlTypeName
            .DiscardLengthPrecisionAndScaleFromSqlTypeName()
            .ToLowerInvariant();
        SqlTypeName = sqlTypeName;

        // set some of the properties using some rudimentary logic
        // as a starting point using generally known conventions
        // for the most common SQL types
        if (BaseTypeName.Contains("serial"))
        {
            IsAutoIncrementing = true;
        }

        var numbers = sqlTypeName.ExtractNumbers();
        if (numbers.Length > 0)
        {
            if (
                BaseTypeName.Contains("char")
                || BaseTypeName.Contains("text")
                || BaseTypeName.Contains("binary")
            )
            {
                Length = numbers[0];

                if (BaseTypeName.Contains("char") && !BaseTypeName.Contains("varchar"))
                {
                    IsFixedLength = true;
                }

                if (
                    BaseTypeName.Contains("nchar")
                    || BaseTypeName.Contains("nvarchar")
                    || BaseTypeName.Contains("ntext")
                )
                {
                    IsUnicode = true;
                }
            }
            else
            {
                Precision = numbers[0];
                if (numbers.Length > 1)
                {
                    Scale = numbers[1];
                }
            }
        }
    }

    public string BaseTypeName { get; set; }
    public string SqlTypeName { get; set; }
    public int? Length { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool? IsAutoIncrementing { get; set; }
    public bool? IsUnicode { get; set; }
    public bool? IsFixedLength { get; set; }

    public override string ToString()
    {
        return SqlTypeName;
    }
}
