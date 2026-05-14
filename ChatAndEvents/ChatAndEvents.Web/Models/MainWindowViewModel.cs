using System;

namespace ChatAndEvents.Web.Models;

public class MainWindowViewModel
{
    public Guid CurrentUserId { get; set; }

    public string CurrentUsername { get; set; } = string.Empty;

    public string ActiveSection { get; set; } = "Conversations";
}
