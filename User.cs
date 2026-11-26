using System;
using System.Collections.Generic;

namespace ShopObyv.Models;

public partial class User
{
    public string Role { get; set; } = null!;

    public string Fcs { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual Role RoleNavigation { get; set; } = null!;

    public virtual ICollection<Zakaz> Zakazs { get; set; } = new List<Zakaz>();
}
