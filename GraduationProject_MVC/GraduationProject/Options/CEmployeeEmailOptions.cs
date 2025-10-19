namespace GraduationProject.Options
{
    public class CEmployeeEmailOptions
    {
        public string FromName { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public SmtpOptions Smtp { get; set; } = new();
        public class SmtpOptions
        {
            public string Host { get; set; } = "smtp.gmail.com";
            public int Port { get; set; } = 587; // 587=STARTTLS, 465=SSL
            public string User { get; set; } = "";
            public string Password { get; set; } = "";
        }
    }
}
