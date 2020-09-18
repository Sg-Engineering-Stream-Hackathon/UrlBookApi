using DataService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelService;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services
{
    public class BookMarkCardSvc :IBookmarkCardSvc
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private string[] UserRoles = new[] { "Administrator", "User" };
        private readonly ApplicationDbContext _db;
        public BookMarkCardSvc(UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }
        public async Task<bool> CreateBookMarkCard(BookmarkCard bookmarkCard)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(bookmarkCard.UserName);
                bookmarkCard.IsCardValidationRequired = !user.isAdmin; //if admin card needs no validation, if user then it needs validation
                bookmarkCard.IsCardExpired = false;
                bookmarkCard.shortUrl = GetShorturl(bookmarkCard.LongUrl);
                await _db.BookMarkCards.AddAsync(bookmarkCard);
                // persist changes in the DB
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while saving the new bookmark card  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return false;
        }

        public static string GetShorturl(string longurl)
        {
            string shorturl;
            string GoogleAPIkey = "AIzaSyBOx-6YrboiToLCW69ArbvoTLavl_oWhFY";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://firebasedynamiclinks.googleapis.com/v1/shortLinks?key=" + GoogleAPIkey);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            const string myFBDomain = "https://urlcatalog.page.link/?link=";

            string longUrl = myFBDomain + longurl;

            // string longurl = "https://YOURDOMAIN.page.link/?link=https://www.YOURLONGURL.com/account?g=sdasdhaskjdh4547576aksgskjg";


            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"longDynamicLink\":\"" + longUrl + "\"}";
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                dynamic data = JObject.Parse(responseText);
                shorturl = data["shortLink"]?.Value;
            }

            return shorturl;
        }

        public async Task<bool> ApproveBookMarkCard(BookmarkCard card)
        {
            try
            {
                var resultCard = await _db.BookMarkCards.FindAsync(card.BookmarkId);
                if(resultCard !=null)
                {
                    resultCard.IsCardValidationRequired = false;
                    _db.BookMarkCards.Update(resultCard);
                    // persist changes in the DB
                    await _db.SaveChangesAsync();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while saving the new bookmark card  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return false;
        }

        public async Task<List<BookmarkCard>> GetAllcards()
        {
            List<BookmarkCard> lstallCards = new List<BookmarkCard>();
            foreach (var card in _db.BookMarkCards)
            {
                if(card?.ExpiryDate <= System.DateTime.Now)
                {
                    card.IsCardExpired = true;
                }

                lstallCards.Add(card);
            }
            return lstallCards;
        }
    }
}
