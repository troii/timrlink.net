
namespace timrlink.net.Core.API
{
    public partial class ProjectTime
    {
        public ProjectTime Clone()
        {
            return (ProjectTime) MemberwiseClone();
        }
    }
}