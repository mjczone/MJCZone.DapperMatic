# GitHub Markdown Syntax using Showdown

See <https://showdownjs.com/docs/markdown-syntax/#simple>

This page demonstrates the typical GitHub Markdown syntax for various elements.

## Headers

Headers are created by using `#`. The number of `#` symbols indicates the header level.

### H3 Header

#### H4 Header

##### H5 Header

###### H6 Header

## Paragraphs

Paragraphs are created by simply writing text. Separate paragraphs by one or more blank lines.

This is a paragraph.

This is another paragraph, separated by a blank line.

## Links

To create links, use the following syntax:

[GitHub](https://github.com)

### Example with Title Attribute

[GitHub](https://github.com "Visit GitHub")

## Images

Images are similar to links, but with an exclamation mark `!` before the link. You can also configure the width and height in a number of ways, see this example and the next.

![GitHub Logo](<https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png> =400x400)

### Image with Title Attribute

![GitHub Logo](https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png?w=500&h=400 "GitHub")

## Ordered Lists

To create an ordered list, simply use numbers followed by a period.

1. First item
2. Second item
3. Third item

## Unordered Lists

To create an unordered list, use asterisks (`*`), plus signs (`+`), or hyphens (`-`).

- Item 1
- Item 2
- Item 3

## Task Lists

GitHub supports task lists. These can be created by adding `- [ ]` for an unchecked box and `- [x]` for a checked box.

- [x] Task 1 (completed)
- [ ] Task 2 (incomplete)

## Blockquotes

Blockquotes are created by using the `>` symbol.

> This is a blockquote.
>
> > It can span multiple lines.
>
> - A list
> - with items

## Bold and Italic

You can make text bold or italic.

*This text will be italic* <br/>
**This text will be bold**

Both bold and italic can use either a * or an _ around the text for styling. This allows you to combine both bold and italic if needed.

**Everyone *must* attend the meeting at 5 o'clock today.**

## Strikethrough

The syntax is the same as GFM, that is, by adding two tilde (~~) characters around a word or groups of words.

a ~~strikethrough~~ element

## Code

### Inline Code

To highlight a small piece of code inline, wrap it with backticks (`).

Use `git status` to check the repository status.

### Code Blocks

To create a code block, use triple backticks (```).

```python
def hello_world():
    print("Hello, World!")
```

And here's some c#.

```csharp
namespace PestControl.Foundry;

public class CentipedeDbConnection: System.Data.Common.DbConnection
{
    // ...
}

public class CentipedeDbMethods: MJCZone.DapperMatic.Interfaces.IDatabaseMethods
{
    // ...
}

public class CentipedeDbMethodsFactory : MJCZone.DapperMatic.Providers.DatabaseMethodsFactoryBase
{
    public override bool SupportsConnection(IDbConnection db)
        => connection.GetType().Name == nameof(CentipedeDbConnection);

    protected override IDatabaseMethods CreateMethodsCore()
        => new CentipedeDbMethods();
}
```

## Tables

| Tables        | Are           | Cool  |
| ------------- |:-------------:| -----:|
| **col 3 is**  | right-aligned | $1600 |
| col 2 is      | *centered*    |   $12 |
| zebra stripes | ~~are neat~~  |    $1 |

All done.
