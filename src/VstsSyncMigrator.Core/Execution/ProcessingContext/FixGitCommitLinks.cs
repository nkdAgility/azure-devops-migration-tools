using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.TeamFoundation.Git.Client;
using Microsoft.TeamFoundation;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class FixGitCommitLinks : ProcessingContextBase
    {


        public FixGitCommitLinks(MigrationEngine me, ITfsProcessingConfig config ) : base(me, config)
        {

        }

        public override string Name
        {
            get
            {
                return "FixGitCommitLinks";
            }
        }

        internal override void InternalExecute()
        {
            try
            {

           
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            var sourceGitRepoService = me.Source.Collection.GetService<GitRepositoryService>();
            var sourceGitRepos = sourceGitRepoService.QueryRepositories(me.Source.Name);
            //////////////////////////////////////////////////
            var targetGitRepoService = me.Target.Collection.GetService<GitRepositoryService>();
            var targetGitRepos = targetGitRepoService.QueryRepositories(me.Target.Name);

            WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            TfsQueryContext tfsqc = new TfsQueryContext(targetStore);
            tfsqc.AddParameter("TeamProject", me.Target.Name);
            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject AND  [System.ExternalLinkCount] > 0 ", me.Target.Name);
            WorkItemCollection  workitems = tfsqc.Execute();
            Trace.WriteLine(string.Format("Update {0} work items?", workitems.Count));
            //////////////////////////////////////////////////
            int current = workitems.Count;
            int count = 0;
            long elapsedms = 0;
            int noteFound = 0;
            foreach (WorkItem workitem in workitems)
            {
                Stopwatch witstopwatch = new Stopwatch();
                witstopwatch.Start();
                workitem.Open();
                List<ExternalLink> newEL = new List<ExternalLink>();
                List<ExternalLink> removeEL = new List<ExternalLink>();
                Trace.WriteLine(string.Format("WI: {0}?", workitem.Id));

                foreach (Link l in workitem.Links)
                {
                    if ( l is ExternalLink && l.ArtifactLinkType.Name == "Fixed in Commit")
                    {
                        ExternalLink el = (ExternalLink)l;
                        //vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a%2f50477259-3058-4dff-ba4c-e8c179ec5327%2f41dd2754058348d72a6417c0615c2543b9b55535
                        string guidbits = el.LinkedArtifactUri.Substring(el.LinkedArtifactUri.LastIndexOf('/')+1);
                        string[] bits = Regex.Split( guidbits, "%2f", RegexOptions.IgnoreCase);
                        if (bits.Count() != 3)
                        {
                            throw new Exception("Regex to split bits in url is not working too great");
                        }
                        string oldGitRepoId = bits[1];
                        string oldCommitId = bits[2];
                        var oldGitRepo = (from g in sourceGitRepos where g.Id.ToString() == oldGitRepoId select g).SingleOrDefault();

                        if(oldGitRepo!=null && oldGitRepo.ProjectReference.Name != me.Target.Name)
                        {

                            var newGitRepo = (from g in targetGitRepos where g.Name == oldGitRepo.Name && g.ProjectReference.Name != oldGitRepo.ProjectReference.Name select g).SingleOrDefault();

                            if (newGitRepo != null)
                            {
                                /// we have FOUND
                                Trace.WriteLine(string.Format("Found {0} to {1}?", oldGitRepo.RemoteUrl, newGitRepo.RemoteUrl));
                                string link = string.Format("vstfs:///git/commit/{0}%2f{1}%2f{2}", newGitRepo.ProjectReference.Id.ToString(), newGitRepo.Id.ToString(), oldCommitId);
                                var elinks = from Link lq in workitem.Links where lq.ArtifactLinkType.Name == "Fixed in Commit" select (ExternalLink)lq;
                                var found = (from Link lq in elinks where (((ExternalLink)lq).LinkedArtifactUri.ToLower() == link.ToLower()) select lq).SingleOrDefault();
                                if (found == null)
                                {
                                    newEL.Add(new ExternalLink(targetStore.Store.RegisteredLinkTypes[ArtifactLinkIds.Commit], link));
                                }
                                removeEL.Add(el);
                            }
                            else
                            {
                                Trace.WriteLine(string.Format("FAIL {0} to ???", oldGitRepo.RemoteUrl));
                            }
                        } else
                        {
                            Trace.WriteLine(string.Format("FAIL {0} to ???", oldGitRepoId));
                            noteFound++;
                        }
                    }
                }
                /// add and remove
                foreach (ExternalLink eln in newEL)
                {
                    
   

                        try
                        {
                            Trace.WriteLine(string.Format("Addming ", eln.LinkedArtifactUri));
                            workitem.Links.Add(eln);
                           
                        }
                        catch (Exception)
                        {

                           // eat exception as somtimes TFS thinks this is an attachemnt
                        }
     
                }
                foreach (ExternalLink elr in removeEL)
                {
                    if (workitem.Links.Contains(elr))
                    {
                        try
                        {
                            Trace.WriteLine(string.Format("Removing ", elr.LinkedArtifactUri));
                            workitem.Links.Remove(elr);
                        }
                        catch (Exception)
                        {

                            // eat exception as somtimes TFS thinks this is an attachemnt
                        }

                    }
                   
                }
                if (workitem.IsDirty)
                {
                    Trace.WriteLine(string.Format("Saving "));
                    workitem.Save();
                }

                    witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Trace.WriteLine(string.Format("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)));

                }
            Trace.WriteLine(string.Format("Did not find old repo for {0} links?", noteFound));
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

    }
}