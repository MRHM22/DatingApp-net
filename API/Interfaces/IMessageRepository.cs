using API.DTO;
using API.Entities;
using API.Helpers;

namespace API.Interface;

public interface IMessageRepository
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message?> GetMessage(int id);
    Task<PagedList<MessageDto>> GetMessagesForUser();
    Task<IEnumerable<MessageDto>> GetMessagesThread(string currentUsername, string recipientUsername);
    Task<bool> SaveAllAsync();
}