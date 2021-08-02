using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yet.Another.Shopping.Cart.Web.Areas.Admin.Models.Catalog
{
    public class SpecificationListModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Published { get; set; }
    }
}
