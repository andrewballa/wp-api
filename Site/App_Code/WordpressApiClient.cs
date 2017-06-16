using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace WordpressApiClient
{
    public class WPBlogPosts
    {
        public int id { get; set; }
        public BlogPostTitle title { get; set; }
        public string link { get; set; }
        public int? featured_media { get; set; }
        public DateTime date { get; set; }
        public DateTime? modified { get; set; }
        public string custom_imageUrl { get; set; }
    }
    public class BlogPostTitle
    {
        public string rendered { get; set; }
    }

    public class WPMedia
    {
        public int id { get; set; }
        public string source_url { get; set; }
    }

    public class ApiClient
    {
        private static HttpClient client;
        private static string lastYear = "";
        private static int allPostsCount;
        private static int allMediaCount;

        public static async Task<string> CallWordPressApi(string categories, string tags)
        {
            // WebRequest.DefaultWebProxy = new WebProxy("127.0.0.1", 8888);

            client = new HttpClient();
            client.BaseAddress = new Uri("https://www.digi.com/blog/wp-json/wp/v2/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            lastYear = DateTime.Now.AddYears(-1).ToString("o");

            //// ## SERVER-SIDE CACHING FOR QA. NOT NEEDED ON PRODCUTION SINCE CDN CACHES CONTENT THERE ##
            //var host = System.Web.HttpContext.Current.Request.Url.Host;
            //if (host == "www-qa.digi.com") // || host=="172.16.1.135" || host=="172.16.1.136"
            //{
            //    //Get the JSON results and cache them for 24 hours, based on the key (which is a querystring) provided to CacheSettings.
            //    //If the key changes(i.e. if the parameters for the API call change), then cache is reset, and data is obtained again.
            //    var cacheSettings = new CacheSettings(1440, string.Format("wpCacheParam|category={0}&tag={1}", categories, tags)) { AllowProgressiveCaching = true };
            //    var getBlotPosts = CacheHelper.Cache(cs => ProcessBlogPosts(categories, tags), cacheSettings);
            //    return await getBlotPosts;
            //}
            ////For Prod, just run the method normally

            //CookieHelper.SetValue("isdigiuser", "test", DateTime.Now.AddYears(1));

            return await ProcessBlogPosts(categories, tags);
        }

        public static async Task<string> ProcessBlogPosts(string categories, string tags)
        {
            List<WPBlogPosts> allPosts = new List<WPBlogPosts>();
            List<WPMedia> allMedia = new List<WPMedia>();

            if (categories != null)
            {
                allPosts.AddRange(await RunPostsQuerying("categories=" + categories));
            }

            if (tags != null)
            {
                allPosts.AddRange(await RunPostsQuerying("tags=" + tags));
            }

            if (allPosts.Count > 0)
            {
                allMedia =  await RunMediaQuerying();
                if (allMedia.Count > 0)
                {
                    foreach (var post in allPosts)
                    {
                        if (post.featured_media != 0 && post.featured_media != null)
                        {
                            var media = allMedia.Find(x => x.id == post.featured_media);
                            post.custom_imageUrl = media.source_url;
                        }
                        else
                        {
                            post.custom_imageUrl = "null";
                        }
                    }
                    return JsonConvert.SerializeObject(allPosts);
                }
            }

            //something went wrong in the Wordpress API retrieval. 
            return JsonConvert.SerializeObject(new { error = "something went wrong" });
        }

        public static async Task<List<WPBlogPosts>> RunPostsQuerying(string param)
        {
            var allPosts = new List<WPBlogPosts>();
            var firstPostsArray = await GetBlogPostsAsync(param, 1, 1);
            if (firstPostsArray.Count > 0)
            {
                foreach (WPBlogPosts p in firstPostsArray)
                {
                    if (allPosts.Find(x => x.id == p.id) == null) { allPosts.Add(p); }
                }

                if (allPostsCount > 1)
                {
                    var restOfPostsArray = await GetBlogPostsAsync(param, 2, allPostsCount);
                    if (restOfPostsArray != null)
                    {
                        foreach (WPBlogPosts p in restOfPostsArray)
                        {
                            if (allPosts.Find(x => x.id == p.id) == null) { allPosts.Add(p); }
                        }
                    }
                }
            }
            return allPosts;
        }

        private static async Task<List<WPBlogPosts>> GetBlogPostsAsync(string param,int startCount, int iterationCount)
        {
            var allPosts = new List<WPBlogPosts>();
            try
            {
                //for (int i = startCount; i > iterationCount; --i)
                for (int i = startCount; i <= iterationCount; i++)
                {
                    var path = string.Format("posts?per_page=100&{0}&page={1}&after={2}", param, i, lastYear); //      
                    var response = await client.GetAsync(path);
                    response.EnsureSuccessStatusCode();
                    allPosts.AddRange(await response.Content.ReadAsAsync<List<WPBlogPosts>>());
                    if (startCount == 1)
                    {
                        allPostsCount = GetPageCount(response.Headers);
                    }
                }
            }
            catch
            {
                return null;
            }

            return allPosts;
        }
        
        public static async Task<List<WPMedia>> RunMediaQuerying()
        {
            var allMedia = new List<WPMedia>();
            var firstMediaArray = await GetMediaAsync(1, 1);
            if (firstMediaArray.Count > 0)
            {
                foreach (var m in firstMediaArray)
                {
                    if (allMedia.Find(x => x.id == m.id) == null) { allMedia.Add(m); }
                }

                if (allMediaCount > 1)
                {
                    var restOfMediaArray = await GetMediaAsync(2, allMediaCount);
                    if (restOfMediaArray != null)
                    {
                        foreach (var m in restOfMediaArray)
                        {
                            if (allMedia.Find(x => x.id == m.id) == null) { allMedia.Add(m); }
                        }
                    }
                }
            }
            return allMedia;
        }

        private static async Task<List<WPMedia>> GetMediaAsync(int startCount, int iterationCount)
        {
            var allMedia = new List<WPMedia>();
            try
            {
                for (int i = startCount; i <= iterationCount; i++)
                {
                    var path = string.Format("media?per_page=100&page={0}&after={1}", i, lastYear);
                    var response = client.GetAsync(path).Result;
                    response.EnsureSuccessStatusCode();
                    allMedia.AddRange(await response.Content.ReadAsAsync<List<WPMedia>>());
                    if (startCount == 1)
                    {
                        allMediaCount = GetPageCount(response.Headers);
                    }
                }
            }
            catch
            {
                return null;
            }
            return allMedia;
        }

        public static int GetPageCount(HttpResponseHeaders header)
        {
            int totalPages = 0;
            try
            {
                IEnumerable<string> values;
                if (header.TryGetValues("x-wp-totalpages", out values))
                {
                    totalPages = Convert.ToInt32(values.First());
                }
            }
            catch { }
            return totalPages;
        }


    }
}