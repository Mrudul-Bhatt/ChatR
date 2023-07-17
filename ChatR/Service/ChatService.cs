namespace ChatR.Service;

public class ChatService
{
    private static readonly Dictionary<string, string> users = new();

    public async Task<bool> AddUsersToList(string userToAdd)
    {
        lock (users)
        {
            if (users.Any(user => user.Key.ToLower() == userToAdd.ToLower())) return false;

            users.Add(userToAdd, null);
            return true;
        }
    }

    public void AddUserConnectionId(string user, string connectionId)
    {
        lock (users)
        {
            if (users.ContainsKey(user)) users[user] = connectionId;
        }
    }

    public string? GetUserByConnectionId(string connectionId)
    {
        lock (users)
        {
            return users.Where(x => x.Value == connectionId)
                .Select(x => x.Key).FirstOrDefault();
        }
    }

    public string? GetConnectionIdByUser(string user)
    {
        lock (users)
        {
            return users.Where(x => x.Key == user)
                .Select(x => x.Value).FirstOrDefault();
        }
    }

    public void RemoveUser(string user)
    {
        lock (users)
        {
            if (users.ContainsKey(user)) users.Remove(user);
        }
    }

    public string[] GetOnlineUsers()
    {
        lock (users)
        {
            return users.OrderBy(x => x.Key)
                .Select(x => x.Key).ToArray();
        }
    }
}