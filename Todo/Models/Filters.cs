namespace Todo.Models
{
    public class Filters
    {
        /// <summary>
        /// This is class to handle the filter which from the parameter of controller.
        /// Class keeps some filter's target properties like: CategoryId, Due, StatusId.
        /// it also keeps boolean propperties to show the status of each filter target
        /// </summary>
        /// <param name="filterString"></param>
        public Filters(string filterString)
        {
            FilterString = filterString?? "all-all-all";
            string[] filters = FilterString.Split("-");
            CategoryId = filters[0];
            Due = filters[1];
            StatusId = filters[2];

        }
        //readonly - can not edit
        public string FilterString { get; }
        public string CategoryId { get; }
        public string Due { get; }
        public string StatusId { get; }

        //if the filter is different from "all" -> bool is true
        public bool HasCategory => CategoryId.ToLower() != "all";
        public bool HasDue => Due.ToLower() != "all";
        public bool HasStatus => StatusId.ToLower() != "all";


        public static Dictionary<string, string> DueFilterValues => 
            new Dictionary<string, string>()
            {
                { "future", "Future"},
                { "past", "Past"},
                { "today", "Today"}
            };
        
        public bool IsFuture => Due.ToLower() == "future";
        public bool IsPast => Due.ToLower() == "past";
        public bool IsToday => Due.ToLower() == "today";
    }
}
