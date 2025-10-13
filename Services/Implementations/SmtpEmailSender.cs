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

        public bool SendEmail(string to, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("dangcongquockhanh@gmail.com", "zvax jepv slof ipgq"),
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