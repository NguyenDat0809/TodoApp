namespace Todo.Models.DTO
{
    public class Modal
    {
        //property for create/Update
        public ToDo? Task { get; set; } = null;

        //property for showing informations in modal
        public List<Category>? Categories { get; set; } = null;
        public List<Status>? Statuses { get; set; } = null;

        //public Dictionary<string, string>? DueFilterValues { get; set; } = null;

    }
}
