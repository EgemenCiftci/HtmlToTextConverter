using System.Web;

namespace HtmlToTextConverter;

/// <summary>
/// Converts HTML to plain text.
/// </summary>
public class HtmlToText
{
    // Static data tables
    protected static Dictionary<string, string> _tags;
    protected static HashSet<string> _ignoreTags;

    // Instance variables
    protected TextBuilder? _text;
    protected string _html = string.Empty;
    protected int _pos;

    // Static constructor (one time only)
    static HtmlToText()
    {
        _tags = new Dictionary<string, string>
        {
            { "address", "\n" },
            { "blockquote", "\n" },
            { "div", "\n" },
            { "dl", "\n" },
            { "fieldset", "\n" },
            { "form", "\n" },
            { "h1", "\n" },
            { "/h1", "\n" },
            { "h2", "\n" },
            { "/h2", "\n" },
            { "h3", "\n" },
            { "/h3", "\n" },
            { "h4", "\n" },
            { "/h4", "\n" },
            { "h5", "\n" },
            { "/h5", "\n" },
            { "h6", "\n" },
            { "/h6", "\n" },
            { "p", "\n" },
            { "/p", "\n" },
            { "table", "\n" },
            { "/table", "\n" },
            { "ul", "\n" },
            { "/ul", "\n" },
            { "ol", "\n" },
            { "/ol", "\n" },
            { "/li", "\n" },
            { "br", "\n" },
            { "/td", "\t" },
            { "/tr", "\n" },
            { "/pre", "\n" }
        };

        _ignoreTags = ["script", "noscript", "style", "object"];
    }

    /// <summary>
    /// Converts the given HTML to plain text and returns the result.
    /// </summary>
    /// <param name="html">HTML to be converted</param>
    /// <returns>Resulting plain text</returns>
    public string Convert(string html)
    {
        // Initialize state variables
        _text = new TextBuilder();
        _html = html;
        _pos = 0;

        // Process input
        while (!EndOfText)
        {
            if (Peek() == '<')
            {
                // HTML tag
                string tag = ParseTag(out _);

                // Handle special tag cases
                if (tag == "body")
                {
                    // Discard content before <body>
                    _text.Clear();
                }
                else if (tag == "/body")
                {
                    // Discard content after </body>
                    _pos = _html.Length;
                }
                else if (tag == "pre")
                {
                    // Enter preformatted mode
                    _text.Preformatted = true;
                    EatWhitespaceToNextLine();
                }
                else if (tag == "/pre")
                {
                    // Exit preformatted mode
                    _text.Preformatted = false;
                }

                if (_tags.TryGetValue(tag, out string? value))
                {
                    _text.Write(value);
                }

                if (_ignoreTags.Contains(tag))
                {
                    EatInnerContent(tag);
                }
            }
            else if (char.IsWhiteSpace(Peek()))
            {
                // Whitespace (treat all as space)
                _text.Write(_text.Preformatted ? Peek() : ' ');
                MoveAhead();
            }
            else
            {
                // Other text
                _text.Write(Peek());
                MoveAhead();
            }
        }
        // Return result
        return HttpUtility.HtmlDecode(_text.ToString());
    }

    // Eats all characters that are part of the current tag
    // and returns information about that tag
    protected string ParseTag(out bool selfClosing)
    {
        string tag = string.Empty;
        selfClosing = false;

        if (Peek() == '<')
        {
            MoveAhead();

            // Parse tag name
            EatWhitespace();
            int start = _pos;
            if (Peek() == '/')
            {
                MoveAhead();
            }

            while (!EndOfText && !char.IsWhiteSpace(Peek()) &&
                Peek() != '/' && Peek() != '>')
            {
                MoveAhead();
            }

            tag = _html[start.._pos].ToLower();

            // Parse rest of tag
            while (!EndOfText && Peek() != '>')
            {
                if (Peek() is '"' or '\'')
                {
                    EatQuotedValue();
                }
                else
                {
                    if (Peek() == '/')
                    {
                        selfClosing = true;
                    }

                    MoveAhead();
                }
            }
            MoveAhead();
        }
        return tag;
    }

    // Consumes inner content from the current tag
    protected void EatInnerContent(string tag)
    {
        string endTag = "/" + tag;

        while (!EndOfText)
        {
            if (Peek() == '<')
            {
                // Consume a tag
                if (ParseTag(out bool selfClosing) == endTag)
                {
                    return;
                }
                // Use recursion to consume nested tags
                if (!selfClosing && !tag.StartsWith("/"))
                {
                    EatInnerContent(tag);
                }
            }
            else
            {
                MoveAhead();
            }
        }
    }

    // Returns true if the current position is at the end of
    // the string
    protected bool EndOfText => _pos >= _html.Length;

    // Safely returns the character at the current position
    protected char Peek()
    {
        return (_pos < _html.Length) ? _html[_pos] : (char)0;
    }

    // Safely advances to current position to the next character
    protected void MoveAhead()
    {
        _pos = Math.Min(_pos + 1, _html.Length);
    }

    // Moves the current position to the next non-whitespace
    // character.
    protected void EatWhitespace()
    {
        while (char.IsWhiteSpace(Peek()))
        {
            MoveAhead();
        }
    }

    // Moves the current position to the next non-whitespace
    // character or the start of the next line, whichever
    // comes first
    protected void EatWhitespaceToNextLine()
    {
        while (char.IsWhiteSpace(Peek()))
        {
            char c = Peek();
            MoveAhead();
            if (c == '\n')
            {
                break;
            }
        }
    }

    // Moves the current position past a quoted value
    protected void EatQuotedValue()
    {
        char c = Peek();
        if (c is '"' or '\'')
        {
            // Opening quote
            MoveAhead();
            // Find end of value
            _pos = _html.IndexOfAny(new char[] { c, '\r', '\n' }, _pos);
            if (_pos < 0)
            {
                _pos = _html.Length;
            }
            else
            {
                MoveAhead();    // Closing quote
            }
        }
    }
}