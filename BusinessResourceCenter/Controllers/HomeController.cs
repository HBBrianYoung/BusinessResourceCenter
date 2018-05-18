using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using BusinessResourceCenter.Models;
using System.IO;



namespace BusinessResourceCenter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //get the list of current workflows and display on the home page
            DashDBContext db = new DashDBContext();
         
            var vWorkflows = from wf in db.vAllWFLastStatusLists
                             select wf;

            vWorkflows = vWorkflows.Where(w => w.workflowstatusID != 4);
            return View(vWorkflows);
           
        }

        public ActionResult edit(int id)
        {
            DashDBContext db = new DashDBContext();

            vAllWFLastStatusLists workflow = db.vAllWFLastStatusLists.Find(id);
            if (workflow == null)
            {
                return HttpNotFound();
            }

            IEnumerable<workflownotes> workflownts = from wfn in db.workflownotes
                                                     orderby wfn.notetimestamp descending
                                                     select wfn;
            workflownts = workflownts.Where(w => w.wfID == id);


            var workflowHist = from wfn in db.workflowtimestamps
                               join workflowstatus wfs in db.workflowstatus on wfn.workflowstatusID equals wfs.wfsid
                               orderby wfn.stamp descending
                               select new workflowtimestampshistory { stamp = wfn.stamp, participant = wfn.participant, wfstatus = wfs.wfstatus, wfNumber = wfn.wfNumber};
            workflowHist = workflowHist.Where(w => w.wfNumber == id);

            /*
            * Get the files associated with this workflow

             var model = new allWFfiles { wffilelist = vWorkflowfiles.ToList() };

             */
            var wffiles = from wf in db.workflowfiles
                                 select wf;

            wffiles = wffiles.Where(w => w.wfID == id);

            var model = new AllWFInfo { ViewWorkFlowData = workflow, wfnotes = workflownts.ToList(), workflowHistory = workflowHist.ToList(), workflowfileList = wffiles.ToList() };

            return View(model);
        }

        public ActionResult Create()
        {
           
            return View();

        }

        [HttpPost]
        public void DocumentUpload(IEnumerable<HttpPostedFileBase> upfile)
        {

            foreach (var file in upfile)
            {
                if (file.ContentLength > 0)
                {
                   
                    var fileName = Path.GetFileName(file.FileName);
                    string outuniquefilename = DateTime.Now.ToString().Replace("/", "").Replace(":", "") + fileName;
                    var path = Path.Combine(Server.MapPath("~/uploads"), outuniquefilename);
                    file.SaveAs(path);

                    //upload the file information to the database and save with the session id
                    SaveDocumentToDB(fileName, outuniquefilename);
                }
            }

            return;
        }

        public void SaveDocumentToDB(string filename, string uniquename)
        {
            

            DashDBContext db = new DashDBContext();

            workflowfiles wff = new workflowfiles
            {
                filename = filename,
                wfsessionid = Session.SessionID.ToString(),
                upload_dtim = DateTime.Now,
                uniquefilename = uniquename
            };

            db.workflowfiles.Add(wff);

            db.SaveChanges();

            return;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "wfNumber, startdate, enddate", Include = "wfTitle, Requestor, Submitter, createddate, clientmatters, descofwork, deadline")] workflows workflow)
        {
            DashDBContext db = new DashDBContext();

            if (ModelState.IsValid)
            {
                workflow.createddate = DateTime.Now;

                db.workflows.Add(workflow);

                try
                {
                    db.SaveChanges();

                    int wfid = workflow.wfNumber;

                    SaveTimeStamp(wfid,1, Request.Form["Submitter"].ToString());

                    SaveFilesToWorkflow(wfid);
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            
            return View();
        }

        public void SaveFilesToWorkflow(int wfid)
        {
            //now that the workflow has been created we need to clean up the files that they uploaded and put the workflow ID on them
            using (var db = new DashDBContext())
            {
                foreach (var wffiles in db.workflowfiles.Where(x => x.wfsessionid.Equals(Session.SessionID.ToString())).ToList())
                {
                    wffiles.wfID = wfid;
                }
                db.SaveChanges();
            }
            return;
        }

        public ActionResult Adminfunctions()
        {
            return View();
        }
        public JsonResult getWorkflowList(string searchPhrase)
        {
            using (var context = new DashDBContext())
            {
                var query = context.Database.SqlQuery<Searchworkflow>("SELECT wfTitle, wfNumber, Requestor, Createddate, deadline FROM workflows WHERE wfTitle like '%" + searchPhrase + "%' OR wfNumber like '%" + searchPhrase + "%' ORDER BY wfTitle").ToList();
                return Json(query, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getAttorneyList(string searchPhrase)
        {
            using (var context = new DWDBContext())
            {
                var query = context.Database.SqlQuery<userlist>("SELECT (tkfirst + ' ' + tklast) as fullName, UserId FROM dw_Timekeeper WHERE JobClass <= 3600 AND Status = 'Active' AND Namer like '%" + searchPhrase + "%' ORDER BY tklast").ToList();
                return Json(query, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getWorkFlowUserList(string searchPhrase)
        {
            using (var context = new DashDBContext())
            {
                var query = context.Database.SqlQuery<userlist>("SELECT TK.[UserId], [Namer] as fullname FROM [HNBACT01].[dw_HB1].[dbo].[dw_Timekeeper] AS TK INNER JOIN [dbo].[workflowusers] ON TK.tkinit = [dbo].[workflowusers].tkinit WHERE Namer like '%" + searchPhrase + "%' ORDER BY Namer").ToList();
                return Json(query, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetCurrOwner(string wfNumber)
        {
            using (var context = new DashDBContext())
            {
                var query = context.Database.SqlQuery<userlist>("SELECT TOP 1 participant as UserId, Namer as fullname FROM [HNBACT01].[dw_HB1].[dbo].[dw_Timekeeper] INNER JOIN [dbo].workflowtimestamps ON UserId = [dbo].workflowtimestamps.participant WHERE wfNumber = " + wfNumber + " AND workflowstatusid = 2 ORDER BY stamp DESC").ToList();
                return Json(query, JsonRequestBehavior.AllowGet);
            }
        }
    
        public void SaveNewOwner(int wfNumber, string CurrOwner, string NewOwner)
        {
            //checkin the previous owner information
            SaveTimeStamp(wfNumber, 3, CurrOwner );

            //checkout to the new owner
            SaveTimeStamp(wfNumber, 2, NewOwner);

            //log that the change was made from an administrator
            LogWFEdits(User.Identity.Name.Substring(User.Identity.Name.LastIndexOf('\\') + 1), wfNumber, "Changed owner from " + CurrOwner + " to " + NewOwner);
        }
        private void SaveTimeStamp(int wfNumber, int wfstatus, string participant)
        {
            DashDBContext db = new DashDBContext();

            workflowtimestamps t = new workflowtimestamps
            {
                wfNumber = wfNumber,
                workflowstatusID = wfstatus,
                participant = participant,
                stamp = DateTime.Now
        };

            db.workflowtimestamps.Add(t);

            db.SaveChanges();

            return;
        }

        public void SaveAttorneyName(string attyID, int wfNumber)
        {
            DashDBContext db = new DashDBContext();

            workflows wf = (from x in db.workflows
                            where x.wfNumber == wfNumber
                            select x).First();
            wf.Requestor = attyID;
            db.SaveChanges();

            LogWFEdits(User.Identity.Name.Substring(User.Identity.Name.LastIndexOf('\\') + 1), wfNumber, "Requesting attorney change");
            return;
        }
        public void SaveClientMatter(string ClientMatter, int wfNumber)
        {
            DashDBContext db = new DashDBContext();

            workflows wf = (from x in db.workflows
                            where x.wfNumber == wfNumber
                            select x).First();
            wf.clientmatters = ClientMatter;
            db.SaveChanges();
            LogWFEdits(User.Identity.Name.Substring(User.Identity.Name.LastIndexOf('\\') + 1), wfNumber, "Client Matter change");
            return;
        }
        public void SaveDescOfWork(string desctext, int wfNumber)
        {
            DashDBContext db = new DashDBContext();

            workflows wf = (from x in db.workflows
                            where x.wfNumber == wfNumber
                            select x).First();
            wf.descofwork = desctext;
            db.SaveChanges();
            LogWFEdits(User.Identity.Name.Substring(User.Identity.Name.LastIndexOf('\\') + 1), wfNumber, "Description of work change");
            return;
        }
        public void SaveDeadlineDate(DateTime deadlineDate, int wfNumber)
        {
            DashDBContext db = new DashDBContext();

            workflows wf = (from x in db.workflows
                            where x.wfNumber == wfNumber
                            select x).First();
            wf.deadline = deadlineDate;
            db.SaveChanges();
            LogWFEdits(User.Identity.Name.Substring(User.Identity.Name.LastIndexOf('\\') + 1), wfNumber, "Deadline date change");
            return;
        }

        private void LogWFEdits(string tkinit, int wfNumber, string changeevent)
        {
            DashDBContext db = new DashDBContext();

            Edit_Log el = new Edit_Log
            {
                wfNumber = wfNumber,
                dtim = DateTime.Now,
                tkinit = tkinit,
                changeevent = changeevent
            };

            db.Edit_Log.Add(el);

            db.SaveChanges();

            return;
        }

        public void ChangeWorkflowStatus(int newstatus, int wfNumber)
        {
            DashDBContext db = new DashDBContext();

            workflowtimestamps wfts = new workflowtimestamps
            {
                wfNumber = wfNumber,
                workflowstatusID = newstatus,
                stamp = DateTime.Now,
                participant = User.Identity.Name.Substring(User.Identity.Name.LastIndexOf('\\') + 1).ToString()
            };

            db.workflowtimestamps.Add(wfts);

            db.SaveChanges();

            return;
        }

        public void AddNewNote(string newnotetext, int wfNumber)
        {
            DashDBContext db = new DashDBContext();

            workflownotes wfnote = new workflownotes
            {
                wfID = wfNumber,
                wfnote = newnotetext,
                author = User.Identity.Name.Substring(User.Identity.Name.LastIndexOf('\\') + 1).ToString(),
                notetimestamp = DateTime.Now
            };

            db.workflownotes.Add(wfnote);

            db.SaveChanges();

            return;
        }

        public JsonResult GetWFTimeStampsJson(int wfNumber)
        {
            using (DashDBContext db = new DashDBContext())
            {
                var workflownts = from wfts in db.workflowtimestamps
                                                         join wfstatus in db.workflowstatus on wfts.workflowstatusID  equals wfstatus.wfsid
                                                         orderby wfts.stamp descending
                                                         select new { timestamp = wfts.stamp, participant = wfts.participant, wfStatus = wfstatus.wfstatus, wfID = wfts.wfNumber, wftsid = wfts.wftsid  } ;
                workflownts = workflownts.Where(w => w.wfID == wfNumber);
                return Json(workflownts.ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        public void SaveWFTSNewTimeStamp(DateTime newTSDate, Guid wfTSID, string reason)
        {
            DashDBContext db = new DashDBContext();

            workflowtimestamps wf = (from x in db.workflowtimestamps
                            where x.wftsid  == wfTSID
                                     select x).First();
            wf.stamp  = newTSDate;
            db.SaveChanges();
            LogWFEdits(User.Identity.Name.Substring(User.Identity.Name.LastIndexOf('\\') + 1), wf.wfNumber, wfTSID + " had the date changed because of " + reason);
            AddNewNote(wfTSID + " had the date changed because of " + reason, wf.wfNumber);
            return;
        }

    }
}