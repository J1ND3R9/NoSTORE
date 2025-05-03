using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;

namespace NoSTORE.Services
{
    public class VerificationService
    {
        private readonly IMongoCollection<Verification> _codes;
        private readonly EmailService _emailService;

        public VerificationService(MongoDbContext dbContext, EmailService emailService)
        {
            _codes = dbContext.GetCollection<Verification>("verification_codes");
            _emailService = emailService;
        }

        public async Task<bool> SendVerificationCodeAsync(string email, CancellationToken ct)
        {
            try
            {
                if (await WasCodeSentRecently(email))
                    return false;
                string code = GenerateRandomCode();
                if (await _emailService.SendCodeAsync(email, code, ct))
                {
                    await SaveCodeAsync(email, code, TimeSpan.FromMinutes(5));
                    return true;
                }
                return false;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Операция отменена пользователем");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task SaveCodeAsync(string email, string code, TimeSpan expirationTime)
        {
            var expiresAt = DateTime.UtcNow.Add(expirationTime);
            var verif = new Verification
            {
                Email = email,
                Code = code,
                ExpiresAt = expiresAt,
                Used = false,
                CreatedAt = DateTime.UtcNow
            };

            await _codes.InsertOneAsync(verif);
        }

        public async Task<bool> IsValidCodeAsync(string email, string code)
        {
            var existingCode = await _codes.Find(c => c.Email == email &&
            c.Code == code &&
            c.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();
            if (existingCode == null)
                return false;
            await _codes.DeleteOneAsync(x => x.Email == email && x.Code == code);
            return true;
        }

        public async Task<bool> WasCodeSentRecently(string email)
        {
            var existingCode = await _codes.Find(c => c.Email == email && c.ExpiresAt > c.CreatedAt).ToListAsync();
            return existingCode.Count > 0;
        }

        public async Task CleanUpOldCodesAsync() =>
            await _codes.DeleteManyAsync(s => s.ExpiresAt < DateTime.UtcNow);
        public string GenerateRandomCode() => (100000 + new Random().Next(900000)).ToString();

    }
}
