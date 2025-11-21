using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.AiGateway.Aggregates.LanguageModel;

public class LanguageModel : IAggregateRoot
{
    protected LanguageModel()
    {
    }

    public LanguageModel(string name, string provider, string baseUrl, string? apiKey, ModelParameters parameters)
    {
        Id = Guid.NewGuid();
        Name = name;
        Provider = provider;
        BaseUrl = baseUrl;
        ApiKey = apiKey;
        Parameters = parameters;
    }

    public Guid Id { get; set; }

    public string Provider { get; set; }

    public string Name { get; set; }

    public string BaseUrl { get; set; }

    public string? ApiKey { get; set; }

    public ModelParameters Parameters { get; set; }

    public void UpdateParameters(ModelParameters parameters)
    {
        Parameters = parameters;
    }
}