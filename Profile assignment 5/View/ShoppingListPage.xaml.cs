using Profile_assignment_5.Model;
using Profile_assignment_5.Services;

namespace Profile_assignment_5.View
{
    public partial class ShoppingListPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Profile _currentProfile;

        public ShoppingListPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            _currentProfile = await _databaseService.GetProfileAsync();

            if (_currentProfile == null)
            {
                await DisplayAlert("Profile Required", "Please create your profile first.", "OK");
                await Shell.Current.GoToAsync("//ProfilePage");
                return;
            }

            var items = await _databaseService.GetShoppingItemsAsync();
            ShoppingItemsCollection.ItemsSource = items;
        }

        private async void OnAddToCartClicked(object sender, EventArgs e)
        {
            if (_currentProfile == null)
            {
                await DisplayAlert("Error", "Profile not found. Please create a profile.", "OK");
                return;
            }

            var button = sender as Button;
            var item = button?.CommandParameter as ShoppingItem;

            if (item == null) return;

            if (item.StockQuantity <= 0)
            {
                await DisplayAlert("Out of Stock", $"{item.Name} is currently out of stock.", "OK");
                return;
            }

            var cartItems = await _databaseService.GetCartItemsAsync(_currentProfile.Id);
            var existingCartItem = cartItems.FirstOrDefault(c => c.ShoppingItemId == item.Id);

            int currentCartQuantity = existingCartItem?.Quantity ?? 0;

            if (currentCartQuantity >= item.StockQuantity)
            {
                await DisplayAlert("Stock Limit", $"Cannot add more {item.Name}. Stock limit reached.", "OK");
                return;
            }

            var cartItem = new ShoppingCart
            {
                ProfileId = _currentProfile.Id,
                ShoppingItemId = item.Id,
                Quantity = 1
            };

            await _databaseService.AddToCartAsync(cartItem);
            await DisplayAlert("Success", $"{item.Name} added to cart!", "OK");
        }

        private async void OnViewCartClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ShoppingCartPage");
        }

        private async void OnGoToProfileClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ProfilePage");
        }
    }
}
