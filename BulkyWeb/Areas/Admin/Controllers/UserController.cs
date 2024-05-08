using Bulky.DataAccess;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;



namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        //private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitofOfWork;
        private readonly ApplicationDbContext _db;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitofOfWork, ApplicationDbContext db)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _unitofOfWork = unitofOfWork;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {


            RoleManagementVM RoleVM = new RoleManagementVM()
            {
                ApplicationUser = _unitofOfWork.ApplicationUser.Get(u=>u.Id == userId, includeProperties:"Company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value= i.Name,
                }),
                CompanyList = _unitofOfWork.Company.GetAll().Select(i => new SelectListItem { Text = i.Name, Value = i.Id.ToString() })
            };

            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitofOfWork.ApplicationUser.Get(u=>u.Id == userId)).GetAwaiter().GetResult().FirstOrDefault();    

            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {



            string oldRole = _userManager.GetRolesAsync(_unitofOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _db.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == roleManagementVM.ApplicationUser.Id);

            if (!(roleManagementVM.ApplicationUser.Role==oldRole))
            {
                //
                if(roleManagementVM.ApplicationUser.Role==SD.Role_Company) { 
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }

                if(oldRole==SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else if(oldRole== SD.Role_Company && applicationUser.CompanyId!= roleManagementVM.ApplicationUser.CompanyId) 
            {
                applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
            }

            _unitofOfWork.ApplicationUser.Update(applicationUser);
            _unitofOfWork.Save();



            return RedirectToAction(nameof(Index));
        }

        #region API Calls
        // this region part is for data table

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _unitofOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();



            foreach(var user in objUserList)
            {
               
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();


                if (user.Company == null)
                {
                    user.Company = new() { Name = "" };
                }
            }

            return Json(new {data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var objFromDb = _unitofOfWork.ApplicationUser.Get(u => u.Id == id);
            if(objFromDb == null)
            {

                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if(objFromDb.LockoutEnd!= null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd= DateTime.Now.AddYears(1000);
            }


            _unitofOfWork.ApplicationUser.Update(objFromDb);
            _unitofOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }
}
