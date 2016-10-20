using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTS.DataBulkEditor.Engine.ComponentContext
{
   public interface IFieldMap
    {
        string Name { get; }

        void Execute(WorkItem source, WorkItem target);
    }
}
