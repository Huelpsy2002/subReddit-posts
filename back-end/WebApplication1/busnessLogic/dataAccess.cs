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

        public async Task insertLane(string name, string userId)
        {
            try
            {
                var subReddit = await fetchSubReddit(name);
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
        public async Task<List<RedditResponse>> getLanes(string userId)
        {

            try
            {
                var lanes = await _database.getLanes(userId);
                Console.WriteLine($"lanes count : {lanes.Count}");
                List<RedditResponse> subRedditPosts = new List<RedditResponse>();
                foreach (var l in lanes)
                {
                    var subReddit = await fetchSubReddit(l.name);
                    subRedditPosts.Add(subReddit);

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
        private async Task<RedditResponse> fetchSubReddit(string subreddit)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("subreddit-post-fetcher/1.0");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("subreddit-post-fetcher/1.0 (contact: hussinali.dev@gmail.com)");

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,application/json;q=0.8,*/*;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

            try
            {
                string url = $"https://www.reddit.com/r/{subreddit}.json";
                var response = await client.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response status: {response.StatusCode}");
                    Console.WriteLine($"Error response content: {errorContent}");
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