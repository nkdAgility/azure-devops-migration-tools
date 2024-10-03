using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MigrationTools.Tools
{
    internal class TfsGitRepositoryToolOptionsValidator : IValidateOptions<TfsGitRepositoryToolOptions>
    {
        public ValidateOptionsResult Validate(string name, TfsGitRepositoryToolOptions options)
        {
            if (options.Mappings == null)
            {
                return ValidateOptionsResult.Fail("Mappings must be set to at least an empty array");
            }
            return ValidateOptionsResult.Success;
        }
    }
}
