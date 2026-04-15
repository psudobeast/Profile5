using Microsoft.Maui.ApplicationModel; // For permissions
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Threading; // <- added to use CancellationToken

namespace Profile_assignment_5.View
{
    public partial class ProfilePage : ContentPage
    {
        private readonly string filePath;

        public ProfilePage()
        {
            InitializeComponent();

            // Define the file path for storing profile data
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "profile.json");

            // Load profile data when page appears
            LoadProfileAsync();
        }

        // Method to load profile data from local storage
        private async void LoadProfileAsync()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    Profile profile = Profile.FromJson(json);

                    // Pre-populate fields
                    NameEntry.Text = profile.Name;
                    SurnameEntry.Text = profile.Surname;
                    EmailEntry.Text = profile.EmailAddress;
                    BioEditor.Text = profile.Bio;

                    // Load profile picture if stored (optional)
                    if (profile.ProfilePictureBase64 != null)
                    {
                        var imageBytes = Convert.FromBase64String(profile.ProfilePictureBase64);
                        ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
                }
            }
        }

        // Save profile data
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            string profilePictureBase64 = null;

            // Save profile picture as Base64 (if changed)
            if (ProfileImage.Source is StreamImageSource streamSource && streamSource.Stream != null)
            {
                using var stream = await streamSource.Stream(CancellationToken.None);
                if (stream != null)
                {
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    profilePictureBase64 = Convert.ToBase64String(memoryStream.ToArray());
                }
            }

            var profile = new Profile
            {
                Name = NameEntry.Text,
                Surname = SurnameEntry.Text,
                EmailAddress = EmailEntry.Text,
                Bio = BioEditor.Text,
                ProfilePictureBase64 = profilePictureBase64
            };

            string json = profile.ToJson();
            try
            {
                await File.WriteAllTextAsync(filePath, json);
                await DisplayAlert("Success", "Profile saved successfully.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save profile: {ex.Message}", "OK");
            }
        }

        // Change picture button handler
        private async void OnChangePictureClicked(object sender, EventArgs e)
        {
            // Request permission
            var status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Cannot access media gallery without permission.", "OK");
                return;
            }

            // Pick photo
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select Profile Picture"
            });

            if (result != null)
            {
                try
                {
                    // Open the picked file as a stream and load into memory so we can persist it later
                    using var pickedStream = await result.OpenReadAsync();
                    if (pickedStream != null)
                    {
                        var ms = new MemoryStream();
                        await pickedStream.CopyToAsync(ms);
                        var bytes = ms.ToArray();

                        // Use a stream-backed ImageSource so OnSaveClicked can read the stream and convert to Base64
                        ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to load image: {ex.Message}", "OK");
                }
            }
        }
    }
}