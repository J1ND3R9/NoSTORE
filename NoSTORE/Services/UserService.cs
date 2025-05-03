using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;
using System.Numerics;
using System.Threading.Tasks;

namespace NoSTORE.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _user;

        public UserService(MongoDbContext dbContext)
        {
            _user = dbContext.GetCollection<User>("users");
        }
        public async Task<List<User>> GetAllAsync() => await _user.Find(_ => true).ToListAsync();
        public async Task<User> GetUserByNicknameAsync(string nick) => await _user.Find(u => u.Nickname == nick).FirstOrDefaultAsync();
        public async Task<User> GetUserByEmailAsync(string email) => await _user.Find(u => u.Email == email).FirstOrDefaultAsync();
        public async Task<User> GetUserByPhoneAsync(string phone) => await _user.Find(u => u.Phone == phone).FirstOrDefaultAsync();

        public async Task<string> GetAvatarById(string id)
        {
            var user = await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
            return user.Avatar;
        }

        public async Task<bool> UserExistsByEmail(string email)
        {
            var exist = await _user.Find(u => u.Email == email).FirstOrDefaultAsync();
            return exist != null;
        }

        public async Task<bool> UserExistsByPhone(string phone)
        {
            var exist = await _user.Find(u => u.Phone == phone).FirstOrDefaultAsync();
            return exist != null;
        }

        public async Task InsertUser(User user)
        {
            if (string.IsNullOrEmpty(user.RoleId))
            {
                user.RoleId = "67e27e725cce8193928f9f28";
            }
            await _user.InsertOneAsync(user);
        }

    }
}
