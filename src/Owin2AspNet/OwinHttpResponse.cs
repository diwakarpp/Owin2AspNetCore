using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Http.Features.Internal;
using Microsoft.Net.Http.Headers;
using Owin2AspNet.Helper;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Owin2AspNet
{
    public class OwinHttpResponse : HttpResponse, IFeatureCache
    {
        private readonly OwinHttpContext _context;
        private readonly IFeatureCollection _features;
        private int _cachedFeaturesRevision = -1;

        private IHttpResponseFeature _response;
        private IResponseCookiesFeature _cookies;

        public OwinHttpResponse(OwinHttpContext context, IFeatureCollection features)
        {
            _context = context;
            _features = features;
        }

        void IFeatureCache.CheckFeaturesRevision()
        {
            if (_cachedFeaturesRevision != _features.Revision)
            {
                _response = null;
                _cookies = null;
                _cachedFeaturesRevision = _features.Revision;
            }
        }

        private IHttpResponseFeature HttpResponseFeature
        {
            get { return FeatureHelper.GetAndCache(this, _features, ref _response); }
        }

        private IResponseCookiesFeature ResponseCookiesFeature
        {
            get
            {
                return FeatureHelper.GetOrCreateAndCache(
                    this,
                    _features,
                    (f) => new ResponseCookiesFeature(f),
                    ref _cookies);
            }
        }

        public override HttpContext HttpContext { get { return _context; } }

        public override int StatusCode
        {
            get { return HttpResponseFeature.StatusCode; }
            set { HttpResponseFeature.StatusCode = value; }
        }

        public override IHeaderDictionary Headers
        {
            get { return HttpResponseFeature.Headers; }
        }

        public override Stream Body
        {
            get { return HttpResponseFeature.Body; }
            set { HttpResponseFeature.Body = value; }
        }

        public override long? ContentLength
        {
            get
            {
                return ParsingHelper.GetContentLength(Headers);
            }
            set
            {
                ParsingHelper.SetContentLength(Headers, value);
            }
        }

        public override string ContentType
        {
            get
            {
                return Headers[HeaderNames.ContentType];
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    HttpResponseFeature.Headers.Remove(HeaderNames.ContentType);
                }
                else
                {
                    HttpResponseFeature.Headers[HeaderNames.ContentType] = value;
                }
            }
        }

        public override IResponseCookies Cookies
        {
            get { return ResponseCookiesFeature.Cookies; }
        }

        public override bool HasStarted
        {
            get { return HttpResponseFeature.HasStarted; }
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            HttpResponseFeature.OnStarting(callback, state);
        }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            HttpResponseFeature.OnCompleted(callback, state);
        }

        public override void Redirect(string location, bool permanent)
        {
            if (permanent)
            {
                HttpResponseFeature.StatusCode = 301;
            }
            else
            {
                HttpResponseFeature.StatusCode = 302;
            }

            Headers[HeaderNames.Location] = location;
        }
    }
}