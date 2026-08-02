namespace SmartTask.Web.Models.ViewModels.Task;

public class CommentViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public DateTime CreateDate { get; set; }
    public bool IsEdited { get; set; }
    public bool CanDelete { get; set; }
}