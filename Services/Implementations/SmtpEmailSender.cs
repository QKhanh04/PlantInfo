using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly string _senderEmail = null!;
        private readonly string _appPassword = null!;

        public SmtpEmailSender(IConfiguration config)
        {
            _senderEmail = config["EmailSettings:SenderEmail"]!;
            _appPassword = config["EmailSettings:AppPassword"]!;
        }


        public bool SendEmail(string to, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_senderEmail, _appPassword),
                    EnableSsl = true,
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("dangcongquockhanh@gmail.com"),
                    Subject = subject,
                    Body = body,
                };
                mailMessage.To.Add(to);
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send OTP error: " + ex.Message);
                return false;
            }
        }

    }
}