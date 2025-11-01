using ApiProject.DTOs;
using ApiProject.Models;
using ApiProject.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ECPayController : ControllerBase
    {
        private const string MerchantID = "3002607";
        private const string HashKey = "pwFHCqoQZGmho4w6";
        private const string HashIV = "EkRm7iFT261dpevs";
        private readonly dbFurniMartContext _context;

        public ECPayController(dbFurniMartContext context)
        {
            _context = context;
        }

        // Post: api/ECPay/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(ReqECPayDTO reqDTO)
        {
            var paymentCode = ECPayPaymentMapper.ToECPayCode(reqDTO.PaymentMethodId);
            var uniTradeNo = $"VN{reqDTO.MerchantTradeNo.ToString()}l{DateTime.Now:MMddHHmmss}";
            if (string.IsNullOrEmpty(paymentCode))
            {
                var result = new ResultDTO
                {
                    Ok = true,
                    Code = StatusCodes.Status200OK,
                    Message = "使用現金付款，訂單已成功建立"
                };
                return Ok(result);
            }
            var item = await _context.TOrders
                .Include(c => c.OrderDetail)
                    .ThenInclude(c => c.ProductVariant)
                        .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(c => c.FOrderId == int.Parse(reqDTO.MerchantTradeNo));

            var itemNames = item.OrderDetail
                .Select(od => $"{od.ProductVariant?.Product?.FName ?? "一般家具"}x{od.FQuantity}")
                .ToList();

            string itemNameStr = string.Join("#", itemNames);
            //itemNameStr = HttpUtility.UrlEncode(itemNameStr, Encoding.UTF8);

            var order = new Dictionary<string, string>
            {
                {"MerchantID", MerchantID },
                {"MerchantTradeNo",  uniTradeNo},
                {"MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
                {"PaymentType", "aio" },
                {"TotalAmount",  ((int)reqDTO.TotalAmount).ToString()},
                {"TradeDesc", "家具銷售支付測試" },
                {"ItemName", itemNameStr }, 
                {"ReturnURL", "https://unseverable-pantomimically-isaiah.ngrok-free.dev/api/ECPay/Callback" }, // 後端API (因為要允許綠界，要使用ngrok)
                {"ChoosePayment", paymentCode },
                {"EncryptType", "1" },
                {"OrderResultURL", "https://unseverable-pantomimically-isaiah.ngrok-free.dev/api/ECPay/RedirectToFront" }, // 前端結果頁面，先用訂單頁 (因為要允許綠界，要使用ngrok)
                //{"IgnorePayment", "WeiXin#BNPL" }
            };

            order["CheckMacValue"] = BuildCheckMacValue(order);

            var sb = new StringBuilder();
            sb.AppendLine("<html><body><form id='ecpay' method='post' action='https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5'>");
            foreach(var kv in order)
            {
                var safeValue = HttpUtility.HtmlEncode(kv.Value);
                sb.AppendLine($"<input type='hidden' name='{kv.Key}' value='{safeValue}' />");
            }
            sb.AppendLine("</form><script>document.getElementById('ecpay').submit();</script></body></html>");
            return Content(sb.ToString(), "text/html;charset=utf-8");
        }

        [HttpPost("Callback")]
        public async Task<IActionResult> CallBack()
        {
            try
            {
                Console.WriteLine("綠界回傳內容");

                if (!Request.HasFormContentType)
                {
                    Console.WriteLine("回傳不是 form 格式");
                    return Content("0|Error");
                }

                // 解析 form 資料
                var form = await Request.ReadFormAsync();
                var dict = form.ToDictionary(x => x.Key, x => x.Value.ToString());

                // 輸出所有回傳內容
                foreach (var kv in dict)
                {
                    Console.WriteLine($"{kv.Key}: {kv.Value}");
                }

                // 取得訂單編號
                if (dict.TryGetValue("MerchantTradeNo", out var tradeNo))
                {
                    Console.WriteLine($"訂單編號: {tradeNo}");
                }

                // 取得付款狀態
                if (dict.TryGetValue("RtnCode", out var rtnCode) && rtnCode == "1")
                {
                    Console.WriteLine("付款成功");
                    // TODO 更新訂單狀態邏輯
                    var orderId = tradeNo.Replace("VN", "").Split('l')[0];
                    var order = await _context.TOrders.FindAsync(int.Parse(orderId));
                    order.FPaymentStatus = 4;
                    order.FOrderStatus = 3;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine("付款失敗");
                }

                return Ok("1|OK");
            }catch (Exception ex)
            {
                Console.WriteLine($"綠界回傳處理發生錯誤: {ex.Message}");
                return Content("0|Error");
            }
        }

        [HttpPost("RedirectToFront")]
        public async Task<IActionResult> RedirectToFront()
        {
            var form = await Request.ReadFormAsync();
            var rtnCode = form["RtnCode"].ToString();
            var success = rtnCode == "1";
            var script = $@"
                <script>
                if (window.opener){{window.opener.postMessage({{
                    type: 'ecpayPayment',
                    status: '{(success ? "success" : "fail" )}'
                    }}, '*');
                }}
                window.close();
                </script>";
            return Content(script, "text/html", Encoding.UTF8);
        }


        private static string BuildCheckMacValue(Dictionary<string, string> dict)
        {
            const string HashKey = "pwFHCqoQZGmho4w6";
            const string HashIV = "EkRm7iFT261dpevs";

            var sorted = dict
                .Where(x => x.Key != "CheckMacValue")
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}")
                .ToList();

            string parameters = string.Join("&", sorted);

            return BuildCheckMacValueCore(parameters, HashKey, HashIV, encryptType: 1);
        }

        // 官方算法
        private static string BuildCheckMacValueCore(string parameters, string hashKey, string hashIV, int encryptType = 0)
        {
            string raw = $"HashKey={hashKey}&{parameters}&HashIV={hashIV}";
            string encoded = HttpUtility.UrlEncode(raw).ToLower();

            string hashResult;
            if (encryptType == 1)
            {
                // SHA256
                using var sha = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(encoded);
                var hash = sha.ComputeHash(bytes);
                hashResult = BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
            else
            {
                using var md5 = MD5.Create();
                var bytes = Encoding.UTF8.GetBytes(encoded);
                var hash = md5.ComputeHash(bytes);
                hashResult = BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }

            return hashResult;
        }

    }
}
