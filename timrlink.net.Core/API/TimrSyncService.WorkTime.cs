using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace timrlink.net.Core.API
{
    public partial class WorkTime
    {
        public WorkTime Clone()
        {
            return (WorkTime) MemberwiseClone();
        }

        public override string ToString()
        {
            return $"WorkTime({nameof(uuid)}: '{uuid}', {nameof(startTime)}: {startTime}, {nameof(endTime)}: {endTime}, {nameof(description)}: '{description}', {nameof(externalUserId)}: '{externalUserId}', {nameof(userUuid)}: '{userUuid}', {nameof(externalWorkItemId)}: {externalWorkItemId}, {nameof(workItemUuid)}: {workItemUuid}, {nameof(status)}: {status})";
        }
    }
}
