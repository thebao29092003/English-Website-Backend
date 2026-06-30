using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace English.Website.Api.Extensions.Helpers
{
    public static class HttpHelper
    {
        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // Hàm phụ trợ xử lý nạp Headers và Authorization
        private static void ApplyHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
        {
            if (headers == null) return;

            foreach (var header in headers)
            {
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = header.Value.Split(' ');
                    if (parts.Length == 2)
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue(parts[0], parts[1]);
                    }
                    else
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue(header.Value);
                    }
                }
                else
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        /// <summary>
        /// Gửi một request GET và tự động deserialize kết quả trả về kiểu TResponse.
        /// </summary>
        public static async Task<TResponse> SendGetAsync<TResponse>(
            HttpClient httpClient,
            string url,
            Dictionary<string, string>? headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            ApplyHeaders(request, headers);

            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException($"GET API request failed with status code {response.StatusCode} on URL {url}. Content: {content}");
            }

            var result = JsonSerializer.Deserialize<TResponse>(content, DefaultJsonOptions);
            if (result == null)
            {
                throw new BadRequestException($"Failed to deserialize GET API response from {url}.");
            }

            return result;
        }

        /// <summary>
        /// Gửi một request POST với payload JSON và tự động deserialize kết quả trả về kiểu TResponse.
        /// </summary>
        public static async Task<TResponse> SendPostJsonAsync<TRequest, TResponse>(
            HttpClient httpClient,
            string url,
            TRequest requestBody,
            Dictionary<string, string>? headers = null)
        {
            var jsonPayload = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            ApplyHeaders(request, headers);

            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("content: ", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException($"POST API request failed with status code {response.StatusCode} on URL {url}. Content: {content}");
            }

            var result = JsonSerializer.Deserialize<TResponse>(content, DefaultJsonOptions);
            if (result == null)
            {
                throw new BadRequestException($"Failed to deserialize POST API response from {url}.");
            }

            return result;
        }
    }
}
