
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApplication1.Data
{
    public class Token
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        
        [BsonElement("jwt")]
        public string Key { get; set; }



        [BsonElement("access_token")]
        public string AccessToken { get; set; }
        [BsonElement("refresh_token")]
        public string RefreshToken { get; set; }
        [BsonElement("expires_at")]
        public DateTime ExpiresAt { get; set; }
    }
}
