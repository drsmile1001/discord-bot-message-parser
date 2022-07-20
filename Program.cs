using System.Text.Json;
using System.Text.RegularExpressions;
using DiscordBotMessageParser;
using Microsoft.EntityFrameworkCore;

var files = Directory.GetFiles("inputs");

var blackList = new[]{
    "找不到接近的 怕:confounded:",
    "",
    "BadArgCount: The input text has too few parameters.",
    "流水號必須為整數",
    "BadArgCount: The input text has too many parameters.",
    "ParseFailed: Failed to parse UInt64.",
    "ParseFailed: Input text may not end on an incomplete escape.",
    "流水號應為整數",
    "沒有預存字串，加點水吧:sweat_drops:",
    "找不到接近的 怕",
    "已設定反應預設集",
    "找不到反應預設集",
    "必須輸入整數流水號",
    "UnknownCommand: Unknown command.",
    "ParseFailed: A quoted parameter is incomplete"
};

var blackRegex = new[]{
    new Regex(@"^找不到 \S+ \#\d+$"),
    new Regex(@"^已刪除 \S+ \#\d+$"),
    new Regex(@"^必須由該字串的創建者\S+刪除$"),
    new Regex(@"^無法找到命令 \S+$"),
    new Regex(@"^沒有預存字串的索引為\S+$"),
    new Regex(@"^已更新\[\S+ \#\d+\]為\[\S+ \#\d+\]$"),
    new Regex(@"^無法找到命令\:.+$"),
    new Regex(@"^已刪除索引.+$"),
    new Regex(@"^找不到索引\:.+$"),
};

var matchingRegex = new[]
{
    new Regex(@"^(?<key>\S+) (\(\S+ ?\%\) )?\#\d+ (?<value>.+)"),
    new Regex(@"^索引\:""(?<key>\S+)""  流水號\:\d+  (?<value>.+)"),
    new Regex(@"^索引似乎是這個\:""(?<key>\S+)"" 相似度高達\:\d+\.\d+\%  流水號\:\d+  (?<value>.+)"),
    new Regex(@"^似乎是這個\:""(?<key>\S+)"" 相似度高達\:\d+\.\d+\%  (?<value>.+)"),
    new Regex(@"^已新增預存字串 索引\:""(?<key>\S+)"" 流水號\:\d+ 字串\:\""(?<value>.+)\""$"),
    new Regex(@"^已增修預存字串 索引\:""(?<key>\S+)"" 字串\:\""(?<value>.+)\""$"),
};

var messages = files.SelectMany(filePath =>
{
    var doc = JsonDocument.Parse(File.ReadAllText(filePath));
    var messages = doc.RootElement.GetProperty("messages").EnumerateArray()
    .Select(x =>
    {
        var message = x.GetProperty("content").GetString()!;

        if (blackList.Contains(message) || blackRegex.Any(r => r.IsMatch(message)))
        {
            return null;
        }

        var match = matchingRegex
            .Select(r => r.Match(message))
            .FirstOrDefault(m => m.Success);

        if (match == null)
        {
            return null;
        }

        return new
        {
            key = match.Groups["key"].Value,
            value = match.Groups["value"].Value,
        };
    })
    .ToArray();
    return messages;
})
.Where(item => item != null)
.Distinct()
.GroupBy(item => item!.key)
.SelectMany(group =>
{
    var index = group.Key;

    return group.Select((item, sn) => new MessagePreset
    {
        Id = Guid.NewGuid().ToString(),
        Index = index,
        SeriesNumber = sn,
        Text = item!.value,
    });
}).ToArray();


var dbcontext = new BotDbContext(new DbContextOptionsBuilder<BotDbContext>()
    .UseSqlite("Data Source=./discord.db")
    .Options);
dbcontext.AddRange(messages);
dbcontext.SaveChanges();
