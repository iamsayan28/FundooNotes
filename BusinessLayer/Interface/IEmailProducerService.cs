using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Models.Email;

namespace BusinessLayer.Interface
{
    public interface IEmailProducerService
    {
        public Task EnqueueAsync(EmailMessage message);
    }
}
