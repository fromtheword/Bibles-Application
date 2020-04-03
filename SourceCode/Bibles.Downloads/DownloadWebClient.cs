using System;
using System.Net;

namespace Bibles.Downloads
{
    internal class DownloadWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest result = base.GetWebRequest(address);

            result.Timeout = 60000 * 5; // 5 Minutes

            return result;
        }
    }
}
