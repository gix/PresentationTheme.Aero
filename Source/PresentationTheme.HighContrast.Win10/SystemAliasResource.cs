namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Windows;

    public class SystemAliasResourceExtension : DynamicResourceExtension
    {
        public SystemAliasResourceExtension(string resourceKey)
            : base(ConvertResourceKey(resourceKey))
        {
        }

        private static object ConvertResourceKey(string resourceKey)
        {
            switch (resourceKey) {
                case "Button.Background.Normal": return SystemColors.ControlBrushKey;
                case "Button.Background.Hot.Color": return SystemColors.HighlightColorKey;
                case "Button.Background.Pressed.Color": return SystemColors.ControlColorKey;
                case "Button.Background.Disabled.Color": return SystemColors.ControlColorKey;
                case "Button.Background.Defaulted.Color": return SystemColors.HighlightColorKey;
                case "Button.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "Button.Border.Hot.Color": return SystemColors.ControlTextColorKey;
                case "Button.Border.Pressed.Color": return SystemColors.ControlTextColorKey;
                case "Button.Border.Disabled.Color": return SystemColors.GrayTextColorKey;
                case "Button.Border.Defaulted.Color": return SystemColors.ControlTextColorKey;
                case "Button.Foreground.Hot.Color": return SystemColors.HighlightTextColorKey;
                case "Button.Foreground.Defaulted.Color": return SystemColors.HighlightTextColorKey;
                case "Button.Foreground.Disabled": return SystemColors.GrayTextBrushKey;

                case "CheckBox.Background.Normal": return SystemColors.ControlBrushKey;
                case "CheckBox.Background.Hot": return SystemColors.ControlBrushKey;
                case "CheckBox.Background.Pressed": return SystemColors.ControlBrushKey;
                case "CheckBox.Background.Disabled": return SystemColors.ControlBrushKey;
                case "CheckBox.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "CheckBox.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "CheckBox.Border.Pressed": return SystemColors.ControlTextBrushKey;
                case "CheckBox.Border.Disabled": return SystemColors.ControlTextBrushKey;
                case "CheckBox.Mark.Normal": return SystemColors.ControlTextBrushKey;
                case "CheckBox.Mark.Hot": return SystemColors.ControlTextBrushKey;
                case "CheckBox.Mark.Pressed": return SystemColors.ControlTextBrushKey;
                case "CheckBox.Mark.Disabled": return SystemColors.ControlTextBrushKey;

                case "ComboBox.Button.Background.Normal": return SystemColors.ControlBrushKey;
                case "ComboBox.Button.Background.Hot.Color": return SystemColors.ControlColorKey;
                case "ComboBox.Button.Background.Pressed.Color": return SystemColors.ControlColorKey;
                case "ComboBox.Button.Background.Disabled.Color": return SystemColors.ControlColorKey;
                case "ComboBox.Button.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "ComboBox.Button.Border.Hot.Color": return SystemColors.HotTrackColorKey;
                case "ComboBox.Button.Border.Pressed.Color": return SystemColors.HotTrackColorKey;
                case "ComboBox.Button.Border.Disabled.Color": return SystemColors.GrayTextColorKey;
                case "ComboBox.Glyph.Normal": return SystemColors.ControlTextBrushKey;
                case "ComboBox.Glyph.Hot.Color": return SystemColors.ControlTextColorKey;
                case "ComboBox.Glyph.Pressed.Color": return SystemColors.ControlTextColorKey;
                case "ComboBox.Glyph.Disabled.Color": return SystemColors.ControlTextColorKey;
                case "ComboBox.Editable.Background.Normal": return SystemColors.ControlBrushKey;
                case "ComboBox.Editable.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "ComboBox.Editable.Border.Disabled": return SystemColors.GrayTextBrushKey;
                case "ComboBox.DropDownButton.Background.Normal": return SystemColors.ControlBrushKey;
                case "ComboBox.DropDownButton.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "ComboBox.ListBox.Background.Hot": return SystemColors.WindowBrushKey;
                case "ComboBox.ListBox.Border.Hot": return SystemColors.WindowTextBrushKey;

                case "Explorer.ListView.Item.Background.Normal": return SystemColors.ControlBrushKey;
                case "Explorer.ListView.Item.Background.Hot": return SystemColors.ControlBrushKey;
                case "Explorer.ListView.Item.Background.Selected": return SystemColors.HighlightBrushKey;
                case "Explorer.ListView.Item.Background.HotSelected": return SystemColors.HighlightBrushKey;
                case "Explorer.ListView.Item.Background.SelectedNotFocus": return SystemColors.HighlightBrushKey;
                case "Explorer.ListView.Item.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "Explorer.ListView.Item.Border.Selected": return SystemColors.ControlTextBrushKey;
                case "Explorer.ListView.Item.Border.HotSelected": return SystemColors.ControlTextBrushKey;
                case "Explorer.ListView.Item.Border.SelectedNotFocus": return SystemColors.ControlTextBrushKey;
                case "Explorer.ListView.Item.Foreground.Hot": return SystemColors.WindowTextBrushKey;
                case "Explorer.ListView.Item.Foreground.Selected": return SystemColors.WindowTextBrushKey;
                case "Explorer.ListView.Item.Foreground.HotSelected": return SystemColors.WindowTextBrushKey;
                case "Explorer.ListView.Item.Foreground.SelectedNotFocus": return SystemColors.WindowTextBrushKey;

                case "Explorer.TreeViewItem.Background.Normal": return SystemColors.ControlBrushKey;
                case "Explorer.TreeViewItem.Background.Hot": return SystemColors.ControlBrushKey;
                case "Explorer.TreeViewItem.Background.Selected": return SystemColors.HighlightBrushKey;
                case "Explorer.TreeViewItem.Background.HotSelected": return SystemColors.HighlightBrushKey;
                case "Explorer.TreeViewItem.Background.SelectedNotFocus": return SystemColors.HighlightBrushKey;
                case "Explorer.TreeViewItem.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "Explorer.TreeViewItem.Border.Selected": return SystemColors.ControlTextBrushKey;
                case "Explorer.TreeViewItem.Border.HotSelected": return SystemColors.ControlTextBrushKey;
                case "Explorer.TreeViewItem.Border.SelectedNotFocus": return SystemColors.ControlTextBrushKey;
                case "Explorer.TreeViewItem.Foreground.Hot": return SystemColors.ControlTextBrushKey; // Inconsistent with ListView
                case "Explorer.TreeViewItem.Foreground.Selected": return SystemColors.HighlightTextBrushKey;
                case "Explorer.TreeViewItem.Foreground.HotSelected": return SystemColors.HighlightTextBrushKey;
                case "Explorer.TreeViewItem.Foreground.SelectedNotFocus": return SystemColors.HighlightTextBrushKey;

                case "GroupBox.Border": return SystemColors.ControlTextBrushKey;

                case "Header.Background.Normal": return SystemColors.ControlBrushKey;
                case "Header.Background.Hot": return SystemColors.ControlBrushKey;
                case "Header.Background.Pressed": return SystemColors.ControlBrushKey;
                case "Header.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "Header.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "Header.Border.Pressed": return SystemColors.ControlTextBrushKey;

                case "ListBox.Background.Normal": return SystemColors.WindowBrushKey;
                case "ListBox.Background.Disabled": return SystemColors.WindowBrushKey;
                case "ListBox.Border.Normal": return SystemColors.WindowTextBrushKey;
                case "ListBox.Border.Disabled": return SystemColors.WindowTextBrushKey;

                case "ListView.Background.Normal": return SystemColors.WindowBrushKey;
                case "ListView.Background.Disabled": return SystemColors.WindowBrushKey;
                case "ListView.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "ListView.Border.Disabled": return SystemColors.ControlTextBrushKey;

                case "Menu.BarBackground.Background": return SystemColors.ControlBrushKey;
                case "Menu.BarBackground.Border": return SystemColors.ControlBrushKey;
                case "Menu.BarItem.Normal.Foreground": return SystemColors.ControlTextBrushKey;
                case "Menu.BarItem.Hot.Background": return SystemColors.ControlBrushKey;
                case "Menu.BarItem.Hot.Border": return SystemColors.ControlTextBrushKey;
                case "Menu.BarItem.Pressed.Background": return SystemColors.ControlBrushKey;
                case "Menu.BarItem.Pressed.Border": return SystemColors.ControlTextBrushKey;
                case "Menu.BarItem.Disabled.Foreground": return SystemColors.GrayTextBrushKey;
                case "Menu.Popup.Background": return SystemColors.ControlBrushKey;
                case "Menu.Popup.Border": return SystemColors.ControlTextBrushKey;
                case "Menu.PopupItem.Normal.Foreground": return SystemColors.ControlTextBrushKey;
                case "Menu.PopupItem.Hot.Background": return SystemColors.HighlightBrushKey;
                case "Menu.PopupItem.Hot.Border": return SystemColors.ControlTextBrushKey;
                case "Menu.PopupItem.Hot.Foreground": return SystemColors.HighlightTextBrushKey;
                case "Menu.PopupItem.Disabled.Foreground": return SystemColors.GrayTextBrushKey;
                case "Menu.PopupItem.DisabledHot.Background": return SystemColors.ControlBrushKey;
                case "Menu.PopupItem.DisabledHot.Border": return SystemColors.GrayTextBrushKey;
                case "Menu.PopupItem.DisabledHot.Foreground": return SystemColors.GrayTextBrushKey;
                case "Menu.PopupSeparator": return SystemColors.ControlTextBrushKey;
                case "Menu.PopupGutter.Background": return SystemColors.ControlBrushKey;
                case "Menu.PopupGutter.Border": return SystemColors.ControlBrushKey;
                case "Menu.PopupCheckBackground.Normal.Background": return SystemColors.ControlBrushKey;
                case "Menu.PopupCheckBackground.Normal.Border": return SystemColors.ControlTextBrushKey;
                case "Menu.PopupCheckBackground.Disabled.Background": return SystemColors.ControlBrushKey;
                case "Menu.PopupCheckBackground.Disabled.Border": return SystemColors.GrayTextBrushKey;
                case "Menu.PopupCheck.Normal.Foreground": return SystemColors.ControlTextBrushKey;
                case "Menu.PopupCheck.Disabled.Foreground": return SystemColors.ControlTextBrushKey;
                case "Menu.PopupSubmenu.Normal.Foreground": return SystemColors.ControlTextBrushKey;
                case "Menu.PopupSubmenu.Disabled.Foreground": return SystemColors.ControlTextBrushKey;

                case "ProgressBar.Fill.Background.Normal": return SystemColors.HighlightBrushKey;
                case "ProgressBar.Fill.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "ProgressBar.Background": return SystemColors.ControlBrushKey;
                case "ProgressBar.Border": return SystemColors.ControlTextBrushKey;
                case "ProgressBar.Indeterminate.Background": return SystemColors.HighlightBrushKey;
                case "ProgressBar.Indeterminate.Border": return SystemColors.ControlTextBrushKey;

                case "ScrollBar.Background": return SystemColors.ControlBrushKey;
                case "ScrollBar.Border": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.Thumb.Background.Normal": return SystemColors.ControlBrushKey;
                case "ScrollBar.Thumb.Background.Hot": return SystemColors.ControlBrushKey;
                case "ScrollBar.Thumb.Background.Pressed": return SystemColors.ControlBrushKey;
                case "ScrollBar.Thumb.Background.Disabled": return SystemColors.ControlBrushKey;
                case "ScrollBar.Thumb.Background.Hover": return SystemColors.ControlBrushKey;
                case "ScrollBar.Thumb.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.Thumb.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.Thumb.Border.Pressed": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.Thumb.Border.Disabled": return SystemColors.GrayTextBrushKey;
                case "ScrollBar.Thumb.Border.Hover": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.Gripper.Normal": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.Gripper.Hot": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.Gripper.Pressed": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.Gripper.Disabled": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.Gripper.Hover": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.ArrowButton.Glyph.Normal": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.ArrowButton.Glyph.Hot": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.ArrowButton.Glyph.Pressed": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.ArrowButton.Glyph.Disabled": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.ArrowButton.Glyph.Hover": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.ArrowButton.Background.Normal": return SystemColors.ControlBrushKey;
                case "ScrollBar.ArrowButton.Background.Hot": return SystemColors.ControlBrushKey;
                case "ScrollBar.ArrowButton.Background.Pressed": return SystemColors.ControlBrushKey;
                case "ScrollBar.ArrowButton.Background.Disabled": return SystemColors.ControlBrushKey;
                case "ScrollBar.ArrowButton.Background.Hover": return SystemColors.ControlBrushKey;
                case "ScrollBar.ArrowButton.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.ArrowButton.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.ArrowButton.Border.Pressed": return SystemColors.ControlTextBrushKey;
                case "ScrollBar.ArrowButton.Border.Disabled": return SystemColors.GrayTextBrushKey;
                case "ScrollBar.ArrowButton.Border.Hover": return SystemColors.ControlTextBrushKey;

                case "Slider.Thumb.Foreground.Normal":
                    // NB: The native trackbar always renders ticks in the tick
                    // theme color (#FFC4C4C4 for AeroLite) when visual styles
                    // are enabled (i.e. always on Win8+), regardless of actual
                    // system colors. This can make the ticks unreadable. When
                    // visual styles are disabled it uses COLOR_BTNTEXT as
                    // fallback which is a much saner choice.
                    return SystemColors.ControlTextBrushKey;

                case "Slider.Thumb.Background.Normal": return SystemColors.ControlTextBrushKey;
                case "Slider.Thumb.Background.Hot": return SystemColors.HighlightBrushKey;
                case "Slider.Thumb.Background.Pressed": return SystemColors.HighlightBrushKey;
                case "Slider.Thumb.Background.Disabled": return SystemColors.GrayTextBrushKey;
                case "Slider.Thumb.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "Slider.Thumb.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "Slider.Thumb.Border.Pressed": return SystemColors.ControlTextBrushKey;
                case "Slider.Thumb.Border.Disabled": return SystemColors.ControlTextBrushKey;
                case "Slider.ThumbTLBR.Background.Normal": return SystemColors.ControlTextBrushKey;
                case "Slider.ThumbTLBR.Background.Hot": return SystemColors.ControlTextBrushKey;
                case "Slider.ThumbTLBR.Background.Pressed": return SystemColors.ControlTextBrushKey;
                case "Slider.ThumbTLBR.Background.Disabled": return SystemColors.GrayTextBrushKey;
                case "Slider.ThumbTLBR.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "Slider.ThumbTLBR.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "Slider.ThumbTLBR.Border.Pressed": return SystemColors.ControlTextBrushKey;
                case "Slider.ThumbTLBR.Border.Disabled": return SystemColors.ControlTextBrushKey;
                case "Slider.Track.Background": return SystemColors.ControlBrushKey;
                case "Slider.Track.Border": return SystemColors.ControlTextBrushKey;

                case "TabItem.Background.Normal": return SystemColors.ControlBrushKey;
                case "TabItem.Background.Hot": return SystemColors.ControlBrushKey;
                case "TabItem.Background.Selected": return SystemColors.ControlBrushKey;
                case "TabItem.Background.Disabled": return SystemColors.ControlBrushKey;
                case "TabItem.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "TabItem.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "TabItem.Border.Selected": return SystemColors.ControlTextBrushKey;
                case "TabItem.Border.Disabled": return SystemColors.GrayTextBrushKey;

                case "TextBox.Background.Normal": return SystemColors.ControlBrushKey;
                case "TextBox.Background.Hot.Color": return SystemColors.ControlColorKey;
                case "TextBox.Background.Focused.Color": return SystemColors.ControlColorKey;
                case "TextBox.Background.ReadOnly.Color": return SystemColors.ControlColorKey;
                case "TextBox.Background.Disabled.Color": return SystemColors.ControlColorKey;
                case "TextBox.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "TextBox.Border.Hot.Color": return SystemColors.HotTrackColorKey;
                case "TextBox.Border.Focused.Color": return SystemColors.HotTrackColorKey;
                case "TextBox.Border.ReadOnly.Color": return SystemColors.ControlTextColorKey;
                case "TextBox.Border.Disabled.Color": return SystemColors.GrayTextColorKey;

                case "ToolBar.Tray.Background": return SystemColors.ControlBrushKey;
                case "ToolBar.Tray.Border": return SystemColors.ControlBrushKey;
                case "ToolBar.Background": return SystemColors.ControlBrushKey;
                case "ToolBar.TextBox.Background.Normal": return SystemColors.ControlBrushKey;
                case "ToolBar.TextBox.Background.Disabled": return SystemColors.ControlBrushKey;
                case "ToolBar.TextBox.Border.Normal": return SystemColors.ControlTextBrushKey;
                case "ToolBar.TextBox.Border.Hot": return SystemColors.HotTrackBrushKey;
                case "ToolBar.TextBox.Border.Focused": return SystemColors.HotTrackBrushKey;
                case "ToolBar.TextBox.Border.Disabled": return SystemColors.GrayTextBrushKey;
                case "ToolBar.Gripper.Fill": return SystemColors.ControlTextBrushKey;
                case "ToolBar.Separator.Background": return SystemColors.ControlTextBrushKey;
                case "ToolBar.Button.Background.Normal": return SystemColors.ControlBrushKey;
                case "ToolBar.Button.Background.Hot": return SystemColors.HighlightBrushKey;
                case "ToolBar.Button.Background.Pressed": return SystemColors.ControlBrushKey;
                case "ToolBar.Button.Background.Checked": return SystemColors.ControlBrushKey;
                case "ToolBar.Button.Background.Disabled": return SystemColors.ControlBrushKey;
                case "ToolBar.Button.Border.Normal": return SystemColors.ControlBrushKey;
                case "ToolBar.Button.Border.Hot": return SystemColors.ControlTextBrushKey;
                case "ToolBar.Button.Border.Pressed": return SystemColors.ControlTextBrushKey;
                case "ToolBar.Button.Border.Checked": return SystemColors.ControlTextBrushKey;
                case "ToolBar.Button.Border.Disabled": return SystemColors.ControlBrushKey;

                case "TreeView.Background.Normal": return SystemColors.ControlBrushKey;
                case "TreeView.Border.Normal": return SystemColors.ControlTextBrushKey;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(resourceKey), resourceKey, "Invalid resource key");
            }
        }
    }
}
