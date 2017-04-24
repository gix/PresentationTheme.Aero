namespace ThemePreviewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

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


        public static MenuNode GetMenu()
        {
            return new MenuNode("&Header",
                new MenuNode("&Header",
                    new MenuNode("Header",
                        new MenuNode("Header",
                            new MenuNode("Item")),
                        new MenuNode("Item")),
                    new MenuNode("Empty",
                        new MenuNode(string.Empty),
                        new MenuNode(string.Empty) { InputGestureText = "Ctrl+A" },
                        new MenuNode(string.Empty) { IsEnabled = false, InputGestureText = "Ctrl+B" }
                        ),
                    new MenuNode("Long",
                        new MenuNode("Long Long Long Long Long Long Long Long Long") {
                            InputGestureText = "Ctrl+N"
                        }),
                    new MenuNode("Item"),
                    new MenuNode("Gesture") { InputGestureText = "Ctrl+N" },
                    new MenuNode("Checked") { IsChecked = true },
                    new MenuNode("Radio") { IsChecked = true, IsRadio = true },
                    new MenuNode { IsSeparator = true },
                    new MenuNode("Disabled Header", new MenuNode("Item")) { IsEnabled = false },
                    new MenuNode("Disabled Item") { IsEnabled = false },
                    new MenuNode("Gesture Disabled") { IsEnabled = false, InputGestureText = "Ctrl+D" },
                    new MenuNode("Checked Disabled") { IsEnabled = false, IsChecked = true },
                    new MenuNode("Radio Disabled") { IsEnabled = false, IsChecked = true, IsRadio = true }
                    ),
                new MenuNode("&Item"),
                new MenuNode("&Checked"),
                new MenuNode("&Radio"),
                new MenuNode("&Disabled") { IsEnabled = false },
                new MenuNode("C&hecked Disabled") { IsEnabled = false, IsChecked = true },
                new MenuNode("R&adio Disabled") { IsEnabled = false, IsChecked = true, IsRadio = true }
                );
        }

        public class MenuNode
        {
            public MenuNode(string text = null, params MenuNode[] children)
            {
                Text = text;
                Children = new List<MenuNode>(children);
            }

            public string Text { get; }
            public bool IsEnabled { get; set; } = true;
            public bool IsChecked { get; set; }
            public bool IsRadio { get; set; }
            public bool IsSeparator { get; set; }
            public string InputGestureText { get; set; }
            public List<MenuNode> Children { get; }
        }
    }
}
