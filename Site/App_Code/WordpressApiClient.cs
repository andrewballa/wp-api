using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
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
        static string lastYear = "";
        
        public static async Task<string> CallWordPressApi(string categories, string tags)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://www.digi.com/blog/wp-json/wp/v2/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            lastYear = DateTime.Now.AddYears(-1).ToString("o");


            List<WPBlogPosts> allPosts = await RunPostsQuerying(categories, tags);
            List<WPMedia> allMedia = await RunMediaQuerying();

            if (allMedia != null && allPosts != null)
            {
                foreach (var post in allPosts)
                {
                    if (post.featured_media != 0 && post.featured_media != null)
                    {
                        var media = allMedia.Find(x => x.id == post.featured_media); //.FirstOrDefault(x => x.id == post.featured_media);
                        post.custom_imageUrl = media.source_url;
                    }
                    else
                    {
                        post.custom_imageUrl = "null";
                    }
                }
                return JsonConvert.SerializeObject(allPosts);
            }

            //something went wrong in the Wordpress API retrieval. 
            return JsonConvert.SerializeObject(new { error = "something went wrong" });
        }


        public static async Task<List<WPBlogPosts>> RunPostsQuerying(string categories, string tags)
        {
            var allPosts = new List<WPBlogPosts>();

            if (categories != null)
            {
                var cparam = "categories=" + categories;
                var cCount = await CountPages("posts?" + cparam + "&after=" + lastYear);
                var cp = await GetBlogPostsAsync(cparam, cCount);
                if (cp != null)
                {
                    foreach (WPBlogPosts p in cp)
                    {
                        if(allPosts.Find(x=> x.id == p.id) == null) { allPosts.Add(p); }
                    }
                    //allPosts.AddRange(cp);
                }
            }

            if (tags != null)
            {
                var tparam = "tags=" + tags;
                var tCount = await CountPages("posts?" + tparam + "&after=" + lastYear);
                var tp = await GetBlogPostsAsync(tparam, tCount);
                if (tp != null)
                {
                    foreach (WPBlogPosts p in tp)
                    {
                        if (allPosts.Find(x => x.id == p.id) == null) { allPosts.Add(p); }
                    }
                    //allPosts.AddRange(tp);
                }
            }

            return allPosts;
        }

        public static async Task<List<WPMedia>> RunMediaQuerying()
        {
            var mCount = await CountPages("media?after=" + lastYear);
            if (mCount != 0)
            {
                return await GetMediaAsync(mCount);
            }
            //something went wrong, return null
            return null;
        }

        private static async Task<List<WPBlogPosts>> GetBlogPostsAsync(string param, int pages)
        {
            var allPosts = new List<WPBlogPosts>();
            try
            {
                for (int i = 1; i <= pages; i++)
                {
                    var path = string.Format("posts?per_page=100&{0}&page={1}&after={2}", param, i, lastYear);
                    var response = await client.GetAsync(path);
                    response.EnsureSuccessStatusCode();
                    allPosts.AddRange(await response.Content.ReadAsAsync<List<WPBlogPosts>>());
                }
            }
            catch (Exception)
            {
                allPosts = null;
            }
            return allPosts;
        }

        private static async Task<List<WPMedia>> GetMediaAsync(int pages)
        {
            var allMedia = new List<WPMedia>();
            try
            {
                for (int i = 1; i <= pages; i++)
                {
                    var path = string.Format("media?per_page=100&page={0}&after={1}", i, lastYear);
                    var response = client.GetAsync(path).Result;
                    response.EnsureSuccessStatusCode();
                    allMedia.AddRange(await response.Content.ReadAsAsync<List<WPMedia>>());
                }
            }
            catch
            {
                allMedia = null;
            }
            return allMedia;
        }

        public static async Task<int> CountPages(string path)
        {
            double totalPages = 0;
            int pagesToIterate;
            try
            {
                var response = await client.GetAsync(path);
                response.EnsureSuccessStatusCode();
                IEnumerable<string> values;
                if (response.Headers.TryGetValues("x-wp-total", out values))
                {
                    totalPages = Convert.ToDouble(values.First());
                }
                pagesToIterate = (int)Math.Ceiling(totalPages / 100);
            }
            catch (Exception ex)
            {
                pagesToIterate = 0;
            }
            return pagesToIterate;
        }



    }
}