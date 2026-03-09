/**
 * @fileoverview Fast standalone tests for translation key conventions and completeness rules.
 * Validates key naming, language parity, and structural patterns.
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests translation key conventions and completeness rules.
/// All registries and mock data are defined inline — no production assembly references.
/// </summary>
public class TranslationKeyCompletenessTests
{
    // --- Mock translation registry: keys for en, fr, es, pt ---

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> TranslationRegistry = new Dictionary<string, IReadOnlyDictionary<string, string>>
    {
        ["en"] = new Dictionary<string, string>
        {
            ["button.save"] = "Save",
            ["button.cancel"] = "Cancel",
            ["button.edit"] = "Edit",
            ["button.delete"] = "Delete",
            ["title.partner"] = "Partner",
            ["title.contact"] = "Contact",
            ["title.opportunity"] = "Opportunity",
            ["message.success"] = "Operation completed successfully",
            ["message.error"] = "An error occurred",
            ["validation.required"] = "This field is required",
            ["validation.email"] = "Please enter a valid email address",
            ["validation.fieldName"] = "The {0} field is invalid",
            ["error.notFound"] = "Resource not found",
            ["error.unauthorized"] = "Unauthorized access",
            ["success.created"] = "Record created successfully",
            ["entity.partner"] = "Partner",
            ["entity.contact"] = "Contact",
            ["entity.opportunity"] = "Opportunity"
        },
        ["fr"] = new Dictionary<string, string>
        {
            ["button.save"] = "Enregistrer",
            ["button.cancel"] = "Annuler",
            ["button.edit"] = "Modifier",
            ["button.delete"] = "Supprimer",
            ["title.partner"] = "Partenaire",
            ["title.contact"] = "Contact",
            ["title.opportunity"] = "Opportunité",
            ["message.success"] = "Opération terminée avec succès",
            ["message.error"] = "Une erreur s'est produite",
            ["validation.required"] = "Ce champ est obligatoire",
            ["validation.email"] = "Veuillez entrer une adresse e-mail valide",
            ["validation.fieldName"] = "Le champ {0} est invalide",
            ["error.notFound"] = "Ressource introuvable",
            ["error.unauthorized"] = "Accès non autorisé",
            ["success.created"] = "Enregistrement créé avec succès",
            ["entity.partner"] = "Partenaire",
            ["entity.contact"] = "Contact",
            ["entity.opportunity"] = "Opportunité"
        },
        ["es"] = new Dictionary<string, string>
        {
            ["button.save"] = "Guardar",
            ["button.cancel"] = "Cancelar",
            ["button.edit"] = "Editar",
            ["button.delete"] = "Eliminar",
            ["title.partner"] = "Socio",
            ["title.contact"] = "Contacto",
            ["title.opportunity"] = "Oportunidad",
            ["message.success"] = "Operación completada con éxito",
            ["message.error"] = "Ocurrió un error",
            ["validation.required"] = "Este campo es obligatorio",
            ["validation.email"] = "Por favor ingrese un correo electrónico válido",
            ["validation.fieldName"] = "El campo {0} es inválido",
            ["error.notFound"] = "Recurso no encontrado",
            ["error.unauthorized"] = "Acceso no autorizado",
            ["success.created"] = "Registro creado exitosamente",
            ["entity.partner"] = "Socio",
            ["entity.contact"] = "Contacto",
            ["entity.opportunity"] = "Oportunidad"
        },
        ["pt"] = new Dictionary<string, string>
        {
            ["button.save"] = "Salvar",
            ["button.cancel"] = "Cancelar",
            ["button.edit"] = "Editar",
            ["button.delete"] = "Excluir",
            ["title.partner"] = "Parceiro",
            ["title.contact"] = "Contato",
            ["title.opportunity"] = "Oportunidade",
            ["message.success"] = "Operação concluída com sucesso",
            ["message.error"] = "Ocorreu um erro",
            ["validation.required"] = "Este campo é obrigatório",
            ["validation.email"] = "Por favor insira um e-mail válido",
            ["validation.fieldName"] = "O campo {0} é inválido",
            ["error.notFound"] = "Recurso não encontrado",
            ["error.unauthorized"] = "Acesso não autorizado",
            ["success.created"] = "Registro criado com sucesso",
            ["entity.partner"] = "Parceiro",
            ["entity.contact"] = "Contato",
            ["entity.opportunity"] = "Oportunidade"
        }
    };

    private static readonly IReadOnlyList<string> CriticalEntityKeys = new[]
    {
        "entity.partner", "entity.contact", "entity.opportunity"
    };

    // --- All languages have same number of keys (3 tests) ---

    [Fact]
    public void AllLanguages_HaveSameNumberOfKeys()
    {
        var counts = TranslationRegistry.Values.Select(d => d.Count).Distinct().ToList();
        counts.Should().HaveCount(1, "all languages must have the same number of keys");
    }

    [Fact]
    public void AllLanguages_KeyCountMatchesEnglish()
    {
        var enCount = TranslationRegistry["en"].Count;
        foreach (var (lang, dict) in TranslationRegistry)
        {
            dict.Count.Should().Be(enCount, $"language '{lang}' must have same key count as English");
        }
    }

    [Fact]
    public void AllLanguages_FourLanguagesPresent()
    {
        TranslationRegistry.Keys.Should().BeEquivalentTo(new[] { "en", "fr", "es", "pt" });
    }

    // --- No language is missing keys present in English (3 tests) ---

    [Fact]
    public void NoLanguage_MissingKeysPresentInEnglish()
    {
        var enKeys = TranslationRegistry["en"].Keys.ToHashSet();
        foreach (var (lang, dict) in TranslationRegistry)
        {
            if (lang == "en") continue;
            var missing = enKeys.Except(dict.Keys).ToList();
            missing.Should().BeEmpty($"language '{lang}' must not be missing keys from English");
        }
    }

    [Fact]
    public void French_HasAllEnglishKeys()
    {
        var enKeys = TranslationRegistry["en"].Keys.ToHashSet();
        var frKeys = TranslationRegistry["fr"].Keys.ToHashSet();
        enKeys.Except(frKeys).Should().BeEmpty();
    }

    [Fact]
    public void SpanishAndPortuguese_HaveAllEnglishKeys()
    {
        var enKeys = TranslationRegistry["en"].Keys.ToHashSet();
        foreach (var lang in new[] { "es", "pt" })
        {
            var langKeys = TranslationRegistry[lang].Keys.ToHashSet();
            enKeys.Except(langKeys).Should().BeEmpty($"'{lang}' must have all English keys");
        }
    }

    // --- Key naming follows dot-notation convention (2 tests) ---

    [Fact]
    public void KeyNaming_FollowsDotNotationConvention()
    {
        foreach (var dict in TranslationRegistry.Values)
        {
            foreach (var key in dict.Keys)
            {
                key.Should().Contain(".", $"key '{key}' must use dot notation (prefix.suffix)");
            }
        }
    }

    [Fact]
    public void KeyNaming_AllKeysHavePrefixAndSuffix()
    {
        foreach (var dict in TranslationRegistry.Values)
        {
            foreach (var key in dict.Keys)
            {
                var parts = key.Split('.');
                parts.Length.Should().BeGreaterThan(1, $"key '{key}' must have prefix.suffix format");
            }
        }
    }

    // --- No empty translation values (2 tests) ---

    [Fact]
    public void NoEmptyTranslationValues_InAnyLanguage()
    {
        foreach (var (lang, dict) in TranslationRegistry)
        {
            var empty = dict.Where(kv => string.IsNullOrWhiteSpace(kv.Value)).ToList();
            empty.Should().BeEmpty($"language '{lang}' must not have empty values");
        }
    }

    [Fact]
    public void NoEmptyTranslationValues_AllValuesNonEmpty()
    {
        foreach (var dict in TranslationRegistry.Values)
        {
            dict.Values.Should().NotContain(v => string.IsNullOrWhiteSpace(v));
        }
    }

    // --- Button keys start with "button." prefix (2 tests) ---

    [Fact]
    public void ButtonKeys_StartWithButtonPrefix()
    {
        var buttonKeys = TranslationRegistry["en"].Keys.Where(k => k.StartsWith("button.")).ToList();
        buttonKeys.Should().NotBeEmpty();
        buttonKeys.Should().OnlyContain(k => k.StartsWith("button."));
    }

    [Fact]
    public void ButtonKeys_AllButtonKeysHaveCorrectPrefix()
    {
        var expectedButtonKeys = new[] { "button.save", "button.cancel", "button.edit", "button.delete" };
        foreach (var key in expectedButtonKeys)
        {
            TranslationRegistry["en"].Should().ContainKey(key);
            key.Should().StartWith("button.");
        }
    }

    // --- Title keys start with "title." prefix (2 tests) ---

    [Fact]
    public void TitleKeys_StartWithTitlePrefix()
    {
        var titleKeys = TranslationRegistry["en"].Keys.Where(k => k.StartsWith("title.")).ToList();
        titleKeys.Should().NotBeEmpty();
        titleKeys.Should().OnlyContain(k => k.StartsWith("title."));
    }

    [Fact]
    public void TitleKeys_AllTitleKeysHaveCorrectPrefix()
    {
        var expectedTitleKeys = new[] { "title.partner", "title.contact", "title.opportunity" };
        foreach (var key in expectedTitleKeys)
        {
            TranslationRegistry["en"].Should().ContainKey(key);
            key.Should().StartWith("title.");
        }
    }

    // --- Validation keys include field name reference (2 tests) ---

    [Fact]
    public void ValidationKeys_IncludeFieldNameReference()
    {
        var validationKeys = TranslationRegistry["en"].Keys.Where(k => k.StartsWith("validation.")).ToList();
        validationKeys.Should().NotBeEmpty();
        var withPlaceholder = TranslationRegistry["en"]
            .Where(kv => kv.Key.StartsWith("validation.") && kv.Value.Contains("{0}"))
            .ToList();
        withPlaceholder.Should().NotBeEmpty("at least one validation key should support field name placeholder");
    }

    [Fact]
    public void ValidationKeys_FieldNameKeyExists()
    {
        TranslationRegistry["en"].Should().ContainKey("validation.fieldName");
        TranslationRegistry["en"]["validation.fieldName"].Should().Contain("{0}");
    }

    // --- No duplicate keys within a language (2 tests) ---

    [Fact]
    public void NoDuplicateKeys_WithinLanguage()
    {
        foreach (var (lang, dict) in TranslationRegistry)
        {
            var keyList = dict.Keys.ToList();
            var distinctCount = keyList.Distinct().Count();
            distinctCount.Should().Be(keyList.Count, $"language '{lang}' must not have duplicate keys");
        }
    }

    [Fact]
    public void NoDuplicateKeys_DictionaryEnforcesUniqueness()
    {
        foreach (var dict in TranslationRegistry.Values)
        {
            dict.Keys.Count().Should().Be(dict.Keys.Distinct().Count());
        }
    }

    // --- Key names use camelCase segments after prefix (2 tests) ---

    [Fact]
    public void KeyNames_UseCamelCaseSegmentsAfterPrefix()
    {
        foreach (var key in TranslationRegistry["en"].Keys)
        {
            var segments = key.Split('.');
            foreach (var seg in segments.Skip(1))
            {
                seg.Should().MatchRegex("^[a-z][a-zA-Z0-9]*$",
                    $"segment '{seg}' in key '{key}' should be camelCase");
            }
        }
    }

    [Fact]
    public void KeyNames_FirstSegmentLowercase()
    {
        var prefixes = new[] { "button", "title", "message", "validation", "error", "success", "entity" };
        foreach (var key in TranslationRegistry["en"].Keys)
        {
            var prefix = key.Split('.')[0];
            prefix.Should().BeOneOf(prefixes);
            prefix[0].Should().Be(char.ToLowerInvariant(prefix[0]));
        }
    }

    // --- Critical UI keys exist in all languages (2 tests) ---

    [Fact]
    public void CriticalUiKeys_ExistInAllLanguages()
    {
        foreach (var key in CriticalEntityKeys)
        {
            foreach (var (lang, dict) in TranslationRegistry)
            {
                dict.Should().ContainKey(key, $"critical key '{key}' must exist in '{lang}'");
            }
        }
    }

    [Fact]
    public void CriticalUiKeys_EntityPartnerContactOpportunity_Present()
    {
        foreach (var lang in new[] { "en", "fr", "es", "pt" })
        {
            TranslationRegistry[lang].Should().ContainKey("entity.partner");
            TranslationRegistry[lang].Should().ContainKey("entity.contact");
            TranslationRegistry[lang].Should().ContainKey("entity.opportunity");
        }
    }
}
