using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace api.Models
{
    public class Events
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? EventId { get; set; }

        public string Title { get; set; } = null!;

        public DateTime Date { get; set; }

        public string? Description { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;
    }
}