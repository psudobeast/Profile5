using Profile_assignment_5.Model;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Profile_assignment_5.Services
{
    public class SupabaseService
    {
        private readonly HttpClient _httpClient;
        private const string SupabaseUrl = "https://jtiluelbxpzolicbfehr.supabase.co";
        private const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imp0aWx1ZWxieHB6b2xpY2JmZWhyIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Mzk2NjUyODMsImV4cCI6MjA1NTI0MTI4M30.n5hSdQK2GsOBKq3fVsJxZJXwJcn6yO6Xu8gCFRW4IEI";

        public SupabaseService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"{SupabaseUrl}/rest/v1/");
            _httpClient.DefaultRequestHeaders.Add("apikey", SupabaseKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {SupabaseKey}");
            _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
        }

        // Profile Methods
        public async Task<Profile> GetProfileAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("profiles?limit=1");
                if (response.IsSuccessStatusCode)
                {
                    var profiles = await response.Content.ReadFromJsonAsync<List<Profile>>();
                    return profiles?.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting profile: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveProfileAsync(Profile profile)
        {
            try
            {
                var json = JsonSerializer.Serialize(profile);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (profile.Id > 0)
                {
                    // Update existing profile
                    response = await _httpClient.PatchAsync($"profiles?id=eq.{profile.Id}", content);
                }
                else
                {
                    // Insert new profile
                    response = await _httpClient.PostAsync("profiles", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<Profile>>();
                    return result?.FirstOrDefault()?.Id ?? profile.Id;
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to save profile: {error}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving profile: {ex.Message}");
                throw;
            }
        }

        // Shopping Item Methods
        public async Task<List<ShoppingItem>> GetShoppingItemsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("shopping_items");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ShoppingItem>>() ?? new List<ShoppingItem>();
                }
                return new List<ShoppingItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting shopping items: {ex.Message}");
                return new List<ShoppingItem>();
            }
        }

        public async Task<ShoppingItem> GetShoppingItemAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"shopping_items?id=eq.{id}&limit=1");
                if (response.IsSuccessStatusCode)
                {
                    var items = await response.Content.ReadFromJsonAsync<List<ShoppingItem>>();
                    return items?.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting shopping item: {ex.Message}");
                return null;
            }
        }

        // Shopping Cart Methods
        public async Task<List<ShoppingCart>> GetCartItemsAsync(int profileId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"shopping_cart?profile_id=eq.{profileId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ShoppingCart>>() ?? new List<ShoppingCart>();
                }
                return new List<ShoppingCart>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting cart items: {ex.Message}");
                return new List<ShoppingCart>();
            }
        }

        public async Task<int> AddToCartAsync(ShoppingCart cartItem)
        {
            try
            {
                // Check if item already exists in cart
                var existingResponse = await _httpClient.GetAsync($"shopping_cart?profile_id=eq.{cartItem.ProfileId}&shopping_item_id=eq.{cartItem.ShoppingItemId}&limit=1");
                if (existingResponse.IsSuccessStatusCode)
                {
                    var existing = await existingResponse.Content.ReadFromJsonAsync<List<ShoppingCart>>();
                    var existingItem = existing?.FirstOrDefault();

                    if (existingItem != null)
                    {
                        // Update existing item
                        existingItem.Quantity += cartItem.Quantity;
                        var json = JsonSerializer.Serialize(existingItem);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await _httpClient.PatchAsync($"shopping_cart?id=eq.{existingItem.Id}", content);

                        return existingItem.Id;
                    }
                }

                // Insert new item
                cartItem.AddedDate = DateTime.UtcNow;
                var newJson = JsonSerializer.Serialize(cartItem);
                var newContent = new StringContent(newJson, Encoding.UTF8, "application/json");
                var insertResponse = await _httpClient.PostAsync("shopping_cart", newContent);

                if (insertResponse.IsSuccessStatusCode)
                {
                    var result = await insertResponse.Content.ReadFromJsonAsync<List<ShoppingCart>>();
                    return result?.FirstOrDefault()?.Id ?? 0;
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding to cart: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> RemoveFromCartAsync(int cartId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"shopping_cart?id=eq.{cartId}");
                return response.IsSuccessStatusCode ? 1 : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing from cart: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UpdateCartQuantityAsync(ShoppingCart cartItem)
        {
            try
            {
                var json = JsonSerializer.Serialize(cartItem);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"shopping_cart?id=eq.{cartItem.Id}", content);

                return response.IsSuccessStatusCode ? 1 : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating cart quantity: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> ClearCartAsync(int profileId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"shopping_cart?profile_id=eq.{profileId}");
                return response.IsSuccessStatusCode ? 1 : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing cart: {ex.Message}");
                return 0;
            }
        }
    }
}
