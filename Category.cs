using System;
using System.Collections.Generic;

namespace ShopObyv.Models;

public partial class Category
{
    public string IdCategory { get; set; } = null!;

    public virtual ICollection<Tovar> Tovars { get; set; } = new List<Tovar>();
}
