using System;
using System.Collections.Generic;

namespace ShopObyv.Models;

public partial class Staty
{
    public string IdStatys { get; set; } = null!;

    public virtual ICollection<Zakaz> Zakazs { get; set; } = new List<Zakaz>();
}
 