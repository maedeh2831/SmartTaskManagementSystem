using System;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

class Program
{
    static void Main(string[] args)
    {
        string docxPath = args[0];
        string chaptersDir = args[1];
        string mode = args.Length > 2 ? args[2] : "all";
        
        if (mode == "refs")
        {
            // Insert references before the existing references section
            File.Copy(docxPath, docxPath + ".bak_refs", true);
            
            using (var doc = WordprocessingDocument.Open(docxPath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;
                
                // Find the "فهرست منابع" section
                var paragraphs = body.Elements<Paragraph>().ToList();
                int insertIndex = paragraphs.Count - 1;
                
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var text = paragraphs[i].InnerText;
                    if (text.Contains("فهرست منابع") || text.Contains("مرجع") || text.Contains("References"))
                    {
                        insertIndex = i;
                        break;
                    }
                }
                
                // Read references file
                string refsPath = Path.Combine(chaptersDir, "references.txt");
                string refsContent = File.ReadAllText(refsPath, System.Text.Encoding.UTF8);
                string[] lines = refsContent.Split('\n');
                
                Console.WriteLine($"Inserting references ({lines.Length} lines) before index {insertIndex}");
                
                var refElement = paragraphs[insertIndex];
                foreach (var line in lines.Reverse())
                {
                    var para = new Paragraph();
                    var run = new Run();
                    var text = new Text(line.TrimEnd('\r'));
                    text.Space = SpaceProcessingModeValues.Preserve;
                    run.Append(text);
                    
                    bool isHeader = line.StartsWith("فهرست") || line.StartsWith("منابع") || line.StartsWith("References");
                    
                    if (isHeader)
                    {
                        var paraProps = new ParagraphProperties();
                        var spacing = new SpacingBetweenLines() { Before = "240", After = "120" };
                        paraProps.Append(spacing);
                        para.Append(paraProps);
                        
                        var runProps = new RunProperties();
                        runProps.Append(new Bold());
                        runProps.Append(new FontSize() { Val = "28" });
                        run.Append(runProps);
                    }
                    else
                    {
                        var paraProps = new ParagraphProperties();
                        var spacing = new SpacingBetweenLines() { Before = "60", After = "60", Line = "360", LineRule = LineSpacingRuleValues.Auto };
                        paraProps.Append(spacing);
                        para.Append(paraProps);
                        
                        var runProps = new RunProperties();
                        runProps.Append(new FontSize() { Val = "22" });
                        run.Append(runProps);
                    }
                    
                    para.Append(run);
                    body.InsertBefore(para, refElement);
                }
                
                doc.MainDocumentPart.Document.Save();
                Console.WriteLine("References inserted successfully!");
            }
        }
        else if (mode == "abstract")
        {
            // Insert abstract after the TOC
            File.Copy(docxPath, docxPath + ".bak_abs", true);
            
            using (var doc = WordprocessingDocument.Open(docxPath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;
                var paragraphs = body.Elements<Paragraph>().ToList();
                
                // Find chapter 1 start to insert abstract before it
                int insertIndex = 0;
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var text = paragraphs[i].InnerText;
                    if (text.Contains("فصل اول") || text.Contains("مقدمه") && text.Contains("پیشینه"))
                    {
                        insertIndex = i;
                        break;
                    }
                }
                
                // Read abstract files
                string absFaPath = Path.Combine(chaptersDir, "abstract_fa.txt");
                string absEnPath = Path.Combine(chaptersDir, "abstract_en.txt");
                
                string absFa = File.ReadAllText(absFaPath, System.Text.Encoding.UTF8);
                string absEn = File.ReadAllText(absEnPath, System.Text.Encoding.UTF8);
                
                string fullAbstract = absFa + "\n\nAbstract\n" + absEn;
                string[] lines = fullAbstract.Split('\n');
                
                Console.WriteLine($"Inserting abstract ({lines.Length} lines) before index {insertIndex}");
                
                var refElement = paragraphs[insertIndex];
                foreach (var line in lines.Reverse())
                {
                    var para = new Paragraph();
                    var run = new Run();
                    var text = new Text(line.TrimEnd('\r'));
                    text.Space = SpaceProcessingModeValues.Preserve;
                    run.Append(text);
                    
                    bool isHeader = line.StartsWith("چکیده") || line.StartsWith("Abstract") || line.StartsWith("واژه‌های کلیدی") || line.StartsWith("Keywords");
                    
                    if (isHeader)
                    {
                        var paraProps = new ParagraphProperties();
                        var spacing = new SpacingBetweenLines() { Before = "240", After = "120" };
                        paraProps.Append(spacing);
                        para.Append(paraProps);
                        
                        var runProps = new RunProperties();
                        runProps.Append(new Bold());
                        runProps.Append(new FontSize() { Val = "28" });
                        run.Append(runProps);
                    }
                    else
                    {
                        var paraProps = new ParagraphProperties();
                        var spacing = new SpacingBetweenLines() { Before = "60", After = "60", Line = "360", LineRule = LineSpacingRuleValues.Auto };
                        paraProps.Append(spacing);
                        
                        var justification = new Justification() { Val = JustificationValues.Both };
                        paraProps.Append(justification);
                        para.Append(paraProps);
                        
                        var runProps = new RunProperties();
                        runProps.Append(new FontSize() { Val = "24" });
                        run.Append(runProps);
                    }
                    
                    para.Append(run);
                    body.InsertBefore(para, refElement);
                }
                
                doc.MainDocumentPart.Document.Save();
                Console.WriteLine("Abstract inserted successfully!");
            }
        }
    }
}
