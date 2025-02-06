using AtlasX.Engine.Connector;
using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppMailController : ControllerBase
{
    private readonly IDiagnosticContext _diagnosticContext;
    private readonly AppSettings _appSettings;
    private const char MailListSplitter = ';';

    public AppMailController(
        IDiagnosticContext diagnosticContext,
        IOptions<AppSettings> appSettings
    )
    {
        _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        _appSettings = appSettings.Value;
    }

    [HttpPost("Send")]
    public IActionResult Send()
    {
        _diagnosticContext.Set("UserId", User.GetUserId());

        var queryParameter = new QueryParameter(Request);
        _diagnosticContext.Set("QueryParameter", queryParameter.Parameters);

        using var mailMessage = new MailMessage();
        // Mail sender address.
        mailMessage.Sender = CreateMailAddress(_appSettings.Email.SenderAddress);

        // Mail from address.
        mailMessage.From = queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.FromParameter)
            ? CreateMailAddress(queryParameter[_appSettings.Email.FromParameter].ToString())
            : mailMessage.Sender;

        //Mail to address.
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.ToParameter))
        {
            var toAddress = queryParameter[_appSettings.Email.ToParameter].ToString()?.Split(MailListSplitter);
            var mail = toAddress?.GetEnumerator();
            while (mail != null && mail.MoveNext())
            {
                if (!string.IsNullOrEmpty(mail.Current?.ToString()) &&
                    !string.IsNullOrWhiteSpace(mail.Current.ToString()))
                {
                    mailMessage.To.Add(CreateMailAddress(mail.Current.ToString()));
                }
            }
        }
        else
        {
            return BadRequest($"The '{_appSettings.Email.ToParameter}' parameter can't be null or empty.");
        }

        //Mail cc address.
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.CcParameter))
        {
            var ccAddress = queryParameter[_appSettings.Email.CcParameter].ToString()?.Split(MailListSplitter);
            var mail = ccAddress?.GetEnumerator();
            while (mail != null && mail.MoveNext())
            {
                if (!string.IsNullOrEmpty(mail.Current?.ToString()) &&
                    !string.IsNullOrWhiteSpace(mail.Current.ToString()))
                {
                    mailMessage.CC.Add(CreateMailAddress(mail.Current.ToString()));
                }
            }
        }

        //Mail bcc address.
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.BccParameter))
        {
            var ccAddress = queryParameter[_appSettings.Email.BccParameter].ToString()?.Split(MailListSplitter);
            var mail = ccAddress?.GetEnumerator();
            while (mail != null && mail.MoveNext())
            {
                if (!string.IsNullOrEmpty(mail.Current?.ToString()) &&
                    !string.IsNullOrWhiteSpace(mail.Current.ToString()))
                {
                    mailMessage.Bcc.Add(CreateMailAddress(mail.Current.ToString()));
                }
            }
        }

        // Attach files.
        if (queryParameter.FileParameters.Count > 0)
        {
            using var file =
                queryParameter.FileParameters.GetEnumerator();

            while (file.MoveNext())
            {
                var attachment = new Attachment(file.Current.ToStream(), file.Current.FileName);
                mailMessage.Attachments.Add(attachment);
            }
        }

        // Subject.
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.SubjectParameter))
        {
            mailMessage.Subject = queryParameter[_appSettings.Email.SubjectParameter].ToString() ?? string.Empty;
            mailMessage.SubjectEncoding = Encoding.UTF8;
        }

        // Body.
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.BodyParameter))
        {
            mailMessage.Body = queryParameter[_appSettings.Email.BodyParameter].ToString() ?? string.Empty;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.IsBodyHtml = true;
        }

        // Priority.
        mailMessage.Priority = queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.PriorityParameter)
            ? PriorityStringToType(queryParameter[_appSettings.Email.PriorityParameter].ToString())
            : MailPriority.Normal;

        try
        {
            using var smtpClient = new SmtpClient(_appSettings.Email.Server, _appSettings.Email.Port);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials =
                new NetworkCredential(_appSettings.Email.Username, _appSettings.Email.Password);
            smtpClient.EnableSsl = _appSettings.Email.EnableSSL;
            smtpClient.Send(mailMessage);

            return Ok();
        }
        catch (Exception ex)
        {
            _diagnosticContext.Set("Exception", ex, true);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }

    private static MailAddress CreateMailAddress(string mail)
    {
        MailAddress mailAddress;
        var mailAddressDetail = mail.Split('|');

        if (mailAddressDetail.Length > 1 && !string.IsNullOrEmpty(mailAddressDetail[1]))
        {
            mailAddress = new MailAddress(mailAddressDetail[0].Trim(), mailAddressDetail[1].Trim(), Encoding.UTF8);
        }
        else
        {
            mailAddress = new MailAddress(mailAddressDetail[0].Trim(), mailAddressDetail[0].Trim(), Encoding.UTF8);
        }

        return mailAddress;
    }

    private static MailPriority PriorityStringToType(string priority)
    {
        if (!Enum.TryParse(priority, out MailPriority mailPriority))
        {
            // Set default priority to normal.
            mailPriority = MailPriority.Normal;
        }

        return mailPriority;
    }
}