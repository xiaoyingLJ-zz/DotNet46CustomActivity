
using Microsoft.Azure.Management.DataFactories.Runtime;
using System.Collections.Generic;

namespace DotNet46CustomActivity
{
    interface ICrossAppDomainDotNetActivity<TExecutionContext>
    {
        IDictionary<string, string> Execute(TExecutionContext context, IActivityLogger logger);
    }
}
