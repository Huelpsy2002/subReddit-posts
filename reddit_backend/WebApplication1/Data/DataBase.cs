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
        public async Task access()
        {

            try
            {

                var database = client.GetDatabase("redditClient");
                var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
                
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
    }
}
