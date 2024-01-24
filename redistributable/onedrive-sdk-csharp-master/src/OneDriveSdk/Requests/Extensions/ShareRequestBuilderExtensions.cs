// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    public partial class ShareRequestBuilder
    {
        /// <summary>
        /// Gets root item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        public IItemRequestBuilder Root
        {
            get { return new ItemRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.Root), this.Client); }
        }
    }
}