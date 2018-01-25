using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BusinessResourceCenter.Models
{
    public class dashboard
    {
        [Key, Column(Order = 0)]
        public string WFID { get; set; }
        public int wfNumber { get; set; }
        public string wfTitle { get; set; }
        public string CurrentOwnerFirstName { get; set; }
        public string CurrentOwnerLastName { get; set; }
        public string RequestorOwnerFirstName { get; set; }
        public string RequestorOwnerLastName { get; set; }
        public DateTime createddate { get; set; }
        public DateTime deadline { get; set; }
        public string wfstatus { get; set; }

    }

    public class workflows
    {
        [Key, Column(Order = 0)]
        public int wfNumber { get; set; }
        public string wfTitle { get; set; }
        public string Requestor { get; set; }
        public string Submitter { get; set; }
        public DateTime? createddate { get; set; }
        public DateTime? deadline { get; set; }
        public DateTime? startdate { get; set; }
        public string clientmatters { get; set; }
        public string descofwork { get; set; }
    }

    public class Searchworkflow
    {
        [Key, Column(Order = 0)]
        public int wfNumber { get; set; }
        public string wfTitle { get; set; }
        public string Requestor { get; set; }
        public DateTime createddate{ get; set; }
        public DateTime deadline { get; set; }
    }

    public class vWorkflows
    {
        [Key, Column(Order = 0)]
        public int wfNumber { get; set; }
        public string wfTitle { get; set; }
        public DateTime createddate { get; set; }
        public DateTime? deadline { get; set; }
        public DateTime? startdate { get; set; }
        public string clientmatters { get; set; }
        public string SubmitterName { get; set; }
        public string RequestorName { get; set; }
        public string Requestor { get; set; }
        public string wfstatus { get; set; }
        public int workflowstatusID { get; set; }
        public DateTime stamp { get; set; }
        public string Submitter { get; set; }
        public string descofwork { get; set; }

    }

    public class vAllWFLastStatusLists
    {
        [Key, Column(Order = 0)]
        public int wfNumber { get; set; }
        public string wfTitle { get; set; }
        public string Submitter { get; set; }
        public string Requestor { get; set; }
        public DateTime createddate { get; set; }
        public DateTime? deadline { get; set; }
        public DateTime? startdate { get; set; }
        public string clientmatters { get; set; }
        public string descofwork { get; set; }
        public string wfstatus { get; set; }
        public DateTime stamp { get; set; }
        public int workflowstatusID { get; set; }
        public string participant { get; set; }
    }

    public class workflowtimestamps
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? wftsid { get; set; }
        public int wfNumber { get; set; }
        public int workflowstatusID { get; set; }
        public string participant { get; set; }
        public DateTime stamp { get; set; }
    }

    public class workflowtimestampshistory
    {
        [Key, Column(Order = 0)]
        public int wfNumber { get; set; }
        public string participant { get; set; }
        public DateTime stamp { get; set; }
        public string wfstatus { get; set; }

    }

    public class workflowstatus
    {
        [Key, Column(Order = 0)]
        public int wfsid { get; set; }
        public string wfstatus { get; set; }
        public string descriptor { get; set; }
    }

    public class Edit_Log
    {
        [Key, Column(Order = 0)]
        public int? wfEditLogID { get; set; }
        public DateTime dtim { get; set; }
        public string tkinit { get; set; }
        public int wfNumber { get; set; }
        public string changeevent { get; set; }
    }

    public class workflownotes
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? wfnoteid { get; set; }
        public int wfID { get; set; }
        public string wfnote { get; set; }
        public string author { get; set; }
        public DateTime notetimestamp { get; set; }
    }

    public class AllWFInfo
    {
        public vAllWFLastStatusLists ViewWorkFlowData { get; set; }
        public List<workflownotes> wfnotes { get; set; }
        public List<workflowtimestampshistory> workflowHistory { get; set; }
    }


    public class DashDBContext : DbContext
    {
        public DbSet<dashboard> dashboard { get; set; }
        public DbSet<workflows> workflows { get; set; }
        public DbSet<Searchworkflow> Searchworkflow { get; set; }
        public DbSet<vWorkflows> vWorkflows { get; set; }
        public DbSet<vAllWFLastStatusLists> vAllWFLastStatusLists { get; set; }
        public DbSet<workflowtimestamps> workflowtimestamps { get; set; }
        public DbSet<Edit_Log> Edit_Log { get; set; }
        public DbSet<workflownotes> workflownotes { get; set; }
        public DbSet<workflowstatus> workflowstatus { get; set; }

        public DbSet<busytime> busytime { get; set; }
    }


    public class userlist
    {
        [Key, Column(Order = 0)]
        public string fullname { get; set; }
        public string UserId { get; set; }
    }
    public class DWDBContext : DbContext
    {
        public DbSet<userlist> userlist { get; set; }
    }
    
    public class ClientMatter
    {
        [Key, Column(Order = 0)]
        public System.Guid matterID { get; set; }
        public string MatterNumber { get; set; }
        public string MatterName { get; set; }
        public string MatterClientNumber { get; set; }
        public string MatterStatusCode { get; set; }
    }
    public class ThreeEDBContext : DbContext
    {
        public DbSet<ClientMatter> ClientMatter { get; set; }
    }

    //these are the classes necessary for all the dashboard display page

    public class ActiveWorkflows
    {
        [Key, Column(Order = 0)]
        public int wfNumber { get; set; }
        public string wfTitle { get; set; }
        public string Requestor { get; set; }
        public DateTime createddate { get; set; }
        public DateTime deadline { get; set; }
        public string participant { get; set; }
    }

    public class busytime
    {
        [Key, Column(Order = 0)]
        public int starthour { get; set; }
        public int starthourcount { get; set; }
    }
    public class avgclose
    {
        public int avgclosetime { get; set; }
    }
    public class AllDashInfo
    {
        public List<Searchworkflow> ViewAllOpenWorkflows { get; set; }
        //public List<avgclose> avgclosetime { get; set; }
        public int avgclosetime { get; set; }
        public List<ActiveWorkflows> workingworkflows { get; set; }
    }

}