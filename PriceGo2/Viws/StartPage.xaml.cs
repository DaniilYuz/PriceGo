using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace PriceGo2.Viws
{
    public partial class StartPage : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public StartPage()
        {
            InitializeComponent();
        }

        private async void btnLogin_Clicked(object sender, EventArgs e)
        {
            string email = emailEntry.Text;
            string password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter email and password", "OK");
                return;
            }

            var loginData = new
            {
                Email = email,
                Password = password
            };

            string jsonData = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync("http://192.168.137.1:88/api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var userData = JsonConvert.DeserializeObject<UserModel>(responseBody);

                    // Save user data in Preferences
                    Preferences.Set("UserId", userData.UserId);
                    Preferences.Set("Email", userData.Email);
                    Preferences.Set("PhotoUrl", userData.PhotoUrl ?? "avatar_placeholder.png");

                    await DisplayAlert("Success", "Login successful!", "OK");

                    // Navigate to main page
                    await Shell.Current.GoToAsync(nameof(mainPage));
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Login Error", errorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Network error: {ex.Message}", "OK");
            }
        }

        private async void btnRegister_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }

    public class UserModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
    }
}
