namespace SmartTask.Web.Models.ViewModels.Task;

public class AttachmentViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = null!;
    public string UploaderName { get; set; } = null!;
    public DateTime CreateDate { get; set; }
    public bool CanDelete { get; set; }

    public string FileSizeDisplay => FileSize < 1024 * 1024
        ? $"{Math.Round(FileSize / 1024.0, 1)} KB"
        : $"{Math.Round(FileSize / (1024.0 * 1024), 1)} MB";
}