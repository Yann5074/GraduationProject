using ApiProject.Services; // 假設你有 ProductService
using OpenAI;
using OpenAI.Chat;

public class SmartAiService : IAiService
{
    private readonly OpenAIClient _client;
    private readonly ProductService _productService;

    public SmartAiService(IConfiguration config, ProductService productService)
    {
        _client = new OpenAIClient(config["OpenAI:ApiKey"]);
        _productService = productService;
    }

    public async Task<string> CompleteAsync(string userText, string? summary = null)
    {
        // 🧩 Step 1：讓AI先幫你「轉成查詢條件」
        var extractPrompt = $@"
        使用者說：「{userText}」
        請根據句子判斷想找的商品類別和價格範圍，用 JSON 格式回覆：
        {{""category"":""類別"",""price_min"":最小預算,""price_max"":最大預算}}
        若無明確價格，price_min 與 price_max 請為 null。
        ";
        var extract = await _client.Chat.GetAsync("gpt-4o-mini", new[] { new ChatMessage(ChatRole.User, extractPrompt) });
        var json = extract.FirstChoice.Message.Content[0].Text;
        var parsed = System.Text.Json.JsonSerializer.Deserialize<QueryDto>(json);

        // 🧩 Step 2：呼叫你的 ProductService 查資料庫
        var products = await _productService.SearchAsync(parsed.Category, parsed.PriceMin, parsed.PriceMax);

        // 🧩 Step 3：再讓AI根據查到的資料「自然語言化」
        var summaryPrompt = $@"
        使用者想找「{parsed.Category}」，預算大約 {parsed.PriceMin}~{parsed.PriceMax}。
        查到以下商品：
        {string.Join("\n", products.Select(p => $"{p.Name}，價格 {p.Price}"))}
        請用自然語氣推薦給使用者。
        ";
        var chat = await _client.Chat.GetAsync("gpt-4o-mini", new[] { new ChatMessage(ChatRole.User, summaryPrompt) });
        return chat.FirstChoice.Message.Content[0].Text;
    }

    private class QueryDto
    {
        public string? Category { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
    }
}
