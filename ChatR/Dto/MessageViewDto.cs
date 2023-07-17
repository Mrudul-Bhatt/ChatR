using System;
using System.ComponentModel.DataAnnotations;

namespace ChatR.Dto;

public class MessageViewDto
{
	[Required]
	public string Sender { get; set; }

	[Required]
	public string Text { get; set; }
}

