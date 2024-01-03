using HtmlToTextConverter;

string html = File.ReadAllText("html.html");
string text = new HtmlToText().Convert(html);

Console.WriteLine("HTML:");
Console.WriteLine(html);
Console.WriteLine();
Console.WriteLine("TEXT:");
Console.WriteLine(text);
