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
        var builder = new StringBuilder()
            .AppendLine("MyShop Gaming Accessories POS")
            .AppendLine($"Invoice #{draft.Id}")
            .AppendLine($"Created: {draft.CreatedTime:yyyy-MM-dd HH:mm}")
            .AppendLine($"Status: {draft.Status}")
            .AppendLine()
            .AppendLine("Items");

        foreach (var item in draft.Items)
        {
            builder.AppendLine($"{item.ProductName} x{item.Quantity} @ {item.UnitSalePrice:N0} = {item.TotalPrice:N0}");
        }

        var subtotal = draft.Items.Sum(x => x.TotalPrice);
        builder.AppendLine()
            .AppendLine($"Subtotal: {subtotal:N0}")
            .AppendLine($"Discount: {draft.DiscountAmount:N0}")
            .AppendLine($"Total: {Math.Max(0m, subtotal - draft.DiscountAmount):N0}");

        await File.WriteAllTextAsync(outputPath, builder.ToString());
        return OperationResult<string>.Ok(outputPath, "Invoice exported.");
    }
}
