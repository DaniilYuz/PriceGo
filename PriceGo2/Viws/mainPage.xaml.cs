using Camera.MAUI;
using Microsoft.ML;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using static PriceGo2.ML_Tech;
using CommunityToolkit;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls.PlatformConfiguration;


namespace PriceGo2.Viws;

public partial class mainPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    private const string ApiUrl = "http://192.168.137.1:88/api/predictions";
    public string filePath;
    public string fileName;

    //byte[] fileBytes;
    public mainPage()
    {
        InitializeComponent();
    }

    public async void UploadImage_Clicked(object sender, EventArgs e)
    {
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

    private void Camera_MediaCaptured(object sender, CommunityToolkit.Maui.Views.MediaCapturedEventArgs e)
    {
        try
        {
            string folderPath = @"/data/user/0/com.companyname.pricego2/cache/";
            string _fileName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string _filePath = Path.Combine(folderPath, _fileName);

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

                using (var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    e.Media.CopyTo(fileStream);
                }
                //await Send_to_API();
                //await DisplayAlert("File Saved", $"File saved at: {filePath}", "OK");
            }
            filePath = _filePath;
            fileName = _fileName;
            //displayLabel_1.Text = "DDADADADADA";

            Test_IMG.Source = _filePath;

            

            
        }
        catch (Exception ex)
        {
            //await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void Take_photo_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Camera.CaptureImage(CancellationToken.None);
            await Task.Delay(6000);

            await DisplayAlert("File path from Take_photo_Clicked", filePath, "OK");
            await Send_to_API();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error!", $"Error: {ex.Message}", "ok");
        }
        
    }
    public async Task Send_to_API()
    {
        try
        {
            mainPage_UploadFile_API Upload = new mainPage_UploadFile_API();

            await DisplayAlert("File path from Send_to_API:", filePath, "ok");
            //using (var fileStream = File.OpenWrite(filePath))
            //{
            //    await e.Media.CopyToAsync(fileStream);
            //}

            using (FileStream stream = File.OpenRead(filePath))
            {
                var result = await Upload.UploadFileAsync(stream, fileName);
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

    private void MY_Account_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync(nameof(ChangeAvatarPage));
    }
}