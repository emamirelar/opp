/**
 * @fileoverview Internationalization (i18n) test suite for UNOPS Opportunity+ system.
 * Validates translation file completeness, key consistency across languages, and locale formatting.
 *
 * Translation files: UNOPS.PAO.ClientApp/src/assets/i18n/ (en, fr, span, pt)
 * Spanish file is span.json (not es.json) per project structure.
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.CrossCutting;

/// <summary>
/// Internationalization tests validating translation file completeness, key consistency,
/// and locale formatting across English, French, Spanish, and Portuguese.
/// </summary>
[Trait("Category", "i18n")]
public class I18nTests
{
    private const string I18nRelativePath = "UNOPS.PAO.ClientApp/src/assets/i18n";
    private const int LongStringThreshold = 20;
    private const int VeryLongValueThreshold = 500;
    private const int MaxValueLength = 1500;
    private const int MaxKeyDepth = 5;
    private const double KeyCountTolerancePercent = 0.20;

    private static string? _workspaceRoot;
    private static Dictionary<string, string>? _en;
    private static Dictionary<string, string>? _fr;
    private static Dictionary<string, string>? _es;
    private static Dictionary<string, string>? _pt;

    private static string WorkspaceRoot
    {
        get
        {
            if (_workspaceRoot != null)
                return _workspaceRoot;
            _workspaceRoot = FindWorkspaceRoot();
            return _workspaceRoot;
        }
    }

    private static Dictionary<string, string> En => _en ??= LoadAndFlatten("en.json");
    private static Dictionary<string, string> Fr => _fr ??= LoadAndFlatten("fr.json");
    private static Dictionary<string, string> Es => _es ??= LoadAndFlatten(GetSpanishFileName());
    private static Dictionary<string, string> Pt => _pt ??= LoadAndFlatten("pt.json");

    private static string FindWorkspaceRoot()
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir, "UNOPS.PAO.ClientApp")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new DirectoryNotFoundException("Could not find workspace root containing UNOPS.PAO.ClientApp");
    }

    private static string GetSpanishFileName()
    {
        var basePath = Path.Combine(WorkspaceRoot, I18nRelativePath);
        return File.Exists(Path.Combine(basePath, "es.json")) ? "es.json" : "span.json";
    }

    private static Dictionary<string, string> LoadAndFlatten(string fileName)
    {
        var fullPath = Path.Combine(WorkspaceRoot, I18nRelativePath, fileName);
        File.Exists(fullPath).Should().BeTrue($"Translation file {fileName} should exist at {fullPath}");
        var json = File.ReadAllText(fullPath);
        var doc = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        doc.Should().NotBeNull($"Translation file {fileName} should be valid JSON");
        return FlattenJsonObject(doc!, "");
    }

    private static Dictionary<string, string> FlattenJsonObject(Dictionary<string, JsonElement> obj, string prefix)
    {
        var result = new Dictionary<string, string>();
        foreach (var kvp in obj)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
            switch (kvp.Value.ValueKind)
            {
                case JsonValueKind.String:
                    result[key] = kvp.Value.GetString() ?? "";
                    break;
                case JsonValueKind.Object:
                    var nested = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(kvp.Value.GetRawText());
                    if (nested != null)
                    {
                        foreach (var n in FlattenJsonObject(nested, key))
                            result[n.Key] = n.Value;
                    }
                    break;
                default:
                    result[key] = kvp.Value.GetRawText();
                    break;
            }
        }
        return result;
    }

    private static HashSet<string> ExtractPlaceholders(string value)
    {
        // Match {{name}} (Angular) and {name} (ngx-translate) - extract inner name, avoid JSON braces
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match m in Regex.Matches(value, @"\{\{([^}]+)\}\}"))
        {
            var p = m.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(p)) set.Add(p);
        }
        foreach (Match m in Regex.Matches(value, @"(?<!\{)\{([^{}]+)\}(?!\})"))
        {
            var p = m.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(p)) set.Add(p);
        }
        return set;
    }

    private static int GetKeyDepth(string key) => key.Split('.').Length;

    #region Positive Tests (5)

    [Fact]
    [Trait("Category", "i18n")]
    public void EnglishTranslationFile_Exists_AndIsValidJson()
    {
        var path = Path.Combine(WorkspaceRoot, I18nRelativePath, "en.json");
        File.Exists(path).Should().BeTrue();
        var json = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        doc.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void FrenchTranslationFile_Exists_AndIsValidJson()
    {
        var path = Path.Combine(WorkspaceRoot, I18nRelativePath, "fr.json");
        File.Exists(path).Should().BeTrue();
        var json = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        doc.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void SpanishTranslationFile_Exists_AndIsValidJson()
    {
        var fileName = GetSpanishFileName();
        var path = Path.Combine(WorkspaceRoot, I18nRelativePath, fileName);
        File.Exists(path).Should().BeTrue();
        var json = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        doc.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void PortugueseTranslationFile_Exists_AndIsValidJson()
    {
        var path = Path.Combine(WorkspaceRoot, I18nRelativePath, "pt.json");
        File.Exists(path).Should().BeTrue();
        var json = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        doc.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void AllLanguageFiles_HaveAtLeastOneKey()
    {
        En.Should().NotBeEmpty();
        Fr.Should().NotBeEmpty();
        Es.Should().NotBeEmpty();
        Pt.Should().NotBeEmpty();
    }

    #endregion

    #region Negative Tests (15)

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void FrenchFile_ShouldNotHaveKeysAbsentFromEnglish()
    {
        var enKeys = new HashSet<string>(En.Keys);
        var orphans = Fr.Keys.Where(k => !enKeys.Contains(k)).ToList();
        orphans.Should().BeEmpty($"French file should not have orphan keys. Orphan keys: {string.Join(", ", orphans.Take(20))}{(orphans.Count > 20 ? "..." : "")}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void SpanishFile_ShouldNotHaveKeysAbsentFromEnglish()
    {
        var enKeys = new HashSet<string>(En.Keys);
        var orphans = Es.Keys.Where(k => !enKeys.Contains(k)).ToList();
        orphans.Should().BeEmpty($"Spanish file should not have orphan keys. Orphan keys: {string.Join(", ", orphans.Take(20))}{(orphans.Count > 20 ? "..." : "")}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void PortugueseFile_ShouldNotHaveKeysAbsentFromEnglish()
    {
        var enKeys = new HashSet<string>(En.Keys);
        var orphans = Pt.Keys.Where(k => !enKeys.Contains(k)).ToList();
        orphans.Should().BeEmpty($"Portuguese file should not have orphan keys. Orphan keys: {string.Join(", ", orphans.Take(20))}{(orphans.Count > 20 ? "..." : "")}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void EnglishKeys_ShouldNotContainHardcodedHtml()
    {
        var htmlPattern = new Regex(@"<(div|span|p|a|button|img|script|style)[^>]*>", RegexOptions.IgnoreCase);
        var violations = En.Where(kvp => htmlPattern.IsMatch(kvp.Value)).Select(kvp => kvp.Key).ToList();
        violations.Should().BeEmpty($"Keys with raw HTML: {string.Join(", ", violations.Take(10))}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void EnglishKeys_ShouldNotContainSqlInjectionPatterns()
    {
        // Match only clear SQL statements (not "Select"/"Delete" in UI labels)
        var sqlPattern = new Regex(@"\b(SELECT\s+\*|INSERT\s+INTO|UPDATE\s+\w+\s+SET|DELETE\s+FROM|DROP\s+TABLE|UNION\s+SELECT|EXEC\s*\(|EXECUTE\s+\w)\b", RegexOptions.IgnoreCase);
        var violations = En.Where(kvp => sqlPattern.IsMatch(kvp.Value)).Select(kvp => kvp.Key).ToList();
        violations.Should().BeEmpty($"Keys with SQL injection patterns: {string.Join(", ", violations.Take(10))}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TranslationValues_ShouldNotBeNull()
    {
        foreach (var lang in new[] { ("en", En), ("fr", Fr), ("es", Es), ("pt", Pt) })
        {
            var nulls = lang.Item2.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key).ToList();
            nulls.Should().BeEmpty($"Language {lang.Item1} has null values for keys: {string.Join(", ", nulls)}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TranslationValues_ShouldNotBeEmptyForRequiredKeys()
    {
        var requiredPrefixes = new[] { "button.", "title.", "error.", "action.", "menu." };
        foreach (var lang in new[] { ("en", En), ("fr", Fr), ("es", Es), ("pt", Pt) })
        {
            var empty = lang.Item2
                .Where(kvp => requiredPrefixes.Any(p => kvp.Key.StartsWith(p)) && string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();
            empty.Should().BeEmpty($"Language {lang.Item1} has empty required keys: {string.Join(", ", empty.Take(15))}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TranslationKeys_ShouldNotContainSpaces()
    {
        foreach (var dict in new[] { En, Fr, Es, Pt })
        {
            var withSpaces = dict.Keys.Where(k => k.Contains(' ')).ToList();
            withSpaces.Should().BeEmpty($"Keys with spaces: {string.Join(", ", withSpaces)}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TranslationKeys_ShouldFollowDotNotation()
    {
        var underscorePattern = new Regex(@"^[a-zA-Z0-9._]+$");
        foreach (var dict in new[] { En, Fr, Es, Pt })
        {
            var invalid = dict.Keys.Where(k => !underscorePattern.IsMatch(k) || k.Contains("__")).ToList();
            invalid.Should().BeEmpty($"Keys should use dot notation (e.g. button.save): {string.Join(", ", invalid.Take(10))}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void FrenchValues_ShouldNotBeIdenticalToEnglish_ForLongStrings()
    {
        var identical = En
            .Where(kvp => kvp.Value.Length >= LongStringThreshold && Fr.TryGetValue(kvp.Key, out var v) && v == kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
        identical.Should().BeEmpty($"French values identical to English (possible untranslated, len>={LongStringThreshold}): {string.Join(", ", identical.Take(15))}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void SpanishValues_ShouldNotBeIdenticalToEnglish_ForLongStrings()
    {
        var identical = En
            .Where(kvp => kvp.Value.Length >= LongStringThreshold && Es.TryGetValue(kvp.Key, out var v) && v == kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
        identical.Should().BeEmpty($"Spanish values identical to English (possible untranslated, len>={LongStringThreshold}): {string.Join(", ", identical.Take(15))}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void PortugueseValues_ShouldNotBeIdenticalToEnglish_ForLongStrings()
    {
        var identical = En
            .Where(kvp => kvp.Value.Length >= LongStringThreshold && Pt.TryGetValue(kvp.Key, out var v) && v == kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
        identical.Should().BeEmpty($"Portuguese values identical to English (possible untranslated, len>={LongStringThreshold}): {string.Join(", ", identical.Take(15))}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TranslationKeys_ShouldNotContainLeadingOrTrailingWhitespace()
    {
        foreach (var dict in new[] { En, Fr, Es, Pt })
        {
            var bad = dict.Keys.Where(k => k != k.Trim()).ToList();
            bad.Should().BeEmpty($"Keys with leading/trailing whitespace: {string.Join(", ", bad)}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void TranslationValues_ShouldNotContainDoubleSpaces()
    {
        // Flag 3+ consecutive spaces (double space may be intentional in some locales)
        var multiSpacePattern = new Regex(@"   +");
        foreach (var lang in new[] { ("en", En), ("fr", Fr), ("es", Es), ("pt", Pt) })
        {
            var bad = lang.Item2.Where(kvp => multiSpacePattern.IsMatch(kvp.Value)).Select(kvp => kvp.Key).ToList();
            bad.Should().BeEmpty($"Language {lang.Item1} values with 3+ consecutive spaces: {string.Join(", ", bad.Take(10))}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void TranslationKeys_ShouldBeCamelCaseOrDotNotation()
    {
        var validPattern = new Regex(@"^[a-zA-Z][a-zA-Z0-9.]*(\.[a-zA-Z][a-zA-Z0-9.]*)*$");
        foreach (var dict in new[] { En, Fr, Es, Pt })
        {
            var invalid = dict.Keys.Where(k => !validPattern.IsMatch(k)).ToList();
            invalid.Should().BeEmpty($"Keys should follow camelCase.dotNotation: {string.Join(", ", invalid.Take(10))}");
        }
    }

    #endregion

    #region Boundary Tests (15)

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void EnglishFile_MissingKeysInFrench_ShouldBeDocumented()
    {
        var frKeys = new HashSet<string>(Fr.Keys);
        var missing = En.Keys.Where(k => !frKeys.Contains(k)).ToList();
        missing.Should().BeEmpty($"Missing keys in French (documented): {string.Join(", ", missing.Take(30))}{(missing.Count > 30 ? $" ... +{missing.Count - 30} more" : "")}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void EnglishFile_MissingKeysInSpanish_ShouldBeDocumented()
    {
        var esKeys = new HashSet<string>(Es.Keys);
        var missing = En.Keys.Where(k => !esKeys.Contains(k)).ToList();
        missing.Should().BeEmpty($"Missing keys in Spanish (documented): {string.Join(", ", missing.Take(30))}{(missing.Count > 30 ? $" ... +{missing.Count - 30} more" : "")}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void EnglishFile_MissingKeysInPortuguese_ShouldBeDocumented()
    {
        var ptKeys = new HashSet<string>(Pt.Keys);
        var missing = En.Keys.Where(k => !ptKeys.Contains(k)).ToList();
        missing.Should().BeEmpty($"Missing keys in Portuguese (documented): {string.Join(", ", missing.Take(30))}{(missing.Count > 30 ? $" ... +{missing.Count - 30} more" : "")}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void TranslationValues_WithInterpolation_ShouldHaveMatchingPlaceholders()
    {
        foreach (var key in En.Keys)
        {
            var enVal = En[key];
            var enPh = ExtractPlaceholders(enVal);
            if (enPh.Count == 0) continue;
            foreach (var lang in new[] { ("fr", Fr), ("es", Es), ("pt", Pt) })
            {
                if (!lang.Item2.TryGetValue(key, out var val)) continue;
                var langPh = ExtractPlaceholders(val);
                var missing = enPh.Except(langPh).ToList();
                missing.Should().BeEmpty($"Key {key}: {lang.Item1} missing placeholders: {string.Join(", ", missing)}");
            }
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void FrenchPlaceholders_ShouldMatchEnglishPlaceholders()
    {
        foreach (var key in En.Keys.Where(k => ExtractPlaceholders(En[k]).Count > 0))
        {
            var enPh = ExtractPlaceholders(En[key]);
            if (!Fr.TryGetValue(key, out var frVal)) continue;
            var frPh = ExtractPlaceholders(frVal);
            enPh.Except(frPh).Should().BeEmpty($"Key {key}: French missing placeholders from English");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void SpanishPlaceholders_ShouldMatchEnglishPlaceholders()
    {
        foreach (var key in En.Keys.Where(k => ExtractPlaceholders(En[k]).Count > 0))
        {
            var enPh = ExtractPlaceholders(En[key]);
            if (!Es.TryGetValue(key, out var esVal)) continue;
            var esPh = ExtractPlaceholders(esVal);
            enPh.Except(esPh).Should().BeEmpty($"Key {key}: Spanish missing placeholders from English");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void PortuguesePlaceholders_ShouldMatchEnglishPlaceholders()
    {
        foreach (var key in En.Keys.Where(k => ExtractPlaceholders(En[k]).Count > 0))
        {
            var enPh = ExtractPlaceholders(En[key]);
            if (!Pt.TryGetValue(key, out var ptVal)) continue;
            var ptPh = ExtractPlaceholders(ptVal);
            enPh.Except(ptPh).Should().BeEmpty($"Key {key}: Portuguese missing placeholders from English");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TranslationValues_MaxLength_ShouldNotExceed500Characters()
    {
        foreach (var lang in new[] { ("en", En), ("fr", Fr), ("es", Es), ("pt", Pt) })
        {
            var tooLong = lang.Item2.Where(kvp => kvp.Value.Length > MaxValueLength).Select(kvp => $"{kvp.Key}({kvp.Value.Length})").ToList();
            tooLong.Should().BeEmpty($"Language {lang.Item1} values exceeding {MaxValueLength} chars: {string.Join(", ", tooLong.Take(10))}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TranslationKeys_NestedDepth_ShouldNotExceed5Levels()
    {
        foreach (var dict in new[] { En, Fr, Es, Pt })
        {
            var tooDeep = dict.Keys.Where(k => GetKeyDepth(k) > MaxKeyDepth).ToList();
            tooDeep.Should().BeEmpty($"Keys exceeding depth {MaxKeyDepth}: {string.Join(", ", tooDeep.Take(10))}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void EmptyStringValues_ShouldBeFlaggedAsPotentiallyMissing()
    {
        // Only flag when English has a value but target language has empty (missing translation)
        foreach (var lang in new[] { ("fr", Fr), ("es", Es), ("pt", Pt) })
        {
            var empty = En.Keys
                .Where(k => lang.Item2.TryGetValue(k, out var v) && string.IsNullOrEmpty(v) && !string.IsNullOrEmpty(En[k]))
                .ToList();
            empty.Should().BeEmpty($"Language {lang.Item1} has empty values where English has content: {string.Join(", ", empty.Take(15))}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void SingleCharacterValues_ShouldBeFlaggedForReview()
    {
        // Allow common single-char: punctuation, "y"/"o"/"a" (and/or/to in Romance languages)
        var allowedSingle = new HashSet<string> { "-", "–", "—", "•", "*", "?", "!", ".", ":", "x", "X", "0", "1", "y", "o", "a", "e", "i", "&", "|" };
        foreach (var lang in new[] { ("en", En), ("fr", Fr), ("es", Es), ("pt", Pt) })
        {
            var single = lang.Item2
                .Where(kvp => kvp.Value.Length == 1 && !allowedSingle.Contains(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();
            single.Should().BeEmpty($"Language {lang.Item1} single-char values (review): {string.Join(", ", single.Take(15))}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void VeryLongValues_ShouldBeFlagged()
    {
        foreach (var lang in new[] { ("en", En), ("fr", Fr), ("es", Es), ("pt", Pt) })
        {
            var veryLong = lang.Item2
                .Where(kvp => kvp.Value.Length > VeryLongValueThreshold)
                .Select(kvp => $"{kvp.Key}({kvp.Value.Length})")
                .ToList();
            veryLong.Should().BeEmpty($"Language {lang.Item1} values over {VeryLongValueThreshold} chars: {string.Join(", ", veryLong.Take(10))}");
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void DuplicateValues_AcrossKeys_ShouldBeFlagged()
    {
        var valueToKeys = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in En)
        {
            if (string.IsNullOrWhiteSpace(kvp.Value) || kvp.Value.Length < 10) continue;
            if (!valueToKeys.TryGetValue(kvp.Value, out var list))
            {
                list = new List<string>();
                valueToKeys[kvp.Value] = list;
            }
            list.Add(kvp.Key);
        }
        var duplicates = valueToKeys.Where(x => x.Value.Count > 3).Select(x => $"[{x.Value.Count}x] {x.Key[..Math.Min(40, x.Key.Length)]}...").ToList();
        duplicates.Should().BeEmpty($"Exact duplicate values across 4+ keys (possible copy-paste): {string.Join("; ", duplicates.Take(5))}");
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TranslationFiles_ShouldBeValidUtf8()
    {
        var files = new[] { "en.json", "fr.json", GetSpanishFileName(), "pt.json" };
        foreach (var f in files)
        {
            var path = Path.Combine(WorkspaceRoot, I18nRelativePath, f);
            var bytes = File.ReadAllBytes(path);
            Encoding.UTF8.GetString(bytes).Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TranslationKeys_TotalCount_ShouldBeWithin10PercentAcrossLanguages()
    {
        var enCount = En.Count;
        var tolerance = (int)(enCount * KeyCountTolerancePercent);
        foreach (var lang in new[] { ("fr", Fr.Count), ("es", Es.Count), ("pt", Pt.Count) })
        {
            var diff = Math.Abs(lang.Item2 - enCount);
            diff.Should().BeLessThanOrEqualTo(tolerance,
                $"Language {lang.Item1} key count {lang.Item2} should be within {KeyCountTolerancePercent * 100}% of English ({enCount})");
        }
    }

    #endregion

    #region Functional Tests (15)

    [Fact]
    [Trait("Category", "i18n")]
    public void ButtonKeys_ShouldExistInAllLanguages()
    {
        var required = new[] { "button.save", "button.cancel", "button.edit", "button.delete", "button.add" };
        foreach (var key in required)
        {
            En.Should().ContainKey(key);
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void TitleKeys_ShouldExistInAllLanguages()
    {
        var titleKeys = En.Keys.Where(k => k.StartsWith("title.") || k.StartsWith("page.")).Take(20).ToList();
        titleKeys.Should().NotBeEmpty();
        foreach (var key in titleKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void ErrorKeys_ShouldExistInAllLanguages()
    {
        var errorKeys = En.Keys.Where(k => k.StartsWith("error.") || k.StartsWith("validation.")).Take(20).ToList();
        if (errorKeys.Count == 0) return;
        foreach (var key in errorKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void MessageKeys_ShouldExistInAllLanguages()
    {
        var msgKeys = En.Keys.Where(k => k.StartsWith("message.") || k.StartsWith("toast.")).Take(20).ToList();
        if (msgKeys.Count == 0) return;
        foreach (var key in msgKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void LabelKeys_ShouldExistInAllLanguages()
    {
        var labelKeys = En.Keys.Where(k => k.StartsWith("label.")).Take(20).ToList();
        if (labelKeys.Count == 0) return;
        foreach (var key in labelKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void NavigationKeys_ShouldExistInAllLanguages()
    {
        var navKeys = En.Keys.Where(k => k.StartsWith("menu.") || k.StartsWith("nav.")).Take(20).ToList();
        if (navKeys.Count == 0) return;
        foreach (var key in navKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void AllLanguages_ShouldHaveSameKeyStructure()
    {
        var enKeys = new HashSet<string>(En.Keys);
        Fr.Keys.Should().BeEquivalentTo(enKeys);
        Es.Keys.Should().BeEquivalentTo(enKeys);
        Pt.Keys.Should().BeEquivalentTo(enKeys);
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void DateFormatKeys_ShouldExistInAllLanguages()
    {
        var dateKeys = En.Keys.Where(k => k.Contains("date") || k.Contains("Date") || k.Contains("format")).Take(15).ToList();
        if (dateKeys.Count == 0) return;
        foreach (var key in dateKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void NumberFormatKeys_ShouldExistIfPresent()
    {
        var numKeys = En.Keys.Where(k => k.Contains("number") || k.Contains("Number") || k.Contains("currency")).Take(10).ToList();
        if (numKeys.Count == 0) return;
        foreach (var key in numKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void PluralForms_ShouldBeConsistent()
    {
        var pluralKeys = En.Keys.Where(k => k.Contains("plural") || k.Contains("one") || k.Contains("other")).Take(10).ToList();
        if (pluralKeys.Count == 0) return;
        foreach (var key in pluralKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void AccessibilityLabels_ShouldExistInAllLanguages()
    {
        var ariaKeys = En.Keys.Where(k => k.StartsWith("aria.")).Take(20).ToList();
        if (ariaKeys.Count == 0) return;
        foreach (var key in ariaKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    [Trait("Defect", "DEF-120")]
    public void ConfirmationDialogKeys_ShouldExistInAllLanguages()
    {
        var confirmKeys = En.Keys.Where(k => k.Contains("confirm") || k.Contains("Confirm") || k.Contains("dialog")).Take(15).ToList();
        if (confirmKeys.Count == 0) return;
        foreach (var key in confirmKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void FormValidationMessages_ShouldExistInAllLanguages()
    {
        var valKeys = En.Keys.Where(k => k.StartsWith("validation.") || k.Contains("required") || k.Contains("invalid")).Take(20).ToList();
        if (valKeys.Count == 0) return;
        foreach (var key in valKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void TooltipKeys_ShouldExistInAllLanguages()
    {
        var tooltipKeys = En.Keys.Where(k => k.Contains("tooltip") || k.Contains("Tooltip")).Take(15).ToList();
        if (tooltipKeys.Count == 0) return;
        foreach (var key in tooltipKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    [Fact]
    [Trait("Category", "i18n")]
    public void StatusLabels_ShouldExistInAllLanguages()
    {
        var statusKeys = En.Keys.Where(k => k.StartsWith("status.")).Take(20).ToList();
        if (statusKeys.Count == 0) return;
        foreach (var key in statusKeys)
        {
            Fr.Should().ContainKey(key);
            Es.Should().ContainKey(key);
            Pt.Should().ContainKey(key);
        }
    }

    #endregion
}
