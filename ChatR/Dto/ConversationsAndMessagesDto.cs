namespace ChatR.Dto;

public class ConversationsAndMessagesDto
{
    public string ConversationId { get; set; }
    public string Name { get; set; }
    public List<MessageViewDto> Messages { get; set; }
}