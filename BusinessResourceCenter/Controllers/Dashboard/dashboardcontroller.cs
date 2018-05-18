using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BusinessResourceCenter.Models;


namespace BusinessResourceCenter.Controllers
{
    public class dashboardcontroller : Controller
    {
        private DashDBContext db = new DashDBContext();
        // GET: dashboardcontroller
        public ActionResult Index()
        {
            DashDBContext db = new DashDBContext();

            var workflow = db.Database.SqlQuery<Searchworkflow>("SELECT workflows.wfnumber, workflows.wftitle, workflows.Requestor, workflows.createddate, workflows.deadline, Count(*) count FROM workflows " +
                                                            " INNER JOIN workflowtimestamps ON workflows.wfNumber = workflowtimestamps.wfnumber " +
                                                            " GROUP BY workflows.wfnumber, workflows.wftitle, workflows.Requestor, workflows.createddate, workflows.deadline " +
                                                            " HAVING COUNT(*) = 1");


            var AvgCountToClose = db.Database.SqlQuery<int>("SELECT AVG(DATEDIFF(hour, createddate, stamp)) AS avgclosetime " +
                                                                    " FROM Workflows " +
                                                                    " INNER JOIN workflowtimestamps ON workflows.wfNumber = workflowtimestamps.wfnumber " +
                                                                    " WHERE workflowtimestamps.workflowstatusID = 4");


            var workingworkflow = db.Database.SqlQuery<ActiveWorkflows>(";with cteRowNumber as (SELECT wfNumber, stamp, workflowtimestamps.workflowstatusID, participant, row_number() over(partition by wfNumber order by stamp desc) as RowNum " +
                                                                        " FROM workflowtimestamps " +
                                                                        " ) select cteRowNumber.wfNumber, wfTitle, Requestor, createddate, deadline, participant " +
                                                                        " FROM cteRowNumber INNER JOIN Workflows ON cteRowNumber.wfNumber = workflows.wfNumber " +
                                                                        " WHERE RowNum = 1 AND workflowstatusID = '2'");

            var model = new AllDashInfo { ViewAllOpenWorkflows = workflow.ToList(), avgclosetime = AvgCountToClose.ToList().FirstOrDefault(), workingworkflows = workingworkflow.ToList() };

            return View(model);
 
        }

        public JsonResult GetBusyTime()
        {
            using (var context = new DashDBContext())
            {
                var query = context.Database.SqlQuery<busytime>("SELECT COUNT(createddate) AS startHourCount, startHour FROM ( SELECT datePart(hour, createddate) AS startHour, createddate FROM Workflows ) A GROUP BY startHour").ToList();
                return Json(query, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult getOpenWorkflows()
        {
            using (var context = new DashDBContext())
            {
                var query = context.Database.SqlQuery<Searchworkflow>("SELECT workflows.wfnumber, workflows.wftitle, workflows.Requestor, workflows.createddate, workflows.deadline, Count(*) count FROM workflows " +
                                                            " INNER JOIN workflowtimestamps ON workflows.wfNumber = workflowtimestamps.wfnumber " +
                                                            " GROUP BY workflows.wfnumber, workflows.wftitle, workflows.Requestor, workflows.createddate, workflows.deadline " +
                                                            " HAVING COUNT(*) = 1").ToList();
                return Json(query, JsonRequestBehavior.AllowGet);
            }
        }
    }
}