﻿using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Http.Features.Internal;
using Microsoft.Net.Http.Headers;
using Owin2AspNet.Helper;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Owin2AspNet
{
    public class OwinHttpRequest : HttpRequest, IFeatureCache
    {
        private readonly OwinHttpContext _context;
        private readonly IFeatureCollection _features;
        private int _cachedFeaturesRevision = -1;

        private IHttpRequestFeature _request;
        private IQueryFeature _query;
        private IFormFeature _form;
        private IRequestCookiesFeature _cookies;

        public OwinHttpRequest(OwinHttpContext context, IFeatureCollection features)
        {
            _context = context;
            _features = features;
        }

        void IFeatureCache.CheckFeaturesRevision()
        {
            if (_cachedFeaturesRevision != _features.Revision)
            {
                _request = null;
                _query = null;
                _form = null;
                _cookies = null;
                _cachedFeaturesRevision = _features.Revision;
            }
        }

        private IHttpRequestFeature HttpRequestFeature
        {
            get { return FeatureHelper.GetAndCache(this, _features, ref _request); }
        }

        private IQueryFeature QueryFeature
        {
            get
            {
                return FeatureHelper.GetOrCreateAndCache(
                    this,
                    _features,
                    (f) => new QueryFeature(f),
                    ref _query);
            }
        }

        private IFormFeature FormFeature
        {
            get
            {
                return FeatureHelper.GetOrCreateAndCache(
                    this,
                    _features,
                    this,
                    (r) => new FormFeature(r),
                    ref _form);
            }
        }

        private IRequestCookiesFeature RequestCookiesFeature
        {
            get
            {
                return FeatureHelper.GetOrCreateAndCache(
                    this,
                    _features,
                    (f) => new RequestCookiesFeature(f),
                    ref _cookies);
            }
        }

        public override HttpContext HttpContext { get { return _context; } }

        public override PathString PathBase
        {
            get { return new PathString(HttpRequestFeature.PathBase); }
            set { HttpRequestFeature.PathBase = value.Value; }
        }

        public override PathString Path
        {
            get { return new PathString(HttpRequestFeature.Path); }
            set { HttpRequestFeature.Path = value.Value; }
        }

        public override QueryString QueryString
        {
            get { return new QueryString(HttpRequestFeature.QueryString); }
            set { HttpRequestFeature.QueryString = value.Value; }
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

        public override Stream Body
        {
            get { return HttpRequestFeature.Body; }
            set { HttpRequestFeature.Body = value; }
        }

        public override string Method
        {
            get { return HttpRequestFeature.Method; }
            set { HttpRequestFeature.Method = value; }
        }

        public override string Scheme
        {
            get { return HttpRequestFeature.Scheme; }
            set { HttpRequestFeature.Scheme = value; }
        }

        public override bool IsHttps
        {
            get { return string.Equals("https", Scheme, StringComparison.OrdinalIgnoreCase); }
            set { Scheme = value ? "https" : "http"; }
        }

        public override HostString Host
        {
            get { return HostString.FromUriComponent(Headers["Host"]); }
            set { Headers["Host"] = value.ToUriComponent(); }
        }

        public override IReadableStringCollection Query
        {
            get { return QueryFeature.Query; }
            set { QueryFeature.Query = value; }
        }

        public override string Protocol
        {
            get { return HttpRequestFeature.Protocol; }
            set { HttpRequestFeature.Protocol = value; }
        }

        public override IHeaderDictionary Headers
        {
            get { return HttpRequestFeature.Headers; }
        }

        public override IReadableStringCollection Cookies
        {
            get { return RequestCookiesFeature.Cookies; }
            set { RequestCookiesFeature.Cookies = value; }
        }

        public override string ContentType
        {
            get { return Headers[HeaderNames.ContentType]; }
            set { Headers[HeaderNames.ContentType] = value; }
        }

        public override bool HasFormContentType
        {
            get { return FormFeature.HasFormContentType; }
        }

        public override IFormCollection Form
        {
            get { return FormFeature.ReadForm(); }
            set { FormFeature.Form = value; }
        }

        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken)
        {
            return FormFeature.ReadFormAsync(cancellationToken);
        }
    }
}