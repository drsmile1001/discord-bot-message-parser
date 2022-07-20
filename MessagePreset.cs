namespace DiscordBotMessageParser;

public class MessagePreset
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 索引
    /// </summary>
    public string Index { get; set; } = string.Empty;

    /// <summary>
    /// 重複索引的流水號
    /// </summary>
    public int SeriesNumber { get; set; }

    /// <summary>
    /// 儲存的字串
    /// </summary>
    public string Text { get; set; } = string.Empty;

    public int CalledCount { get; set; } = 0;
}