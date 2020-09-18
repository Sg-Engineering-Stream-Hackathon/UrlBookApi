using Microsoft.AspNetCore.Http;
using ModelService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Interfaces
{
    public interface IBookmarkCardSvc
    {
        Task<bool> CreateBookMarkCard(BookmarkCard bookmarkCard);
        //Task<bool> CreateBookMarkCard(IFormCollection formData);
        Task<bool> ApproveBookMarkCard(BookmarkCard card);
        Task<List<BookmarkCard>> GetAllcards();
    }
}
