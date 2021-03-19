// MS official docs and announcements say to use delegation key created to generate SAS tokens. 
// Bur we do not use Azure AD and stuff does not work. So old approach should be used by using default key to generate SAS. 
// But parsing key out of cns is internal and there is no API on container client to generate sas. So I have copied internal parser for now.

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.Storage
{
    // TODO: Consider making public if there's ever a reason for developers to use this type
    internal sealed class SharedAccessSignatureCredentials
    {
        /// <summary>
        /// Gets the SAS token used to authenticate requests to the Storage
        /// service.
        /// </summary>
        public string SasToken { get; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="SharedAccessSignatureCredentials"/> class.
        /// </summary>
        /// <param name="sasToken">
        /// The SAS token used to authenticate requests to the Storage service.
        /// </param>
        public SharedAccessSignatureCredentials(string sasToken) =>
            SasToken = sasToken;
    }
}
