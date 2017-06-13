using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WordpressApiClient;

public class QueryString
{
    public string CategoryIdList { get; set; }
    public string TagIdList { get; set; }
}


public class WordpressController : ApiController
{
    //[HttpPost]
    
    public async Task<HttpResponseMessage> Get([FromUri]string categories, string tags)
    {
        var json = await ApiClient.CallWordPressApi(categories, tags);

        return new HttpResponseMessage
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }
}
