using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebHostEnvironment _webHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Company> objCompanyList = _unitOfWork.Company.GetAll();
            return View(objCompanyList);
        }


        //Create

        public IActionResult Upsert(int? id)
        {

            //this is called Projection: select some field from a table 
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            //ViewBag.CategoryList = CategoryList;
            ViewData["CategoryList"] = CategoryList;


            if(id==null || id == 0)
            {
                //create
                return View(new Company());
            }
            else
            {
                //update
                Company companyObj = _unitOfWork.Company.Get(u=>u.Id==id);
                return View(companyObj);
            }

      
        }


        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
           
                if(companyObj.Id == 0)
                {
                _unitOfWork.Company.Add(companyObj);
        

                }
                else
                {
                    _unitOfWork.Company.Update(companyObj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            { 
                return View(companyObj);
            }

        }



       

        ////Delete
        //public IActionResult Delete(int? id) {
        
        //    if(id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Company? productFromDb = _unitOfWork.Company.Get(u=>u.Id == id);

        //    if (productFromDb == null)
        //    {
        //        return NotFound();
        //    }
        
        //    return View(productFromDb);
        //}

        //[HttpPost]
        //[ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{
        //    Company? obj= _unitOfWork.Company.Get(u=> u.Id == id);

        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }

        //    _unitOfWork.Company.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Company deleted succesfully";
        //    return RedirectToAction(nameof(Index));
        //}
        #region API Calls
        // this region part is for data table

        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<Company> objCompanyList = _unitOfWork.Company.GetAll();
            return Json(new {data = objCompanyList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _unitOfWork.Company.Get(u=>u.Id==id);

            if(companyToBeDeleted == null)
            {
                return Json(new {success= false, message ="Error while deleting"});
            }

           
            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
