// using System.Text.Json.Nodes;
// using MongoDB.Bson;
// using ResQueue.Dtos;
// using ResQueue.Models;
//
// namespace ResQueue.Mappers;
//
// public static class MessageMapper
// {
//     public static MessageDto ToDto(Message message)
//     {
//         JsonNode body;
//         string bodyEncoding;
//         if (message.Body is BsonDocument)
//         {
//             body = JsonNode.Parse(message.Body.ToJson()) ?? throw new Exception();
//             bodyEncoding = "json";
//         }
//         else if (message.Body is BsonString s)
//         {
//             body = s.Value;
//             bodyEncoding = "string";
//         }
//         else if (message.Body is BsonBinaryData bin)
//         {
//             body = Convert.ToBase64String(bin.Bytes);
//             bodyEncoding = "base64";
//         }
//         else
//         {
//             throw new Exception($"Unsupported body type: {message.Body.GetType()}");
//         }
//
//         return new MessageDto()
//         {
//             Id = message.Id.ToString(),
//             Body = body,
//             BodyEncoding = bodyEncoding,
//             RabbitmqMetadata = message.RabbitMQMeta is not null
//                 ? MapRabbitmqMetadataToDto(message.RabbitMQMeta)
//                 : null,
//             CreatedAt = message.CreatedAt,
//             UpdatedAt = message.UpdatedAt,
//             IsReviewed = message.IsReviewed
//         };
//     }
//
//     static RabbitmqMessageMetadataDto MapRabbitmqMetadataToDto(RabbitMQMessageMeta meta)
//     {
//         return new()
//         {
//             Redelivered = meta.Redelivered,
//             Exchange = meta.Exchange,
//             RoutingKey = meta.RoutingKey,
//             Properties = new()
//             {
//                 AppId = meta.Properties.AppId,
//                 ClusterId = meta.Properties.ClusterId,
//                 ContentEncoding = meta.Properties.ContentEncoding,
//                 ContentType = meta.Properties.ContentType,
//                 CorrelationId = meta.Properties.CorrelationId,
//                 DeliveryMode = meta.Properties.DeliveryMode,
//                 Expiration = meta.Properties.Expiration,
//                 Headers = meta.Properties.Headers,
//                 MessageId = meta.Properties.MessageId,
//                 Priority = meta.Properties.Priority,
//                 ReplyTo = meta.Properties.ReplyTo,
//                 Timestamp = meta.Properties.Timestamp,
//                 Type = meta.Properties.Type,
//                 UserId = meta.Properties.UserId,
//             },
//         };
//     }
// }