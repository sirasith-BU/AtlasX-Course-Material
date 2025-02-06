using AtlasX.Engine.Constants;
using AtlasX.Web.Service.Core;
using System.Collections.Generic;

namespace AtlasX.Web.Service.OAuth.ProcedureModels;

public class UmUserIParameter
{
    public string AppDataProcedure => ProcedureName.UM_USER_I;

    public string Username { get; set; }
    public string Password { get; set; }
    public int Title { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Dept1 { get; set; }
    public int Dept2 { get; set; }
    public int Dept3 { get; set; }
    public int Dept4 { get; set; }
    public int Position { get; set; }
    public string Address { get; set; }
    public string Tel { get; set; }
    public string Email { get; set; }
    public int Status { get; set; }
    public string Image { get; set; }
    public int Source { get; set; }

    public Dictionary<string, object> ToDictionary(AppSettings appSettings)
    {
        Dictionary<string, object> dictParams = new Dictionary<string, object>
        {
            { appSettings.Database.ProcedureParameter, ProcedureName.UM_USER_I },
            { "USERNAME", Username },
            { "PASSWORD", Password },
            // {"TITLE",Title},
            { "NAME", Name },
            { "SURNAME", Surname },
            // {"DEPT1",Dept1},
            // {"DEPT2",Dept2},
            // {"DEPT3",Dept3},
            // {"DEPT4",Dept4},
            // {"POSITION",Position},
            { "ADDRESS", Address },
            { "TEL", Tel },
            { "EMAIL", Email },
            { "STATUS", Status },
            { "IMAGE", Image },
            { "SOURCE", Source }
        };

        // For clean data in UM_USER table.
        if (Title > 0)
        {
            dictParams.Add("TITLE", Title);
        }

        if (Dept1 > 0)
        {
            dictParams.Add("DEPT1", Dept1);
        }

        if (Dept2 > 0)
        {
            dictParams.Add("DEPT2", Dept2);
        }

        if (Dept3 > 0)
        {
            dictParams.Add("DEPT3", Dept3);
        }

        if (Dept4 > 0)
        {
            dictParams.Add("DEPT4", Dept4);
        }

        if (Position > 0)
        {
            dictParams.Add("POSITION", Position);
        }

        return dictParams;
    }
}