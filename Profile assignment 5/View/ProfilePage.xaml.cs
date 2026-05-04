using Profile_assignment_5.Services;

namespace Profile_assignment_5.View
{
    public partial class ProfilePage : ContentPage
    {
        private readonly SupabaseService _supabaseService;
        private Profile _currentProfile;

        public ProfilePage(SupabaseService supabaseService)
        {
            InitializeComponent();
            _supabaseService = supabaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadProfileAsync();
        }

        // Method to load profile data from database
        private async Task LoadProfileAsync()
        {
            _currentProfile = await _supabaseService.GetProfileAsync();

            if (_currentProfile != null)
            {
                // Pre-populate fields
                NameEntry.Text = _currentProfile.Name;
                SurnameEntry.Text = _currentProfile.Surname;
                EmailEntry.Text = _currentProfile.EmailAddress;
                BioEditor.Text = _currentProfile.Bio;

                // Load profile picture if stored
                if (!string.IsNullOrEmpty(_currentProfile.ProfilePictureBase64))
                {
                    try
                    {
                        var imageBytes = Convert.FromBase64String(_currentProfile.ProfilePictureBase64);
                        ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Failed to load profile picture: {ex.Message}", "OK");
                    }
                }
            }
        }

        // Save profile data
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            string profilePictureBase64 = _currentProfile?.ProfilePictureBase64;

            // Save profile picture as Base64 (if changed)
            if (ProfileImage.Source is StreamImageSource streamSource && streamSource.Stream != null)
            {
                try
                {
                    using var stream = await streamSource.Stream(CancellationToken.None);
                    if (stream != null)
                    {
                        var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        profilePictureBase64 = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
                catch { }
            }

            var profile = new Profile
            {
                Id = _currentProfile?.Id ?? 0,
                Name = NameEntry.Text,
                Surname = SurnameEntry.Text,
                EmailAddress = EmailEntry.Text,
                Bio = BioEditor.Text,
                ProfilePictureBase64 = profilePictureBase64
            };

            try
            {
                await _supabaseService.SaveProfileAsync(profile);
                await DisplayAlert("Success", "Profile saved successfully to Supabase!", "OK");
                await LoadProfileAsync();
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

        // Navigate to Shopping List
        private async void OnGoToShoppingClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ShoppingListPage");
        }
    }
}
