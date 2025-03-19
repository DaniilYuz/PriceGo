using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PriceGo2.Viws
{
    public class predictions_web_API
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "http://192.168.137.1:88/api/predictions";

        public predictions_web_API(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();  
        }

        public async Task<string> Predictions()
        {
            try
            {
                int timeoutMilliseconds = 30000;
                int checkIntervalMilliseconds = 500;
                int elapsedTime = 0;

                while (elapsedTime < timeoutMilliseconds)
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(ApiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var predictions = JsonSerializer.Deserialize<Dictionary<string, float>>(json);

                        if (predictions == null || predictions.Count == 0)
                        {
                            return "No data";
                        }

                        string message = string.Join("\n", predictions
                            .Where(kvp => kvp.Value > Convert.ToSingle(0.4))
                            .Select(kvp => $"The object on photo is {kvp.Key} with a {kvp.Value * 100} chance"));

                        await Shell.Current.DisplayAlert("Response From Server", message, "Ok");

                        return "Prediction displayed successfully.";
                    }
                    else
                    {
                        await Task.Delay(checkIntervalMilliseconds);
                        elapsedTime += checkIntervalMilliseconds;
                    }
                }

                return "Timeout: POST request has not completed yet.";
            }
            catch (Exception ex)
            {
                return $"Error API 2: {ex.Message}";
            }
        }
    }
}