using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NightFisionAutomatedPrintAndPickList
{
    internal class GeneratePdfService
    {
        public void GeneratePdf(string outputPath)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var react = new XRect(10, 10, 100, 50);
            gfx.DrawRectangle(XBrushes.Black, react);

            document.Save(outputPath);
        }
    }
}
