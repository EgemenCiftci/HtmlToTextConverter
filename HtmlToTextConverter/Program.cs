using HtmlToTextConverter;

string html = File.ReadAllText("html.html");
string text = HtmlUtilities.ConvertToPlainText(html);

Console.WriteLine("HTML:");
Console.WriteLine(html);
Console.WriteLine();
Console.WriteLine("TEXT:");
Console.WriteLine(text);
