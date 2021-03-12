
namespace timrlink.net.Core.API
{
    public partial class ProjectTime
    {
        public ProjectTime Clone()
        {
            return (ProjectTime) MemberwiseClone();
        }

        public override string ToString()
        {
            return $"ProjectTime({nameof(externalTaskIdField)}: {externalTaskIdField}, {nameof(startTime)}: {startTime}, {nameof(endTime)}: {endTime})";
        }
    }
}