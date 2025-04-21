using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Tree;

namespace API.Controllers;

[Authorize]
public class MessageController(IUnitOfWork unitToWork , IMapper mapper): BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto){
        var username = User.GetUsername();

        if(username == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You cannot message yourself");

        var sender =  await unitToWork.UserRepository.GetUserByUserNameAsync(username);
        var recipient = await unitToWork.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

        if(recipient == null || sender ==null || sender.UserName ==null || recipient.UserName ==null ) 
            return BadRequest("Cannot send message at this time");

        var message = new Message{
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        unitToWork.MessageRepository.AddMessage(message);
        if(await unitToWork.Complete()) return Ok(mapper.Map<MessageDto>(message));

        return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageFprUser(
        [FromQuery]MessageParams messageParams )
    {
        messageParams.Username = User.GetUsername();
        var messages = await unitToWork.MessageRepository.GetMessagesForUser(messageParams);
        Response.AddPaginationHeader(messages);
        return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username){
        var currentUsername = User.GetUsername();

        return Ok(await unitToWork.MessageRepository.GetMessagesThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id){
        var username = User.GetUsername();
        var message = await unitToWork.MessageRepository.GetMessage(id);

        if(message == null) return BadRequest("Cannot delete this message");
        if(message.SenderUsername != username || message.RecipientUsername != username) return Forbid();

        if(message.SenderUsername == username ) message.SenderDeleted = true;
        if(message.RecipientUsername == username ) message.RecipientDeleted = true;

        if(message is {SenderDeleted: true, RecipientDeleted: true}){
            unitToWork.MessageRepository.DeleteMessage(message);
        }

        if(await unitToWork.Complete()) return Ok();

        return BadRequest("Problem deleting the message");
    }

}