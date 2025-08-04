using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebApplication1.Data;



namespace WebApplication1.busnessLogic
{
   


    public class DataAccess
    {
        private readonly IConfiguration _iconfigration;
        private readonly DataBase _database;
        public DataAccess(IConfiguration configuration, DataBase dataBase)
        {
            _iconfigration = configuration;
            _database = dataBase;
        }
        public async Task<Token> getToken(string jwt)
        {

            try
            {

                var token = await _database.getToken(jwt);
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Business Logic :Token fetch failed:");
                Console.WriteLine(ex.Message);
                return null;

            }

        }

        public async Task insertLane(string name, string userId,string token)
        {
            try
            {
                var subReddit = await fetchSubReddit(name,token);
                if (subReddit != null)
                {
                    var inserted = await _database.insertLane(name, userId);
                    if (!inserted)
                        throw new DuplicateLaneException("subReddit already exist");
                }
                else
                {
                    throw new InvalidSubredditException("SubReddit does not exist");
                }
            }
            catch (DuplicateLaneException ex)
            {
                throw new DuplicateLaneException(ex.Message);

            }
            catch (InvalidSubredditException ex)
            {
                throw new InvalidSubredditException(ex.Message);

            }


            catch (Exception ex)
            {

                throw new Exception("Failed to insert lane: " + ex.Message);
            }
        }
        public async Task<List<RedditResponse>> getLanes(string userId , string token)
        {

            try
            {
                var lanes = await _database.getLanes(userId);
                Console.WriteLine($"lanes count : {lanes.Count}");
                List<RedditResponse> subRedditPosts = new List<RedditResponse>();
                foreach (var l in lanes)
                {
                    var subReddit = await fetchSubReddit(l.name,token);
                    subRedditPosts.Add(subReddit);
                   await  Task.Delay(2000);//rate limiting

                }
                return subRedditPosts;
            }

            catch (Exception exp)
            {

                Console.WriteLine("Error in Business Logic: lanes fetching failed");
                Console.WriteLine(exp.Message);
                return null;

            }
        }
        public async Task<bool> deleteLane(string name, string userId)
        {
            try
            {
                var isdeleted = await _database.deleteLane(name, userId);
                return isdeleted;


            }
            catch (Exception ex)
            {

                Console.WriteLine("Error in Business Logic: lane insertion failed");
                return false;
            }
        }
        private async Task<RedditResponse> fetchSubReddit(string subreddit ,string token)
        {
           

            using HttpClient client = new HttpClient();
            var url = $"https://oauth.reddit.com/r/{subreddit}/hot?limit={25}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("User-Agent", "SubBrowse");

            try
            {
                var response = await client.SendAsync(request);


                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Error response status: {response.StatusCode}");
                    return null;
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response status: {response.StatusCode}");
                    
                    throw new Exception("Error fetching subreddit");
                    
                }

                string jsonString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(jsonString);
                var postsArray = doc.RootElement
                                    .GetProperty("data")
                                    .GetProperty("children");

                var posts = new List<Post>();
                int index = 1;

                foreach (var postElement in postsArray.EnumerateArray())
                {
                    var data = postElement.GetProperty("data");

                    string title = data.GetProperty("title").GetString();
                    string selftext = data.GetProperty("selftext").GetString();
                    string urlLink = data.GetProperty("url").GetString();
                    int ups = data.GetProperty("ups").GetInt32();
                    string permalink = data.GetProperty("permalink").GetString();
                    string subredditName = data.GetProperty("subreddit").GetString();
                    string author = data.GetProperty("author").GetString();
                    int num_comments = data.GetProperty("num_comments").GetInt32();





                    posts.Add(new Post
                    {
                        index = index++,
                        subreddit = subredditName,
                        title = title,
                        content = selftext,
                        votes = ups,
                        postUrl = "https://reddit.com" + permalink,
                        author = author,
                        comments = num_comments,

                    });
                }

                return new RedditResponse
                {
                    SubredditName = subreddit,
                    NumberOfPosts = posts.Count,
                    Posts = posts
                };
            }
            catch (HttpRequestException e)
            {
                throw new HttpRequestException(e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }



        }




    }





}