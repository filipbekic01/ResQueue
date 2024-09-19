using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ResQueue.Models;

namespace ResQueue;

public class ResqueueUserManager : UserManager<User>
{
    private readonly IMongoCollection<User> _usersCollection;

    public ResqueueUserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators,
        IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger,
        IMongoCollection<User> usersCollection) : base(store,
        optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _usersCollection = usersCollection;
    }

    public async Task<User?> FirstOrDefaultByStripeId(string stripeId)
    {
        return await _usersCollection.Find(u => u.StripeId == stripeId).FirstOrDefaultAsync();
    }
}