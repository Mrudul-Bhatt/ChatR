using ChatR.Dto;
using ChatR.Service;
using Microsoft.AspNetCore.SignalR;

namespace ChatR.Hubs;

public class ChatHub : Hub
{
    private readonly ChatService _chatService;

    public ChatHub(ChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        // await Groups.AddToGroupAsync(Context.ConnectionId, "Come2Chat");
        await Clients.Caller.SendAsync("UserConnected");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Come2Chat");

        var user = _chatService.GetUserByConnectionId(Context.ConnectionId);
        _chatService.RemoveUser(user);
        await DisplayOnlineUsers();
        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddUserConnectionId(string name)
    {
        _chatService.AddUserConnectionId(name, Context.ConnectionId);
        await DisplayOnlineUsers();
    }

    public async Task ReceiveMessage(MessageDto messageDto)
    {
        var connectionId = _chatService.GetConnectionIdByUser(messageDto.To);

        var messageViewDto = new MessageViewDto()
        {
            Sender = messageDto.From,
            Text = messageDto.Content
        };

        if (connectionId != null) await Clients.Client(connectionId).SendAsync("NewMessage", messageViewDto);
    }

    private async Task DisplayOnlineUsers()
    {
        var onlineUsers = _chatService.GetOnlineUsers();
        await Clients.Groups("Come2Chat").SendAsync("OnlineUsers", onlineUsers);
    }

    public async Task SendMessageToPrivateConnection(string connectionId, string message)
    {
        await Clients.Client(connectionId).SendAsync("ReceiveMessagePrivate", message);
    }
}