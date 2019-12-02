using System.Linq;

namespace Lifecycle.Supply
{
    class Program
    {
        static void Main(string[] args)
        {
            var argsWithCommand = new[] {"supply"}.Union(args).ToArray();
            new SpaBuildpack.SpaBuildpack().Run(argsWithCommand);
        }
    }
}