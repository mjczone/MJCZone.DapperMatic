using Akov.NetDocsProcessor.Output;
using Akov.NetDocsProcessor.Xml;

namespace Akov.NetDocsProcessor.Extensions;

internal static class XmlMemberElementExtensions
{
    public static void FillBy(this IXmlMemberBaseElement element, XmlMember xmlMember)
    {
        element.Summary = xmlMember.Summary?.InnerXml?.Trim();
        element.Example = xmlMember.Example?.InnerXml?.Trim();
        element.Remarks = xmlMember.Remarks?.InnerXml?.Trim();
        element.TypeParameters = xmlMember
            .TypeParameters?.Select(t => new TypeParameterInfo { Name = t.Name, Text = t.Text })
            .ToList();

        if (element is IXmlMemberElement member)
        {
            member.Exceptions = xmlMember
                .Exceptions?.Select(e => new ExceptionInfo
                {
                    Text = e.Text,
                    Reference = e.Reference
                })
                .ToList();

            member.Parameters = xmlMember
                .Parameters?.Select(t => new ParameterInfo { Name = t.Name, Text = t.Text })
                .ToList();

            member.Returns = xmlMember.Returns?.InnerXml;
        }
    }
}
