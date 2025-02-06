using AtlasX.Web.Service.OAuth.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AtlasX.Web.Service.Repositories;

public class UserInfoFakeRepository : IUserInfoRepository
{
    private readonly List<UserInfo> _userInfos = new();

    public UserInfoFakeRepository()
    {
        // For test only
        _userInfos.Add(new UserInfo { Id = 1, Username = "username1" });
        _userInfos.Add(new UserInfo { Id = 2, Username = "username2" });
        _userInfos.Add(new UserInfo { Id = 3, Username = "username3" });
        _userInfos.Add(new UserInfo { Id = 4, Username = "username4" });
        _userInfos.Add(new UserInfo { Id = 5, Username = "username5" });
    }

    public UserInfo Get(string username, string password, string dataSource)
    {
        return _userInfos.FirstOrDefault(u => u.Username == username);
    }

    public UserInfo Get(int userId)
    {
        return _userInfos.FirstOrDefault(u => u.Id == userId);
    }

    public UserInfo Get(string username)
    {
        return _userInfos.FirstOrDefault(u => u.Username == username);
    }
}