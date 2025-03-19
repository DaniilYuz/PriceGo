using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace PriceGo2.Viws
{
    public partial class RegisterPage : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            string email = emailEntry.Text;
            string password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter email and password", "OK");
                return;
            }

            var registerData = new
            {
                Email = email,
                Password = password
            };

            string jsonData = JsonConvert.SerializeObject(registerData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync("http://192.168.137.1:88/api/Auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Registration successful!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Registration Error", errorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Network Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync(".."); 
        }
    }
}