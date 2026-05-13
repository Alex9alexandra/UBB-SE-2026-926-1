using System;

namespace Events_GSS.Data.Services.userServices;

public class CurrentUserContext
{
    public CurrentUserContext(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}
