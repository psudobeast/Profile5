using SQLite;

namespace Profile_assignment_5.Model
{
    public class ShoppingCart
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int ProfileId { get; set; }

        [Indexed]
        public int ShoppingItemId { get; set; }

        public int Quantity { get; set; }

        public DateTime AddedDate { get; set; }
    }
}
