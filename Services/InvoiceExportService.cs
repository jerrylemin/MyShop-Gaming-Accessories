using ProjectTest.Models;
using ProjectTest.Repositories;
using System.Text;

namespace ProjectTest.Services;

public class InvoiceExportService
{
    private readonly OrderRepository _orderRepository;

    public InvoiceExportService(OrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OperationResult<string>> ExportInvoiceAsync(int orderId, string outputPath)
    {
        var draft = await _orderRepository.GetDraftByIdAsync(orderId);
        if (draft is null)
        {
            return OperationResult<string>.Fail("Order not found.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? AppContext.BaseDirectory);
        await File.WriteAllBytesAsync(outputPath, BuildPdf(draft));
        return OperationResult<string>.Ok(outputPath, "Invoice exported.");
    }

    private static byte[] BuildPdf(OrderDraft draft)
    {
        var subtotal = draft.Items.Sum(x => x.TotalPrice);
        var finalTotal = Math.Max(0m, subtotal - draft.DiscountAmount);
        var lines = new List<string>
        {
            "MyShop Gaming Accessories POS",
            $"Invoice #{draft.Id}",
            $"Created: {draft.CreatedTime:yyyy-MM-dd HH:mm}",
            $"Status: {draft.Status}",
            $"Customer: {DisplayOrDash(draft.CustomerName)}",
            $"Salesperson: {DisplayOrDash(draft.SalespersonName)}",
            "",
            "Items",
            "Product | Qty | Unit price | Line total"
        };

        foreach (var item in draft.Items)
        {
            lines.Add($"{item.ProductName} | {item.Quantity} | {item.UnitSalePrice:N0} | {item.TotalPrice:N0}");
        }

        lines.Add("");
        lines.Add($"Subtotal: {subtotal:N0}");
        lines.Add($"Discount: {draft.DiscountAmount:N0}");
        lines.Add($"Final total: {finalTotal:N0}");

        return SimplePdfWriter.Write("MyShop Invoice", lines);
    }

    private static string DisplayOrDash(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static class SimplePdfWriter
    {
        public static byte[] Write(string title, IReadOnlyList<string> lines)
        {
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
            };

            var content = BuildPageContent(title, lines);
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream");

            var builder = new StringBuilder();
            builder.AppendLine("%PDF-1.4");
            builder.AppendLine("%\u00e2\u00e3\u00cf\u00d3");

            var offsets = new List<int> { 0 };
            for (var index = 0; index < objects.Count; index++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
                builder.AppendLine($"{index + 1} 0 obj");
                builder.AppendLine(objects[index]);
                builder.AppendLine("endobj");
            }

            var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
            builder.AppendLine("xref");
            builder.AppendLine($"0 {objects.Count + 1}");
            builder.AppendLine("0000000000 65535 f ");
            foreach (var offset in offsets.Skip(1))
            {
                builder.AppendLine($"{offset:0000000000} 00000 n ");
            }

            builder.AppendLine("trailer");
            builder.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            builder.AppendLine("startxref");
            builder.AppendLine(xrefOffset.ToString());
            builder.AppendLine("%%EOF");

            return Encoding.ASCII.GetBytes(builder.ToString());
        }

        private static string BuildPageContent(string title, IReadOnlyList<string> lines)
        {
            var content = new StringBuilder();
            content.AppendLine("BT");
            content.AppendLine("/F1 18 Tf");
            content.AppendLine("50 790 Td");
            content.AppendLine($"({Escape(title)}) Tj");
            content.AppendLine("/F1 10 Tf");
            content.AppendLine("0 -28 Td");

            var currentLine = 0;
            foreach (var line in lines.SelectMany(WrapLine))
            {
                if (currentLine > 0)
                {
                    content.AppendLine("0 -16 Td");
                }

                content.AppendLine($"({Escape(line)}) Tj");
                currentLine++;
                if (currentLine >= 45)
                {
                    content.AppendLine($"({Escape("... invoice truncated for page length")}) Tj");
                    break;
                }
            }

            content.AppendLine("ET");
            return content.ToString();
        }

        private static IEnumerable<string> WrapLine(string line)
        {
            var sanitized = ToPdfAscii(line);
            const int maxLength = 96;
            if (sanitized.Length <= maxLength)
            {
                yield return sanitized;
                yield break;
            }

            for (var index = 0; index < sanitized.Length; index += maxLength)
            {
                yield return sanitized.Substring(index, Math.Min(maxLength, sanitized.Length - index));
            }
        }

        private static string ToPdfAscii(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                builder.Append(character is >= ' ' and <= '~' ? character : '?');
            }

            return builder.ToString();
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }
    }
}
