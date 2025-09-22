using System.Globalization;
using System.Text.RegularExpressions;
using UNOPS.PAO.ExternalDataService.Models.Configuration;

namespace UNOPS.PAO.ExternalDataService.Infrastructure.Utilities;

public class TransformationEngine
{
    private readonly ILogger<TransformationEngine> _logger;

    public TransformationEngine(ILogger<TransformationEngine> logger)
    {
        _logger = logger;
    }

    public object? ApplyTransformations(object? value, List<FieldTransformation> transformations, string fieldName)
    {
        if (value == null || !transformations.Any())
            return value;

        var currentValue = value;

        foreach (var transformation in transformations)
        {
            try
            {
                currentValue = ApplyTransformation(currentValue, transformation, fieldName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Transformation '{TransformationType}' failed for field '{FieldName}' with value '{Value}'",
                    transformation.Type, fieldName, value);
                // Continue with original value on transformation failure
                return value;
            }
        }

        return currentValue;
    }

    private object? ApplyTransformation(object? value, FieldTransformation transformation, string fieldName)
    {
        if (value == null) return null;

        var stringValue = value.ToString() ?? string.Empty;

        return transformation.Type.ToLower() switch
        {
            "trim" => stringValue.Trim(),
            "uppercase" => stringValue.ToUpper(),
            "lowercase" => stringValue.ToLower(),
            "title_case" => ToTitleCase(stringValue),
            "remove_whitespace" => Regex.Replace(stringValue, @"\s+", ""),
            "normalize_whitespace" => Regex.Replace(stringValue.Trim(), @"\s+", " "),
            "remove_special_chars" => Regex.Replace(stringValue, @"[^\w\s]", ""),
            "validate_email" => ValidateEmail(stringValue),
            "format_phone" => FormatPhoneNumber(stringValue),
            "round" => RoundDecimal(value, transformation.Parameters),
            "format_date" => FormatDate(value, transformation.Parameters),
            "replace" => ReplaceValue(stringValue, transformation.Parameters),
            "regex_replace" => RegexReplace(stringValue, transformation.Parameters),
            "substring" => SubstringValue(stringValue, transformation.Parameters),
            "pad_left" => PadLeft(stringValue, transformation.Parameters),
            "pad_right" => PadRight(stringValue, transformation.Parameters),
            "status_mapping" => MapStatus(stringValue, transformation.Parameters),
            "currency_format" => FormatCurrency(value, transformation.Parameters),
            "remove_prefix" => RemovePrefix(stringValue, transformation.Parameters),
            "remove_suffix" => RemoveSuffix(stringValue, transformation.Parameters),
            "default_if_empty" => string.IsNullOrEmpty(stringValue) ? GetParameterValue(transformation.Parameters, "default_value", stringValue) : stringValue,
            "clamp" => ClampNumeric(value, transformation.Parameters),
            _ => throw new NotSupportedException($"Transformation type '{transformation.Type}' is not supported")
        };
    }

    private string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }

    private string ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return email;
        
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        if (!emailRegex.IsMatch(email))
        {
            _logger.LogWarning("Invalid email format: {Email}", email);
        }
        
        return email.Trim().ToLower();
    }

    private string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return phone;
        
        // Remove all non-digit characters
        var digits = Regex.Replace(phone, @"[^\d]", "");
        
        // Format based on length (assuming international format)
        return digits.Length switch
        {
            10 => $"({digits[..3]}) {digits[3..6]}-{digits[6..]}",
            11 when digits[0] == '1' => $"+1 ({digits[1..4]}) {digits[4..7]}-{digits[7..]}",
            _ => phone // Return original if format is unclear
        };
    }

    private object RoundDecimal(object value, Dictionary<string, object> parameters)
    {
        if (!decimal.TryParse(value.ToString(), out var decimalValue))
            return value;

        var decimalPlaces = GetParameterValue(parameters, "decimal_places", 2);
        return Math.Round(decimalValue, decimalPlaces);
    }

    private string FormatDate(object value, Dictionary<string, object> parameters)
    {
        if (!DateTime.TryParse(value.ToString(), out var dateValue))
            return value.ToString() ?? string.Empty;

        var format = GetParameterValue(parameters, "format", "yyyy-MM-dd");
        return dateValue.ToString(format);
    }

    private string ReplaceValue(string input, Dictionary<string, object> parameters)
    {
        var oldValue = GetParameterValue(parameters, "old_value", "");
        var newValue = GetParameterValue(parameters, "new_value", "");
        
        return input.Replace(oldValue, newValue);
    }

    private string RegexReplace(string input, Dictionary<string, object> parameters)
    {
        var pattern = GetParameterValue(parameters, "pattern", "");
        var replacement = GetParameterValue(parameters, "replacement", "");
        
        if (string.IsNullOrEmpty(pattern)) return input;
        
        return Regex.Replace(input, pattern, replacement);
    }

    private string SubstringValue(string input, Dictionary<string, object> parameters)
    {
        var start = GetParameterValue(parameters, "start", 0);
        var length = GetParameterValue(parameters, "length", input.Length - start);
        
        if (start >= input.Length) return string.Empty;
        if (start + length > input.Length) length = input.Length - start;
        
        return input.Substring(start, length);
    }

    private string PadLeft(string input, Dictionary<string, object> parameters)
    {
        var totalWidth = GetParameterValue(parameters, "width", input.Length);
        var paddingChar = GetParameterValue(parameters, "char", " ")[0];
        
        return input.PadLeft(totalWidth, paddingChar);
    }

    private string PadRight(string input, Dictionary<string, object> parameters)
    {
        var totalWidth = GetParameterValue(parameters, "width", input.Length);
        var paddingChar = GetParameterValue(parameters, "char", " ")[0];
        
        return input.PadRight(totalWidth, paddingChar);
    }

    private string MapStatus(string input, Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("mapping", out var mappingObj) && mappingObj is Dictionary<string, object> mapping)
        {
            var normalizedInput = input.ToUpper().Trim();
            foreach (var kvp in mapping)
            {
                if (kvp.Key.ToUpper() == normalizedInput)
                {
                    return kvp.Value.ToString() ?? input;
                }
            }
        }
        
        return input; // Return original if no mapping found
    }

    private string FormatCurrency(object value, Dictionary<string, object> parameters)
    {
        if (!decimal.TryParse(value.ToString(), out var decimalValue))
            return value.ToString() ?? string.Empty;

        var currencyCode = GetParameterValue(parameters, "currency", "USD");
        var decimalPlaces = GetParameterValue(parameters, "decimal_places", 2);
        
        return decimalValue.ToString($"C{decimalPlaces}", new CultureInfo("en-US"));
    }

    private string RemovePrefix(string input, Dictionary<string, object> parameters)
    {
        var prefix = GetParameterValue(parameters, "prefix", "");
        
        if (string.IsNullOrEmpty(prefix) || !input.StartsWith(prefix))
            return input;
            
        return input.Substring(prefix.Length);
    }

    private string RemoveSuffix(string input, Dictionary<string, object> parameters)
    {
        var suffix = GetParameterValue(parameters, "suffix", "");
        
        if (string.IsNullOrEmpty(suffix) || !input.EndsWith(suffix))
            return input;
            
        return input.Substring(0, input.Length - suffix.Length);
    }

    private object ClampNumeric(object value, Dictionary<string, object> parameters)
    {
        if (!decimal.TryParse(value.ToString(), out var decimalValue))
            return value;

        var min = GetParameterValue(parameters, "min", decimal.MinValue);
        var max = GetParameterValue(parameters, "max", decimal.MaxValue);
        
        return Math.Max(min, Math.Min(max, decimalValue));
    }

    private T GetParameterValue<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (!parameters.TryGetValue(key, out var value))
            return defaultValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}
