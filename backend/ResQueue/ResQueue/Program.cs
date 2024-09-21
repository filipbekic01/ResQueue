using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using MongoDB.Bson;
using ResQueue.Endpoints;
using ResQueue.Features.Broker.AcceptBrokerInvitation;
using ResQueue.Features.Broker.CreateBrokerInvitation;
using ResQueue.Features.Broker.ManageBrokerAccess;
using ResQueue.Features.Broker.SyncBroker;
using ResQueue.Features.Broker.UpdateBroker;
using ResQueue.Features.Messages.ArchiveMessages;
using ResQueue.Features.Messages.CloneMessage;
using ResQueue.Features.Messages.CreateMessage;
using ResQueue.Features.Messages.PublishMessages;
using ResQueue.Features.Messages.ReviewMessages;
using ResQueue.Features.Messages.SyncMessages;
using ResQueue.Features.Messages.UpdateMessage;
using ResQueue.Features.Stripe.CancelSubscription;
using ResQueue.Features.Stripe.ChangeCard;
using ResQueue.Features.Stripe.ChangePlan;
using ResQueue.Features.Stripe.ContinueSubscription;
using ResQueue.Features.Stripe.CreateSubscription;
using ResQueue.Features.Stripe.EventHandler;
using ResQueue.Models;

namespace ResQueue;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var settingsSection = builder.Configuration.GetRequiredSection("Settings");
        builder.Services.Configure<Settings>(settingsSection);
        var settings = settingsSection.Get<Settings>()!;

        builder.Services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("http://localhost:5173");
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowCredentials();
            });
        });

        builder.Services.AddHttpClient();

        builder.Services.AddIdentityMongoDbProvider<User, Role, ObjectId>(opt =>
                {
                    opt.Password.RequiredLength = 4;
                    opt.Password.RequireDigit = false;
                    opt.Password.RequireLowercase = false;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequireUppercase = false;

                    opt.User.RequireUniqueEmail = true;
                },
                mongoOptions => { mongoOptions.ConnectionString = $"{settings.MongoDBConnectionString}"; })
            .AddUserManager<ResqueueUserManager>();

        builder.Services.AddMongoDb(settings);

        builder.Services.AddSingleton<IEmailSender<User>, DummyEmailSender>();

        builder.Services.AddTransient<ISyncBrokerFeature, SyncBrokerFeature>();
        builder.Services.AddTransient<IUpdateBrokerFeature, UpdateBrokerFeature>();
        builder.Services.AddTransient<IManageBrokerAccessFeature, ManageBrokerAccessFeature>();
        builder.Services.AddTransient<ICreateBrokerInvitationFeature, CreateBrokerInvitationFeature>();
        builder.Services.AddTransient<IAcceptBrokerInvitationFeature, AcceptBrokerInvitationFeature>();

        builder.Services.AddTransient<ISyncMessagesFeature, SyncMessagesFeature>();
        builder.Services.AddTransient<ICreateMessageFeature, CreateMessageFeature>();
        builder.Services.AddTransient<IPublishMessagesFeature, PublishMessagesFeature>();
        builder.Services.AddTransient<IArchiveMessagesFeature, ArchiveMessagesFeature>();
        builder.Services.AddTransient<ICloneMessageFeature, CloneMessageFeature>();
        builder.Services.AddTransient<IUpdateMessageFeature, UpdateMessageFeature>();
        builder.Services.AddTransient<IReviewMessagesFeature, ReviewMessagesFeature>();

        builder.Services.AddTransient<ICreateSubscriptionFeature, CreateSubscriptionFeature>();
        builder.Services.AddTransient<ICancelSubscriptionFeature, CancelSubscriptionFeature>();
        builder.Services.AddTransient<IContinueSubscriptionFeature, ContinueSubscriptionFeature>();
        builder.Services.AddTransient<IChangePlanFeature, ChangePlanFeature>();
        builder.Services.AddTransient<IChangeCardFeature, ChangeCardFeature>();
        builder.Services.AddTransient<IEventHandlerFeature, EventHandlerFeature>();

        builder.Services.ConfigureApplicationCookie(options => { options.ExpireTimeSpan = TimeSpan.FromDays(30); });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseCors("AllowAll");

        string[] frontendRoutes =
        [
            "^login",
            "^forgot-password$",
            "^support$",
            "^pricing$",
            "^app",
        ];
        app.UseRewriter(frontendRoutes.Aggregate(
            new RewriteOptions(),
            (options, route) => options.AddRewrite(route, "/index.html", true))
        );
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions()
        {
            OnPrepareResponse = (context) =>
            {
                context.Context.Response.Headers.CacheControl =
                    context.Context.Request.Path.StartsWithSegments("/assets/")
                        ? "public, max-age=31536000, immutable"
                        : "no-cache, no-store";
            }
        });

        app.UseAuthentication();
        app.UseAuthorization();

        var apiGroup = app.MapGroup("api");
        apiGroup.MapIdentityApi<User>();
        apiGroup.MapAuthEndpoints();
        apiGroup.MapBrokerEndpoints();
        apiGroup.MapQueueEndpoints();
        apiGroup.MapExchangeEndpoints();
        apiGroup.MapMessageEndpoints();
        apiGroup.MapStripeEndpoints();
        apiGroup.MapUserEndpoints();

        app.Run();
    }
}