using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using System;

namespace Owin2AspNet.Helper
{
    internal static class FeatureHelper
    {
        public static T GetAndCache<T>(
            IFeatureCache cache,
            IFeatureCollection features,
            ref T cachedObject)
            where T : class
        {
            cache.CheckFeaturesRevision();

            T obj = cachedObject;
            if (obj == null)
            {
                obj = features.Get<T>();
                cachedObject = obj;
            }
            return obj;
        }

        public static T GetOrCreate<T>(
            IFeatureCollection features,
            Func<T> factory)
            where T : class
        {
            T obj = features.Get<T>();
            if (obj == null)
            {
                obj = factory();
                features.Set(obj);
            }

            return obj;
        }

        public static T GetOrCreateAndCache<T>(
            IFeatureCache cache,
            IFeatureCollection features,
            Func<T> factory,
            ref T cachedObject)
            where T : class
        {
            cache.CheckFeaturesRevision();

            T obj = cachedObject;
            if (obj == null)
            {
                obj = features.Get<T>();
                if (obj == null)
                {
                    obj = factory();
                    cachedObject = obj;
                    features.Set(obj);
                }
            }
            return obj;
        }

        public static T GetOrCreateAndCache<T>(
            IFeatureCache cache,
            IFeatureCollection features,
            Func<IFeatureCollection, T> factory,
            ref T cachedObject)
            where T : class
        {
            cache.CheckFeaturesRevision();

            T obj = cachedObject;
            if (obj == null)
            {
                obj = features.Get<T>();
                if (obj == null)
                {
                    obj = factory(features);
                    cachedObject = obj;
                    features.Set(obj);
                }
            }
            return obj;
        }

        public static T GetOrCreateAndCache<T>(
            IFeatureCache cache,
            IFeatureCollection features,
            HttpRequest request,
            Func<HttpRequest, T> factory,
            ref T cachedObject)
            where T : class
        {
            cache.CheckFeaturesRevision();

            T obj = cachedObject;
            if (obj == null)
            {
                obj = features.Get<T>();
                if (obj == null)
                {
                    obj = factory(request);
                    cachedObject = obj;
                    features.Set(obj);
                }
            }
            return obj;
        }
    }
}