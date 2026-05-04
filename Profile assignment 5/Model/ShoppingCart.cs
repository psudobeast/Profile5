using SQLite;
using System.Text.Json.Serialization;

namespace Profile_assignment_5.Model
{
    public class ShoppingCart
    {
        [PrimaryKey, AutoIncrement]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Indexed]
        [JsonPropertyName("profile_id")]
        public int ProfileId { get; set; }

        [Indexed]
        [JsonPropertyName("shopping_item_id")]
        public int ShoppingItemId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("added_date")]
        public DateTime AddedDate { get; set; }
    }
}
