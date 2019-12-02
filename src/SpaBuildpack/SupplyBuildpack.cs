namespace SpaBuildpack
{
    public abstract class SupplyBuildpack : BuildpackBase
    {
        protected override int DoRun(string[] args)
        {
            var command = args[0];
            switch (command)
            {
                case "supply":
                    DoApply(args[1], args[2], args[3], int.Parse(args[4]));
                    break;
                default:
                    return base.DoRun(args);
            }

            return 0;
        }

        // supply buildpacks may get this lifecycle event, but since only one buildpack will be selected if detection is used, it must be final
        // therefore supply buildpacks always must reply with false
        protected sealed override bool Detect(string buildPath) => false;  
    }
}