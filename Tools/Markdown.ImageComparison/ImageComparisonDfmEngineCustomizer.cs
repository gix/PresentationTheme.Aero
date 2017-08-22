namespace Markdown.ImageComparison
{
    using System.Collections.Generic;
    using System.Composition;
    using Microsoft.DocAsCode.Dfm;
    using Microsoft.DocAsCode.MarkdownLite;

    [Export(typeof(IDfmEngineCustomizer))]
    public class ImageComparisonDfmEngineCustomizer : IDfmEngineCustomizer
    {
        public void Customize(DfmEngineBuilder builder, IReadOnlyDictionary<string, object> parameters)
        {
            var index = builder.BlockRules.FindIndex(r => r is MarkdownHeadingBlockRule);
            builder.BlockRules = builder.BlockRules.Insert(
                index, new ImageComparisonBlockRule());
        }
    }
}