using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartTask.Web.Models.ViewModels.Report;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class ReportExportService : IReportExportService
    {
        public byte[] GenerateWorkspacePdf(WorkspaceReportViewModel model)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text($"گزارش Workspace: {model.WorkspaceName}")
                        .SemiBold().FontSize(16);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingTop(10).Text($"کل Task ها: {model.TotalTasks}   |   تکمیل‌شده: {model.CompletedTasks}   |   نرخ تکمیل: {model.CompletionRate}%   |   عقب‌افتاده: {model.OverdueTasksCount}");

                        col.Item().PaddingTop(15).Text("Task های عقب‌افتاده").SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Text("عنوان").SemiBold();
                                h.Cell().Text("پروژه").SemiBold();
                                h.Cell().Text("موعد").SemiBold();
                                h.Cell().Text("روز تأخیر").SemiBold();
                            });
                            foreach (var t in model.TopOverdueTasks)
                            {
                                table.Cell().Text(t.Title);
                                table.Cell().Text(t.ProjectName);
                                table.Cell().Text(t.DueDate.ToString("yyyy/MM/dd"));
                                table.Cell().Text(t.DaysOverdue.ToString());
                            }
                        });

                        col.Item().PaddingTop(15).Text("حجم کاری اعضا").SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Text("عضو").SemiBold();
                                h.Cell().Text("تخصیص‌یافته").SemiBold();
                                h.Cell().Text("تکمیل‌شده").SemiBold();
                                h.Cell().Text("زمان (ساعت)").SemiBold();
                            });
                            foreach (var m in model.MemberWorkload)
                            {
                                table.Cell().Text(m.FullName);
                                table.Cell().Text(m.AssignedTasksCount.ToString());
                                table.Cell().Text(m.CompletedTasksCount.ToString());
                                table.Cell().Text((m.TotalMinutesLogged / 60.0).ToString("0.0"));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("تولید شده در تاریخ ");
                        x.Span(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateWorkspaceExcel(WorkspaceReportViewModel model)
        {
            using var workbook = new XLWorkbook();

            var summarySheet = workbook.Worksheets.Add("خلاصه");
            summarySheet.Cell(1, 1).Value = "کل Task ها";
            summarySheet.Cell(1, 2).Value = model.TotalTasks;
            summarySheet.Cell(2, 1).Value = "تکمیل‌شده";
            summarySheet.Cell(2, 2).Value = model.CompletedTasks;
            summarySheet.Cell(3, 1).Value = "نرخ تکمیل (%)";
            summarySheet.Cell(3, 2).Value = model.CompletionRate;
            summarySheet.Cell(4, 1).Value = "عقب‌افتاده";
            summarySheet.Cell(4, 2).Value = model.OverdueTasksCount;

            var overdueSheet = workbook.Worksheets.Add("Task های عقب‌افتاده");
            overdueSheet.Cell(1, 1).Value = "عنوان";
            overdueSheet.Cell(1, 2).Value = "پروژه";
            overdueSheet.Cell(1, 3).Value = "موعد";
            overdueSheet.Cell(1, 4).Value = "روز تأخیر";
            var row = 2;
            foreach (var t in model.TopOverdueTasks)
            {
                overdueSheet.Cell(row, 1).Value = t.Title;
                overdueSheet.Cell(row, 2).Value = t.ProjectName;
                overdueSheet.Cell(row, 3).Value = t.DueDate.ToString("yyyy/MM/dd");
                overdueSheet.Cell(row, 4).Value = t.DaysOverdue;
                row++;
            }

            var membersSheet = workbook.Worksheets.Add("حجم کاری اعضا");
            membersSheet.Cell(1, 1).Value = "عضو";
            membersSheet.Cell(1, 2).Value = "تخصیص‌یافته";
            membersSheet.Cell(1, 3).Value = "تکمیل‌شده";
            membersSheet.Cell(1, 4).Value = "زمان (ساعت)";
            row = 2;
            foreach (var m in model.MemberWorkload)
            {
                membersSheet.Cell(row, 1).Value = m.FullName;
                membersSheet.Cell(row, 2).Value = m.AssignedTasksCount;
                membersSheet.Cell(row, 3).Value = m.CompletedTasksCount;
                membersSheet.Cell(row, 4).Value = Math.Round(m.TotalMinutesLogged / 60.0, 1);
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateProjectPdf(ProjectReportViewModel model)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text($"گزارش پروژه: {model.ProjectName}").SemiBold().FontSize(16);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingTop(10).Text($"کل Task ها: {model.TotalTasks}   |   تکمیل‌شده: {model.CompletedTasks}   |   نرخ تکمیل: {model.CompletionRate}%   |   عقب‌افتاده: {model.OverdueTasksCount}");

                        col.Item().PaddingTop(15).Text("Task های عقب‌افتاده").SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Text("عنوان").SemiBold();
                                h.Cell().Text("موعد").SemiBold();
                                h.Cell().Text("روز تأخیر").SemiBold();
                            });
                            foreach (var t in model.TopOverdueTasks)
                            {
                                table.Cell().Text(t.Title);
                                table.Cell().Text(t.DueDate.ToString("yyyy/MM/dd"));
                                table.Cell().Text(t.DaysOverdue.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("تولید شده در تاریخ ");
                        x.Span(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateProjectExcel(ProjectReportViewModel model)
        {
            using var workbook = new XLWorkbook();

            var summarySheet = workbook.Worksheets.Add("خلاصه");
            summarySheet.Cell(1, 1).Value = "کل Task ها";
            summarySheet.Cell(1, 2).Value = model.TotalTasks;
            summarySheet.Cell(2, 1).Value = "تکمیل‌شده";
            summarySheet.Cell(2, 2).Value = model.CompletedTasks;
            summarySheet.Cell(3, 1).Value = "نرخ تکمیل (%)";
            summarySheet.Cell(3, 2).Value = model.CompletionRate;

            var membersSheet = workbook.Worksheets.Add("حجم کاری اعضا");
            membersSheet.Cell(1, 1).Value = "عضو";
            membersSheet.Cell(1, 2).Value = "تخصیص‌یافته";
            membersSheet.Cell(1, 3).Value = "تکمیل‌شده";
            var row = 2;
            foreach (var m in model.MemberWorkload)
            {
                membersSheet.Cell(row, 1).Value = m.FullName;
                membersSheet.Cell(row, 2).Value = m.AssignedTasksCount;
                membersSheet.Cell(row, 3).Value = m.CompletedTasksCount;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}