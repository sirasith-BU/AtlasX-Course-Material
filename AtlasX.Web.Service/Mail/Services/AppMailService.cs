using System;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Text;
using AtlasX.Engine.Connector;
using AtlasX.Web.Service.Core;
using Microsoft.Extensions.Options;
using Serilog;

namespace AtlasX.Web.Service.Mail.Services;

public class AppMailService : IAppMailService
{
    private readonly AppSettings _appSettings;
    private const char MailListSplitter = ';';

    public AppMailService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    public bool Send(MailMessage mailMessage)
    {
        try
        {
            using var smtpClient = new SmtpClient(_appSettings.Email.Server, _appSettings.Email.Port);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials =
                new NetworkCredential(_appSettings.Email.Username, _appSettings.Email.Password);
            smtpClient.EnableSsl = _appSettings.Email.EnableSSL;
            smtpClient.Send(mailMessage);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"AppMailService: {ex.Message}");
            return false;
        }
    }

    public MailMessage CreateMessageFromRequest(QueryParameter queryParameter)
    {
        var mailMessage = new MailMessage();

        // Mail sender address.
        mailMessage.Sender = _appSettings.Email.SenderAddress.ToMailAddress();

        // Mail from address. User default from sender.
        mailMessage.From = mailMessage.Sender;
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.FromParameter))
        {
            mailMessage.From = queryParameter[_appSettings.Email.FromParameter].ToString().ToMailAddress();
        }

        //Mail to address.
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.ToParameter))
        {
            AddMailAddress(mailMessage.To, queryParameter[_appSettings.Email.ToParameter].ToString());
        }
        else
        {
            return null;
        }

        //Mail cc address.
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.CcParameter))
        {
            AddMailAddress(mailMessage.CC, queryParameter[_appSettings.Email.CcParameter].ToString());
        }

        //Mail bcc address.
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.BccParameter))
        {
            AddMailAddress(mailMessage.Bcc, queryParameter[_appSettings.Email.BccParameter].ToString());
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
        mailMessage.Priority = MailPriority.Normal;
        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_appSettings.Email.PriorityParameter))
        {
            mailMessage.Priority = queryParameter[_appSettings.Email.PriorityParameter].ToString().ToMailPriority();
        }

        return mailMessage;
    }


    private static void AddMailAddress(MailAddressCollection mailAddressCollection, string mailListStr)
    {
        var mailAddresses = mailListStr.Split(MailListSplitter);
        var mail = mailAddresses.GetEnumerator();

        while (mail.MoveNext())
        {
            if (mail.Current != null && !string.IsNullOrEmpty(mail.Current.ToString()) &&
                !string.IsNullOrWhiteSpace(mail.Current.ToString()))
            {
                mailAddressCollection.Add(mail.Current.ToString().ToMailAddress());
            }
        }
    }
}