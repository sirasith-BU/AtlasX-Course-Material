using AtlasX.Engine.AppSettings;
using AtlasX.Engine.Connector.Models;
using System.Collections.Generic;

namespace AtlasX.Web.Service.Core;

public class AppSettings
{
    public General General { get; set; }
    public OAuth OAuth { get; set; }
    public UM UM { get; set; }
    public LDAP LDAP { get; set; }
    public Database Database { get; set; }
    public FileServer FileServer { get; set; }
    public Email Email { get; set; }

    public Firebase Firebase { get; set; }
}

#region Model & Hard Config

public class General
{
    public string UserIdField { get; set; }
}

public class OAuth : IOAuth
{
    public int AccessTokenExpires { get; set; }
    public int RefreshTokenExpires { get; set; }
    public int AuthorizationCodeExpires { get; set; }
    public string Issuer { get; set; }
    public string SecretKey { get; set; }
    public bool MultiRefreshToken { get; set; }
    public int VerifyCodeExpires { get; set; }

    public string SecretSalt { get; } = "f08cbad583dd7d06744e9265f58af66f";
    public RefreshTokenStrategy Strategy { get; } = RefreshTokenStrategy.Multiple;
}

public class UM
{
    public string ForgetPasswordBaseUrl { get; set; }
    public string ForgetPasswordUserIdField { get; set; }

    public string ForgetPasswordTokenField { get; set; }

    /// <value>
    /// Use default configuration from database. If false, web service will use 
    /// default config from JSON file from <c>/Config/app.config.json</c> for production 
    /// and <c>/Config/app.config.Development.json</c> for development.
    /// </value>
    public bool UseDefaultConfigFromDatabase { get; set; }

    /// <value>
    /// Define role id to get configuration from database, 
    /// it's the <c>Geust User</c> role name by default.
    /// Please confirm with Business Analyst to make sure 
    /// that is correct id within database.
    /// </value>
    public int DefaultConfigRoleId { get; set; }
}

public class LDAP
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool SecureSocketLayer { get; set; }
    public string DistinguishedName { get; set; }
    public string AdminUser { get; set; }
    public string AdminPassword { get; set; }
    public string UserIdField { get; set; }
    public string UsernameField { get; set; }
    public string FirstNameField { get; set; }
    public string LastNameField { get; set; }
    public string MailField { get; set; }
}

public class FileServer
{
    public string FileSourceParameter { get; set; }
    public string FilePathParameter { get; set; }
    public string FileIdParameter { get; set; }
    public string FileListParameter { get; set; }
    public string DefaultFileSource { get; set; }
    public Dictionary<string, FileSource> FileSource { get; set; }
}

public class FileSource
{
    public string RemotePath { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Domain { get; set; }
}

public class Email
{
    public string Server { get; set; }
    public int Port { get; set; }
    public bool EnableSSL { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string SenderAddress { get; set; }
    public string FromParameter { get; set; }
    public string ToParameter { get; set; }
    public string CcParameter { get; set; }
    public string BccParameter { get; set; }
    public string SubjectParameter { get; set; }
    public string BodyParameter { get; set; }
    public string PriorityParameter { get; set; }
}

#endregion