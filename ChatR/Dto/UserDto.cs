using System.ComponentModel.DataAnnotations;

namespace ChatR.Dto;

public class UserDto
{
    [Required] public string Name { get; set; }
}