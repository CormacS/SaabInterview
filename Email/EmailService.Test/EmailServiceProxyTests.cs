using System;
using NUnit.Framework;

namespace EmailService.Test
{
    [TestFixture]
    public class EmailServiceProxyTests
    {
        [Test]
        public void ShallThrowExceptionOnNullIncidentTitle()
        {
            var proxy = new EmailServiceProxy();
            Assert.That(() => proxy.SendEmailToAdministrator(null, null), Throws.TypeOf<ArgumentNullException>());
        }
    }
}
