using AtlasX.Engine.Connector;
using AtlasX.Engine.Extensions;
using AtlasX.Engine.Reports;
using AtlasX.Web.Service.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Telerik.Reporting.Processing;
using Telerik.Reporting.Services;
using Telerik.Reporting.Services.AspNetCore;

namespace AtlasX.Web.Service.Controllers;

// [Authorize]
[Route("api/reports")]
public class ReportsController : ReportsControllerBase
{
    private readonly IReportServiceConfiguration _reportServiceConfiguration;
    private readonly AppSettings _appSettings;

    public ReportsController(
        IReportServiceConfiguration reportServiceConfiguration,
        IOptions<AppSettings> appSettings
    )
        : base(reportServiceConfiguration)
    {
        _reportServiceConfiguration = reportServiceConfiguration;
        _appSettings = appSettings.Value;
    }

    [HttpGet("view")]
    [HttpPost("view")]
    public async Task<IActionResult> ViewReportAsync()
    {
        return await ViewReport();
    }

    [HttpGet("export")]
    [HttpPost("export")]
    public async Task<IActionResult> ExportReportAsync()
    {
        return await ExportReport();
    }

    [HttpGet("view")]
    [HttpPost("view")]
    private async Task<IActionResult> ViewReport()
    {
        // Convert http request content to query parameter.
        var queryParameter = new QueryParameter(Request);

        if (!queryParameter.ContainsKeyAndValueIsNotEmpty("REPORT_CONFIG"))
        {
            return BadRequest($"The 'REPORT_CONFIG' can't be undefined or empty value.");
        }

        // Deserialize json string to object model.
        var reportConfigStream = queryParameter["REPORT_CONFIG"].ToStream();
        var reportConfig = await JsonSerializer.DeserializeAsync<ReportConfig>(reportConfigStream);

        if (string.IsNullOrEmpty(reportConfig.TemplateName))
        {
            return BadRequest(
                $"The 'REPORT_CONFIG' not found 'template_name' element or can't be undefined or empty value.");
        }

        ViewData["TemplateName"] = reportConfig.TemplateName;
        ViewData["ReportViewerTitle"] = reportConfig.ReportViewerTitle;

        if (queryParameter.ContainsKeyAndValueIsNotEmpty("REPORT_PARAMS"))
        {
            ViewData["Parameters"] = queryParameter["REPORT_PARAMS"].ToString();
        }

        return View("ViewReport");
    }

    private async Task<IActionResult> ExportReport()
    {
        // Convert http request content to query parameter.
        var queryParameter = new QueryParameter(Request);

        if (!queryParameter.ContainsKeyAndValueIsNotEmpty("REPORT_CONFIG"))
        {
            return BadRequest($"The 'REPORT_CONFIG' can't be undefined or empty value.");
        }

        // Deserialize json string to object model.
        var reportConfigStream = queryParameter["REPORT_CONFIG"].ToStream();
        var reportConfig = await JsonSerializer.DeserializeAsync<ReportConfig>(reportConfigStream);

        if (string.IsNullOrEmpty(reportConfig.TemplateName))
        {
            return BadRequest(
                $"The 'REPORT_CONFIG' not found 'template_name' element or can't be undefined or empty value.");
        }

        ViewData["TemplateName"] = reportConfig.TemplateName;

        var reportProcessor = new ReportProcessor();
        var deviceInfo = new Hashtable();
        var currentParameterValues = new Dictionary<string, object>();
        var reportSource = _reportServiceConfiguration.ReportSourceResolver.Resolve(
            reportConfig.TemplateName,
            OperationOrigin.ResolveReportParameters,
            currentParameterValues
        );

        if (queryParameter.ContainsKeyAndValueIsNotEmpty("REPORT_PARAMS"))
        {
            var reportParameterStream = queryParameter["REPORT_PARAMS"].ToStream();
            var reportParameter =
                await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(reportParameterStream);
            foreach (var parameter in reportParameter)
            {
                reportSource.Parameters.Add(parameter.Key, parameter.Value.ToString());
            }
        }

        var result = reportProcessor.RenderReport(reportConfig.ExportExtension, reportSource, deviceInfo);
        var contentDisposition = new ContentDisposition
        {
            Inline = true,
            FileName = $"{result.DocumentName}.{result.Extension}"
        };
        Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

        return File(result.DocumentBytes, result.MimeType);
    }

    protected override HttpStatusCode SendMailMessage(MailMessage mailMessage)
    {
        using var smtpClient = new SmtpClient(_appSettings.Email.Server, _appSettings.Email.Port);
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential(_appSettings.Email.Username, _appSettings.Email.Password);
        smtpClient.EnableSsl = _appSettings.Email.EnableSSL;
        smtpClient.Send(mailMessage);

        return HttpStatusCode.OK;
    }
}