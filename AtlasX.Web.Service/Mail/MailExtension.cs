using System;
using System.Net.Mail;
using System.Text;

namespace AtlasX.Web.Service.Mail;

public static class MailExtension
{
    /// <summary>Create the MailAddress object from string with 'EMAIL|DISPLAY_NAME' pattern.</summary>
    /// <example>For example:
    /// <code>
    ///     var formAddress = "example@domain.com".ToMailAddress()
    /// </code>
    /// </example>
    public static MailAddress ToMailAddress(this string mail)
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

    /// <summary>Create the mail priority type from string.</summary>
    /// <example>For example:
    /// <code>
    ///     var mailPriority = "High".ToMailPriority()
    /// </code>
    /// </example>
    public static MailPriority ToMailPriority(this string priority)
    {
        if (!Enum.TryParse(priority, out MailPriority mailPriority))
        {
            // Set default priority to normal.
            mailPriority = MailPriority.Normal;
        }

        return mailPriority;
    }
}