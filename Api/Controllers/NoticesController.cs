using Api.Data;
using Api.Models;
using Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticesController : ControllerBase
    {
        private readonly AuthDbContext _authDbContext;

        public NoticesController(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Client")]
        public IActionResult GetAllNotices()
        {
            var notices = _authDbContext.Notices.ToList();
            return Ok(notices);
        }

       

       
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetNoticeById(int id)
        {
            var notice =
                _authDbContext.Notices.Find(id);

            if (notice is null)
            {
                return NotFound();
            }

            return Ok(notice);
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AddNotices(AddNoticesDTO entity)
        {     
            var notice =
                new Notices
                {  
                    NoticeMessage = entity.NoticeMessage
                };
                        
            _authDbContext.Notices.Add(notice);
            _authDbContext.SaveChanges();

            return Ok(notice);
        }

        
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateNotice(int id,UpdateNoticesDTO noticesEntity)
        {
            var notice =_authDbContext.Notices.Find(id);
            if (notice is null)
            {
                return NotFound();
            }

            notice.NoticeMessage = noticesEntity.NoticeMessage;

            _authDbContext.SaveChanges();

            return Ok(notice);
        }

       
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteNotice(int id)
        {
            var notice = _authDbContext.Notices.Find(id);
            if (notice is null)
            {
                return NotFound();
            }

            _authDbContext.Notices.Remove(notice);
            _authDbContext.SaveChanges();

            return Ok();
        }
    }
}
   
 