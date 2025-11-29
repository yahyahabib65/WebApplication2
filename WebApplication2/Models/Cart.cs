using System.Collections.Generic;
using System.Linq;

namespace WebApplication2.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(Product product, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (item == null)
            {
                Items.Add(new CartItem { Product = product, Quantity = quantity });
            }
            else
            {
                item.Quantity += quantity;
            }
        }

        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.Product.Id == productId);
        }

        public decimal GetTotal()
        {
            return Items.Sum(i => i.Product.Price * i.Quantity);
        }
    }
}
