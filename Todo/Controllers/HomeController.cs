using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Todo.Models;
using Todo.Models.DataAccess;
using Todo.Models.DTO;

namespace Todo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ToDoDBContext _context;

        public HomeController(ILogger<HomeController> logger, ToDoDBContext context)
        {
            _logger = logger;
            _context = context;
        }


        /// <summary>
        /// The action to show all Categories, Statuses and filter of Due which taken by static way in Filters class
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Index(string id)
        {
            var filters = new Filters(id);
            ViewBag.Filters = filters;
            //get all catergories(1)
            ViewBag.Categories = _context.Categories.ToList();
            //get all statuses (2)
            ViewBag.Statuses = _context.Statuses.ToList();
            //get all due status(3)
            ViewBag.DueFilters = Filters.DueFilterValues;
            //get all todo including Category and Status 's information(4)
            IQueryable<ToDo> query = _context.ToDos
                .Include(t => t.Category)
                .Include(t => t.Status);

            //check filter to filter the content from (1)(2)(3)(4)
            if (filters.HasCategory)
                query = query.Where(t => t.CategoryId == filters.CategoryId);
            if (filters.HasStatus)
                query = query.Where(t => t.StatusId == filters.StatusId);
            if (filters.HasDue)
            {
                //we need to get today which having data type is DateTime
                //why Today (not NOW) -> to filter better the todo following day by day 
                var today = DateTime.Today;
                //check past/today/future
                if (filters.IsPast)
                    query = query.Where(t => t.DueDate < DateTime.Today);
                else if (filters.IsToday)
                    query = query.Where(t => t.DueDate == DateTime.Today);
                else if (filters.IsFuture)
                    query = query.Where(t => t.DueDate > DateTime.Today);
            }
            //Returns a list from the most distant date to the latest date
            var todos = query.OrderBy(t => t.DueDate).ToList();
            return View(todos);
        }

        [HttpPost]
        public IActionResult Add(ToDo task)
        {
            var isValidAll = false;
            //if every validation is valid
            if (ModelState.IsValid)
            {
                _context.ToDos.Add(task);
                _context.SaveChanges();

                //add invalid input to modal to show up
                //modal.Task.Description = ModelState[];
                isValidAll = true;
            }//if not, redirect to notify validation warning
            var modal = GetModal();
            modal.Task = task;

            //return PartialView("_ModalFormPartial", modal);
            return Json(new { isValidAll = isValidAll });
        }

        [HttpPost]
        public IActionResult Edit(ToDo task)
        {
            var isValidAll = false;
            //if every validation is valid
            if (ModelState.IsValid)
            {
                _context.ToDos.Update(task);
                _context.SaveChanges();

                //add invalid input to modal to show up
                //modal.Task.Description = ModelState[];
                isValidAll = true;
            }//if not, redirect to notify validation warning
            var modal = GetModal();
            modal.Task = task;

            //return PartialView("_ModalFormPartial", modal);
            return Json(new { isValidAll = isValidAll });
        }

        [HttpPost]
        public IActionResult CheckValidation(ToDo task)
        {
            var isValidAll = false;
            if (ModelState.IsValid)
            {
                isValidAll = true;
            }
            return Json(new { isValidAll = isValidAll });
        }


        /// <summary>
        /// This action join all the filter from client and redirect to Index action to filter the todo task
        /// </summary>
        /// <param name="filter">get from form</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join("-", filter);
            return RedirectToAction("Index", "Home", new { id = id });
        }

        /// <summary>
        /// This action allow user to close todo task -> Status to "close", then redirect to home page with last filter
        /// </summary>
        /// <param name="id"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        [HttpPost]
        //[FromRoute] ~ url/{id}
        public IActionResult MarkCompleted([FromRoute] string id, int selectedId)
        {
            //recheck to make sure selected todo still exists in database
            var selected = _context.ToDos.Find(selectedId);

            if (selected is not null)
            {
                selected.StatusId = "close";
                _context.Update(selected);
                _context.SaveChanges();
            }
            return RedirectToAction("Index", "Home", new { id = id });
        }

        /// <summary>
        /// This action delete completed todo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteCompleted(string id)
        {
            //get all todo having status "closed" -> completed
            var toDelete = _context.ToDos.Where(t => t.StatusId == "close").ToList();
            if (toDelete != null || toDelete.Count != 0)
            {
                toDelete.ForEach(t => _context.ToDos.Remove(t));

                _context.SaveChanges();
            }


            return RedirectToAction("Index", "Home", new { id = id });
        }
        //something cool - i want to discover
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        #region Other Function
        public Modal GetModal(int? selectedId = null)
        {
            //create modal to return
            var modal = new Modal()
            {
                Task = new()
                {
                    Description = "",
                    DueDate = DateTime.Now,
                    CategoryId = "",
                    StatusId = "open"
                },
                Categories = _context.Categories.ToList(),
                Statuses = _context.Statuses.ToList()
            };//in case of ADD - no selectId parameter
            if (selectedId != null)
            {
                var task = _context.ToDos.Find(selectedId);
                modal.Task = task;
            }
            return modal;
        }

        #endregion

        #region Call API
        /// <summary>
        /// action trả về Modal để sử dụng ở trong chức năng add hoặc edit.
        /// Có 2 kiểu modal trả về
        ///     1. nếu add thì trả về modal với task null
        ///     2. nếu edit thì trả về modal với task được chọn
        /// </summary>
        /// <param name="selectedId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetModalAPI(int? selectedId = null)
        {
            var modal = GetModal(selectedId);

            return Json(new { data = modal });
        }

        /// <summary>
        /// action trả về 1 partial view đã được đưa dữ liệu (modal) vào
        /// </summary>
        /// <param name="modal"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetModalPartial(Modal modal)
        {
            return PartialView("_ModalFormPartial", modal);
        }

        #endregion
    }
}
