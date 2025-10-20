namespace GraduationProject.Options
{
    //信箱驗證 => 讀取設定檔(appsettings.json)
    public class CEmployeeEmailOptions
    {
        //寄件者名稱
        public string FromName { get; set; } = "";
        //寄件者郵件地址
        public string FromAddress { get; set; } = "";
        public SmtpOptions Smtp { get; set; } = new();
        public class SmtpOptions
        {
            //SMTP 主機位址
            public string Host { get; set; } = "smtp.gmail.com";
            //通訊埠號
            public int Port { get; set; } = 587; // 587=STARTTLS, 465=SSL
            //登入 SMTP 的帳號
            public string User { get; set; } = "";
            //登入 SMTP 的密碼（App Password）
            public string Password { get; set; } = "";
        }
    }
}
