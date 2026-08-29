using System;
using System.IO;
using DocumentFormat.OpenXml.Packaging;

class Program
{
    static void Main()
    {
        var dir = "I:\Repository\SmartTask";
        foreach (var f in Directory.GetFiles(dir, "*.docx"))
        {
            if (f.Contains(".bak")) continue;
            Console.WriteLine($"File: {Path.GetFileName(f)}");
            Console.WriteLine($"Size: {new FileInfo(f).Length} bytes");
            Console.WriteLine($"Modified: {new FileInfo(f).LastWriteTime}");
            
            using (var doc = WordprocessingDocument.Open(f, false))
            {
                var body = doc.MainDocumentPart!.Document.Body!;
                var text = body.InnerText;
                Console.WriteLine($"Has abstract: {text.Contains("\u0686\u06A9\u06CC\u062F\u0647")}");
                Console.WriteLine($"Has refs: {text.Contains("\u0641\u0647\u0631\u0633\u062A \u0645\u0646\u0627\u0628\u0639")}");
                Console.WriteLine($"Has ch2: {text.Contains("\u0641\u0635\u0644 \u062F\u0648\u0645")}");
                Console.WriteLine($"Has ch3: {text.Contains("\u0641\u0635\u0644 \u0633\u0648\u0645")}");
                Console.WriteLine($"Has ch4: {text.Contains("\u0641\u0635\u0644 \u0686\u0647\u0627\u0631\u0645")}");
                Console.WriteLine($"Has ch5: {text.Contains("\u0641\u0635\u0644 \u067E\u0646\u062C\u0645")}");
            }
            Console.WriteLine();
        }
    }
}
