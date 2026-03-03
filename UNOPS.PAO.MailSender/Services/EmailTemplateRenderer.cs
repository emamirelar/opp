using Microsoft.Extensions.Logging;
using RazorLight;
using UNOPS.PAO.MailSender.Interfaces;

namespace UNOPS.PAO.MailSender;

public class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly RazorLightEngine _engine;
    private readonly ILogger<EmailTemplateRenderer> _logger;

    public EmailTemplateRenderer(ILogger<EmailTemplateRenderer> logger)
    {
        _logger = logger;
        _engine = new RazorLightEngineBuilder()
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        try
        {
            return await RenderFromSource(templateName, model);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Template rendering failed for source: {TemplateName}", templateName);
        }

        throw new ArgumentException($"Could not find template: {templateName}");
    }

    private async Task<string> RenderFromSource<T>(string templateName, T model)
    {
        var isHtmlTemplate = templateName.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
        var isRazorTemplate = templateName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);

        var resourceName = isHtmlTemplate || isRazorTemplate 
            ? templateName 
            : $"{templateName}.cshtml";

        var assembly = typeof(T).Assembly;
        var resourceStream = assembly.GetManifestResourceStream(resourceName);

        if (resourceStream == null)
            throw new ArgumentException($"Template {resourceName} not found");

        using var reader = new StreamReader(resourceStream);
        var templateContent = await reader.ReadToEndAsync();

        return isHtmlTemplate 
            ? await RenderHtmlTemplateAsync(templateContent, model) 
            : await RenderRazorTemplateAsync(templateContent, model);
    }
    
    private async Task<string> RenderRazorTemplateAsync<T>(string razorTemplate, T model)
    {
        return await _engine.CompileRenderStringAsync(
            typeof(T).FullName ?? "Template",
            razorTemplate,
            model
        );
    }

    private Task<string> RenderHtmlTemplateAsync<T>(string htmlTemplate, T model)
    {
        var renderedTemplate = htmlTemplate;
        if (model != null)
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                var value = prop.GetValue(model)?.ToString() ?? string.Empty;
                // Support both {{ PropertyName }} (with spaces) and {{PropertyName}} (no spaces)
                renderedTemplate = renderedTemplate.Replace($"{{{{ {prop.Name} }}}}", value);
                renderedTemplate = renderedTemplate.Replace($"{{{{{prop.Name}}}}}", value);
            }
        }

        return Task.FromResult(renderedTemplate);
    }
}
