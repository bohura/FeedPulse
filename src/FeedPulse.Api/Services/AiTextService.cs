using FeedPulse.Api.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FeedPulse.Api.Services
{
    public class AiTextService : IAiTextService
    {
        private readonly HttpClient httpClient;

        public AiTextService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public Task<string> GenerateTextAsync(
            AiTextRequest request,
            CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);

            return request.Profile.Provider switch
            {
                AiProvider.OpenAiCompatible => GenerateWithOpenAiCompatibleAsync(request, cancellationToken),
                AiProvider.OpenAi => GenerateWithOpenAiCompatibleAsync(request, cancellationToken),
                AiProvider.DeepSeek => GenerateWithOpenAiCompatibleAsync(request, cancellationToken),
                _ => throw new NotSupportedException(
                    $"AI provider '{request.Profile.Provider}' is not supported yet.")
            };
        }

        private static void ValidateRequest(AiTextRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Profile.BaseUrl))
            {
                throw new InvalidOperationException("AI base URL is missing.");
            }

            if (request.Profile.Provider != AiProvider.Ollama &&
                string.IsNullOrWhiteSpace(request.Profile.ApiKey))
            {
                throw new InvalidOperationException("AI API key is missing.");
            }
            if (request.Profile is null)
            {
                throw new ArgumentNullException(nameof(request.Profile));
            }

            if (!request.Profile.IsEnabled)
            {
                throw new InvalidOperationException("AI profile is disabled.");
            }

            if (string.IsNullOrWhiteSpace(request.Profile.Model))
            {
                throw new InvalidOperationException("AI model is missing.");
            }

            if (string.IsNullOrWhiteSpace(request.InputText))
            {
                throw new InvalidOperationException("Input text is empty.");
            }

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                throw new InvalidOperationException("Prompt is empty.");
            }

            if (string.IsNullOrWhiteSpace(request.TargetLanguage))
            {
                throw new InvalidOperationException("Target language is empty.");
            }
        }

        private async Task<string> GenerateWithOpenAiCompatibleAsync(
    AiTextRequest request,
    CancellationToken cancellationToken)
        {
            var endpoint = BuildChatCompletionsUrl(request.Profile);

            var systemPrompt = BuildSystemPrompt(request);

            var payload = new
            {
                model = request.Profile.Model,
                temperature = 0.2,
                messages = new object[]
                {
            new
            {
                role = "system",
                content = systemPrompt
            },
            new
            {
                role = "user",
                content = request.InputText
            }
                }
            };

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);

            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", request.Profile.ApiKey.Trim());

            requestMessage.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await httpClient.SendAsync(requestMessage, cancellationToken);

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"AI request failed with status code {(int)response.StatusCode}: {responseText}");
            }

            using var document = JsonDocument.Parse(responseText);

            if (!document.RootElement.TryGetProperty("choices", out var choices))
            {
                throw new InvalidOperationException("AI response does not contain choices.");
            }

            var firstChoice = choices.EnumerateArray().FirstOrDefault();

            if (firstChoice.ValueKind == JsonValueKind.Undefined)
            {
                throw new InvalidOperationException("AI response choices is empty.");
            }

            if (!firstChoice.TryGetProperty("message", out var message))
            {
                throw new InvalidOperationException("AI response does not contain message.");
            }

            if (!message.TryGetProperty("content", out var contentElement))
            {
                throw new InvalidOperationException("AI response does not contain content.");
            }

            var content = contentElement.GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("AI response content is empty.");
            }

            return content.Trim();
        }

        private static string BuildChatCompletionsUrl(AiProfile profile)
        {
            var baseUrl = profile.BaseUrl.Trim();

            if (baseUrl.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            {
                return baseUrl;
            }

            return $"{baseUrl.TrimEnd('/')}/chat/completions";
        }

        private static string BuildSystemPrompt(AiTextRequest request)
        {
            return
                $"{request.Prompt.Trim()}\n\n" +
                $"Target language: {request.TargetLanguage.Trim()}.";
        }
    }
}