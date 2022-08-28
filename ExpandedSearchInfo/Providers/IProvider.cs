using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ExpandedSearchInfo.Configs;
using ExpandedSearchInfo.Sections;

namespace ExpandedSearchInfo.Providers {
    public interface IProvider {
        string Name { get; }

        string Description { get; }

        BaseConfig Config { get; }

        /// <summary>
        ///     If this provider is capable of parsing the search info for custom Uris, this should be true.
        ///
        ///     Note that normal Uris are parsed by the plugin itself, so this can remain false for providers
        ///     that only handle normal Uris.
        /// </summary>
        bool ExtractsUris { get; }

        void DrawConfig();

        /// <summary>
        ///     Determine if this provider should run on the given Uri.
        /// </summary>
        /// <param name="uri">Uri to test</param>
        /// <returns>true if this provider's Extract method should be run for the HTTP response from this Uri</returns>
        bool Matches(Uri uri);

        /// <summary>
        ///     For providers that require Uris, this can return null.
        ///     For providers that don't require Uris, this must return a Uri extracted from the given search info.
        /// </summary>
        /// <param name="objectId">The actor ID associated with the search info</param>
        /// <param name="info">A character's full search info</param>
        /// <returns>null for providers that require Uris, a Uri for providers that don't</returns>
        IEnumerable<Uri>? ExtractUris(uint objectId, string info);

        /// <summary>
        ///     Extract the search info to be displayed given the HTTP response from a Uri.
        /// </summary>
        /// <param name="response">HTTP response from a Uri</param>
        /// <returns>null if search info could not be extracted or the search info as a string if it could</returns>
        Task<ISearchInfoSection?> ExtractInfo(HttpResponseMessage response);

        /// <summary>
        ///     Modify any requests made for this provider before they are sent.
        /// </summary>
        /// <param name="request">HTTP request about to be sent</param>
        void ModifyRequest(HttpRequestMessage request) {
        }
    }
}
