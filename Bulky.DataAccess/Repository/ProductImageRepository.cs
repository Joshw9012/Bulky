using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {

        public ProductImageRepository(ApplicationDbContext db):base(db)
        {
            
        }


        public void Update(ProductImage obj)
        {
            _db.ProductImages.Update(obj);
        }
    }
}
