namespace api.Models
{
    public class MfaRequest
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}