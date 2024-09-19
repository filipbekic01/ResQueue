using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using Resqueue.Dtos;
using Resqueue.Endpoints;
using Resqueue.Features.Broker.AcceptBrokerInvitation;
using Resqueue.Features.Broker.CreateBrokerInvitation;
using Resqueue.Features.Broker.ManageBrokerAccess;
using Resqueue.Features.Broker.SyncBroker;
using Resqueue.Features.Broker.UpdateBroker;
using Resqueue.Features.Messages.ArchiveMessages;
using Resqueue.Features.Messages.CloneMessage;
using Resqueue.Features.Messages.CreateMessage;
using Resqueue.Features.Messages.PublishMessages;
using Resqueue.Features.Messages.ReviewMessages;
using Resqueue.Features.Messages.SyncMessages;
using Resqueue.Features.Messages.UpdateMessage;
using Resqueue.Features.Stripe.CancelSubscription;
using Resqueue.Features.Stripe.ChangeCard;
using Resqueue.Features.Stripe.ChangePlan;
using Resqueue.Features.Stripe.ContinueSubscription;
using Resqueue.Features.Stripe.CreateSubscription;
using Resqueue.Features.Stripe.EventHandler;
using Resqueue.Models;

namespace Resqueue;

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
                mongoOptions => { mongoOptions.ConnectionString = $"{settings.MongoDBConnectionString}/identity"; })
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