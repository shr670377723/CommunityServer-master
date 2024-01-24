// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    public partial interface IThumbnailSetRequestBuilder
    {
        IThumbnailRequestBuilder this[string size] { get; }
    }
}