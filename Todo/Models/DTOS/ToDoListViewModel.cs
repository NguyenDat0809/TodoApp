namespace Todo.Models.DTOS
{
    /// <summary>
    /// this class to create a ViewModel for IEnumerable<ToDo> with full information of filter
    /// </summary>
    public class ToDoListViewModel
    {
        public IEnumerable<ToDo>? ToDos { get; set; }
        public string? Filters { get; set; } 
        public string ? SearchDes { get; set; }
        public int CurrentPage { get; set; }
        private int _pageSize = 5;
        public int PageSize { get { return _pageSize; } } 
    }
}
