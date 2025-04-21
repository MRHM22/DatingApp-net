using API.DTO;
using API.Entities;
using API.Extensions;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub(IUnitOfWork unitToWork, IMapper mapper,
    IHubContext<PresenceHub> presentHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"];

        if(Context.User == null || string.IsNullOrEmpty(otherUser))
            throw new System.Exception("Cannot join group"); 
        var groupName = GetGroupName(Context.User.GetUsername(),otherUser); 
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group  = await AddToGroup(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup",group);

        var messages = await unitToWork.MessageRepository.GetMessagesThread(Context.User.GetUsername(),otherUser!); 

        if(unitToWork.HasChanges()) await unitToWork.Complete();
        await Clients.Caller.SendAsync("ReceiveMessageThread",messages);
    }

    public override async Task OnDisconnectedAsync(System.Exception? exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup",group);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto){
        var username = Context.User?.GetUsername() ?? throw new System.Exception("could not get user");

        if(username == createMessageDto.RecipientUsername.ToLower())
            throw new HubException("You cannot message yourself");

        var sender =  await unitToWork.UserRepository.GetUserByUserNameAsync(username);
        var recipient = await unitToWork.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

        if(recipient == null || sender ==null || sender.UserName ==null || recipient.UserName ==null ) 
            throw new HubException("Cannot send message at this time");

        var message = new Message{
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName,recipient.UserName);
        var group = await unitToWork.MessageRepository.GetMessageGroup(groupName);

        if(group != null && group.Connections.Any(x => x.Username == recipient.UserName)){
            message.DateRead = DateTime.UtcNow;
        }else{
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if(connections != null && connections?.Count != null){
                await presentHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                    new {username = sender.UserName, knownAs = sender.KnownAs});
            }
        }

        unitToWork.MessageRepository.AddMessage(message);
        if(await unitToWork.Complete()){
            await Clients.Group(groupName).SendAsync("New Message", mapper.Map<MessageDto>(message));
        }

    }

    private async Task<Group> AddToGroup(string groupName){
        var username = Context.User?.GetUsername() ?? throw new System.Exception("Cannot get usernanme");
        var group = await unitToWork.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection{ConnectionId = Context.ConnectionId, Username =username};

        if(group == null){
            group = new Group{Name = groupName};
            unitToWork.MessageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);
        if( await unitToWork.Complete()) return group;

        throw new HubException("Failed to join group");
    }

    private async Task<Group> RemoveFromMessageGroup(){
        var group =  await unitToWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection =group?.Connections.FirstOrDefault(x=> x.ConnectionId == Context.ConnectionId);
        if(connection != null && group != null) {
            unitToWork.MessageRepository.RemoveConnection(connection);
            if (await unitToWork.Complete()) return group;
        }
        throw new System.Exception("Failed to remove from group");
    }

    private string GetGroupName(string caller, string? other){
        var stringCompare = string.CompareOrdinal(caller,other)<0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}