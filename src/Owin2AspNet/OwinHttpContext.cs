using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Authentication.Internal;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.AspNet.Http.Features.Authentication.Internal;
using Microsoft.AspNet.Http.Features.Internal;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Owin;
using Owin2AspNet.Helper;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;

namespace Owin2AspNet
{
    public class OwinHttpContext : HttpContext, IFeatureCache
    {
        private readonly HttpRequest _request;
        private readonly HttpResponse _response;
        private ConnectionInfo _connection;
        private AuthenticationManager _authenticationManager;

        private IItemsFeature _items;
        private IServiceProvidersFeature _serviceProviders;
        private IHttpAuthenticationFeature _authentication;
        private IHttpRequestLifetimeFeature _lifetime;
        private ISessionFeature _session;
        private WebSocketManager _websockets;

        private IFeatureCollection _features;
        private int _cachedFeaturesRevision = -1;

        public OwinHttpContext(IOwinContext owinContext)
        {
            _features = new FeatureCollection(new OwinFeatureCollection(owinContext.Environment));
            _request = new OwinHttpRequest(this, _features);
            _response = new OwinHttpResponse(this, _features);
        }

        void IFeatureCache.CheckFeaturesRevision()
        {
            if (_cachedFeaturesRevision != _features.Revision)
            {
                _items = null;
                _serviceProviders = null;
                _authentication = null;
                _lifetime = null;
                _session = null;
                _cachedFeaturesRevision = _features.Revision;
            }
        }

        private IItemsFeature ItemsFeature
        {
            get
            {
                return FeatureHelper.GetOrCreateAndCache(
                    this,
                    _features,
                    () => new ItemsFeature(),
                    ref _items);
            }
        }

        private IServiceProvidersFeature ServiceProvidersFeature
        {
            get
            {
                return FeatureHelper.GetOrCreateAndCache(
                    this,
                    _features,
                    () => new ServiceProvidersFeature(),
                    ref _serviceProviders);
            }
        }

        private IHttpAuthenticationFeature HttpAuthenticationFeature
        {
            get
            {
                return FeatureHelper.GetOrCreateAndCache(
                    this,
                    _features,
                    () => new HttpAuthenticationFeature(),
                    ref _authentication);
            }
        }

        private IHttpRequestLifetimeFeature LifetimeFeature
        {
            get
            {
                return FeatureHelper.GetOrCreateAndCache(
                    this,
                    _features,
                    () => new HttpRequestLifetimeFeature(),
                    ref _lifetime);
            }
        }

        private ISessionFeature SessionFeature
        {
            get { return FeatureHelper.GetAndCache(this, _features, ref _session); }
            set
            {
                _features.Set(value);
                _session = value;
            }
        }

        private IHttpRequestIdentifierFeature RequestIdentifierFeature
        {
            get
            {
                return FeatureHelper.GetOrCreate<IHttpRequestIdentifierFeature>(
                  _features,
                  () => new HttpRequestIdentifierFeature());
            }
        }

        public override IFeatureCollection Features { get { return _features; } }

        public override HttpRequest Request { get { return _request; } }

        public override HttpResponse Response { get { return _response; } }

        public override ConnectionInfo Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new DefaultConnectionInfo(_features);
                }
                return _connection;
            }
        }

        public override AuthenticationManager Authentication
        {
            get
            {
                if (_authenticationManager == null)
                {
                    _authenticationManager = new DefaultAuthenticationManager(_features);
                }
                return _authenticationManager;
            }
        }

        public override ClaimsPrincipal User
        {
            get
            {
                var user = HttpAuthenticationFeature.User;
                if (user == null)
                {
                    user = new ClaimsPrincipal(new ClaimsIdentity());
                    HttpAuthenticationFeature.User = user;
                }
                return user;
            }
            set { HttpAuthenticationFeature.User = value; }
        }

        public override IDictionary<object, object> Items
        {
            get { return ItemsFeature.Items; }
            set { ItemsFeature.Items = value; }
        }

        public override IServiceProvider ApplicationServices
        {
            get { return ServiceProvidersFeature.ApplicationServices; }
            set { ServiceProvidersFeature.ApplicationServices = value; }
        }

        public override IServiceProvider RequestServices
        {
            get { return ServiceProvidersFeature.RequestServices; }
            set { ServiceProvidersFeature.RequestServices = value; }
        }

        public override CancellationToken RequestAborted
        {
            get { return LifetimeFeature.RequestAborted; }
            set { LifetimeFeature.RequestAborted = value; }
        }

        public override string TraceIdentifier
        {
            get { return RequestIdentifierFeature.TraceIdentifier; }
            set { RequestIdentifierFeature.TraceIdentifier = value; }
        }

        public override ISession Session
        {
            get
            {
                var feature = SessionFeature;
                if (feature == null)
                {
                    throw new InvalidOperationException("Session has not been configured for this application " +
                        "or request.");
                }
                return feature.Session;
            }
            set
            {
                var feature = SessionFeature;
                if (feature == null)
                {
                    feature = new DefaultSessionFeature();
                    SessionFeature = feature;
                }
                feature.Session = value;
            }
        }

        public override WebSocketManager WebSockets
        {
            get
            {
                if (_websockets == null)
                {
                    _websockets = new DefaultWebSocketManager(_features);
                }
                return _websockets;
            }
        }

        public override void Abort()
        {
            LifetimeFeature.Abort();
        }
    }
}