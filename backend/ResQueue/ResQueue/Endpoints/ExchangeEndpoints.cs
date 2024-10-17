namespace ResQueue.Endpoints;

public static class ExchangeEndpoints
{
    public static void MapExchangeEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("exchanges")
            .RequireAuthorization();
        //
        // group.MapGet("{brokerId}",
        //     async (IMongoCollection<Exchange> exchangesCollection,
        //         IMongoCollection<Broker> brokersCollection,
        //         UserManager<User> userManager,
        //         HttpContext httpContext,
        //         string brokerId) =>
        //     {
        //         // Get user
        //         var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
        //         if (user == null)
        //         {
        //             return Results.Unauthorized();
        //         }
        //
        //         // Validate broker
        //         var brokerFilter = Builders<Broker>.Filter.And(
        //             Builders<Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(brokerId)),
        //             Builders<Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id),
        //             Builders<Broker>.Filter.Eq(b => b.DeletedAt, null)
        //         );
        //         if (!await brokersCollection.Find(brokerFilter).AnyAsync())
        //         {
        //             return Results.Unauthorized();
        //         }
        //
        //         var filter = Builders<Exchange>.Filter.And(
        //             Builders<Exchange>.Filter.Eq(q => q.BrokerId, ObjectId.Parse(brokerId))
        //         );
        //
        //         var exchanges = await exchangesCollection.Find(filter).ToListAsync();
        //
        //         return Results.Ok(exchanges.Select(q => new ExchangeDto()
        //         {
        //             Id = q.Id.ToString(),
        //             RawData = q.RawData.ToString()
        //         }));
        //     });
    }
}