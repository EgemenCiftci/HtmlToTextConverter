using System.Text;

namespace HtmlToTextConverter;

/// <summary>
/// A StringBuilder class that helps eliminate excess whitespace.
/// </summary>
public class TextBuilder
{
    private readonly StringBuilder _text;
    private readonly StringBuilder _currLine;
    private int _emptyLines;
    private bool _preformatted;

    // Construction
    public TextBuilder()
    {
        _text = new StringBuilder();
        _currLine = new StringBuilder();
        _emptyLines = 0;
        _preformatted = false;
    }

    /// <summary>
    /// Normally, extra whitespace characters are discarded.
    /// If this property is set to true, they are passed
    /// through unchanged.
    /// </summary>
    public bool Preformatted
    {
        get => _preformatted;
        set
        {
            if (value)
            {
                // Clear line buffer if changing to
                // preformatted mode
                if (_currLine.Length > 0)
                {
                    FlushCurrLine();
                }

                _emptyLines = 0;
            }
            _preformatted = value;
        }
    }

    /// <summary>
    /// Clears all current text.
    /// </summary>
    public void Clear()
    {
        _text.Length = 0;
        _currLine.Length = 0;
        _emptyLines = 0;
    }

    /// <summary>
    /// Writes the given string to the output buffer.
    /// </summary>
    /// <param name="s"></param>
    public void Write(string s)
    {
        foreach (char c in s)
        {
            Write(c);
        }
    }

    /// <summary>
    /// Writes the given character to the output buffer.
    /// </summary>
    /// <param name="c">Character to write</param>
    public void Write(char c)
    {
        if (_preformatted)
        {
            // Write preformatted character
            _ = _text.Append(c);
        }
        else
        {
            if (c == '\r')
            {
                // Ignore carriage returns. We'll process
                // '\n' if it comes next
            }
            else if (c == '\n')
            {
                // Flush current line
                FlushCurrLine();
            }
            else if (char.IsWhiteSpace(c))
            {
                // Write single space character
                int len = _currLine.Length;
                if (len == 0 || !char.IsWhiteSpace(_currLine[len - 1]))
                {
                    _ = _currLine.Append(' ');
                }
            }
            else
            {
                // Add character to current line
                _ = _currLine.Append(c);
            }
        }
    }

    // Appends the current line to output buffer
    protected void FlushCurrLine()
    {
        // Get current line
        string line = _currLine.ToString().Trim();

        // Determine if line contains non-space characters
        string tmp = line.Replace("&nbsp;", string.Empty);
        if (tmp.Length == 0)
        {
            // An empty line
            _emptyLines++;
            if (_emptyLines < 2 && _text.Length > 0)
            {
                _ = _text.AppendLine(line);
            }
        }
        else
        {
            // A non-empty line
            _emptyLines = 0;
            _ = _text.AppendLine(line);
        }

        // Reset current line
        _currLine.Length = 0;
    }

    /// <summary>
    /// Returns the current output as a string.
    /// </summary>
    public override string ToString()
    {
        if (_currLine.Length > 0)
        {
            FlushCurrLine();
        }

        return _text.ToString();
    }
}
