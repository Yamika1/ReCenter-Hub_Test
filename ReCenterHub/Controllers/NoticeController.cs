using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReCenterHub.Models;
using ReCenterHub.Services;


namespace ReCenterHub.Controllers
{
    [Authorize(Roles = "Client,Admin")]
    public class NoticeController : Controller
    {
        private readonly NoticeService _noticeService;
        
        public NoticeController(NoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(AdminIndex));
            }
            else if(User.IsInRole("Client"))
            {
                return RedirectToAction(nameof(ClientIndex));
            }
            else
            {
                return Forbid();
            }
        }

        public async Task<IActionResult> AdminIndex()
        {
            var notices = await _noticeService.GetAllNoticesAsync();
            return View(notices);
        }

        public async Task<IActionResult> ClientIndex()
        {
            var notices = await _noticeService.GetAllNoticesAsync();
            return View(notices);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NoticeId,NoticeMessage")] Notices notices )
        {
            if (ModelState.IsValid)
            {
                await _noticeService.CreateAsync(notices);
                TempData["SuccessMessage"] = "A notice has been created";
                return RedirectToAction(nameof(Index));
            }
            return View(notices);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notice = await _noticeService.GetNoticeByIdAsync(id.Value);
            if (notice == null)
            {
                return NotFound();
            }
            return View(notice);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NoticeId,NoticeMessage")] Notices notices)
        {
            
            if (ModelState.IsValid)
            {
              await _noticeService.UpdateAsync(notices);
               TempData["SuccessMessage"] = "A notice has been updated";
                return RedirectToAction(nameof(Index));
            }
            return View(notices);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var notice = await _noticeService.GetNoticeByIdAsync(id);
            if (notice != null)
            {
                await _noticeService.Delete(notice);
            }
            TempData["SuccessMessage"] = "A notice has been deleted";
            return RedirectToAction(nameof(Index));
        }


    }
}
