using API.DTO;
using API.Entities;
using API.Helpers;

namespace API.Interface;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByIdAsync(int id);
    Task<AppUser?> GetUserByUserNameAsync(string username);
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMemberAsync(string username);
}