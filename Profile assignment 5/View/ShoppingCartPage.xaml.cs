using Profile_assignment_5.Model;
using Profile_assignment_5.Services;

namespace Profile_assignment_5.View
{
    public class CartItemViewModel
    {
        public ShoppingCart Cart { get; set; }
        public ShoppingItem Item { get; set; }
    }

    public partial class ShoppingCartPage : ContentPage
    {
        private readonly SupabaseService _supabaseService;
        private Profile _currentProfile;

        public ShoppingCartPage(SupabaseService supabaseService)
        {
            InitializeComponent();
            _supabaseService = supabaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCartAsync();
        }

        private async Task LoadCartAsync()
        {
            _currentProfile = await _supabaseService.GetProfileAsync();

            if (_currentProfile == null)
            {
                await DisplayAlert("Profile Required", "Please create your profile first.", "OK");
                await Shell.Current.GoToAsync("//ProfilePage");
                return;
            }

            var cartItems = await _supabaseService.GetCartItemsAsync(_currentProfile.Id);
            var viewModels = new List<CartItemViewModel>();

            foreach (var cart in cartItems)
            {
                var item = await _supabaseService.GetShoppingItemAsync(cart.ShoppingItemId);
                if (item != null)
                {
                    viewModels.Add(new CartItemViewModel
                    {
                        Cart = cart,
                        Item = item
                    });
                }
            }

            CartItemsCollection.ItemsSource = viewModels;
            UpdateTotal(viewModels);
        }

        private void UpdateTotal(List<CartItemViewModel> items)
        {
            var total = items.Sum(vm => vm.Item.Price * vm.Cart.Quantity);
            TotalLabel.Text = $"${total:F2}";
        }

        private async void OnIncreaseQuantityClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var viewModel = button?.CommandParameter as CartItemViewModel;

            if (viewModel == null) return;

            if (viewModel.Cart.Quantity >= viewModel.Item.StockQuantity)
            {
                await DisplayAlert("Stock Limit", $"Cannot add more. Only {viewModel.Item.StockQuantity} available in stock.", "OK");
                return;
            }

            viewModel.Cart.Quantity++;
            await _supabaseService.UpdateCartQuantityAsync(viewModel.Cart);
            await LoadCartAsync();
        }

        private async void OnDecreaseQuantityClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var viewModel = button?.CommandParameter as CartItemViewModel;

            if (viewModel == null) return;

            if (viewModel.Cart.Quantity > 1)
            {
                viewModel.Cart.Quantity--;
                await _supabaseService.UpdateCartQuantityAsync(viewModel.Cart);
                await LoadCartAsync();
            }
            else
            {
                bool confirm = await DisplayAlert("Remove Item", $"Remove {viewModel.Item.Name} from cart?", "Yes", "No");
                if (confirm)
                {
                    await _supabaseService.RemoveFromCartAsync(viewModel.Cart.Id);
                    await LoadCartAsync();
                }
            }
        }

        private async void OnRemoveItemClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var viewModel = button?.CommandParameter as CartItemViewModel;

            if (viewModel == null) return;

            bool confirm = await DisplayAlert("Remove Item", $"Remove {viewModel.Item.Name} from cart?", "Yes", "No");
            if (confirm)
            {
                await _supabaseService.RemoveFromCartAsync(viewModel.Cart.Id);
                await LoadCartAsync();
            }
        }

        private async void OnClearCartClicked(object sender, EventArgs e)
        {
            if (_currentProfile == null) return;

            bool confirm = await DisplayAlert("Clear Cart", "Remove all items from your cart?", "Yes", "No");
            if (confirm)
            {
                await _supabaseService.ClearCartAsync(_currentProfile.Id);
                await LoadCartAsync();
            }
        }

        private async void OnBackToShoppingClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ShoppingListPage");
        }
    }
}
