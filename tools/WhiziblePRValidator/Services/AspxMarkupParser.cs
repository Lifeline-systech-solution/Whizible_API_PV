using Whizible.PRValidator.Models;
using Whizible.PRValidator.Rules;

namespace Whizible.PRValidator.Services;

public sealed class AspxElement
{
    public string Name { get; init; } = string.Empty;
    public int Line { get; init; }
    public bool IsServerControl { get; init; }
    public string? Id { get; init; }
    public bool RunatServer { get; init; }
    public bool HasRunat { get; init; }
    public string? RunatValue { get; init; }
}

public sealed class AspxDirective
{
    public string Name { get; init; } = string.Empty;
    public int Line { get; init; }
    public Dictionary<string, string> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class AspxDocument
{
    public List<AspxDirective> Directives { get; } = new();
    public List<AspxElement> ServerElements { get; } = new();
}

/// <summary>
/// Stack parser for ASPX/ASCX markup. Script and style bodies are treated as
/// raw text so JavaScript comparisons are not read as tags.
/// </summary>
public sealed class AspxMarkupParser
{
    private static readonly HashSet<string> VoidHtml = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta",
        "param", "source", "track", "wbr"
    };

    private static readonly HashSet<string> TemplateTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "ItemTemplate", "AlternatingItemTemplate", "HeaderTemplate", "FooterTemplate",
        "SeparatorTemplate", "EditItemTemplate", "InsertItemTemplate", "SelectedItemTemplate",
        "EmptyDataTemplate", "LayoutTemplate", "GroupTemplate", "PagerTemplate",
        "GroupSeparatorTemplate", "EmptyItemTemplate"
    };

    private readonly string _text;
    private readonly string _filePath;
    private readonly AspNetControlCatalog _catalog;
    private readonly bool _enforceUnknownControls;
    private readonly List<ValidationError> _issues = new();
    private readonly List<int> _lineStarts;
    private readonly AspxDocument _document = new();
    private readonly Dictionary<string, RegisteredPrefix> _prefixes = new(StringComparer.OrdinalIgnoreCase);

    private int _index;

    public AspxMarkupParser(string text, string filePath, AspNetControlCatalog catalog, bool enforceUnknownControls)
    {
        _text = text ?? string.Empty;
        _filePath = filePath;
        _catalog = catalog;
        _enforceUnknownControls = enforceUnknownControls;
        _lineStarts = BuildLineStarts(_text);
    }

    public (AspxDocument Document, List<ValidationError> Issues) Parse()
    {
        var stack = new Stack<OpenTag>();
        var idScopes = new Stack<HashSet<string>>();
        idScopes.Push(new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        while (_index < _text.Length)
        {
            if (StartsWith("<%--"))
            {
                if (!ConsumeUntil("--%>", out _))
                    Issue(RuleIds.AspxMarkup, LineAt(_index), ColumnAt(_index), "Server comment is not closed.", "Close the comment with --%>.");
                continue;
            }

            if (StartsWith("<%@"))
            {
                ParseDirective();
                continue;
            }

            if (StartsWith("<%"))
            {
                if (!ConsumeCodeBlock())
                    Issue(RuleIds.AspxMarkup, LineAt(_index), ColumnAt(_index), "Server code block is not closed.", "Close the block with %>.");
                continue;
            }

            if (StartsWith("<!--"))
            {
                if (!ConsumeUntil("-->", out _))
                    Issue(RuleIds.AspxMarkup, LineAt(_index), ColumnAt(_index), "HTML comment is not closed.", "Close the comment with -->.");
                continue;
            }

            if (StartsWith("<!"))
            {
                ConsumeUntil(">", out _);
                continue;
            }

            if (StartsWith("<?"))
            {
                ConsumeUntil("?>", out _);
                continue;
            }

            if (StartsWith("</"))
            {
                ParseEndTag(stack, idScopes);
                continue;
            }

            if (IsTagStart())
            {
                ParseStartTag(stack, idScopes);
                continue;
            }

            _index++;
        }

        while (stack.Count > 0)
        {
            var open = stack.Pop();
            Issue(RuleIds.AspxMarkup, open.Line, open.Column,
                $"Unclosed tag <{open.Name}>.",
                $"Add a closing </{open.Name}> tag.");
        }

        return (_document, _issues);
    }

    private void ParseDirective()
    {
        var start = _index;
        _index += 3;
        SkipWhitespace();
        var name = ReadName();
        if (name.Length == 0)
        {
            Issue(RuleIds.AspxDirective, LineAt(start), ColumnAt(start), "Directive is missing a name.", "Use <%@ Page %>, <%@ Control %>, or <%@ Register %>.");
            ConsumeUntil("%>", out _);
            return;
        }

        var directive = new AspxDirective { Name = name, Line = LineAt(start) };
        while (_index < _text.Length && !StartsWith("%>"))
        {
            SkipWhitespace();
            if (StartsWith("%>") || _index >= _text.Length)
                break;
            var attrStart = _index;
            var attr = ReadName();
            if (attr.Length == 0)
            {
                Issue(RuleIds.AspxDirective, LineAt(attrStart), ColumnAt(attrStart), "Directive has a malformed attribute.", "Write attributes as Name=\"value\".");
                if (!ConsumeUntil("%>", out _))
                    return;
                break;
            }

            SkipWhitespace();
            string value = string.Empty;
            if (Peek('='))
            {
                _index++;
                SkipWhitespace();
                if (!TryReadQuoted(out value, out var quoteError))
                {
                    Issue(RuleIds.AspxDirective, LineAt(attrStart), ColumnAt(attrStart), quoteError ?? "Directive attribute is missing a quoted value.", "Wrap the attribute value in double quotes.");
                    continue;
                }
            }

            directive.Attributes[attr] = value;
        }

        if (!StartsWith("%>"))
            Issue(RuleIds.AspxDirective, directive.Line, 1, $"Directive <%@ {name} %> is not closed.", "End the directive with %>.");
        else
            _index += 2;

        _document.Directives.Add(directive);
        RememberRegister(directive);
    }

    private void RememberRegister(AspxDirective directive)
    {
        if (!directive.Name.Equals("Register", StringComparison.OrdinalIgnoreCase))
            return;
        if (!directive.Attributes.TryGetValue("TagPrefix", out var prefix) || string.IsNullOrWhiteSpace(prefix))
            return;
        _prefixes[prefix] = new RegisteredPrefix(
            directive.Attributes.TryGetValue("TagName", out var tag) ? tag : null,
            directive.Attributes.ContainsKey("Namespace"),
            directive.Attributes.ContainsKey("Src"));
    }

    private void ParseStartTag(Stack<OpenTag> stack, Stack<HashSet<string>> idScopes)
    {
        var start = _index;
        _index++;
        var name = ReadName();
        if (name.Length == 0)
        {
            Issue(RuleIds.AspxMarkup, LineAt(start), ColumnAt(start), "Tag name is missing.", "Write a complete start tag such as <div> or <asp:Label runat=\"server\" />.");
            return;
        }

        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var selfClosing = false;
        while (_index < _text.Length)
        {
            SkipWhitespace();
            if (_index >= _text.Length)
                break;
            if (StartsWith("/>"))
            {
                selfClosing = true;
                _index += 2;
                break;
            }

            if (Peek('>'))
            {
                _index++;
                break;
            }

            var attrAt = _index;
            var attr = ReadName();
            if (attr.Length == 0)
            {
                Issue(RuleIds.AspxMarkup, LineAt(attrAt), ColumnAt(attrAt), $"Malformed attribute on <{name}>.", "Use name=\"value\" with a matching quotation mark.");
                RecoverToTagEnd();
                return;
            }

            SkipWhitespace();
            var value = string.Empty;
            if (Peek('='))
            {
                _index++;
                SkipWhitespace();
                if (Peek('"') || Peek('\''))
                {
                    if (!TryReadQuoted(out value, out var quoteError))
                    {
                        Issue(RuleIds.AspxMarkup, LineAt(attrAt), ColumnAt(attrAt), quoteError ?? $"Attribute {attr} has an invalid quotation mark.", "Close the attribute quote.");
                        RecoverToTagEnd();
                        return;
                    }
                }
                else
                {
                    value = ReadUnquoted();
                }
            }

            if (attributes.ContainsKey(attr))
            {
                Issue(RuleIds.AspxMarkup, LineAt(attrAt), ColumnAt(attrAt), $"Duplicate attribute '{attr}' on <{name}>.", "Keep a single attribute.");
            }

            attributes[attr] = value;
        }

        var prefix = PrefixOf(name);
        var local = LocalOf(name);
        var isAsp = prefix.Equals("asp", StringComparison.OrdinalIgnoreCase);
        var registered = _prefixes.TryGetValue(prefix, out var reg);
        var isServerTag = isAsp || registered;
        attributes.TryGetValue("runat", out var runat);
        var hasRunat = attributes.ContainsKey("runat");
        var runatServer = hasRunat && string.Equals(runat, "server", StringComparison.OrdinalIgnoreCase);

        if (hasRunat && !runatServer)
        {
            Issue(RuleIds.AspxRunat, LineAt(start), ColumnAt(start),
                $"Invalid runat value '{runat}' on <{name}>.",
                "Set runat=\"server\". The value is case-insensitive, so runat=\"Server\" is valid.");
        }

        if (isServerTag && !runatServer)
        {
            Issue(RuleIds.AspxRunat, LineAt(start), ColumnAt(start),
                $"Server control <{name}> is missing runat=\"server\".",
                "Add runat=\"server\" to the control.");
        }

        if (isAsp && _enforceUnknownControls && _catalog.LoadedFromAssemblies &&
            !_catalog.ServerControlNames.Contains(local))
        {
            Issue(RuleIds.AspxServerControl, LineAt(start), ColumnAt(start),
                $"<{name}> is not a server control in the referenced ASP.NET assemblies.",
                "Use a control from System.Web or register the custom control with <%@ Register %>.");
        }
        else if (!isAsp && registered && reg != null && !reg.AnyTypeInNamespace)
        {
            if (reg.TagName == null || !reg.TagName.Equals(local, StringComparison.OrdinalIgnoreCase))
            {
                Issue(RuleIds.AspxServerControl, LineAt(start), ColumnAt(start),
                    $"<{name}> does not match the <%@ Register %> TagName for prefix '{prefix}'.",
                    "Use the registered tag name or add another <%@ Register %> directive.");
            }
        }
        else if (!isAsp && prefix.Length > 0 && !registered && !name.Equals(local, StringComparison.Ordinal))
        {
            Issue(RuleIds.AspxServerControl, LineAt(start), ColumnAt(start),
                $"Tag prefix '{prefix}' is not registered.",
                "Add <%@ Register TagPrefix=\"" + prefix + "\" ... %> or remove the prefix.");
        }

        var isServer = isServerTag || runatServer;
        attributes.TryGetValue("id", out var id);
        if (isServer && !string.IsNullOrWhiteSpace(id))
        {
            var scope = idScopes.Peek();
            if (!scope.Add(id))
            {
                Issue(RuleIds.AspxDuplicateId, LineAt(start), ColumnAt(start),
                    $"Duplicate server-side ID '{id}'.",
                    "Give this control a unique ID within its naming container.");
            }

            _document.ServerElements.Add(new AspxElement
            {
                Name = name,
                Line = LineAt(start),
                IsServerControl = true,
                Id = id,
                HasRunat = hasRunat,
                RunatServer = runatServer,
                RunatValue = runat
            });
        }

        var pushesScope = TemplateTags.Contains(local) ||
                          (isServer && _catalog.NamingContainerNames.Contains(local));
        if (pushesScope && !selfClosing)
            idScopes.Push(new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        var isVoid = VoidHtml.Contains(local) && prefix.Length == 0;
        if (!selfClosing && !isVoid)
        {
            stack.Push(new OpenTag(name, LineAt(start), ColumnAt(start), pushesScope));
            if (local.Equals("script", StringComparison.OrdinalIgnoreCase) ||
                local.Equals("style", StringComparison.OrdinalIgnoreCase))
            {
                if (ConsumeRawUntilEnd(local) &&
                    stack.Count > 0 &&
                    stack.Peek().Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var closed = stack.Pop();
                    if (closed.PushesScope && idScopes.Count > 1)
                        idScopes.Pop();
                }
            }
        }
    }

    private void ParseEndTag(Stack<OpenTag> stack, Stack<HashSet<string>> idScopes)
    {
        var start = _index;
        _index += 2;
        var name = ReadName();
        SkipWhitespace();
        if (Peek('>'))
            _index++;
        else
            Issue(RuleIds.AspxMarkup, LineAt(start), ColumnAt(start), $"Closing tag </{name}> is malformed.", "End the closing tag with >.");

        if (name.Length == 0)
        {
            Issue(RuleIds.AspxMarkup, LineAt(start), ColumnAt(start), "Closing tag is missing a name.", "Write a closing tag such as </div>.");
            return;
        }

        if (stack.Count == 0)
        {
            Issue(RuleIds.AspxMarkup, LineAt(start), ColumnAt(start), $"Unexpected closing tag </{name}>.", "Remove the closing tag or open the element first.");
            return;
        }

        var top = stack.Peek();
        if (!top.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            Issue(RuleIds.AspxMarkup, LineAt(start), ColumnAt(start),
                $"Closing tag </{name}> does not match open tag <{top.Name}> from line {top.Line}.",
                $"Close <{top.Name}> before </{name}>, or correct the nesting.");
            return;
        }

        var closed = stack.Pop();
        if (closed.PushesScope && idScopes.Count > 1)
            idScopes.Pop();
    }

    private bool ConsumeRawUntilEnd(string tagName)
    {
        var closer = "</" + tagName;
        while (_index < _text.Length)
        {
            if (StartsWith(closer, StringComparison.OrdinalIgnoreCase))
            {
                _index += closer.Length;
                SkipWhitespace();
                if (Peek('>'))
                    _index++;
                return true;
            }

            _index++;
        }

        Issue(RuleIds.AspxMarkup, LineAt(Math.Max(0, _index - 1)), 1,
            $"<{tagName}> is not closed.",
            $"Add </{tagName}>.");
        return false;
    }

    private bool ConsumeCodeBlock()
    {
        var start = _index;
        _index += 2;
        var quote = '\0';
        while (_index < _text.Length)
        {
            var c = _text[_index];
            if (quote == '\0' && StartsWith("%>"))
            {
                _index += 2;
                return true;
            }

            if (quote == '\0' && (c == '"' || c == '\''))
                quote = c;
            else if (quote != '\0' && c == quote && !IsEscaped(_index))
                quote = '\0';
            _index++;
        }

        return false;
    }

    private bool TryReadQuoted(out string value, out string? error)
    {
        value = string.Empty;
        error = null;
        if (_index >= _text.Length || (_text[_index] != '"' && _text[_index] != '\''))
        {
            error = "Attribute value must be quoted.";
            return false;
        }

        var quote = _text[_index++];
        var start = _index;
        while (_index < _text.Length)
        {
            if (_text[_index] == quote && !IsEscaped(_index))
            {
                value = _text[start.._index];
                _index++;
                return true;
            }

            _index++;
        }

        error = "Attribute quotation mark is not closed.";
        return false;
    }

    private void RecoverToTagEnd()
    {
        while (_index < _text.Length && _text[_index] != '>')
            _index++;
        if (_index < _text.Length)
            _index++;
    }

    private string ReadUnquoted()
    {
        var start = _index;
        while (_index < _text.Length && !char.IsWhiteSpace(_text[_index]) && _text[_index] != '>' && _text[_index] != '/')
            _index++;
        return _text[start.._index];
    }

    private string ReadName()
    {
        var start = _index;
        while (_index < _text.Length)
        {
            var c = _text[_index];
            if (char.IsLetterOrDigit(c) || c is '_' or ':' or '-' or '.')
                _index++;
            else
                break;
        }

        return _text[start.._index];
    }

    private void SkipWhitespace()
    {
        while (_index < _text.Length && char.IsWhiteSpace(_text[_index]))
            _index++;
    }

    private bool ConsumeUntil(string marker, out bool found)
    {
        var at = _text.IndexOf(marker, _index, StringComparison.Ordinal);
        if (at < 0)
        {
            _index = _text.Length;
            found = false;
            return false;
        }

        _index = at + marker.Length;
        found = true;
        return true;
    }

    private bool IsTagStart()
    {
        if (!Peek('<') || _index + 1 >= _text.Length)
            return false;
        var next = _text[_index + 1];
        return char.IsLetter(next) || next == '_';
    }

    private bool StartsWith(string value, StringComparison comparison = StringComparison.Ordinal)
    {
        return _index + value.Length <= _text.Length &&
               string.Compare(_text, _index, value, 0, value.Length, comparison) == 0;
    }

    private bool Peek(char value) => _index < _text.Length && _text[_index] == value;

    private bool IsEscaped(int index)
    {
        var slashes = 0;
        for (var i = index - 1; i >= 0 && _text[i] == '\\'; i--)
            slashes++;
        return slashes % 2 == 1;
    }

    private void Issue(string rule, int line, int column, string message, string fix)
    {
        _issues.Add(ValidationError.Create(rule, ValidationSeverity.Error, _filePath, line, column, null, null, message, fix));
    }

    private int LineAt(int index)
    {
        var line = 1;
        var low = 0;
        var high = _lineStarts.Count - 1;
        while (low <= high)
        {
            var mid = (low + high) / 2;
            if (_lineStarts[mid] <= index)
            {
                line = mid + 1;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return line;
    }

    private int ColumnAt(int index)
    {
        var line = LineAt(index);
        var start = _lineStarts[Math.Max(0, line - 1)];
        return index - start + 1;
    }

    private static List<int> BuildLineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                starts.Add(i + 1);
        }

        return starts;
    }

    private static string PrefixOf(string name)
    {
        var colon = name.IndexOf(':');
        return colon < 0 ? string.Empty : name[..colon];
    }

    private static string LocalOf(string name)
    {
        var colon = name.IndexOf(':');
        return colon < 0 ? name : name[(colon + 1)..];
    }

    private readonly record struct OpenTag(string Name, int Line, int Column, bool PushesScope);

    private sealed record RegisteredPrefix(string? TagName, bool AnyTypeInNamespace, bool FromSrc);
}
