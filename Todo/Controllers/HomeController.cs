using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Todo.Models;
using Todo.Models.DataAccess;
using Todo.Models.DTO;
using static Todo.Helper;

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


        /// <summary>
        /// This action join all the filter from client and redirect to Index action to filter the todo task
        /// </summary>
        /// <param name="filter">get from form</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join("-", filter);
            return Redirect(Url.Action("Index", "Home", new { id = id }, HttpContext.Request.Scheme));
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
            return Redirect(Url.Action("Index", "Home", new { id = id }, HttpContext.Request.Scheme));
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


            return Redirect(Url.Action("Index", "Home", new { id = id }, HttpContext.Request.Scheme));
        }


        //something cool - i want to discover
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: Home/AddOrEdit(Insert)
        // GET: Home/AddOrEdit/5(Update)
        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(int id)
        {
            var task = new ToDo()
            {
                DueDate = DateTime.Now,
            };
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Statuses = _context.Statuses.ToList();
            if (id != 0)
            {
                task = await _context.ToDos.FindAsync(id);
                if (task == null)
                    return NotFound();
            }
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEdit(ToDo todo)
        {
            int id = todo?.Id ?? 0;

            var filters = new Filters(null);
            ViewBag.Filters = filters;
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Statuses = _context.Statuses.ToList();
            ViewBag.DueFilters = Filters.DueFilterValues;
            var list = _context.ToDos
                .Include(t => t.Category)
                .Include(t => t.Status).ToList();

            //add new
            if (ModelState.IsValid)
            {
                if (id == 0)
                {
                    _context.ToDos.Add(todo);
                    await _context.SaveChangesAsync();
                }
                //update
                else
                {
                    _context.Update(todo);
                    await _context.SaveChangesAsync();
                }
               
                return Json(new { isValid = true });
              
            }
            else
            {
                //return with validation error task
                return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", todo) });
            }
            //html is the view with html with the same modal we use to call function
            //The purpose is to show validation
        }

    }
}
