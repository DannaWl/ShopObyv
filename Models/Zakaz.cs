using System;
using System.Collections.Generic;

namespace ShopObyv.Models;

public partial class Zakaz
{
    public int NomZakaza { get; set; }

    public int ArticleZakaa { get; set; }

    public DateTime? DateOrder { get; set; }

    public DateTime DarteDilivery { get; set; }

    public int Pvz { get; set; }

    public string LoginUsers { get; set; } = null!;

    public int Qr { get; set; }

    public string Statys { get; set; } = null!;

    public virtual User LoginUsersNavigation { get; set; } = null!;

    public virtual AdressPvz PvzNavigation { get; set; } = null!;

    public virtual Staty StatysNavigation { get; set; } = null!;
}
