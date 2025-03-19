using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PriceGo2.Viws
{
    public class mainPage_UploadFile_API
    {
        private readonly HttpClient _httpClient;

        public mainPage_UploadFile_API()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                var httpContent = new MultipartFormDataContent();
                var content = new StreamContent(fileStream);

                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                httpContent.Add(content, "file", fileName);

                var result = await _httpClient.PostAsync("http://192.168.137.1:88/api/UploadFile", httpContent);

                if (!result.IsSuccessStatusCode)
                {
                    return $"Error: return from server: {result.StatusCode}";
                }

                var response = await result.Content.ReadAsStringAsync();
                await Shell.Current.DisplayAlert("Response From Server (UploadFileAsync)", response, "Ok");

                return response;
            }
            catch (Exception ex)
            {
                return $"Error API: {ex.Message}";
            }
        }
    }
}
