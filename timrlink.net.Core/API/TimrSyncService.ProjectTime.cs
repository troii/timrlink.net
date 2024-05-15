
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
            return $"ProjectTime({nameof(uuid)}: '{uuid}', {nameof(startTime)}: {startTime}, {nameof(endTime)}: {endTime}, {nameof(description)}: '{description}', {nameof(externalUserId)}: {externalUserId}, {nameof(userUuid)}: '{userUuid}', {nameof(externalTaskId)}: {externalTaskId}, {nameof(taskUuid)}: {taskUuid}, {nameof(status)}: {status})";
        }
    }
}
