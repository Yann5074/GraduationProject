using ApiProject.Infrastructure;
using ApiProject.Services;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

public class SmartAiService : IAiService
{
    private readonly OpenAIClient _client;
    private readonly IProductService _productService; // 建議介面注入

    public SmartAiService(IConfiguration config, IProductService productService)
    {
        _client = new OpenAIClient(config["OpenAI:ApiKey"]);
        _productService = productService;
    }

    public async Task<string> CompleteAsync(string userText, string? summary = null)
    {
        // Step 1：抽取 類別/顏色/價格
        var extractPrompt = $@"
只回 JSON（不要多餘文字），格式：
{{""category"":""類別"",""color"":""顏色"",""price_min"":最小或null,""price_max"":最大或null}}
使用者說：「{userText}」
";
        var extract = await _client.Chat.GetAsync("gpt-4o-mini",
            new[] { new ChatMessage(ChatRole.User, extractPrompt) });

        var json = extract.FirstChoice.Message.Content[0].Text ?? "{}";
        var parsed = JsonSerializer.Deserialize<QueryDto>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        // Step 2：呼叫服務查 DB（用 tProductVariant × tCategory × tColor）
        var items = await _productService.SearchAsync(
            categoryKeyword: parsed.Category,
            colorKeyword: parsed.Color,
            min: parsed.PriceMin,
            max: parsed.PriceMax
        );

        if (items.Count == 0)
            return $"目前找不到「{parsed.Color ?? ""}{parsed.Category ?? "商品"}」在你預算範圍內的選項，要不要調整條件再試看看？";

        var lines = string.Join("\n", items.Select(x => $"・{x.Color}的{x.Category}「{x.Name}」— {x.Price:N0} 元"));
        return $"幫你挑了幾個：\n{lines}\n需要我再縮小（或放寬）預算、或換個顏色嗎？";
    }

    private class QueryDto
    {
        public string? Category { get; set; }
        public string? Color { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
    }
}
