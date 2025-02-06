using AtlasX.Web.Service.OAuth.Models;

namespace AtlasX.Web.Service.OAuth.Repositories.Interfaces;

public interface IUserInfoRepository
{
    UserInfo Get(string username, string password, string dataSource);
    UserInfo Get(int userId);
    UserInfo Get(string username);
}