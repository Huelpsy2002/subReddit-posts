
async function getSubredditData(subreddit) {

    try {
        const response = await fetch(`https://www.reddit.com/r/${subreddit}.json`);
        if(response!=undefined && response.status!=200){
            throw new Error("error")
        }
        const json = await response.json()
        const posts = json.data.children.map((post, index) => {
            const {
                title,
                selftext,
                url,
                ups,
                permalink,
                subreddit,
                preview,
                author,
                num_comments,
                created_utc,
                is_video
            } = post.data;

            // Check if the URL is an image
            const isImage = /\.(jpg|jpeg|png|gif)$/.test(url);
            const img = isImage ? url : preview?.images?.[0]?.source?.url || null;

            return {
                index: index + 1,
                subreddit,
                title,
                content: selftext,
                img,
                votes: ups,
                postUrl: `https://reddit.com${permalink}`,
                author,
                comments: num_comments,
                created: new Date(created_utc * 1000).toLocaleString(),
                is_video
            };
        });

        return {
            subredditName: subreddit,
            numberOfPosts: posts.length,
            posts
        };
    }
    catch (error) {
        if (error.response.status === 404) {
            throw new Error("subReddit not found")
          } else {
             throw new Error("error fetching subReddit")
          }
    }
}

export default getSubredditData
