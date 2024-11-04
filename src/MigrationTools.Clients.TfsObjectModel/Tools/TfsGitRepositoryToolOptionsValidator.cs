using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Serilog;

namespace MigrationTools.Tools
{
    internal class TfsGitRepositoryToolOptionsValidator : IValidateOptions<TfsGitRepositoryToolOptions>
    {
        public ValidateOptionsResult Validate(string name, TfsGitRepositoryToolOptions options)
        {
            if (options.Mappings == null)
            {
                Log.Debug("TfsGitRepositoryToolOptionsValidator::Validate::Fail");
                return ValidateOptionsResult.Fail("Mappings must be set to at least an empty array");
            }
            Log.Debug("TfsGitRepositoryToolOptionsValidator::Validate::Success");
            return ValidateOptionsResult.Success;
        }
    }
}
