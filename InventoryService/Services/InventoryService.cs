using InventoryService.Data;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace InventoryService.Services;

public class InventoryService
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(InventoryDbContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AllocateAsync(Guid orderId, List<OrderItemDto> items)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in items)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventoryItem == null)
                {
                    // Auto-create item with stock if not exists
                    inventoryItem = new InventoryItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = 100, 
                        ReservedQuantity = 0
                    };
                    _context.InventoryItems.Add(inventoryItem);
                }

                if (inventoryItem.Quantity - inventoryItem.ReservedQuantity < item.Quantity)
                {
                    _logger.LogWarning($"Insufficient stock for Product {item.ProductId}. Available: {inventoryItem.Quantity - inventoryItem.ReservedQuantity}, Requested: {item.Quantity}");
                    return false;
                }

                inventoryItem.ReservedQuantity += item.Quantity;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation($"Allocated stock for Order {orderId}");
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error allocating stock");
            return false;
        }
    }

    public async Task CommitAsync(Guid orderId, List<OrderItemDto> items)
    {
        
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in items)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventoryItem != null)
                {
                    inventoryItem.ReservedQuantity -= item.Quantity;
                    inventoryItem.Quantity -= item.Quantity;
                }
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation($"Committed stock for Order {orderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing stock");
        }
    }

    public async Task ReleaseAsync(Guid orderId, List<OrderItemDto> items)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in items)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventoryItem != null)
                {
                    inventoryItem.ReservedQuantity -= item.Quantity;
                }
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation($"Released stock for Order {orderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing stock");
        }
    }
}
