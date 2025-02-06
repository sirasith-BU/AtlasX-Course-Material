using AtlasX.Web.Service.Notification.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AtlasX.Web.Service.Notification.Services;

public abstract class NotificationServiceBase
{
    protected static async Task<int> SendMessageAsync(DataTable dataTable, IUserTokenRepository userTokenRepository,
        Func<List<string>, NotificationMessage, Task<int>> sendMessageAsync)
    {
        var rows = dataTable.Select();
        var payloadColumns = dataTable.Columns.Cast<DataColumn>()
            .ToList()
            .Where(c => !c.ColumnName.StartsWith("NOTI_"))
            .Select(c => c.ColumnName);

        var messages = rows.Select(row => new NotificationMessage
            {
                UserId = (int)row["NOTI_USER_ID"],
                Title = row["NOTI_TITLE"]?.ToString() ?? "",
                Body = row["NOTI_MESSAGE"]?.ToString() ?? "",
                Badge = int.Parse(row["NOTI_BADGE"]?.ToString() ?? "1"),
                Icon = row["NOTI_ICON"]?.ToString(),
                Sound = row["NOTI_SOUND"]?.ToString(),
                Payload = payloadColumns.ToDictionary(c => c, c => row[c].ToString())
            })
            .ToList();

        // TODO: หาแนวทางปรับ Performance โดยการ Group Message ที่เหมือนกัน
        var totalSuccess = 0;
        foreach (NotificationMessage m in messages)
        {
            var tokens = userTokenRepository.GetRefreshTokens(m.UserId).ToList();
            totalSuccess += await sendMessageAsync(tokens, m);
        }

        return totalSuccess;
    }
}