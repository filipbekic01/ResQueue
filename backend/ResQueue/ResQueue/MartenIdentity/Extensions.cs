using Marten;
using Microsoft.AspNetCore.Identity;

namespace ResQueue.MartenIdentity;

public static class Extensions
{
    /// <summary>
    /// Adds the stores to the service container, plus a IDocumentStore as a singleton, using the provided connection string.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <param name="builder"></param>
    /// <param name="postgresConnectionString"></param>
    /// <returns></returns>
    public static IdentityBuilder AddMartenStores<TUser, TRole>(this IdentityBuilder builder,
        string postgresConnectionString)
        where TUser : IdentityUser, IClaimsUser
        where TRole : IdentityRole
    {
        IDocumentStore documentStore = CreateDocumentStore(postgresConnectionString);

        builder = builder
            .AddRoleStore<MartenRoleStore<TRole>>()
            .AddRoleManager<RoleManager<TRole>>()
            .AddUserStore<MartenUserStore<TUser>>()
            .AddUserManager<UserManager<TUser>>();

        builder.Services.AddSingleton<IDocumentStore>(documentStore);

        return builder;
    }

    public static IdentityBuilder AddMartenStores<TUser, TRole>(this IdentityBuilder builder)
        where TUser : IdentityUser, IClaimsUser
        where TRole : IdentityRole
    {
        return builder
            .AddRoleStore<MartenRoleStore<TRole>>()
            .AddRoleManager<RoleManager<TRole>>()
            .AddUserStore<MartenUserStore<TUser>>()
            .AddUserManager<UserManager<TUser>>();
    }

    public static IDocumentStore CreateDocumentStore(string connectionString,
        string databaseSchemaName = "aspnetidentity",
        string databaseOwner = "aspnetidentity", int connectionLimit = -1)
    {
        var documentStore = DocumentStore.For(options =>
        {
            options.CreateDatabasesForTenants(c =>
            {
                c.ForTenant()
                    .CheckAgainstPgDatabase()
                    .WithOwner(databaseOwner)
                    .WithEncoding("UTF-8")
                    .ConnectionLimit(connectionLimit)
                    .OnDatabaseCreated(_ => { Console.WriteLine($"Postgres '{_.Database}' database created"); });
            });

            options.DatabaseSchemaName = databaseSchemaName;
            options.Connection(connectionString);
            options.Schema.For<IdentityRole>().Index(x => x.Id);
            options.Schema.For<IdentityUser>().Index(x => x.Id);
        });

        return documentStore;
    }
}