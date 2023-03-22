using iTextSharp.text.pdf;
using iTextSharp.text;
using Document = iTextSharp.text.Document;
using System.Reflection.Metadata;

namespace NightFisionAutomatedPrintAndPickList.Services
{
    internal class GeneratePdfService
    {
        private static object _Lock = new object();

        public void GeneratePdf(Assembly assembly, StockOnHand stockOnHand)
        {
            var pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PDF");

            // Check if the Logs directory exists, create it if it doesn't
            if (!Directory.Exists(pdfPath))
            {
                Directory.CreateDirectory(pdfPath);
            }

            var outputPath = Path.Combine(pdfPath, $"{assembly.SalesOrderNumber}_{DateTime.Now:MMddyyyy_HHmmssfff}.pdf");

            // Use a lock to prevent multiple threads from accessing the file simultaneously
            lock (_Lock)
            {
                // Create a new document
                Document document = new Document();

                // Set the document margins and page size
                document.SetMargins(30f, 30f, 50f, 50f);
                document.SetPageSize(PageSize.A4);

                // Create a new PDF writer
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create));

                // Open the document
                document.Open();

                // Generate header
                generateHeader(document, assembly);

                // Add grey block
                generateGreyBlock(document);

                // Add Assembly Information
                addAssemblyInformation(document, assembly);

                // Generate components header
                addComponentsHeader(document, assembly);

                // Add components
                addComponents(document, assembly, stockOnHand);

                // Create footer created details data
                addFooterData(writer, document, assembly);

                // Close the document
                document.Close();

            }
        }

        private void generateHeader(Document document, Assembly assembly)
        {
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, 8);
            Font fontH1 = new Font(baseFont, 21);
            Font fontH2 = new Font(baseFont, 12);

            // Create a table with two columns
            var table = new PdfPTable(2);
            table.WidthPercentage = 100;
            // Set the relative widths of the columns
            table.SetWidths(new float[] { 60f, 40f });

            // Add the logo to the first column
            var logo = Image.GetInstance("Assets/night-fision-logo.png");
            logo.ScaleAbsolute(250, 42);
            var logoCell = new PdfPCell(logo);
            logoCell.PaddingTop = 15f;
            logoCell.PaddingLeft = 10f;
            logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
            logoCell.Border = Rectangle.NO_BORDER;
            logoCell.BackgroundColor = new BaseColor(0, 0, 0);
            table.AddCell(logoCell);

            var infoColumn = new PdfPCell();
            infoColumn.Border = Rectangle.NO_BORDER;

            var infoTable = new PdfPTable(1);
            infoTable.WidthPercentage = 100f;


            var textCell1 = new PdfPCell(new Phrase("Assembly", fontH1));
            textCell1.Border = Rectangle.NO_BORDER;
            textCell1.HorizontalAlignment = Element.ALIGN_RIGHT;
            textCell1.Padding = 5f;
            infoTable.AddCell(textCell1);

            var textCell2 = new PdfPCell(new Phrase(assembly.AssemblyNumber, fontH2));
            textCell2.Border = Rectangle.NO_BORDER;
            textCell2.HorizontalAlignment = Element.ALIGN_RIGHT;
            textCell2.Padding = 5f;
            infoTable.AddCell(textCell2);

            var textCell3 = new PdfPCell(new Phrase(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), font));
            textCell3.Border = Rectangle.NO_BORDER;
            textCell3.HorizontalAlignment = Element.ALIGN_RIGHT;
            textCell3.Padding = 5f;
            infoTable.AddCell(textCell3);

            infoColumn.AddElement(infoTable);
            table.AddCell(infoColumn);

            //Add the table to document
            document.Add(table);
        }

        private void generateGreyBlock(Document document)
        {
            var greyCell = new PdfPCell();
            greyCell.BackgroundColor = new BaseColor(245, 245, 245);
            greyCell.Border = Rectangle.NO_BORDER;
            greyCell.FixedHeight = 15f;

            var table = new PdfPTable(1);
            table.WidthPercentage = 100;
            table.SpacingBefore = 20f;
            table.SpacingAfter = 0f;
            table.DefaultCell.Padding = 0f;
            table.AddCell(greyCell);

            //Add the table to document
            document.Add(table);
        }

        private void addAssemblyInformation(Document document, Assembly assembly)
        {
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, 9);

            // Create a table with two rows and two columns
            var table = new PdfPTable(5);
            table.SpacingBefore = 20f;
            table.WidthPercentage = 100;

            PdfPCell emptyCell = new PdfPCell(new Phrase(""));

            // Add the data to the table
            var sourceLabelCell = new PdfPCell(new Phrase("Source Warehouse: ", font));
            sourceLabelCell.Border = Rectangle.NO_BORDER;
            sourceLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            sourceLabelCell.Padding = 5f;
            table.AddCell(sourceLabelCell);

            var sourceWarehouseCell = new PdfPCell(new Phrase(assembly.SourceWarehouse?.WarehouseName, font));
            sourceWarehouseCell.Border = Rectangle.NO_BORDER;
            sourceWarehouseCell.Padding = 5f;
            table.AddCell(sourceWarehouseCell);

            var emptySpanCell = new PdfPCell(emptyCell);
            emptySpanCell.Border = Rectangle.NO_BORDER;
            table.AddCell(emptySpanCell);


            var assembledLabelCell = new PdfPCell(new Phrase("Assembled Date: ", font));
            assembledLabelCell.Border = Rectangle.NO_BORDER;
            assembledLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            assembledLabelCell.Padding = 5f;
            table.AddCell(assembledLabelCell);

            var assembledDateCell = new PdfPCell(new Phrase(assembly.AssembleBy?.ToString("dd/MM/yyyy"), font));
            assembledDateCell.Border = Rectangle.NO_BORDER;
            assembledDateCell.HorizontalAlignment = Element.ALIGN_CENTER;
            assembledDateCell.Padding = 5f;
            table.AddCell(assembledDateCell);

            var destinationLabelCell = new PdfPCell(new Phrase("Destination Warehouse: ", font));
            destinationLabelCell.Border = Rectangle.NO_BORDER;
            destinationLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            destinationLabelCell.Padding = 5f;
            table.AddCell(destinationLabelCell);

            var destinationWarehouseCell = new PdfPCell(new Phrase(assembly.DestinationWarehouse?.WarehouseName, font));
            destinationWarehouseCell.Border = Rectangle.NO_BORDER;
            destinationWarehouseCell.Padding = 5f;
            table.AddCell(destinationWarehouseCell);

            var emptySpanCell2 = new PdfPCell(emptyCell);
            emptySpanCell2.Border = Rectangle.NO_BORDER;
            table.AddCell(emptySpanCell2);

            var statusLabelCell = new PdfPCell(new Phrase("Assembly Status: ", font));
            statusLabelCell.Border = Rectangle.NO_BORDER;
            statusLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            statusLabelCell.Padding = 5f;
            table.AddCell(statusLabelCell);

            var assemblyStatusCell = new PdfPCell(new Phrase(assembly.AssemblyStatus, font));
            assemblyStatusCell.Border = Rectangle.NO_BORDER;
            assemblyStatusCell.HorizontalAlignment = Element.ALIGN_CENTER;
            assemblyStatusCell.Padding = 5f;
            table.AddCell(assemblyStatusCell);

            // Add the table to the document
            document.Add(table);
        }

        private void addComponentsHeader(Document document, Assembly assembly)
        {
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, 8f);

            // Create a table with 3 columns
            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.SpacingBefore = 30f;

            // Add the first cell
            PdfPCell cell1 = new PdfPCell(new Phrase("Output : GLK-005-290-313-OGZG", font));
            cell1.Border = 0;
            cell1.BackgroundColor = new BaseColor(188, 214, 49);
            cell1.FixedHeight = 30f;
            cell1.Padding = 8f;
            cell1.HorizontalAlignment = Element.ALIGN_CENTER;
            cell1.VerticalAlignment = Element.ALIGN_MIDDLE;
            table.AddCell(cell1);

            // Add the second cell
            PdfPCell cell2 = new PdfPCell(new Phrase("Output Qty : 1.00", font));
            cell2.Border = 0;
            cell2.BackgroundColor = new BaseColor(188, 214, 49);
            cell2.FixedHeight = 30f;
            cell2.Padding = 8f;
            cell2.HorizontalAlignment = Element.ALIGN_CENTER;
            cell2.VerticalAlignment = Element.ALIGN_MIDDLE;
            table.AddCell(cell2);

            // Add the third cell
            PdfPCell cell3 = new PdfPCell(new Phrase("Bin: Created for Invoice 47075.", font));
            cell3.Border = 0;
            cell3.BackgroundColor = new BaseColor(188, 214, 49);
            cell3.FixedHeight = 30f;
            cell3.Padding = 8f;
            cell3.HorizontalAlignment = Element.ALIGN_CENTER;
            cell3.VerticalAlignment = Element.ALIGN_MIDDLE;
            table.AddCell(cell3);

            // Add the table to the document
            document.Add(table);
        }

        private void addComponents(Document document, Assembly assembly, StockOnHand stockOnHand)
        {
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, 8f);
            Font fontH1 = new Font(baseFont, 21, Font.BOLD);
            Font fontTableHeader = new Font(baseFont, 9, Font.BOLD);

            // Define the table and set its properties
            var table = new PdfPTable(6);
            table.WidthPercentage = 100;
            table.SpacingBefore = 30f;
            table.SetWidths(new float[] { 1f, 1f, 1f, 1f, 1f, 1f });

            // Define the headers for the table
            var headers = new string[] { "Initials", "Part", "Bin", "Quantity", "Wastage Qty", "Available" };

            // Add compponent header
            var cellHeader = new PdfPCell(new Phrase("COMPONENTS", fontH1));
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHeader.Colspan = 6;
            cellHeader.Padding = 8f;
            cellHeader.Border = Rectangle.NO_BORDER;
            table.AddCell(cellHeader);

            // Add the headers to the table
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, fontTableHeader));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 8f;
                cell.Border = Rectangle.BOTTOM_BORDER;
                cell.BorderWidth = 0.5f;
                cell.BorderColor = new BaseColor(0, 0, 0);
                table.AddCell(cell);
            }

            // Add the data to the table
            for (int i = 0; i < assembly.AssemblyLines?.Length; i++)
            {
                AssemblyLines line = assembly.AssemblyLines[i];

                var initial = new PdfPCell(new Phrase((i + 1).ToString(), font));
                initial.HorizontalAlignment = Element.ALIGN_CENTER;
                initial.Padding = 8f;
                initial.Border = Rectangle.BOTTOM_BORDER;
                initial.BorderWidth = 0.5f;
                initial.BorderColor = new BaseColor(0, 0, 0);
                table.AddCell(initial);

                var part = new PdfPCell(new Phrase(line?.Product?.ProductDescription, font));
                part.HorizontalAlignment = Element.ALIGN_CENTER;
                part.Padding = 8f;
                part.Border = Rectangle.BOTTOM_BORDER;
                part.BorderWidth = 0.5f;
                part.BorderColor = new BaseColor(0, 0, 0);
                table.AddCell(part);

                var bin = new PdfPCell(new Phrase("bin " + i.ToString(), font));
                bin.HorizontalAlignment = Element.ALIGN_CENTER;
                bin.Padding = 8f;
                bin.Border = Rectangle.BOTTOM_BORDER;
                bin.BorderWidth = 0.5f;
                bin.BorderColor = new BaseColor(0, 0, 0);
                table.AddCell(bin);

                var quantity = new PdfPCell(new Phrase(line?.Quantity, font));
                quantity.HorizontalAlignment = Element.ALIGN_CENTER;
                quantity.Padding = 8f;
                quantity.Border = Rectangle.BOTTOM_BORDER;
                quantity.BorderWidth = 0.5f;
                quantity.BorderColor = new BaseColor(0, 0, 0);
                table.AddCell(quantity);

                var wastage = new PdfPCell(new Phrase(line?.WastageQuantity, font));
                wastage.HorizontalAlignment = Element.ALIGN_CENTER;
                wastage.Padding = 8f;
                wastage.Border = Rectangle.BOTTOM_BORDER;
                wastage.BorderWidth = 0.5f;
                wastage.BorderColor = new BaseColor(0, 0, 0);
                table.AddCell(wastage);

                var available = new PdfPCell(new Phrase(stockOnHand?.AvailableQty, font));
                available.HorizontalAlignment = Element.ALIGN_CENTER;
                available.Padding = 8f;
                available.Border = Rectangle.BOTTOM_BORDER;
                available.BorderWidth = 0.5f;
                available.BorderColor = new BaseColor(0, 0, 0);
                table.AddCell(available);
            }


            // Add the table to the document
            document.Add(table);
        }

        private void addFooterData(PdfWriter writer, Document document, Assembly assembly)
        {
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, 8f);

            var table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.SpacingBefore = 50f;
            table.DefaultCell.Border = Rectangle.NO_BORDER;
            // Set the relative widths of the columns
            table.SetWidths(new float[] { 15f, 35f, 20f, 30f });

            var createdLabel = new PdfPCell(new Phrase("Created By :", font));
            createdLabel.Border = Rectangle.BOTTOM_BORDER;
            createdLabel.Padding = 8f;
            createdLabel.BorderWidth = 0.5f;
            table.AddCell(createdLabel);

            var createdBy = new PdfPCell(new Phrase(assembly.CreatedBy, font));
            createdBy.Border = Rectangle.BOTTOM_BORDER;
            createdBy.BorderWidth = 0.5f;
            createdBy.HorizontalAlignment = Element.ALIGN_LEFT;
            createdBy.Padding = 8f;
            table.AddCell(createdBy);

            var createdOnLabel = new PdfPCell(new Phrase("Created On :", font));
            createdOnLabel.Border = Rectangle.BOTTOM_BORDER;
            createdOnLabel.BorderWidth = 0.5f;
            createdOnLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
            createdOnLabel.Padding = 8f;
            table.AddCell(createdOnLabel);

            var createdOn = new PdfPCell(new Phrase(assembly.CreatedOn?.ToString("dd/MM/yyyy hh:mm:ss tt"), font));
            createdOn.Border = Rectangle.BOTTOM_BORDER;
            createdOn.BorderWidth = 0.5f;
            createdOn.Padding = 8f;
            table.AddCell(createdOn);

            var assembleByLabel = new PdfPCell(new Phrase("Assembled By :", font));
            assembleByLabel.Border = Rectangle.BOTTOM_BORDER;
            assembleByLabel.BorderWidth = 0.5f;
            assembleByLabel.Padding = 8f;
            table.AddCell(assembleByLabel);

            var assembleOn = new PdfPCell(new Phrase("", font));
            assembleOn.Border = Rectangle.BOTTOM_BORDER;
            assembleOn.BorderWidth = 0.5f;
            assembleOn.HorizontalAlignment = Element.ALIGN_LEFT;
            assembleOn.Padding = 8f;
            table.AddCell(assembleOn);

            var assembleDateLabel = new PdfPCell(new Phrase("Date :", font));
            assembleDateLabel.Border = Rectangle.BOTTOM_BORDER;
            assembleDateLabel.BorderWidth = 0.5f;
            assembleDateLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
            assembleDateLabel.Padding = 8f;
            table.AddCell(assembleDateLabel);

            var assembleDate = new PdfPCell(new Phrase("", font));
            assembleDate.Border = Rectangle.BOTTOM_BORDER;
            assembleDate.BorderWidth = 0.5f;
            assembleDate.Padding = 8f;
            table.AddCell(assembleDate);

            var inspectedByLabel = new PdfPCell(new Phrase("Inspected By :", font));
            inspectedByLabel.Border = Rectangle.BOTTOM_BORDER;
            inspectedByLabel.BorderWidth = 0.5f;
            inspectedByLabel.Padding = 8f;
            table.AddCell(inspectedByLabel);

            var inspectedOn = new PdfPCell(new Phrase("", font));
            inspectedOn.Border = Rectangle.BOTTOM_BORDER;
            inspectedOn.BorderWidth = 0.5f;
            inspectedOn.HorizontalAlignment = Element.ALIGN_LEFT;
            inspectedOn.Padding = 8f;
            table.AddCell(inspectedOn);

            var inspectedDateLabel = new PdfPCell(new Phrase("Date :", font));
            inspectedDateLabel.Border = Rectangle.BOTTOM_BORDER;
            inspectedDateLabel.BorderWidth = 0.5f;
            inspectedDateLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
            inspectedDateLabel.Padding = 8f;
            table.AddCell(inspectedDateLabel);

            var inspectedDate = new PdfPCell(new Phrase("", font));
            inspectedDate.Border = Rectangle.BOTTOM_BORDER;
            inspectedDate.BorderWidth = 0.5f;
            inspectedDate.HorizontalAlignment = Element.ALIGN_LEFT;
            inspectedDate.Padding = 8f;
            table.AddCell(inspectedDate);

            var completedByLabel = new PdfPCell(new Phrase("Completed By :", font));
            completedByLabel.Border = Rectangle.BOTTOM_BORDER;
            completedByLabel.BorderWidth = 0.5f;
            completedByLabel.Padding = 8f;
            table.AddCell(completedByLabel);

            var completedBy = new PdfPCell(new Phrase("", font));
            completedBy.Border = Rectangle.BOTTOM_BORDER;
            completedBy.BorderWidth = 0.5f;
            completedBy.Padding = 8f;
            table.AddCell(completedBy);

            var completedDateLabel = new PdfPCell(new Phrase("Date :", font));
            completedDateLabel.Border = Rectangle.BOTTOM_BORDER;
            completedDateLabel.BorderWidth = 0.5f;
            completedDateLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
            completedDateLabel.Padding = 8f;
            table.AddCell(completedDateLabel);

            var completedDate = new PdfPCell(new Phrase("", font));
            completedDate.Border = Rectangle.BOTTOM_BORDER;
            completedDate.BorderWidth = 0.5f;
            completedDate.HorizontalAlignment = Element.ALIGN_RIGHT;
            completedDate.Padding = 8f;
            table.AddCell(completedDate);

            var modifiedByLabel = new PdfPCell(new Phrase("Last Modified By :", font));
            modifiedByLabel.Border = Rectangle.BOTTOM_BORDER;
            modifiedByLabel.BorderWidth = 0.5f;
            modifiedByLabel.Padding = 8f;
            table.AddCell(modifiedByLabel);

            var modifiedBy = new PdfPCell(new Phrase(assembly.LastModifiedBy, font));
            modifiedBy.Border = Rectangle.BOTTOM_BORDER;
            modifiedBy.BorderWidth = 0.5f;
            modifiedBy.HorizontalAlignment = Element.ALIGN_LEFT;
            modifiedBy.Padding = 8f;
            table.AddCell(modifiedBy);

            var modifiedDateLabel = new PdfPCell(new Phrase("Date :", font));
            modifiedDateLabel.Border = Rectangle.BOTTOM_BORDER;
            modifiedDateLabel.BorderWidth = 0.5f;
            modifiedDateLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
            modifiedDateLabel.Padding = 8f;
            table.AddCell(modifiedDateLabel);

            var modifedDate = new PdfPCell(new Phrase(assembly.LastModifiedOn?.ToString("dd/MM/yyyy hh:mm:ss tt"), font));
            modifedDate.Border = Rectangle.BOTTOM_BORDER;
            modifedDate.BorderWidth = 0.5f;
            modifedDate.Padding = 8f;
            table.AddCell(modifedDate);


            // check if there is enough space left on the current page
            if (writer.GetVerticalPosition(false) < 200)
            {
                // not enough space on the current page, add a new page
                document.NewPage();
            }

            // Add the table to the document
            document.Add(table);
        }
    }
}
