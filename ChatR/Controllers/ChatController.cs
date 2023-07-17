using ChatR.Dto;
using ChatR.Service;
using Microsoft.AspNetCore.Mvc;

namespace ChatR.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly ChatDBService _chatDbService;
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService, ChatDBService chatDbService)
    {
        _chatService = chatService;
        _chatDbService = chatDbService;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterUser(UserDto user)
    {
        if (await _chatService.AddUsersToList(user.Name)) return NoContent();

        return BadRequest("This name is taken");
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveConversations(string user)
    {
        return Ok(await _chatDbService.GetInitiatedActiveConversations(user));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllActiveConversations(string user)
    {
        var initiatedByUser = await _chatDbService.GetInitiatedActiveConversations(user);
        var nonInitiatedByUser = await _chatDbService.GetNonInitiatedActiveConversations(user);

        initiatedByUser.AddRange(nonInitiatedByUser);
        return Ok(initiatedByUser);
    }

    [HttpGet]
    public async Task<IActionResult> GetConversationsAndMessageHistory(string user)
    {
        return Ok(await _chatDbService.GetConversationsAndMessages(user));
    }

    [HttpGet]
    public async Task<IActionResult> GetUserByName(string name)
    {
        var users = await _chatDbService.GetUserByName(name);
        return Ok(users);
    }

    [HttpGet]
    public async Task<IActionResult> GetLimitedConversationById(string conversationId, int pageNo, string loggedInUser)
    {
        var messages = await _chatDbService.RetrievePrevious20Messages(conversationId, pageNo, 20, loggedInUser);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation(string receiverName, string user)
    {
        var newConvo = await _chatDbService.CreateConversation(receiverName, user);
        return Ok(newConvo);
        //return Ok(await _chatService.GetConnectionIdByUser(receiverName));
    }

    [HttpPost]
    public async Task<IActionResult> LoadMessageOnDBQueue(string conversationId, string senderId, string content)
    {
        return Ok(await _chatDbService.LoadMessageOnDBQueue(conversationId, senderId, content));
    }
}