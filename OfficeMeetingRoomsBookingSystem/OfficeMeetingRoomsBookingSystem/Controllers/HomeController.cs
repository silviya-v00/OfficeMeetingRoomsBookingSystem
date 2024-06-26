﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeMeetingRoomsBookingSystem.Models;
using OfficeMeetingRoomsBookingSystem.Utils;
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
        private readonly IConfiguration _configuration;
        private DBUtil _dbUtil;
        private MailUtil _mailUtil;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger,
                              IConfiguration configuration,
                              SignInManager<ApplicationUser> signInManager,
                              UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _configuration = configuration;
            _dbUtil = new DBUtil(_configuration.GetConnectionString("DefaultConnection"));
            _mailUtil = new MailUtil(_configuration);
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<ApplicationUser> GetApplicationUser()
        {
            return await _userManager.GetUserAsync(User);
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var passwordResetLink = Url.Action("ResetPassword", "Home",
                            new { email = model.Email, token = token }, Request.Scheme);

                    var subject = "Password Reset";
                    var messageBody = "To reset your password, please click the following link: <a href='" + passwordResetLink + "'>Reset Password</a>";

                    _mailUtil.SendEmail(model.Email, subject, messageBody);

                    return View("ForgotPasswordConfirmation");
                }

                // Even if there is no registered user with the email, do not reveal that the user does not exist
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return View("ResetPasswordConfirmation");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }

                return View("ResetPasswordConfirmation");
            }

            return View(model);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MeetingRoomsMap()
        {
            List<int> floors = _dbUtil.GetMeetingRoomFloors();

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

                if (selectedDate.Date != today || (selectedDate.Date == today && currentTime.Hour < 23))
                {
                    while (startTime < selectedDate.Date.AddDays(1))
                    {
                        string startTimeString = startTime.ToString("HH:mm");
                        string endTimeString = startTime.AddHours(1).ToString("HH:mm");
                        string timeSlot = $"{startTimeString}-{endTimeString}";
                        timeSlots.Add(timeSlot);
                        startTime = startTime.AddHours(1);
                    }
                }
            }

            return Json(timeSlots);
        }

        [HttpGet]
        public IActionResult ShowMeetingRooms(int floor, string selectedDateStr, string timeSlot)
        {
            KeyValuePair<DateTime, DateTime> bookingTime = CommonUtil.GetStartAndEndBookingDateTime(selectedDateStr, timeSlot);

            List<MeetingRoomDetails> meetingRooms = _dbUtil.GetMeetingRoomDetailsByFloorAndDateTime(floor, bookingTime.Key, bookingTime.Value);

            return PartialView("_MeetingRoomsPartial", meetingRooms);
        }

        [HttpGet]
        public async Task<IActionResult> OpenMeetingRoomBookingForm(int roomId, string roomName, string selectedDateStr, string timeSlot)
        {
            var currentUser = await GetApplicationUser();

            KeyValuePair<DateTime, DateTime> bookingTime = CommonUtil.GetStartAndEndBookingDateTime(selectedDateStr, timeSlot);

            MeetingRoomBooking meetingRoom = new MeetingRoomBooking()
            {
                MeetingRoomID = roomId,
                MeetingRoomName = roomName,
                StartDateTime = bookingTime.Key,
                EndDateTime = bookingTime.Value,
                FullName = currentUser.FirstName + " " + currentUser.LastName
            };

            return PartialView("_MeetingRoomBookingPartial", meetingRoom);
        }

        [HttpPost]
        public async Task<IActionResult> BookMeetingRoom(int meetingRoomID, string meetingRoomName, DateTime startDateTime, DateTime endDateTime)
        {
            var currentUser = await GetApplicationUser();
            string errMsg = _dbUtil.BookMeetingRoom(currentUser.Id, meetingRoomID, meetingRoomName, startDateTime, endDateTime);

            if (String.IsNullOrEmpty(errMsg))
                return Json(new { success = true, message = "" });
            else
                return Json(new { success = false, message = errMsg });
        }

        public async Task<IActionResult> BookedMeetingRooms()
        {
            var currentUser = await GetApplicationUser();

            List<MeetingRoomBooking> bookedRooms = _dbUtil.GetBookedMeetingRooms(currentUser.Id);

            return View(bookedRooms);
        }

        [HttpPost]
        public IActionResult DeleteBooking(int bookingID)
        {
            _dbUtil.DeleteBooking(bookingID);

            return Ok();
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
