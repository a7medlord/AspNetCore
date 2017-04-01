using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CloudApp.Data;
using CloudApp.Models;
using CloudApp.Models.BusinessModel;
using CloudApp.Models.ManpulateModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Reporting.WebForms;

namespace CloudApp.Controllers
{
    public class TreatmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private IHostingEnvironment _env;
        public TreatmentsController(ApplicationDbContext context , UserManager<ApplicationUser> user , IHostingEnvironment env)
        {
            _context = context;
            _userManager = user;
            _env = env;
        }
       
        public IActionResult GetSample0Report()
        {

            ReportDataSource custmertDataSource = new ReportDataSource();

            ReportDataSource reportDataSource = new ReportDataSource();



            // Qoution Report
            reportDataSource.Name = "ReportDataSet";
            reportDataSource.Value = _context.Treatment.ToList();
            //Custumer Report
            custmertDataSource.Name = "CustmerDataSet";
            custmertDataSource.Value = _context.Custmer.ToList();
       



            LocalReport local = new LocalReport();
            local.DataSources.Add(reportDataSource);
            //local.SubreportProcessing += delegate (object sender, SubreportProcessingEventArgs args)
            //{
            //    args.DataSources.Add(custmertDataSource);
            //    args.DataSources.Add(instrumentsDataSource);

            //};

            local.ReportPath = "Report/Sm0Report.rdlc";
            local.EnableExternalImages = true;
           // double amount = instruments.Sum(d => d.Amount);

        //    ToWord toWord = new ToWord((decimal)amount, new CurrencyInfo(CurrencyInfo.Currencies.SaudiArabia));

            //ReportParameter[] parameters = {

            //    new ReportParameter("num",  toWord.ConvertToArabic())
            //   };

            //local.SetParameters(parameters);

            byte[] rendervalue = local.Render("Pdf", "");

            return File(rendervalue, "application/pdf");
        }
        [HttpPost]
        public async Task<JsonResult> UploadFile()
        {
                string guid = Guid.NewGuid().ToString();
                string filepath = "sample1attachment/" + guid + ".jpg";
                var strem = new FileStream(Path.Combine(_env.WebRootPath, filepath), FileMode.Create);
                await Request.Form.Files[0].CopyToAsync(strem);
                strem.Close();
                strem.Dispose();
            return Json(guid);
        }

       
        public  IActionResult Index()
        {
           List<TreamntsModelViewForInddex> lists = new List<TreamntsModelViewForInddex>();
            var listoftremantsample1 = _context.Treatment.Include(treatment => treatment.Custmer).ThenInclude(custmer => custmer.Sample).Include(treatment => treatment.ApplicationUser).ToList();
            foreach (Treatment treatment in listoftremantsample1)
            {
                TreamntsModelViewForInddex row = new TreamntsModelViewForInddex()
                {
                    Id = treatment.Id,
                    Clint = CheckNullValue(treatment.Custmer.Name),
                    Owner = CheckNullValue(treatment.Owner),
                    AqarType = CheckNullValue(treatment.Tbuild),
                    CityAndHy = CheckNullValue(treatment.City + " / " + treatment.Gada),
                    Mothmen =ChekNull(treatment.ApplicationUser),
                    SampleId = CheckNullValue(treatment.Custmer.Sample.Name) ,
                    State = GetState(treatment.IsIntered , treatment.IsThmin , treatment.IsAduit , treatment.IsApproved)
                };

                lists.Add(row);
            }
            return View(lists);
        }

        private string CheckNullValue(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                return "�� ���� �� �����";
            }
            return item;
        }

        string ChekNull(ApplicationUser user)
        {
            if (user == null)
            {
                return "�� ���� ��� ";
            }
           return user.EmployName;
        }

        string GetState(params bool[] state)
        {
            bool IsIntered = state[0];
            bool IsThmin = state[1];
            bool IsAduit = state[2];
            bool IsApproved = state[3];
            if (IsIntered && IsThmin == false)
            {
                return "��� �������";
            }
            if (IsThmin && IsAduit == false)
            {
                return "��� �������";
            }
            if (IsAduit && IsApproved == false)
            {
                return "��� �������";
            }
            if (IsApproved)
            {
                return "��������";
            }
            return "��� �������";
        }

       
        public async Task<IActionResult> Details(long? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatment.SingleOrDefaultAsync(m => m.Id == id);
            if (treatment == null)
            {
                return NotFound();
            }

            return View(treatment);
        }

     
        public async Task<IActionResult> Create(int id)
        {
            ViewData["CustmerId"] = new SelectList(_context.Custmer, "Id", "Name");

            ViewData["UserId"] = new SelectList(await _userManager.GetUsersInRoleAsync("th"), "Id", "EmployName");

            Custmer cms = _context.Custmer.SingleOrDefault(custmer => custmer.Id == id);
            var sampleid = cms?.SampleId ?? 1;

            switch (sampleid)
            {
                case 1 :
                    ViewData["Aqartype"] = new SelectList(_context.Flag.Where(d=>d.FlagValue  ==FlagsName.Aqar), "Value", "Value");
                    ViewData["Gentype"] = new SelectList(_context.Flag.Where(d => d.FlagValue == FlagsName.Gen), "Value", "Value");

                    return View(new Treatment());
                case 2:
                    return RedirectToAction("Create","R1Smaple");
                case 3:
                    return RedirectToAction("Create", "R2Smaple");
                default:
                    return View();
            } 
        }

        public IActionResult Select_custmer()
        {
            ViewData["CustmerId"] = new SelectList(_context.Custmer, "Id", "Name");
            return View("Models_Custmor");
        }
        
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind ]Treatment treatment , string ids)
        {
          
            if (ModelState.IsValid)
            {
             
                if (!string.IsNullOrEmpty(ids))
                {
                    string[] imgsids = ids.Split(';');
                    treatment.AttachmentForTreaments = new List<AttachmentForTreament>();
                    for (int i = 0; i < imgsids.Length - 1; i++)
                    {
                        treatment.AttachmentForTreaments.Add(new AttachmentForTreament() { AttachmentId = imgsids[i] });
                    }
                }
              

                if (treatment.IsAduit && User.IsInRole("au"))
                {
                    treatment.Adutit = _userManager.GetUserId(User);
                }
                if (treatment.IsApproved && User.IsInRole("apr"))
                {
                    treatment.Approver = _userManager.GetUserId(User);
                } if (treatment.IsIntered && User.IsInRole("en"))
                {
                    treatment.Intered = _userManager.GetUserId(User);
                } if (treatment.IsThmin && User.IsInRole("th"))
                {
                    treatment.Muthmen = _userManager.GetUserId(User);
                }
                _context.Add(treatment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", new {Id=treatment.Id});
            }
            await GetListBind(treatment.CustmerId);
            return View("Create",treatment);
        }

        async Task GetListBind(long cmsSelectId)
        {
            ViewData["CustmerId"] = new SelectList(_context.Custmer, "Id", "Name", cmsSelectId);
            ViewData["UserId"] = new SelectList(await _userManager.GetUsersInRoleAsync("th"), "Id", "EmployName");
            ViewData["Aqartype"] = new SelectList(_context.Flag.Where(d => d.FlagValue == FlagsName.Aqar), "Value", "Value");
            ViewData["Gentype"] = new SelectList(_context.Flag.Where(d => d.FlagValue == FlagsName.Gen), "Value", "Value");
        }

        public JsonResult RemoveFile(string name)
        {
            _context.Remove(_context.AttachmentForTreaments.SingleOrDefault(treament => treament.AttachmentId == name));
            _context.SaveChanges();
            return Json("true");
        }
     
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatment.Include(treatment1 => treatment1.AttachmentForTreaments).SingleOrDefaultAsync(m => m.Id == id);

            if (treatment == null)
            {
                return NotFound();
            }

            string files = "";
            foreach (AttachmentForTreament file in treatment.AttachmentForTreaments)
            {
                    files += file.AttachmentId + ";";
            }
            ViewData["imgs"] = files;
           await GetListBind(treatment.CustmerId);
            return View(treatment);
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind] Treatment treatment , string ids)
        {
            if (id != treatment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(ids))
                    {
                        string[] imgsids = ids.Split(';');
                        treatment.AttachmentForTreaments = new List<AttachmentForTreament>();
                        for (int i = 0; i < imgsids.Length - 1; i++)
                        {
                            treatment.AttachmentForTreaments.Add(new AttachmentForTreament() { AttachmentId = imgsids[i] });
                        }
                    }


                    if (treatment.IsAduit && User.IsInRole("au"))
                    {
                        treatment.Adutit = _userManager.GetUserId(User);
                    }
                    if (treatment.IsApproved && User.IsInRole("apr"))
                    {
                        treatment.Approver = _userManager.GetUserId(User);
                    }
                    if (treatment.IsIntered && User.IsInRole("en"))
                    {
                        treatment.Intered = _userManager.GetUserId(User);
                    }
                    if (treatment.IsThmin && User.IsInRole("th"))
                    {
                        treatment.Muthmen = _userManager.GetUserId(User);
                    }
                    _context.Update(treatment);
                    await _context.SaveChangesAsync();
                    RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TreatmentExists(treatment.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction("Index");
            }
          await  GetListBind(treatment.CustmerId);
            return View(treatment);
        }

      
        public JsonResult Delete(long? id)
        {
            _context.Remove(_context.Treatment.SingleOrDefault(treatment => treatment.Id == id));
            _context.SaveChanges();
            return Json("true");
        }
        
        private bool TreatmentExists(long id)
        {
            return _context.Treatment.Any(e => e.Id == id);
        }
    }
}
