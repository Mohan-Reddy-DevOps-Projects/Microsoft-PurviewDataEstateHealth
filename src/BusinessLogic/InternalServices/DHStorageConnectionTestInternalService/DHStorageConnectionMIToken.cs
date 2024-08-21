// <copyright file="DHStorageConnectionMIToken.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using global::Azure.Core;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class DHStorageConnectionMIToken : TokenCredential
    {
        private string token;

        public DHStorageConnectionMIToken(string token)
        {
            this.token = token;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new AccessToken(this.token, DateTimeOffset.Now.Add(TimeSpan.FromDays(1)));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(this.GetToken(requestContext, cancellationToken));
        }
    }
}
