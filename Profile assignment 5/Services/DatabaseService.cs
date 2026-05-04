using SQLite;
using Profile_assignment_5.Model;

namespace Profile_assignment_5.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "shopping.db");
            _database = new SQLiteAsyncConnection(dbPath);

            InitializeDatabaseAsync().Wait();
        }

        private async Task InitializeDatabaseAsync()
        {
            await _database.CreateTableAsync<Profile>();
            await _database.CreateTableAsync<ShoppingItem>();
            await _database.CreateTableAsync<ShoppingCart>();

            await SeedShoppingItemsAsync();
        }

        private async Task SeedShoppingItemsAsync()
        {
            var existingItems = await _database.Table<ShoppingItem>().CountAsync();
            if (existingItems == 0)
            {
                var items = new List<ShoppingItem>
                {
                    new ShoppingItem { Name = "Laptop", Description = "High performance laptop", Price = 999.99m, StockQuantity = 15, ImageUrl = "laptop_icon.png" },
                    new ShoppingItem { Name = "Smartphone", Description = "Latest model smartphone", Price = 699.99m, StockQuantity = 25, ImageUrl = "phone_icon.png" },
                    new ShoppingItem { Name = "Headphones", Description = "Wireless noise-cancelling", Price = 199.99m, StockQuantity = 50, ImageUrl = "headphones_icon.png" },
                    new ShoppingItem { Name = "Tablet", Description = "10-inch display tablet", Price = 449.99m, StockQuantity = 20, ImageUrl = "tablet_icon.png" },
                    new ShoppingItem { Name = "Smartwatch", Description = "Fitness tracking smartwatch", Price = 299.99m, StockQuantity = 30, ImageUrl = "watch_icon.png" },
                    new ShoppingItem { Name = "Keyboard", Description = "Mechanical gaming keyboard", Price = 129.99m, StockQuantity = 40, ImageUrl = "keyboard_icon.png" },
                    new ShoppingItem { Name = "Mouse", Description = "Wireless ergonomic mouse", Price = 49.99m, StockQuantity = 60, ImageUrl = "mouse_icon.png" },
                    new ShoppingItem { Name = "Monitor", Description = "27-inch 4K monitor", Price = 399.99m, StockQuantity = 18, ImageUrl = "monitor_icon.png" }
                };

                await _database.InsertAllAsync(items);
            }
        }

        // Profile Methods
        public async Task<Profile> GetProfileAsync()
        {
            return await _database.Table<Profile>().FirstOrDefaultAsync();
        }

        public async Task<int> SaveProfileAsync(Profile profile)
        {
            if (profile.Id != 0)
            {
                return await _database.UpdateAsync(profile);
            }
            else
            {
                return await _database.InsertAsync(profile);
            }
        }

        // Shopping Item Methods
        public async Task<List<ShoppingItem>> GetShoppingItemsAsync()
        {
            return await _database.Table<ShoppingItem>().ToListAsync();
        }

        public async Task<ShoppingItem> GetShoppingItemAsync(int id)
        {
            return await _database.Table<ShoppingItem>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        // Shopping Cart Methods
        public async Task<List<ShoppingCart>> GetCartItemsAsync(int profileId)
        {
            return await _database.Table<ShoppingCart>().Where(c => c.ProfileId == profileId).ToListAsync();
        }

        public async Task<int> AddToCartAsync(ShoppingCart cartItem)
        {
            var existingItem = await _database.Table<ShoppingCart>()
                .Where(c => c.ProfileId == cartItem.ProfileId && c.ShoppingItemId == cartItem.ShoppingItemId)
                .FirstOrDefaultAsync();

            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
                return await _database.UpdateAsync(existingItem);
            }
            else
            {
                cartItem.AddedDate = DateTime.Now;
                return await _database.InsertAsync(cartItem);
            }
        }

        public async Task<int> RemoveFromCartAsync(int cartId)
        {
            return await _database.DeleteAsync<ShoppingCart>(cartId);
        }

        public async Task<int> UpdateCartQuantityAsync(ShoppingCart cartItem)
        {
            return await _database.UpdateAsync(cartItem);
        }

        public async Task<int> ClearCartAsync(int profileId)
        {
            return await _database.ExecuteAsync("DELETE FROM ShoppingCart WHERE ProfileId = ?", profileId);
        }
    }
}
