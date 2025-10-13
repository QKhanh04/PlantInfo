using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.Services.Interfaces
{
    public interface IEmailSender
    {
        bool SendEmail(string to, string subject, string body);

    }
}