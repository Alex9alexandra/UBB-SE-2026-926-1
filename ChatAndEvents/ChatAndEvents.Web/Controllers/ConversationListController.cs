using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatAndEvents.Web.Models;
using ChatAndEvents.Data.ChatData.services;
using Microsoft.AspNetCore.Authorization;

namespace ChatAndEvents.Web.Controllers
{
    [Authorize]
    public class ConversationListController : Controller
    {
        private readonly IConversationListService _conversationListService;
        
        private readonly Guid _currentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public ConversationListController(IConversationListService conversationListService)
        {
            _conversationListService = conversationListService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string activeTab = "All", string searchQuery = "")
        {
            var viewModel = new ConversationListViewModel
            {
                ActiveTab = activeTab,
                SearchQuery = searchQuery
            };

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                viewModel.Conversations = await _conversationListService.SearchAsync(_currentUserId, searchQuery);
            }
            else
            {
                viewModel.Conversations = activeTab switch
                {
                    ConversationListViewModel.AllTab => await _conversationListService.GetAllAsync(_currentUserId),
                    ConversationListViewModel.DirectMessagesTab => await _conversationListService.GetDmsAsync(_currentUserId),
                    ConversationListViewModel.GroupsTab => await _conversationListService.GetGroupsAsync(_currentUserId),
                    ConversationListViewModel.FavoritesTab => await _conversationListService.GetFavouritesAsync(_currentUserId),
                    ConversationListViewModel.UnreadTab => await _conversationListService.GetUnreadAsync(_currentUserId),
                    _ => await _conversationListService.GetAllAsync(_currentUserId),
                };
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavourite(Guid conversationId, string currentTab, string currentSearch)
        {
            var favourites = await _conversationListService.GetFavouritesAsync(_currentUserId);
            bool isFavourite = favourites.Exists(c => c.Id == conversationId);
            
            await _conversationListService.SetFavouriteAsync(conversationId, _currentUserId, !isFavourite);
            
            return RedirectToAction("Index", new { activeTab = currentTab, searchQuery = currentSearch });
        }
    }
}