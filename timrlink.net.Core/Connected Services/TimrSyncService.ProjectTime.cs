
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
            return $"ProjectTime({nameof(externalUserId)}: {externalUserId}, {nameof(externalTaskId)}: {externalTaskId}, {nameof(startTime)}: {startTime}, {nameof(endTime)}: {endTime})";
        }
    }
}
