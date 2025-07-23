namespace WebApplication1.Data
{
    public class RedditUserDto
    {
        public string name { get; set; }                  
        public string id { get; set; }                    
        public string icon_img { get; set; }              
        public SubredditDto subreddit { get; set; }
        public double created_utc { get; set; }           
        public int gold_creddits { get; set; }
        public int total_karma { get; set; }              // Total karma (Reddit-computed)
        public int link_karma { get; set; }               
        public int comment_karma { get; set; }
    }
    

    public class SubredditDto
    {
        public string url { get; set; }                   
        public string icon_img { get; set; }              
    }
}
