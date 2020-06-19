using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.AspNetIdentityServer.Models {
    public class MockEmailSender : IEmailSender {

        private readonly ILogger<MockEmailSender> _logger;

        public MockEmailSender(ILogger<MockEmailSender> logger) {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage) {
            _logger.LogInformation("Email sent to {email} with subject {subject}");
            return Task.CompletedTask;
        }
    }
}
