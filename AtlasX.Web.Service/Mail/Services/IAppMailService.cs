using System.Net.Mail;
using AtlasX.Engine.Connector;

namespace AtlasX.Web.Service.Mail.Services;

public interface IAppMailService
{
    bool Send(MailMessage mailMessage);

    MailMessage CreateMessageFromRequest(QueryParameter parameter);
}