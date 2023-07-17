using ChatR.Data;
using ChatR.Dto;
using MongoDB.Driver;

namespace ChatR.Service;

public class ChatDBService
{
    private readonly IMongoCollection<Conversation> _conversationsCollections;
    private readonly IMongoCollection<Message> _messagesCollections;
    private readonly MongoDbContext _mongoContext;
    private readonly IMongoCollection<UserConversation> _userConversationsCollections;
    private readonly IMongoCollection<User> _usersCollection;

    public ChatDBService(MongoDbContext mongoContext)
    {
        _mongoContext = mongoContext;
        var database = _mongoContext.GetDatabase("ChatDB");
        _usersCollection = database.GetCollection<User>("users");
        _conversationsCollections = database.GetCollection<Conversation>("conversations");
        _userConversationsCollections = database.GetCollection<UserConversation>("user_conversations");
        _messagesCollections = database.GetCollection<Message>("messages");
    }

    public async Task<List<ActiveConversationDto>> GetInitiatedActiveConversations(string user)
    {
        var userFromDb = await _usersCollection.Find(x => x.Username == user).FirstOrDefaultAsync();

        var result = (from ucc in _userConversationsCollections.AsQueryable()
            join cc in _conversationsCollections.AsQueryable() on ucc.ConversationId equals cc.Id
            where ucc.UserId == userFromDb.Id
            select new ActiveConversationDto
            {
                ConversationId = cc.Id,
                Name = cc.Name
            }).ToList();

        return result;
    }

    public async Task<List<ActiveConversationDto>> GetNonInitiatedActiveConversations(string username)
    {
        var result = (from cc in _conversationsCollections.AsQueryable()
            join ucc in _userConversationsCollections.AsQueryable() on cc.Id equals ucc.ConversationId
            where cc.Name == username
            join u in _usersCollection.AsQueryable() on ucc.UserId equals u.Id
            select new ActiveConversationDto
            {
                ConversationId = cc.Id,
                Name = u.Username
            }).ToList();

        return result;
    }

    public async Task<List<User>> GetUserByName(string name)
    {
        var users = await _usersCollection.Find(x => x.Username == name).ToListAsync();
        return users;
    }

    public async Task<ActiveConversationDto> CreateConversation(string receiverName, string user)
    {
        var conversation = new Conversation
        {
            Name = receiverName,
            CreatedAt = DateTime.Now
        };

        await _conversationsCollections.InsertOneAsync(conversation);

        var userModel = await _usersCollection.Find(x => x.Username == user).FirstOrDefaultAsync();

        var userConversation = new UserConversation
        {
            ConversationId = conversation.Id,
            UserId = userModel.Id
        };

        await _userConversationsCollections.InsertOneAsync(userConversation);

        return new ActiveConversationDto { ConversationId = conversation.Id, Name = receiverName };
    }

    public async Task<bool> LoadMessageOnDBQueue(string conversationId, string senderId, string messageContent)
    {
        // send all types of messages to rabbitmq

        var user = await _usersCollection.Find(x => x.Username == senderId).FirstOrDefaultAsync();

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = user.Id,
            Content = messageContent,
            CreatedAt = DateTime.Now
        };

        await _messagesCollections.InsertOneAsync(message);
        return true;
    }

    public async Task<List<MessageViewDto>> RetrievePrevious20Messages(string conversationId, int pageNo, int pageSize,
        string loggedInUser)
    {
        var usersInvolved = _messagesCollections.AsQueryable()
            .Where(x => x.ConversationId == conversationId)
            .GroupBy(d => d.SenderId)
            .Select(g => g.First())
            .ToList();

        if (usersInvolved.Count == 0) return new List<MessageViewDto>();

        var user1 = _usersCollection.AsQueryable().FirstOrDefault(x => x.Id == usersInvolved[0].SenderId);

        var user2 = usersInvolved.Count == 2
            ? _usersCollection.AsQueryable().FirstOrDefault(x => x.Id == usersInvolved[1].SenderId)
            : null;

        var messages = _messagesCollections.AsQueryable()
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.CreatedAt)
            .Skip((pageNo - 1) * pageSize)
            .Take(pageSize).ToList();

        var messageView = messages.Select(m => new MessageViewDto
        {
            Sender = m.SenderId == user1.Id ? user1.Username == loggedInUser ? "You" : user1.Username :
                user2.Username == loggedInUser ? "You" : user2.Username,
            Text = m.Content
        }).ToList();


        return messageView;
    }

    public async Task<List<ConversationsAndMessagesDto>> GetConversationsAndMessages(string user)
    {
        var list1 = await GetInitiatedActiveConversations(user);
        var list2 = await GetNonInitiatedActiveConversations(user);

        list1.AddRange(list2);

        var list = new List<ConversationsAndMessagesDto>();

        foreach (var conv in list1)
        {
            var messages = await RetrievePrevious20Messages(conv.ConversationId, 1, 20, user);
            list.Add(new ConversationsAndMessagesDto
            {
                ConversationId = conv.ConversationId,
                Name = conv.Name,
                Messages = messages
            });
        }

        return list;
    }
}