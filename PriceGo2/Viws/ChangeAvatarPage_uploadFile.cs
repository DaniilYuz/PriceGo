using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGo2.Viws
{
    internal class ChangeAvatarPage_uploadFile
    {
        private readonly HttpClient _httpClient;

        public ChangeAvatarPage_uploadFile()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, int userId)
        {
            try
            {
                var httpContent = new MultipartFormDataContent();
                var content = new StreamContent(fileStream);

                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                httpContent.Add(content, "file", fileName);
                httpContent.Add(new StringContent(userId.ToString()), "userId");

                var result = await _httpClient.PostAsync("http://192.168.137.1:88/api/Auth/ChangeAvatar", httpContent);

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
