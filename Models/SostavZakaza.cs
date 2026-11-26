using System;
using System.Collections.Generic;

namespace ShopObyv.Models;

public partial class SostavZakaza
{
    public int IdSostavaZakaza { get; set; }

    public string ArticleZakaa { get; set; } = null!;

    public int KolvoVzakaze { get; set; }

    public virtual Tovar ArticleZakaaNavigation { get; set; } = null!;

    public virtual Zakaz IdSostavaZakazaNavigation { get; set; } = null!;
}
