using Microsoft.Extensions.Options;
using MongoDB.Driver;
using api.Models;

namespace api.Services
{
    public class DatabaseService
    {
        private readonly IMongoCollection<Users> _users;
        private readonly IMongoCollection<Events> _events;

        public DatabaseService(IOptions<MongoDBSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<Users>(settings.UsersCollection);
            _events = database.GetCollection<Events>(settings.EventsCollection);
        }
        //USERS CRUD
        public async Task<List<Users>> GetUsers() =>
            await _users.Find(_ => true).ToListAsync();
        public async Task CreateUsers(Users newUsers)
        {
            newUsers.Password = BCrypt.Net.BCrypt.HashPassword(newUsers.Password);
            await _users.InsertOneAsync(newUsers);
        }

        public async Task UpdateUserAsync(string id, Users updateUser)
        {
            await _users.ReplaceOneAsync(e => e.UserId == id, updateUser);
        }
        public async Task<Users?> GetUserAsync(string id)
        {
            return await _users.Find(e => e.UserId == id).FirstOrDefaultAsync();
        }
        //EVENTS CRUD
        public async Task<List<Events>> GetEvents() =>
            await _events.Find(_ => true).ToListAsync();
        public async Task<List<Events>> GetEventsByUser(string userId)
        {
            return await _events.Find(e => e.UserId == userId).ToListAsync();
        }

        public async Task CreateEvent(Events newEvent)
        {
            await _events.InsertOneAsync(newEvent);
        }
        public async Task UpdateEventAsync(string id, Events updateEvent)
        {
            await _events.ReplaceOneAsync(e => e.EventId == id, updateEvent);
        }
        public async Task<Events?> GetEventAsync(string id)
        {
            return await _events.Find(e => e.EventId == id).FirstOrDefaultAsync();
        }
        public async Task<bool> DeleteEventAsync(string id)
        {
            var result = await _events.DeleteOneAsync(e => e.EventId == id);
            return result.DeletedCount > 0;
        }

    }
}