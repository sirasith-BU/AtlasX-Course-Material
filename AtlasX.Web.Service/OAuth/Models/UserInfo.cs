using System.Collections.Generic;

namespace AtlasX.Web.Service.OAuth.Models;

public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; }
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

    public UserInfo()
    {
    }

    public UserInfo(Dictionary<string, object> user)
    {
        Id = int.Parse(user.GetValueOrDefault("USER_ID")?.ToString() ?? "0");
        Username = user.GetValueOrDefault("USERNAME")?.ToString();
        Title = int.Parse(user.GetValueOrDefault("TITLE")?.ToString() ?? "0");
        Name = user.GetValueOrDefault("NAME")?.ToString();
        Surname = user.GetValueOrDefault("SURNAME")?.ToString();
        Dept1 = int.Parse(user.GetValueOrDefault("DEPT1")?.ToString() ?? "0");
        Dept2 = int.Parse(user.GetValueOrDefault("DEPT2")?.ToString() ?? "0");
        Dept3 = int.Parse(user.GetValueOrDefault("DEPT3")?.ToString() ?? "0");
        Dept4 = int.Parse(user.GetValueOrDefault("DEPT4")?.ToString() ?? "0");
        Position = int.Parse(user.GetValueOrDefault("POSITION")?.ToString() ?? "0");
        Address = user.GetValueOrDefault("ADDRESS")?.ToString();
        Tel = user.GetValueOrDefault("TEL")?.ToString();
        Email = user.GetValueOrDefault("EMAIL")?.ToString();
        Status = int.Parse(user.GetValueOrDefault("STATUS")?.ToString() ?? "0");
        Image = user.GetValueOrDefault("IMAGE")?.ToString();
        Source = int.Parse(user.GetValueOrDefault("SOURCE")?.ToString() ?? "0");
    }
}