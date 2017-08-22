namespace Markdown.ImageComparison
{
    using System.Collections.Generic;
    using System.Composition;
    using System.Text.RegularExpressions;
    using Microsoft.DocAsCode.Dfm;
    using Microsoft.DocAsCode.MarkdownLite;

    public class ImageComparisonBlockToken : IMarkdownToken
    {
        public ImageComparisonBlockToken(
            IMarkdownRule rule, IMarkdownContext context, string label1,
            string href1, string label2, string href2, SourceInfo sourceInfo)
        {
            Rule = rule;
            Context = context;
            Label1 = label1;
            Href1 = href1;
            Label2 = label2;
            Href2 = href2;
            SourceInfo = sourceInfo;
        }

        public IMarkdownRule Rule { get; }

        public IMarkdownContext Context { get; }

        public string Label1 { get; }
        public string Href1 { get; }
        public string Label2 { get; }
        public string Href2 { get; }

        public SourceInfo SourceInfo { get; }
    }

    public class ImageComparisonBlockRule : IMarkdownRule
    {
        public virtual string Name => "ImageComparison";

        public virtual Regex LabelRegex { get; } = new Regex(@"^\[!cmp *\[(?<label1>[^]]+?)\] *\((?<image1>[^)]+?)\) *\[(?<label2>[^]]+?)\] *\((?<image2>[^)]+?)\)\]");

        public virtual IMarkdownToken TryMatch(IMarkdownParser parser, IMarkdownParsingContext context)
        {
            var match = LabelRegex.Match(context.CurrentMarkdown);
            if (match.Length == 0)
                return null;

            var sourceInfo = context.Consume(match.Length);
            return new ImageComparisonBlockToken(
                this,
                parser.Context,
                match.Groups["label1"].Value,
                match.Groups["image1"].Value,
                match.Groups["label2"].Value,
                match.Groups["image2"].Value,
                sourceInfo);
        }
    }

    public sealed class ImageComparisonRendererPart
        : DfmCustomizedRendererPartBase<IMarkdownRenderer, ImageComparisonBlockToken, MarkdownBlockContext>
    {
        public override string Name => "ImageComparisonRendererPart";

        public override bool Match(
            IMarkdownRenderer renderer, ImageComparisonBlockToken token,
            MarkdownBlockContext context) => true;

        public override StringBuffer Render(
            IMarkdownRenderer renderer, ImageComparisonBlockToken token,
            MarkdownBlockContext context)
        {
            return
                "<figure class=\"image-cmp\">" +
                  "<div class=\"image-cmp-l\"><img src=\"" + token.Href1 + "\" alt=\"" + token.Label1 + "\"></div>" +
                  "<div class=\"image-cmp-r\"><img src=\"" + token.Href2 + "\" alt=\"" + token.Label2 + "\"></div>" +
                "</figure>";
        }
    }

    [Export(typeof(IDfmCustomizedRendererPartProvider))]
    public class ImageComparisonRendererPartProvider : IDfmCustomizedRendererPartProvider
    {
        public IEnumerable<IDfmCustomizedRendererPart> CreateParts(
            IReadOnlyDictionary<string, object> parameters)
        {
            yield return new ImageComparisonRendererPart();
        }
    }
}
