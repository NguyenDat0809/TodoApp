using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Todo.Models
{
    public class ToDo
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "PLease enter a description")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "PLease enter due date")]
        public DateTime? DueDate { get; set; }

        [ForeignKey(nameof(Category))]
        [Required(ErrorMessage = " Please enter a category")]
        public string CategoryId { get; set; }
        [ValidateNever] //no need to validate
        public Category Category { get; set; }

        [ForeignKey(nameof(Status))]
        
        public string  StatusId { get; set; }
        [ValidateNever] //no need to validate
        public Status Status { get; set; }

        public bool Overdue => StatusId == "open" && DueDate < DateTime.Now;
    }
}
