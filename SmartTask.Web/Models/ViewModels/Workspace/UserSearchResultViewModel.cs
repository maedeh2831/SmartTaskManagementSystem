/*
| Module      : Workspace
| ViewModel   : UserSearchResultViewModel
| Purpose     : نمایش نتیجه جستجوی کاربر برای دعوت به Workspace.
*/
namespace SmartTask.Web.Models.ViewModels.Workspace;
public class UserSearchResultViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}