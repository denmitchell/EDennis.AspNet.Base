using EDennis.NetApp.Base;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// adapted from https://medium.com/@MisterKevin_js/enabling-email-verification-in-asp-net-core-identity-ui-2-1-b87f028a97e0
    /// </summary>
    public class EmailSender : IEmailSender {

        // Our private configuration variables
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSSL;
        private readonly string _userName;
        private readonly string _password;

        // Get our parameterized configuration
        public EmailSender(IOptionsMonitor<EmailSenderOptions> options) {
            _host = options.CurrentValue.SmtpHost;
            _port = options.CurrentValue.SmtpPort;
            _enableSSL = options.CurrentValue.EnableSSL;
            _userName = options.CurrentValue.UserName;
            _password = options.CurrentValue.Password;
        }

        /// <summary>
        /// Sends an email 
        /// </summary>
        /// <param name="emails">semicolon-delimited list of email recipients.
        /// If an address ends with "**", it is BCC'd; 
        /// otherwise, if it ends with "*", it is CC'd;
        /// otherwise, it is sent as TO
        /// </param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns></returns>
        public Task SendEmailAsync(string emails, string subject, string htmlMessage) {

            var emailList = emails.Split(';');
            var bcc = emailList.Where(e => e.EndsWith("**")).Select(e=>e[0..^2]);
            var cc = emailList.Where(e => e.EndsWith("*") && !e.EndsWith("**")).Select(e => e[0..^1]);
            var to = emailList.Where(e => !e.EndsWith("*"));

            var client = new SmtpClient(_host, _port) {
                Credentials = new NetworkCredential(_userName, _password),
                EnableSsl = _enableSSL
            };

            var msg = new MailMessage();

            msg.To.AddRange(to);
            msg.CC.AddRange(cc);
            msg.Bcc.AddRange(bcc);
            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = htmlMessage;

            return client.SendMailAsync(msg);
        }


    }

    internal static class MailAddressCollectionExtensions {
        public static void AddRange(this MailAddressCollection mac, IEnumerable<string> addresses) {
            foreach (var addr in addresses)
                mac.Add(addr);
        }
    }

}
