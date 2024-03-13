// <copyright file="ObligationHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper
{
    using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ObligationHelper
    {
        public static bool IsAccessAllowed(ObligationDictionary obligationsV2, string obligationContainerType, string requiredPermission, string domainId)
        {
            return IsAccessAllowed(obligationsV2, new List<string>() { obligationContainerType }, requiredPermission, domainId);
        }

        public static bool IsAccessAllowed(ObligationDictionary obligationsV2, List<string> obligationContainerTypes, string requiredPermission, string domainId)
        {
            // todo(han): need to consider callerCategory
            if (obligationsV2 == null)
            {
                return false;
            }
            return obligationContainerTypes.Any(obligationContainerType =>
            {
                var obligation = obligationsV2.GetValueOrDefault(obligationContainerType, null)?.GetValueOrDefault(requiredPermission, null);
                return InteralIsAccessAllowed(obligation, domainId);
            });
        }

        public static List<Obligation> GetObligations(ObligationDictionary obligationsV2, string obligationContainerType, string permission)
        {
            return GetObligations(obligationsV2, new List<string>() { obligationContainerType }, permission);
        }

        public static List<Obligation> GetObligations(ObligationDictionary obligationsV2, List<string> obligationContainerTypes, string permission)
        {
            // todo(han): need to consider callerCategory
            var obligations = obligationContainerTypes.Select(obligationContainerType =>
            {
                var obligation = obligationsV2?.GetValueOrDefault(obligationContainerType, null)?.GetValueOrDefault(permission, null);
                return obligation;
            }).Where((obligation) => obligation != null).ToList();
            if (obligations.Count == 0)
            {
                obligations.Add(new Obligation()
                {
                    Type = ObligationType.Permit,
                });
            }
            return obligations;
        }

        private static bool InteralIsAccessAllowed(Obligation obligation, string domainId)
        {
            if (domainId == null)
            {
                throw new ArgumentNullException(nameof(domainId));
            }

            if (obligation == null)
            {
                return false;
            }

            if (string.Equals(obligation.Type, ObligationType.NotApplicable, StringComparison.OrdinalIgnoreCase))
            {
                if (obligation.BusinessDomains == null || obligation.BusinessDomains.Count <= 0)
                {
                    // For this case, all items are allowed
                    return true;
                }
                else if (obligation.BusinessDomains.Any((item) => string.Equals(item, domainId, StringComparison.OrdinalIgnoreCase)))
                {
                    // find any matched item from disallowed list
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (string.Equals(obligation.Type, ObligationType.Permit, StringComparison.OrdinalIgnoreCase))
            {
                if (obligation.BusinessDomains == null || obligation.BusinessDomains.Count <= 0)
                {
                    // For this case, all items are not allowed
                    return false;
                }
                else if (obligation.BusinessDomains.Any((item) => string.Equals(item, domainId, StringComparison.OrdinalIgnoreCase)))
                {
                    // find any matched item from allowed list
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}