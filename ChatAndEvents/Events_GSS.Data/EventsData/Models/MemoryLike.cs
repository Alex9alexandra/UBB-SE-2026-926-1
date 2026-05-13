using System;

namespace Events_GSS.Data.Models;

public class MemoryLike
{
    public int MemoryId { get; set; }

    public Guid UserId { get; set; }

    public Memory Memory { get; set; } = null!;

    public User User { get; set; } = null!;
}
