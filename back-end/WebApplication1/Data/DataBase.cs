using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace WebApplication1.Data
{
    public class DataBase
    {


        private MongoClient client { get; set; }

        public DataBase(IConfiguration configuration)
        {

            string ConnectionStrings = configuration.GetConnectionString("mongoDB");
            var settings = MongoClientSettings.FromConnectionString(ConnectionStrings);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            client = new MongoClient(settings);

        }


        public async Task EnsureTokenTTLIndex() // this is used to delete expired token from db automatically
        {
            var database = client.GetDatabase("redditClient");
            var collection = database.GetCollection<Token>("tokens");

            var indexKeys = Builders<Token>.IndexKeys.Ascending(t => t.ExpiresAt);
            var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
            var indexModel = new CreateIndexModel<Token>(indexKeys, indexOptions);

            try
            {
                await collection.Indexes.CreateOneAsync(indexModel);
                Console.WriteLine("TTL index created on ExpiresAt field.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create TTL index:");
                Console.WriteLine(ex.Message);
            }
        }

        public async Task setDbConstrains()
        {
            var database = client.GetDatabase("redditClient");

            var collection = database.GetCollection<lane>("lanes");
            Console.WriteLine(collection.Indexes);
            var keys = Builders<lane>.IndexKeys.Ascending(l => l.name).Ascending(l => l.userId);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<lane>(keys, indexOptions);

            await collection.Indexes.CreateOneAsync(indexModel);
        }


        public async Task access()
        {

            try
            {

                var database = client.GetDatabase("redditClient");
                var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
                await EnsureTokenTTLIndex();
                await setDbConstrains();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }

        }
        public void insertTokken(string jwt, string accessToken, string refreshToken, DateTime expires_at)
        {
            var database = client.GetDatabase("redditClient");
            var collection = database.GetCollection<Token>("tokens");

            var token = new Token
            {
                Key = jwt,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expires_at
            };

            try
            {
                collection.InsertOne(token);
                Console.WriteLine("Token inserted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Insert failed:");
                Console.WriteLine(ex.Message);
            }

        }
        public async Task<Token> getToken(string jwt)
        {
            var database = client.GetDatabase("redditClient");
            var collection = database.GetCollection<Token>("tokens");
            try
            {


                var token = await collection.Find(t => t.Key == jwt).FirstOrDefaultAsync();
                if (token != null)
                {
                    Console.WriteLine("token :" + token);
                }
                else
                {
                    Console.WriteLine("token found");
                }
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Token fetch failed:");
                Console.WriteLine(ex.Message);
                return null;

            }

        }
        public async Task<bool> insertLane(string name, string userId)
        {
            var database = client.GetDatabase("redditClient");
            var collection = database.GetCollection<lane>("lanes");

            var lane = new lane
            {
                userId = userId,
                name = name
            };

            try
            {
                await collection.InsertOneAsync(lane);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }

            catch (Exception ex)
            {
                throw new Exception("server Error:insertion failed");
            }

        }
        public async Task<List<lane>> getLanes(string userId)
        {
            var database = client.GetDatabase("redditClient");
            var collection = database.GetCollection<lane>("lanes");
            try
            {
                var lanes = await collection.Find(l => l.userId == userId).ToListAsync();
                return lanes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("error fetching lanes");
                return null;
            }
        }
        public async Task<bool> deleteLane(string name, string userId)
        {
            var database = client.GetDatabase("redditClient");
            var collection = database.GetCollection<lane>("lanes");

            var filter = Builders<lane>.Filter.And(
                Builders<lane>.Filter.Eq(l => l.name, name),
                Builders<lane>.Filter.Eq(l => l.userId, userId)
            );

            try
            {
                var result = await collection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    Console.WriteLine("Lane deleted successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Lane not found.");
                    return false;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Delete failed:");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
