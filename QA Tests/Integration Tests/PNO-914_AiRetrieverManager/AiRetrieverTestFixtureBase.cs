/**
 * @fileoverview PNO-914 AiRetrieverManager test fixture base.
 * Tests AiRetrieverManager settings, input validation, and error handling.
 *
 * IAPAuthHelper has non-virtual methods and requires Google Cloud credentials in its
 * constructor. AiRetrieverManager creates HttpClient internally. These constraints mean
 * tests focus on: (1) settings/creation, (2) input validation, (3) expected exceptions
 * when infrastructure is unavailable.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Configuration;

namespace UNOPS.PAO.IntegrationTests.PNO914.AiRetriever;

/// <summary>
/// Shared fixture base for PNO-914 AiRetrieverManager tests.
/// Uses an uninitialized IAPAuthHelper (bypassing Google Cloud credential requirements)
/// to test real AiRetrieverManager behavior including proper error handling.
/// </summary>
public abstract class AiRetrieverTestFixtureBase
{
    protected readonly Mock<IConfiguration> MockConfiguration;
    protected readonly Mock<ILogger<AiRetrieverManager>> MockLogger;
    protected ExternalApiSettings Settings;

    protected AiRetrieverTestFixtureBase()
    {
        MockConfiguration = new Mock<IConfiguration>();
        MockLogger = new Mock<ILogger<AiRetrieverManager>>();

        MockConfiguration.Setup(x => x.GetSection("Development:IAPSimulation:Enabled").Value).Returns((string?)null);

        Settings = new ExternalApiSettings
        {
            BaseUrl = "https://api.test.unops.org/",
            OAuthClientId = "test-client-id",
            Timeout = 30
        };
    }

    /// <summary>
    /// Creates AiRetrieverManager with an uninitialized IAPAuthHelper.
    /// The IAPAuthHelper is created via RuntimeHelpers to bypass its constructor
    /// (which requires Google Cloud credentials). Method calls on the manager will
    /// throw when they attempt IAP authentication, which is the expected behavior
    /// in a test environment without cloud credentials.
    /// </summary>
    protected AiRetrieverManager CreateManager()
    {
        var iapHelper = (IAPAuthHelper)RuntimeHelpers.GetUninitializedObject(typeof(IAPAuthHelper));
        var options = Options.Create(Settings);
        return new AiRetrieverManager(
            iapHelper,
            options,
            MockConfiguration.Object,
            MockLogger.Object);
    }
}

[CollectionDefinition("PNO914AiRetriever_Collection")]
public class PNO914AiRetrieverCollection { }
