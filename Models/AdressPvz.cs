using System;
using System.Collections.Generic;

namespace ShopObyv.Models;

public partial class AdressPvz
{
    public int IdPvz { get; set; }

    public int IndexPvz { get; set; }

    public string Gorod { get; set; } = null!;

    public string Street { get; set; } = null!;

    public int? Dom { get; set; }

    public virtual ICollection<Zakaz> Zakazs { get; set; } = new List<Zakaz>();
}
