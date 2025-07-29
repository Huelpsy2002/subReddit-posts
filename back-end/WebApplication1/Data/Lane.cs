
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApplication1.Data
{
    public class lane
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("userId")]
        public string userId { get; set; }

        [BsonElement("LaneName")]
        public string name { get; set; }

    }


    public class Post
    {
        public int index { get; set; }
        public string subreddit { get; set; }
        public string title { get; set; }
        public string content { get; set; }

        public int votes { get; set; }
        public string postUrl { get; set; }
        public string author { get; set; }
        public int comments { get; set; }
        
    }

    public class RedditResponse
    {
        public string SubredditName { get; set; }
        public int NumberOfPosts { get; set; }
        public List<Post> Posts { get; set; }
    }
}
