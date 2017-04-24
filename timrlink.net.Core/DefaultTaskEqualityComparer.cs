using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
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
            if (x.ExternalId != y.ExternalId)
            {
                logger?.LogDebug($"ExternalId: {x.ExternalId} != {y.ExternalId}");
                return false;
            }

            if (x.Name != y.Name)
            {
                logger?.LogDebug($"Name: {x.Name} != {y.Name}");
                return false;
            }

            if (x.Description != y.Description)
            {
                logger?.LogDebug($"Description: {x.Description} != {y.Description}");
                return false;
            }

            if (x.ParentExternalId != y.ParentExternalId)
            {
                logger?.LogDebug($"ParentExternalId: {x.ParentExternalId} != {y.ParentExternalId}");
                return false;
            }

            if (x.CustomField1 != y.CustomField1)
            {
                logger?.LogDebug($"CustomField1: {x.CustomField1} != {y.CustomField1}");
                return false;
            }

            if (x.CustomField2 != y.CustomField2)
            {
                logger?.LogDebug($"CustomField2: {x.CustomField2} != {y.CustomField2}");
                return false;
            }

            if (x.CustomField3 != y.CustomField3)
            {
                logger?.LogDebug($"CustomField3: {x.CustomField3} != {y.CustomField3}");
                return false;
            }

            if (x.Start?.Year != y.Start?.Year
                || x.Start?.Month != y.Start?.Month
                || x.Start?.Day != y.Start?.Day)
            {
                logger?.LogDebug($"Start: {x.Start} != {y.Start}");
                return false;
            }

            if (x.End?.Year != y.End?.Year
                || x.End?.Month != y.End?.Month
                || x.End?.Day != y.End?.Day)
            {
                logger?.LogDebug($"End: {x.End} != {y.End}");
                return false;
            }

            if (x.Billable != y.Billable)
            {
                logger?.LogDebug($"Billable: {x.Billable} != {y.Billable}");
                return false;
            }

            if (x.Bookable != y.Bookable)
            {
                logger?.LogDebug($"Bookable: {x.Bookable} != {y.Bookable}");
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