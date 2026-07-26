namespace KeyValueStore.Application;

[GenerateBinarySerializer]
public sealed partial class UserProfile
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
