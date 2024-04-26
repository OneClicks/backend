
using backend.Service.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
namespace backend.Service
{


    namespace API.Services
    {
        public class EmailService : IEmailService
        {
            private readonly IConfiguration Config;
            public EmailService(IConfiguration configuration)
            {
                Config = configuration;
            }
            public bool SendEmail(Message message)
            {
                var emailMessage = CreateEmailMessage(message);
                return Send(emailMessage);
            }

            private bool Send(MimeMessage emailMessage)
            {
                using var client = new SmtpClient();
                try
                {
                    client.Connect("smtp.gmail.com", 465, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate("oneclicks.marketing@gmail.com", "posxmqlnqkcguisu");

                    client.Send(emailMessage);

                    client.Disconnect(true);
                    client.Dispose();
                    return true;
                }
                catch
                {
                    client.Disconnect(true);
                    client.Dispose();
                    return false;
                }
            }

            private MimeMessage CreateEmailMessage(Message message)
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("OneClicks - Support", "oneclicks.marketing@gmail.com"));
                emailMessage.To.AddRange(message.To);
                emailMessage.Subject = message.Subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text)
                {
                    Text = message.Content
                };
                return emailMessage;
            }


        }
        public class Message
        {
            public List<MailboxAddress> To { get; set; }
            public string Subject { get; set; }

            public string Content { get; set; }

            public Message(IEnumerable<string> to, string sub, string con)
            {
                To = new List<MailboxAddress>();
                To.AddRange(to.Select(x => new MailboxAddress("OneClicks - Support", x)));
                Subject = sub;
                Content = con;
            }

        }
    }

}
