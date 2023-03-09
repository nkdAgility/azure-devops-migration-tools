using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.FieldMaps
{
    public class NodePathNotAnchoredException: Exception
    {
        public NodePathNotAnchoredException() { }

       public NodePathNotAnchoredException(string message) : base(message)
        {

        }

        public NodePathNotAnchoredException(string message, Exception innerException) : base(message, innerException) { }
    }
}
