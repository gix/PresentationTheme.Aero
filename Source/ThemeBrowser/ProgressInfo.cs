namespace ThemeBrowser
{
    public class ProgressInfo : ViewModel
    {
        private string taskName;
        private ProgressType progressType;
        private int progress;

        public ProgressInfo()
        {
        }

        public string TaskName
        {
            get => taskName;
            set => SetProperty(ref taskName, value);
        }

        public ProgressType ProgressType
        {
            get => progressType;
            set => SetProperty(ref progressType, value);
        }

        public int Progress
        {
            get => progress;
            set => SetProperty(ref progress, value);
        }
    }
}