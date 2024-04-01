namespace Todo.Models
{
    public class Filters
    {
        /// <summary>
        /// Class to holder filter elements: FilterString, SearchDes, CurrentPage
        /// </summary>
        /// <param name="filterString"></param>
        public Filters(string filterString, string? searchDes = null, string? pageIndex = null)
        {
            //filterstring
            FilterString = filterString ?? "all-all-all";
            string[] filters = FilterString.Split("-");
            CategoryId = filters[0];
            Due = filters[1];
            StatusId = filters[2];

            //search description
            searchDes = searchDes?.Trim()?? "";
            SearchDes = searchDes != null ? searchDes : "";

            //currentPage - pagination
            CurrentPage = pageIndex != null ? int.Parse(pageIndex) : 1;
        }
        //readonly - can not edit
        public string FilterString { get; }
        public string CategoryId { get; }
        public string Due { get; }
        public string StatusId { get; }
        public string SearchDes { get; }

        public int CurrentPage { get; }


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
