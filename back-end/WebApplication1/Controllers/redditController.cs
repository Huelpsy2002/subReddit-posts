using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Runtime;
using System.Text;
using System.Text.Json;
using WebApplication1.busnessLogic;
using WebApplication1.Data;


namespace WebApplication1.Controllers


{




    [Route("api")]
    [ApiController]
    public class RedditController : ControllerBase
    {
        private readonly AuthLogic _authLogic;
        private readonly DataAccess _dataAccess;
        public RedditController(AuthLogic authLogic, DataAccess dataAccess)
        {
            _authLogic = authLogic;
            _dataAccess = dataAccess;
        }


        [HttpPost("callBack")]
        public async Task<IActionResult> receiveCodeAndGenerateJwt([FromBody] CodeRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { message = "Code is required" });
            }
            try
            {
                var (jwt, expires_at) = await _authLogic.ExchangeCodeForAccessTokenAsync(request.Code);
                Response.Cookies.Append("token", jwt, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = expires_at,
                });

                return Ok(new { message = "Login successful" });


            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return BadRequest(new { message = error.Message });
            }
        }



        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetRedditMe()
        {
            var jwt = Request.Cookies["token"]; ;
            var tokenInfo = await _dataAccess.getToken(jwt);
            // if (!TokenStore.Tokens.TryGetValue(jwt, out var tokenInfo))
            //     return Unauthorized("Invalid or expired session");

            if (tokenInfo == null)
            {
                return Unauthorized("Invalid or expired session");
            }
            if (DateTime.UtcNow > tokenInfo.ExpiresAt.ToUniversalTime())

                return Unauthorized("Reddit access token expired");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.AccessToken);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SubBrowse");

            var response = await httpClient.GetAsync("https://oauth.reddit.com/api/v1/me");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Reddit API error: {response.StatusCode}");
                return StatusCode((int)response.StatusCode, "Reddit API call failed");
            }
            var json = await response.Content.ReadAsStringAsync();
            var redditUser = JsonSerializer.Deserialize<RedditUserDto>(json);

            return Ok(new { user = redditUser });


        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var jwt = Request.Cookies["token"]; ;

            Response.Cookies.Append("token", jwt, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1), // expire immediately
                Secure = false,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("saveLane")]
        [Authorize]

        public async Task<IActionResult> SaveLane([FromBody] LaneDto laneDto)
        {
            var jwt = Request.Cookies["token"]; ;
            var tokenInfo = await _dataAccess.getToken(jwt);

            if (tokenInfo == null)
            {
                return Unauthorized("Invalid or expired session");
            }
            if (DateTime.UtcNow > tokenInfo.ExpiresAt.ToUniversalTime())

                return Unauthorized("Reddit access token expired");
            try
            {
                await _dataAccess.insertLane(laneDto.laneName, laneDto.userId);
                return Ok(new { message = "lane saved" });


            }
            catch (DuplicateLaneException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidSubredditException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to insert lane.");

            }

        }

        [HttpGet("getLanes")]
        [Authorize]

        public async Task<IActionResult> GetLanes([FromQuery] string userId)
        {
            var jwt = Request.Cookies["token"]; ;
            var tokenInfo = await _dataAccess.getToken(jwt);

            if (tokenInfo == null)
            {
                return Unauthorized("Invalid or expired session");
            }
            if (DateTime.UtcNow > tokenInfo.ExpiresAt.ToUniversalTime())

                return Unauthorized("Reddit access token expired");
            try
            {
                var lanes = await _dataAccess.getLanes(userId);

                if (lanes != null)
                {
                    return Ok(new { lanes });
                }
                return BadRequest(new { message = "error fetching lanes" });
            }
            catch (Exception err)
            {
                return StatusCode(500, "Failed to get lanes.");

            }
        }
        [HttpGet("deleteLane")]
        [Authorize]

        public async Task<IActionResult> deleteLane([FromQuery] string userId, string laneName)
        {
            var jwt = Request.Cookies["token"]; ;
            var tokenInfo = await _dataAccess.getToken(jwt);

            if (tokenInfo == null)
            {
                return Unauthorized("Invalid or expired session");
            }
            if (DateTime.UtcNow > tokenInfo.ExpiresAt.ToUniversalTime())

                return Unauthorized("Reddit access token expired");
            try
            {
                var isdeleted = await _dataAccess.deleteLane(laneName, userId);
                if (isdeleted)
                {
                    return Ok(new { message = "lane deleted" });

                }
                return BadRequest(new { message = "deletion failed" });
            }
            catch (Exception err)
            {
                return StatusCode(500, "server error.");

            }
        }

    }
}
