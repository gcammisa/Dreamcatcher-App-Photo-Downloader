using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections;


using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DC_Photo_DL_Xamarin
{
    public class PageParser
    {
        public async Task<string[]> ParsePage(string _shareUrl)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0.1; RedMi Note 5 Build/RB3N5C; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/68.0.3440.91 Mobile Safari/537.36");

            var content = await client.GetStringAsync(_shareUrl);

            ArrayList results = new ArrayList();

            //Parsing the webpage
            foreach (var pageLine in content.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (pageLine.Contains("file.candlemystar.com/cache/post") && !pageLine.Contains("meta"))
                {
                    String tempUrl = pageLine.Replace("<img src=\"", "").Replace("\"/>", "").Replace("/cache", "").Replace("thumb-", "").Replace("_612x0", "").Trim();
                    results.Add(tempUrl);
                }
            }

            return (String[])results.ToArray(typeof(string));
        }
    }
}