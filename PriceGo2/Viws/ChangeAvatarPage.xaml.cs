using Microsoft.Maui.Storage;

namespace PriceGo2.Viws;

public partial class ChangeAvatarPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    private const string ApiUrl = "http://192.168.137.1:88/api/Auth/";
    public string filePath;
    public string fileName;
    public int userId;

    public ChangeAvatarPage()
    {
        InitializeComponent();
        LoadUserDataAsync();
    }

    // Method to load user data from the API
    private async void LoadUserDataAsync()
    {
        try
        {
            // Retrieve UserId from local storage
            userId = Preferences.Get("UserId", 0);
            if (userId == 0)
            {
                await DisplayAlert("Error", "UserId not found. Please log in again.", "OK");
                return;
            }

            // Send a request to the API to fetch user data
            var response = await _httpClient.GetAsync($"{ApiUrl}getUserProfile?userId={userId}");

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", "Failed to fetch user data.", "OK");
                return;
            }

            // Parse the response
            var jsonData = await response.Content.ReadAsStringAsync();
            var userProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<UserProfile>(jsonData);

            // Update UI elements with user data
            emailLabel.Text = userProfile.Email;
            IdUserLabel.Text = $"ID: {userId}";
            //avatarImage.Source = userProfile.PhotoUrl;
            avatarImage.Source = $"{ApiUrl.Replace("api/Auth/", "")}{userProfile.PhotoUrl}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    // Method triggered when the user clicks the "Change Avatar" button
    private async void OnChangeAvatarClicked(object sender, EventArgs e)
    {
        //try
        //{
        //    // Allow the user to pick a photo
        //    var uploadFile = await MediaPicker.PickPhotoAsync();

        //    if (uploadFile == null)
        //    {
        //        await DisplayAlert("No File", "No photo selected.", "OK");
        //        return;
        //    }

        //    // Save the selected photo to a cache directory
        //    var fileName = uploadFile.FileName;
        //    var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

        //    using (var inputStream = await uploadFile.OpenReadAsync())
        //    using (var outputStream = File.Create(filePath))
        //    {
        //        await inputStream.CopyToAsync(outputStream);
        //    }

        //    // Send the photo to the API
        //    using (var fileStream = File.OpenRead(filePath))
        //    {
        //        var content = new MultipartFormDataContent();
        //        content.Add(new StreamContent(fileStream), "file", fileName);
        //        content.Add(new StringContent(Preferences.Get("UserId", 0).ToString()), "userId");

        //        var response = await _httpClient.PostAsync($"http://192.168.137.1:88/api/Auth/ChangeAvatar", content);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            await DisplayAlert("Success", "Avatar changed successfully!", "OK");

        //            // Reload user data to update UI
        //            LoadUserDataAsync();
        //        }
        //        else
        //        {
        //            await DisplayAlert("Error", "Failed to update avatar.", "OK");
        //        }
        //    }
        //}
        //catch (Exception ex)
        //{
        //    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        //}

        try
        {

            var uploadFile = await MediaPicker.PickPhotoAsync();

            if (uploadFile == null)
            {
                await DisplayAlert("No File", "No photo selected.", "OK");
                return;
            }

            fileName = uploadFile.FileName;
            filePath = Path.Combine(FileSystem.CacheDirectory, fileName);


            if (!Directory.Exists(FileSystem.CacheDirectory))
            {
                await DisplayAlert("Error", "Cache directory does not exist.", "OK");
                return;
            }

            using (var inputStream = await uploadFile.OpenReadAsync())
            using (var outputStream = File.Create(filePath))
            {
                await inputStream.CopyToAsync(outputStream);
            }


            if (!File.Exists(filePath))
            {
                await DisplayAlert("Error", $"File does not exist at {filePath}.", "OK");
                return;
            }


            var fileStream = await uploadFile.OpenReadAsync();


            if (fileStream == null)
            {
                await DisplayAlert("Error", "Failed to open the file stream.", "OK");
                return;
            }


            if (fileStream.Length == 0)
            {
                await DisplayAlert("Error", "File stream is empty.", "OK");
                return;
            }

            var mediaArgs = new CommunityToolkit.Maui.Views.MediaCapturedEventArgs(fileStream);

            if (mediaArgs == null)
            {
                await DisplayAlert("Error", "Failed to create MediaCapturedEventArgs.", "OK");
                return;
            }

            await Send_to_API();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}\nPath: {filePath}", "OK");
        }


    }

    public async Task Send_to_API()
    {
        try
        {
            ChangeAvatarPage_uploadFile Upload = new ChangeAvatarPage_uploadFile();

            await DisplayAlert("File path from Send_to_API:", filePath, "ok");
            //using (var fileStream = File.OpenWrite(filePath))
            //{
            //    await e.Media.CopyToAsync(fileStream);
            //}

            using (FileStream stream = File.OpenRead(filePath))
            {
                var result = await Upload.UploadFileAsync(stream, fileName, userId);
                await DisplayAlert("Result: ", result, "ok");
            }


            predictions_web_API _prediction = new predictions_web_API();
            string predictionResult = await _prediction.Predictions();
            await DisplayAlert("Predict!", predictionResult, "OK");
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error: {ex.Message}";


            if (ex.InnerException != null)
            {
                errorMessage += $"\nNested  error: {ex.InnerException.Message}";
            }

            await DisplayAlert("Error", errorMessage, "OK");
        }
    }

    // Class to deserialize user profile data from the API
    public class UserProfile
    {
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
    }

    private void Change_pass_Clicked(object sender, EventArgs e)
    {

    }
}
