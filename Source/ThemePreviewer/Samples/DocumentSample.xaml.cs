namespace ThemePreviewer.Samples
{
    using System.IO;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Xps.Packaging;
    using Path = System.IO.Path;

    public partial class DocumentSample
    {
        public DocumentSample()
        {
            InitializeComponent();
            documentViewer.Document = CreateDocument();
        }

        private static FixedDocumentSequence CreateDocument()
        {
            string tempFileName = Path.Combine(Path.GetTempPath(), "ThemePreviewer.DocumentSample.Fixed.xps");
            if (File.Exists(tempFileName))
                File.Delete(tempFileName);

            var document = new XpsDocument(tempFileName, FileAccess.ReadWrite);
            try {
                return CreateDocument(document);
            } finally {
                document.Close();
                // DocumentViewer tries to reload from the loaded uri if the
                // theme changes.
                //File.Delete(tempFileName);
            }
        }

        private static FixedDocumentSequence CreateDocument(XpsDocument document)
        {
            var docSeqWriter = document.AddFixedDocumentSequence();
            var docWriter = docSeqWriter.AddFixedDocument();
            var pageWriter = docWriter.AddFixedPage();

            var xmlWriter = pageWriter.XmlWriter;
            xmlWriter.WriteStartElement("FixedPage");
            xmlWriter.WriteAttributeString("xmlns", "http://schemas.microsoft.com/xps/2005/06");
            xmlWriter.WriteStartElement("StackPanel");
            xmlWriter.WriteAttributeString("Margin", "20");

            xmlWriter.WriteStartElement("TextBlock");
            xmlWriter.WriteString("Dock of the Bay");
            xmlWriter.WriteEndElement();

            var ellipse = new Ellipse {
                Width = 50,
                Height = 50,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                ToolTip = "Wow",
                Margin = new Thickness(0, 20, 0, 0)
            };

            XamlWriter.Save(ellipse, xmlWriter);

            xmlWriter.WriteEndElement(); // StackPanel
            xmlWriter.WriteEndElement(); // FixedPage

            pageWriter.Commit();
            docWriter.Commit();
            docSeqWriter.Commit();

            return document.GetFixedDocumentSequence();
        }
    }
}
