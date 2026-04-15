using System.Text.Json;

public class Profile
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string EmailAddress { get; set; }
    public string Bio { get; set; }

    // Optional: Store Profile Picture as Base64 string
    public string ProfilePictureBase64 { get; set; }

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