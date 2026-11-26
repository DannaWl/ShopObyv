using System;
using System.Collections.Generic;

namespace ShopObyv.Models;

public partial class Tovar
{
    public string Article { get; set; } = null!;

    public string NameTovar { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public decimal Price { get; set; }

    public string Supplier { get; set; } = null!;

    public string Manufacture { get; set; } = null!;

    public string Category { get; set; } = null!;

    public int Discount { get; set; }

    public int UnitStorage { get; set; }

    public string Description { get; set; } = null!;

    public string Image { get; set; } = null!;

    public string BackColor
    {
        get => Discount > 15 ? "#2E8B57" : "Transparent";
    }

    public string StorageColor
    {
        get => UnitStorage == 0 ? "blue" : "Transparent";
    }

    public decimal? NewPrice
    {
        get => Discount > 0 ? (Price - (Price * ((decimal)Discount / 100))) : null;
    }

    public string ForgroundPrice
    {
        get => Price > 0 ? "red" : "Transparent";
    }

    public string textDecorations
    {
        get => Price > 0 ? "Strikethrough" : "None";
    }


    //public string Photo
    //{
    //    get
    //    {
    //        if (Image == null)
    //            return @"\image\picture.png";
    //        else
    //            return "\\image\\" + Image;
    //    }
    //}
    public virtual Category CategoryNavigation { get; set; } = null!;

    public virtual Manufactre ManufactureNavigation { get; set; } = null!;

    public virtual Supplier SupplierNavigation { get; set; } = null!;

    public virtual Unit UnitNavigation { get; set; } = null!;
}
