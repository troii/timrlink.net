using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using timrlink.net.Core.API;

namespace timrlink.net.Core
{
    class DefaultTaskEqualityComparer : IEqualityComparer<Task>
    {
        private readonly ILogger<DefaultTaskEqualityComparer> logger;

        public DefaultTaskEqualityComparer()
        {
        }

        public DefaultTaskEqualityComparer(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DefaultTaskEqualityComparer>();
        }

        public bool Equals(Task x, Task y)
        {
            if (x.externalId != y.externalId)
            {
                logger?.LogDebug($"ExternalId: {x.externalId} != {y.externalId}");
                return false;
            }

            if (x.name != y.name)
            {
                logger?.LogDebug($"Name: {x.name} != {y.name}");
                return false;
            }

            if (x.description != y.description)
            {
                logger?.LogDebug($"Description: {x.description} != {y.description}");
                return false;
            }

            if (x.parentExternalId != y.parentExternalId)
            {
                logger?.LogDebug($"ParentExternalId: {x.parentExternalId} != {y.parentExternalId}");
                return false;
            }

            if (x.customField1 != y.customField1)
            {
                logger?.LogDebug($"CustomField1: {x.customField1} != {y.customField1}");
                return false;
            }

            if (x.customField2 != y.customField2)
            {
                logger?.LogDebug($"CustomField2: {x.customField2} != {y.customField2}");
                return false;
            }

            if (x.customField3 != y.customField3)
            {
                logger?.LogDebug($"CustomField3: {x.customField3} != {y.customField3}");
                return false;
            }

            if (x.start?.Year != y.start?.Year
                || x.start?.Month != y.start?.Month
                || x.start?.Day != y.start?.Day)
            {
                logger?.LogDebug($"Start: {x.start} != {y.start}");
                return false;
            }

            if (x.end?.Year != y.end?.Year
                || x.end?.Month != y.end?.Month
                || x.end?.Day != y.end?.Day)
            {
                logger?.LogDebug($"End: {x.end} != {y.end}");
                return false;
            }

            if (x.billable != y.billable)
            {
                logger?.LogDebug($"Billable: {x.billable} != {y.billable}");
                return false;
            }

            if (x.bookable != y.bookable)
            {
                logger?.LogDebug($"Bookable: {x.bookable} != {y.bookable}");
                return false;
            }

            if (x.customField1 != y.customField1)
            {
                logger?.LogDebug($"CustomField1: {x.end} != {y.end}");
                return false;
            }

            if (x.customField2 != y.customField2)
            {
                logger?.LogDebug($"CustomField1: {x.end} != {y.end}");
                return false;
            }

            if (x.customField3 != y.customField3)
            {
                logger?.LogDebug($"CustomField1: {x.end} != {y.end}");
                return false;
            }

            /*
            if (x.uuid != y.uuid)
            {
                logger?.LogDebug($"UUID: {x.end} != {y.end}");
                return false;
            }
            */

            if (x.budgetPlanningType != y.budgetPlanningType)
            {
                logger?.LogDebug($"BudgetPlanningType: {x.end} != {y.end}");
                return false;
            }

            if (x.budgetPlanningTypeInherited != y.budgetPlanningTypeInherited)
            {
                logger?.LogDebug($"BudgetPlanningTypeInherited: {x.end} != {y.end}");
                return false;
            }

            if (x.hoursPlanned != y.hoursPlanned)
            {
                logger?.LogDebug($"HoursPlanned: {x.end} != {y.end}");
                return false;
            }

            if (x.hourlyRate != y.hourlyRate)
            {
                logger?.LogDebug($"HourlyRate: {x.end} != {y.end}");
                return false;
            }

            if (x.budgetPlanned != y.budgetPlanned)
            {
                logger?.LogDebug($"HourlyRate: {x.end} != {y.end}");
                return false;
            }

            return true;
        }

        public int GetHashCode(Task obj)
        {
            return obj.GetHashCode();
        }
    }
}
