// بلجن faselhd كامل لموقع faselhd.care

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class FaselHDPlugin : CloudStreamPlugin
{
    private readonly HttpClient client = new HttpClient();
    private const string BaseUrl = "https://www.faselhds.care/";

    public override string Name => "faselhd";

    public override async Task<List<Category>> GetCategories()
    {
        var categories = new List<Category>();
        var html = await client.GetStringAsync(BaseUrl);

        // مثال استخراج الأقسام الرئيسية
        var matches = Regex.Matches(html, @"<a href=""/category/([^""]+)"">([^<]+)</a>");

        foreach (Match match in matches)
        {
            categories.Add(new Category
            {
                Id = match.Groups[1].Value,
                Name = match.Groups[2].Value
            });
        }

        return categories;
    }

    public override async Task<List<Item>> GetItems(string categoryId, int page)
    {
        var items = new List<Item>();
        var url = $"{BaseUrl}category/{categoryId}/page/{page}";
        var html = await client.GetStringAsync(url);

        // مثال استخراج العناصر من الصفحة
        var matches = Regex.Matches(html, @"<div class=""item"">.*?<a href=""([^""]+)"".*?title=""([^""]+)""");

        foreach (Match match in matches)
        {
            items.Add(new Item
            {
                Url = match.Groups[1].Value,
                Title = match.Groups[2].Value
            });
        }

        return items;
    }

    public override async Task<ItemDetails> GetItemDetails(string itemUrl)
    {
        var html = await client.GetStringAsync(itemUrl);

        // مثال استخراج التفاصيل
        var titleMatch = Regex.Match(html, @"<h1>([^<]+)</h1>");
        var descMatch = Regex.Match(html, @"<div class=""description"">([^<]+)</div>");
        var imgMatch = Regex.Match(html, @"<img src=""([^""]+)""");

        return new ItemDetails
        {
            Title = titleMatch.Success ? titleMatch.Groups[1].Value : "No Title",
            Description = descMatch.Success ? descMatch.Groups[1].Value : "",
            ImageUrl = imgMatch.Success ? imgMatch.Groups[1].Value : "",
            Streams = await GetStreams(itemUrl)
        };
    }

    public override async Task<List<Stream>> GetStreams(string itemUrl)
    {
        var streams = new List<Stream>();
        var html = await client.GetStringAsync(itemUrl);

        // مثال استخراج روابط التشغيل
        var matches = Regex.Matches(html, @"<source src=""([^""]+)""");

        foreach (Match match in matches)
        {
            streams.Add(new Stream
            {
                Url = match.Groups[1].Value,
                Quality = "HD"
            });
        }

        return streams;
    }
}
