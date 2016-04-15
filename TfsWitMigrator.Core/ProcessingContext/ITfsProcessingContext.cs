using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsWitMigrator.Core
{
   public enum ProcessingStatus { Running, Failed, Complete, None };

    public interface ITfsProcessingContext
    {
       

        string Name { get; }
        ProcessingStatus Status { get; }
        void Execute();
    }


}
