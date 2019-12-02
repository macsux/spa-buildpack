using System;
using System.Linq;

namespace Lifecycle.Supply
{
    class Program
    {
        static void Main(string[] args)
        {
            var argsWithCommand = new[] {"finalize"}.Union(args).ToArray();
            new SpaBuildpack.SpaBuildpack().Run(argsWithCommand);
        }
    }
}