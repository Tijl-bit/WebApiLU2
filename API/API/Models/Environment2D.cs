namespace API.Models
{
    public class Environment2D
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OwnerUserId { get; set; }
        public int MaxLength { get; set; }
        public int MaxHeight { get; set; }
    }
}
