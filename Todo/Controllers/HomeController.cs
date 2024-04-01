using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Todo.Models;
using Todo.Models.DataAccess;
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


        #region Actions to render list and redirect to Index page
        /// <summary>
        /// Action to return index page with filtered list
        /// </summary>
        /// <param name="id"></param>
        /// <param name="searchDes"></param>
        /// <returns></returns>
        public IActionResult Index(string id, string searchDes, string pageIndex)
        {
            #region Initialize Filters to hole information and setup information for View
            var filters = new Filters(id, searchDes, pageIndex);
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
            #endregion

            #region Filter the information for showing objective list
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
                    query = query.Where(t => t.DueDate < DateTime.Now);
                else if (filters.IsToday)
                    query = query.Where(t => t.DueDate == DateTime.Now);
                else if (filters.IsFuture)
                    query = query.Where(t => t.DueDate > DateTime.Now);
            }
            //Returns list from the furthest distant date to the latest date
            var todos = query.OrderByDescending(t => t.DueDate).ToList();

            //if there is the specific search Description target, filter 
            if (!string.IsNullOrEmpty(filters.SearchDes))
            {
                todos = todos.Where(x => x.Description.Contains(filters.SearchDes)).ToList();
            }
            #endregion

            #region Filter according to current page
            var recoredSize = 5;
            int targetPage = filters.CurrentPage;
            int totalPage = (int)Math.Ceiling(todos.Count / (double)recoredSize);
            ViewBag.TotalPage = totalPage;
            todos = todos.Skip((targetPage - 1) * recoredSize).Take(recoredSize).ToList();
            #endregion

            return View(todos);
        }

        /// <summary>
        /// Action to return filter elements for action Index
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="searchDes"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult FilterPaging(string filterString, string? searchDes = null, string? pageIndex = null)
        {
            return RedirectToAction("Index", "Home", new { id = filterString, searchDes = searchDes, pageIndex = pageIndex });
        }
        /// <summary>
        /// Action to return filter elements for action Index
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="searchDes"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Filter(string[] filter, string? searchDes = null, string? pageIndex = null)
        {
            string id = string.Join("-", filter);
            return RedirectToAction("Index", "Home", new { id = id, searchDes = searchDes.Trim(), pageIndex = pageIndex });
        }
        #endregion


        /// <summary>
        /// Action to mark task completed - StatusId - close
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
        /// Action to deleted completed (StatusId: close) tasks
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

        /// <summary>
        /// Action to delete specific task
        /// </summary>
        /// <param name="id"></param>
        /// <param name="selectedId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Delete([FromRoute] string id, int selectedId)
        {
            //get all todo having status "closed" -> completed
            var toDelete = _context.ToDos.Find(selectedId);
            if (toDelete != null)
            {
                _context.ToDos.Remove(toDelete);
                _context.SaveChanges();
            }
            return RedirectToAction("Index", "Home", new { id = id });
        }


        //something cool - i want to discover
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

        #region Create and Add AJAX CALL
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
                task = _context.ToDos.Find(id);
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
            //var list = _context.ToDos
            //    .Include(t => t.Category)
            //    .Include(t => t.Status).ToList();

            //add new
            if (ModelState.IsValid)
            {
                if (id == 0)
                {
                    _context.ToDos.Add(todo);
                    _context.SaveChanges();
                }
                //update
                else
                {
                    _context.ToDos.Update(todo);
                    _context.SaveChanges();
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
        #endregion

    }
}
