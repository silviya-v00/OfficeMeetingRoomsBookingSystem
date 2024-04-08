using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeMeetingRoomsBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeMeetingRoomsBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger,
                              SignInManager<ApplicationUser> signInManager,
                              UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public RedirectToActionResult RedirectToHome()
        {
            return RedirectToAction("MeetingRoomsMap", "Home");
        }

        public async Task<IActionResult> LoginPage()
        {
            var userName = User.Identity.Name;
            var existingUser = await _userManager.FindByNameAsync(String.IsNullOrEmpty(userName) ? "" : userName);

            if (existingUser != null)
                return RedirectToHome();
            else
                return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                var result = await _signInManager.PasswordSignInAsync(existingUser.UserName, password, false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToHome();
                }
            }

            ViewData["ErrorMessage"] = "Invalid login attempt.";
            return View("LoginPage", ViewBag.ActiveTab = "login");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string firstName, string lastName, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ViewData["ErrorMessage"] = "Email is already registered.";
                return View("LoginPage", ViewBag.ActiveTab = "register");
            }

            var newUser = new ApplicationUser { UserName = email, Email = email, FirstName = firstName, LastName = lastName };
            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                return RedirectToHome();
            }
            else
            {
                List<IdentityError> errorList = result.Errors.ToList();
                var errors = string.Join("<br/>", errorList.Select(e => e.Description));
                ViewData["ErrorMessage"] = errors;
                return View("LoginPage", ViewBag.ActiveTab = "register");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("LoginPage", "Home");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MeetingRoomsMap()
        {
            List<int> floors = new List<int> { 1, 2, 3 };

            ViewBag.Floors = floors;

            return View();
        }

        [HttpGet]
        public IActionResult GetTimeSlots(string selectedDateStr)
        {
            List<string> timeSlots = new List<string>();
            DateTime selectedDate;
            if (DateTime.TryParse(selectedDateStr, out selectedDate))
            {
                DateTime today = DateTime.Today;
                DateTime currentTime = DateTime.Now;
                DateTime startTime = selectedDate.Date;

                if (selectedDate.Date == today && currentTime.Hour < 23)
                {
                    startTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, currentTime.Hour + 1, 0, 0);
                }

                while (startTime < selectedDate.Date.AddDays(1))
                {
                    string startTimeString = startTime.ToString("HH:mm");
                    string endTimeString = startTime.AddHours(1).ToString("HH:mm");
                    string timeSlot = $"{startTimeString}-{endTimeString}";
                    timeSlots.Add(timeSlot);
                    startTime = startTime.AddHours(1);
                }
            }

            return Json(timeSlots);
        }

        [HttpGet]
        public IActionResult ShowMeetingRooms(int floor, string day, string timeSlot)
        {
            var meetingRooms = new List<MeetingRoomBooking> {
                new MeetingRoomBooking { MeetingRoomName = "Room 1", MeetingRoomCapacity = 10, MeetingRoomTakenSpaces = 5 },
                new MeetingRoomBooking { MeetingRoomName = "Room 2", MeetingRoomCapacity = 8, MeetingRoomTakenSpaces = 3 },
                new MeetingRoomBooking { MeetingRoomName = "Room 3", MeetingRoomCapacity = 12, MeetingRoomTakenSpaces = 7 },
                new MeetingRoomBooking { MeetingRoomName = "Room 4", MeetingRoomCapacity = 2, MeetingRoomTakenSpaces = 1 }
            };

            return PartialView("_MeetingRoomsPartial", meetingRooms);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
