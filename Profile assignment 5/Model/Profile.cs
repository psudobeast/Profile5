using System.Text.Json;
using System.Text.Json.Serialization;
using SQLite;

public class Profile
{
    [PrimaryKey, AutoIncrement]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("surname")]
    public string Surname { get; set; }

    [JsonPropertyName("email_address")]
    public string EmailAddress { get; set; }

    [JsonPropertyName("bio")]
    public string Bio { get; set; }

    [JsonPropertyName("profile_picture_base64")]
    public string ProfilePictureBase64 { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Method to serialize Profile object to JSON string
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    // Static method to deserialize JSON string to Profile object
    public static Profile FromJson(string json)
    {
        return JsonSerializer.Deserialize<Profile>(json);
    }

}
