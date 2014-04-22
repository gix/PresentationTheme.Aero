namespace ThemePreviewer
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ItemGenerator
    {
        public static IEnumerable<string[]> Generate(int count = 30)
        {
            var types = new[] { "Zip Archive", "7z Archive", "FLAC" };
            for (int i = 0; i < count; ++i) {
                var x = (i * 1373);
                var name = $"Item {i:D2}";
                var type = types[x % types.Length];
                var size = $"{x % 31} KB";
                yield return new[] { name, size, type };
            }
        }

        public static TreeNode GetTree()
        {
            return new TreeNode("Solution 'ThemePreviewer'",
                new TreeNode("ThemePreviewer",
                    new TreeNode("Properties"),
                    new TreeNode("References",
                        new TreeNode("System.Core")),
                    new TreeNode("Resources",
                        new TreeNode("Styles.xaml")),
                    new TreeNode("Program.cs")),
                new TreeNode("ThemePreviewer.Tests",
                    Enumerable.Range(1, 30).Select(x => new TreeNode($"Test {x}")).ToArray()));
        }

        public class TreeNode
        {
            public TreeNode(string name, params TreeNode[] children)
            {
                Name = name;
                Children = new List<TreeNode>(children);
            }

            public string Name { get; }
            public List<TreeNode> Children { get; }
        }
    }
}
