using System;
using System.Collections.Generic;

namespace ShopObyv.Models;

public partial class Supplier
{
    public string IdSupplier { get; set; } = null!;

    public virtual ICollection<Tovar> Tovars { get; set; } = new List<Tovar>();
}
