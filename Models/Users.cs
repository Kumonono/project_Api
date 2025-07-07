using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace api.Models
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }
        public string Name { get; set; } = null!;

        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ResetCode { get; set; }
        public DateTime? ResetCodeExpiration { get; set; }
    }
}