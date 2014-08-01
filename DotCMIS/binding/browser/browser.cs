using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;

using DotCMIS.Exceptions;
using DotCMIS.Enums;
using DotCMIS.Data;
using DotCMIS.Data.Extensions;
using DotCMIS.Data.Impl;
using DotCMIS.Binding.Impl;
using DotCMIS.Binding.Services;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace DotCMIS.Binding.Browser
{
    internal class RepositoryUrlCache
    {
        private Dictionary<string, string> ReposioryUrls;
        private Dictionary<string, string> RootUrls;

        private object Lock = new object();

        internal RepositoryUrlCache()
        {
            ReposioryUrls = new Dictionary<string, string>();
            RootUrls = new Dictionary<string, string>();
        }

        internal void AddRepository(string repositoryId, string repositoryUrl, string rootUrl)
        {
            if (repositoryId == null)
            {
                throw new ArgumentNullException("Repository Id is not set!");
            }
            if (repositoryUrl == null)
            {
                throw new ArgumentNullException("Repository URL is not set!");
            }
            if (rootUrl == null)
            {
                throw new ArgumentNullException("Root URL is not set!");
            }

            lock (Lock)
            {
                ReposioryUrls[repositoryId] = repositoryUrl;
                RootUrls[repositoryId] = rootUrl;
            }
        }

        internal void RemoveRepository(string repositoryId)
        {
            lock (Lock)
            {
                ReposioryUrls.Remove(repositoryId);
                RootUrls.Remove(repositoryId);
            }
        }

        internal UrlBuilder GetRepositoryUrl(string repositoryId)
        {
            lock (Lock)
            {
                string url;
                if (ReposioryUrls.TryGetValue(repositoryId, out url))
                {
                    return new UrlBuilder(url);
                }
                else
                {
                    return null;
                }
            }
        }

        internal UrlBuilder GetRepositoryUrl(string repositoryId, string selector)
        {
            UrlBuilder url = GetRepositoryUrl(repositoryId);
            if (url == null)
            {
                return null;
            }
            url.AddParameter(Parameters.ParamSelector, selector);
            return url;
        }

        internal UrlBuilder GetRootUrl(string repositoryId)
        {
            lock (Lock)
            {
                string url;
                if (RootUrls.TryGetValue(repositoryId, out url))
                {
                    return new UrlBuilder(url);
                }
                else
                {
                    return null;
                }
            }
        }

        internal UrlBuilder GetObjectUrl(string repositoryId, string objectId)
        {
            UrlBuilder url = GetRootUrl(repositoryId);
            if (url == null)
            {
                return null;
            }
            url.AddParameter(Parameters.ParamObjectId, objectId);
            return url;
        }

        internal UrlBuilder GetObjectUrl(string repositoryId, string objectId, string selector)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);
            if (url == null)
            {
                return null;
            }
            url.AddParameter(Parameters.ParamSelector, selector);
            return url;
        }

        internal UrlBuilder GetPathUrl(string repositoryId, string path)
        {
            UrlBuilder result = GetRootUrl(repositoryId);
            if (result == null)
            {
                return null;
            }
            result.AddPath(path);
            return result;
        }

        internal UrlBuilder GetPathUrl(string repositoryId, string path, string selector)
        {
            UrlBuilder result = GetPathUrl(repositoryId, path);
            if (result == null)
            {
                return null;
            }
            result.AddParameter(Parameters.ParamSelector, selector);
            return result;
        }
    }

    internal class BrowserBindingSessionUtility
    {
        private static readonly string SessionRepositoryUrlCache = "org.apache.chemistry.dotcmis.binding.browser.repositoryurls";

        internal static RepositoryUrlCache GetRepositoryUrlCache(BindingSession session)
        {
            RepositoryUrlCache repositoryUrlCache = (RepositoryUrlCache)session.GetValue(SessionRepositoryUrlCache);
            if (repositoryUrlCache == null)
            {
                repositoryUrlCache = new RepositoryUrlCache();
                session.PutValue(SessionRepositoryUrlCache, repositoryUrlCache);
            }
            return repositoryUrlCache;
        }

        internal static void ClearAllCaches(BindingSession session)
        {
            session.RemoveValue(SessionRepositoryUrlCache);
        }

        internal static void ClearRepositoryCache(BindingSession session, string repositoryId)
        {
            RepositoryUrlCache repositoryUrlCache = (RepositoryUrlCache)session.GetValue(SessionRepositoryUrlCache);
            if (repositoryUrlCache != null)
            {
                repositoryUrlCache.RemoveRepository(repositoryId);
            }
        }
    }

    internal class CmisBrowserSpi : ICmisSpi
    {
        private BindingSession Session;

        private RepositoryService RepositoryService;
        private NavigationService NavigationService;
        private ObjectService ObjectService;
        private VersioningService VersioningService;
        private DiscoveryService DiscoveryService;
        private MultiFilingService MultiFilingService;
        private RelationshipService RelationshipService;
        private PolicyService PolicyService;
        private AclService AclService;

        public void initialize(BindingSession session)
        {
            Session = session;

            RepositoryService = new RepositoryService(session);
            NavigationService = new NavigationService(session);
            ObjectService = new ObjectService(session);
            VersioningService = new VersioningService(session);
            DiscoveryService = new DiscoveryService(session);
            MultiFilingService = new MultiFilingService(session);
            RelationshipService = new RelationshipService(session);
            PolicyService = new PolicyService(session);
            AclService = new AclService(session);
        }

        public IRepositoryService GetRepositoryService()
        {
            return RepositoryService;
        }

        public INavigationService GetNavigationService()
        {
            return NavigationService;
        }

        public IObjectService GetObjectService()
        {
            return ObjectService;
        }

        public IVersioningService GetVersioningService()
        {
            return VersioningService;
        }

        public IRelationshipService GetRelationshipService()
        {
            return RelationshipService;
        }

        public IDiscoveryService GetDiscoveryService()
        {
            return DiscoveryService;
        }

        public IMultiFilingService GetMultiFilingService()
        {
            return MultiFilingService;
        }

        public IAclService GetAclService()
        {
            return AclService;
        }

        public IPolicyService GetPolicyService()
        {
            return PolicyService;
        }

        public void ClearAllCaches()
        {
            BrowserBindingSessionUtility.ClearAllCaches(Session);
        }

        public void ClearRepositoryCache(string repositoryId)
        {
            BrowserBindingSessionUtility.ClearRepositoryCache(Session, repositoryId);
        }

        public void Dispose()
        {
            //  nothing to do
        }
    }

    internal abstract class AbstractBrowserService
    {
        protected BindingSession Session;
        protected bool Succinct;

        protected AbstractBrowserService(BindingSession session)
        {
            Session = session;
            string succinct = (string)session.GetValue(SessionParameter.BrowserSuccinct);
            Succinct = (succinct == null ? true : bool.Parse(succinct));
        }

        protected String GetSuccinctParameter()
        {
            return Succinct ? "true" : null;
        }

        protected string GetServiceUrl()
        {
            return Session.GetValue(SessionParameter.BrowserUrl) as string;
        }

        private RepositoryUrlCache GetRepositoryUrlCache()
        {
            return BrowserBindingSessionUtility.GetRepositoryUrlCache(Session);
        }

        protected CmisBaseException ConvertStatusCode(HttpStatusCode code, string message, string errorContent, Exception e)
        {
            switch (code)
            {
                case HttpStatusCode.Moved:
                case HttpStatusCode.Found:
                case HttpStatusCode.SeeOther:
                case HttpStatusCode.TemporaryRedirect:
                    return new CmisConnectionException("Redirects are not supported (HTTP status code " + code + "): "
                            + message, errorContent, e);
                case HttpStatusCode.BadRequest:
                    return new CmisInvalidArgumentException(message, errorContent, e);
                case HttpStatusCode.Forbidden:
                    return new CmisPermissionDeniedException(message, errorContent, e);
                case HttpStatusCode.NotFound:
                    return new CmisObjectNotFoundException(message, errorContent, e);
                case HttpStatusCode.MethodNotAllowed:
                    return new CmisNotSupportedException(message, errorContent,e);
                case HttpStatusCode.Conflict:
                    return new CmisConstraintException(message, errorContent, e);
                default:
                    return new CmisRuntimeException(message, errorContent, e);
            }
        }

        protected HttpUtils.Response Read(UrlBuilder url)
        {
            HttpUtils.Response resp = HttpUtils.InvokeGET(url, Session);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            return resp;
        }

        protected HttpUtils.Response Post(UrlBuilder url, string contentType, HttpUtils.Output writer)
        {
            HttpUtils.Response resp = HttpUtils.InvokePOST(url, contentType, writer, Session);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.Created)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            return resp;
        }

        protected void PostAndConsume(UrlBuilder url, string contentType, HttpUtils.Output writer)
        {
            HttpUtils.Response resp = Post(url, contentType, writer);
            if (resp.Stream != null)
            {
                try
                {
                    byte[] buffer = new byte[64 * 1024];
                    while (resp.Stream.Read(buffer, 0, buffer.Length) > 0)
                    {
                        //  consume
                    }
                }
                catch (IOException)
                {
                }
                finally
                {
                    resp.Stream.Close();
                }
            }
        }

        protected JObject ParseObject(Stream stream)
        {
            object o = JToken.ReadFrom(new JsonTextReader(new StreamReader(stream)));
            if (o is JObject)
            {
                return o as JObject;
            }
            throw new CmisConnectionException("Unexpected object!");
        }

        protected JArray ParseArray(Stream stream)
        {
            object o = JToken.ReadFrom(new JsonTextReader(new StreamReader(stream)));
            if (o is JArray)
            {
                return o as JArray;
            }
            throw new CmisConnectionException("Unexpected object!");
        }

        protected IList<IRepositoryInfo> GetRepositoriesInternal(string repositoryId)
        {
            IList<IRepositoryInfo> repoInfos = new List<IRepositoryInfo>();

            UrlBuilder url = null;
            if (repositoryId != null)
            {
                url = GetRepositoryUrlCache().GetRepositoryUrl(repositoryId);
            }
            if (url == null)
            {
                url = new UrlBuilder(GetServiceUrl());
            }

            HttpUtils.Response resp = Read(url);

            JObject json = ParseObject(resp.Stream);
            foreach (JToken repo in json.PropertyValues())
            {
                string repositoryUrl, rootUrl;
                IRepositoryInfo repoInfo = BrowserConverter.ConvertRepositoryInfo(repo, out repositoryUrl, out rootUrl);
                repoInfos.Add(repoInfo);
                GetRepositoryUrlCache().AddRepository(repoInfo.Id, repositoryUrl, rootUrl);
            }

            return repoInfos;
        }

        private void CheckRepositoryUrlCache(string repositoryId)
        {
            UrlBuilder result = GetRepositoryUrlCache().GetRepositoryUrl(repositoryId);
            if (result == null)
            {
                GetRepositoriesInternal(repositoryId);
                result = GetRepositoryUrlCache().GetRepositoryUrl(repositoryId);
            }
            if (result == null)
            {
                throw new CmisObjectNotFoundException(string.Format("Unknown repository {0}!", repositoryId));
            }
        }

        protected UrlBuilder GetRepositoryUrl(string repositoryId, string selector)
        {
            CheckRepositoryUrlCache(repositoryId);
            return GetRepositoryUrlCache().GetRepositoryUrl(repositoryId, selector);
        }

        protected UrlBuilder GetRepositoryUrl(string repositoryId)
        {
            CheckRepositoryUrlCache(repositoryId);
            return GetRepositoryUrlCache().GetRepositoryUrl(repositoryId);
        }

        protected UrlBuilder GetObjectUrl(string repositoryId, string objectId, string selector)
        {
            CheckRepositoryUrlCache(repositoryId);
            return GetRepositoryUrlCache().GetObjectUrl(repositoryId, objectId, selector);
        }

        protected UrlBuilder GetObjectUrl(string repositoryId, string objectId)
        {
            CheckRepositoryUrlCache(repositoryId);
            return GetRepositoryUrlCache().GetObjectUrl(repositoryId, objectId);
        }

        protected UrlBuilder GetPathUrl(string repositoryId, string objectId, string selector)
        {
            CheckRepositoryUrlCache(repositoryId);
            return GetRepositoryUrlCache().GetPathUrl(repositoryId, objectId, selector);
        }

        protected ITypeDefinition GetTypeDefinitionInternal(string repositoryId, string typeId)
        {
            UrlBuilder url = GetRepositoryUrl(repositoryId,BrowserConstants.SelectorTypeDefinition);
            url.AddParameter(Parameters.ParamTypeId, typeId);
            HttpUtils.Response resp = Read(url);
            JObject json = ParseObject(resp.Stream);
            return BrowserConverter.ConvertTypeDefinition(json);
        }

        public ITypeDefinition GetTypeDefinition(string repositoryId, string typeId)
        {
            TypeDefinitionCache cache = Session.GetTypeDefinitionCache();
            ITypeDefinition type = cache.Get(repositoryId, typeId);
            if (type == null)
            {
                type = GetTypeDefinitionInternal(repositoryId, typeId);
                if (type != null)
                {
                    cache.Put(repositoryId, type);
                }
            }
            return type;
        }
    }

    internal class ClientTypeCache
    {
        private string RepositoryId;
        private AbstractBrowserService Service;

        public ClientTypeCache(string repositoryId, AbstractBrowserService service)
        {
            RepositoryId = repositoryId;
            Service = service;
        }

        public ITypeDefinition GetTypeDefinition(string typeId)
        {
            return Service.GetTypeDefinition(RepositoryId, typeId);
        }
    }

    internal class RepositoryService : AbstractBrowserService, IRepositoryService
    {
        public RepositoryService(BindingSession session)
            : base(session)
        {
        }

        public IList<IRepositoryInfo> GetRepositoryInfos(IExtensionsData extension)
        {
            return GetRepositoriesInternal(null);
        }

        public IRepositoryInfo GetRepositoryInfo(string repositoryId, IExtensionsData extension)
        {
            IList<IRepositoryInfo> repos = GetRepositoriesInternal(repositoryId);
            foreach(IRepositoryInfo repo in repos)
            {
                if (repo.Id != null && repo.Id == repositoryId)
                {
                    return repo;
                }
            }
            throw new CmisObjectNotFoundException("Repository not found!");
        }

        public ITypeDefinition GetTypeDefinition(string repositoryId, string typeId, IExtensionsData extension)
        {
            return GetTypeDefinition(repositoryId, typeId);
        }

        public IList<ITypeDefinitionContainer> GetTypeDescendants(string repositoryId, string typeId, long? depth,
            bool? includePropertyDefinitions, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetTypeDescendants");
        }


        public ITypeDefinitionList GetTypeChildren(string repositoryId, string typeId, bool? includePropertyDefinitions,
            long? maxItems, long? skipCount, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetTypeChildren");
        }
    }

    internal class NavigationService : AbstractBrowserService, INavigationService
    {
        public NavigationService(BindingSession session)
            : base(session)
        {
        }

        public IObjectInFolderList GetChildren(string repositoryId, string folderId, string filter, string orderBy,
            bool? includeAllowableActions, IncludeRelationshipsFlag? includeRelationships, string renditionFilter,
            bool? includePathSegment, long? maxItems, long? skipCount, IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, folderId, BrowserConstants.SelectorChildren);
            url.AddParameter(Parameters.ParamFilter, filter);
            url.AddParameter(Parameters.ParamOrderBy, orderBy);
            url.AddParameter(Parameters.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(Parameters.ParamRelationships, includeRelationships);
            url.AddParameter(Parameters.ParamRenditionFilter, renditionFilter);
            url.AddParameter(Parameters.ParamPathSegment, includePathSegment);
            url.AddParameter(Parameters.ParamMaxItems, maxItems);
            url.AddParameter(Parameters.ParamSkipCount, skipCount);
            url.AddParameter(Parameters.ParamSuccinct, GetSuccinctParameter());

            HttpUtils.Response resp = Read(url);
            JObject json = ParseObject(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            return BrowserConverter.ConvertObjectInFolderList(json, typeCache);
        }

        public IList<IObjectInFolderContainer> GetDescendants(string repositoryId, string folderId, long? depth, string filter,
            bool? includeAllowableActions, IncludeRelationshipsFlag? includeRelationships, string renditionFilter,
            bool? includePathSegment, IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, folderId, BrowserConstants.SelectorDescendants);
            url.AddParameter(Parameters.ParamDepth, depth);
            url.AddParameter(Parameters.ParamFilter, filter);
            url.AddParameter(Parameters.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(Parameters.ParamRelationships, includeRelationships);
            url.AddParameter(Parameters.ParamRenditionFilter, renditionFilter);
            url.AddParameter(Parameters.ParamPathSegment, includePathSegment);
            url.AddParameter(Parameters.ParamSuccinct, GetSuccinctParameter());

            HttpUtils.Response resp = Read(url);
            JArray json = ParseArray(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            return BrowserConverter.ConvertDescendants(json, typeCache);
        }

        public IList<IObjectInFolderContainer> GetFolderTree(string repositoryId, string folderId, long? depth, string filter,
            bool? includeAllowableActions, IncludeRelationshipsFlag? includeRelationships, string renditionFilter,
            bool? includePathSegment, IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, folderId, BrowserConstants.SelectorFolderTree);
            url.AddParameter(Parameters.ParamDepth, depth);
            url.AddParameter(Parameters.ParamFilter, filter);
            url.AddParameter(Parameters.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(Parameters.ParamRelationships, includeRelationships);
            url.AddParameter(Parameters.ParamRenditionFilter, renditionFilter);
            url.AddParameter(Parameters.ParamPathSegment, includePathSegment);
            url.AddParameter(Parameters.ParamSuccinct, GetSuccinctParameter());

            HttpUtils.Response resp = Read(url);
            JArray json = ParseArray(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            return BrowserConverter.ConvertDescendants(json, typeCache);
        }

        public IList<IObjectParentData> GetObjectParents(string repositoryId, string objectId, string filter,
            bool? includeAllowableActions, IncludeRelationshipsFlag? includeRelationships, string renditionFilter,
            bool? includeRelativePathSegment, IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BrowserConstants.SelectorParents);
            url.AddParameter(Parameters.ParamFilter, filter);
            url.AddParameter(Parameters.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(Parameters.ParamRelationships, includeRelationships);
            url.AddParameter(Parameters.ParamRenditionFilter, renditionFilter);
            url.AddParameter(Parameters.ParamRelativePathSegment, includeRelativePathSegment);
            url.AddParameter(Parameters.ParamSuccinct, GetSuccinctParameter());

            HttpUtils.Response resp = Read(url);
            JArray json = ParseArray(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            return BrowserConverter.ConvertObjectParents(json, typeCache);
        }

        public IObjectList GetCheckedOutDocs(string repositoryId, string folderId, string filter, string orderBy,
            bool? includeAllowableActions, IncludeRelationshipsFlag? includeRelationships, string renditionFilter,
            long? maxItems, long? skipCount, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetCheckedOutDocs");
        }

        public IObjectData GetFolderParent(string repositoryId, string folderId, string filter, ExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetFolderParent");
        }
    }

    internal class ObjectService : AbstractBrowserService, IObjectService
    {
        public ObjectService(BindingSession session)
            : base(session)
        {
        }

        public string CreateDocument(string repositoryId, IProperties properties, string folderId, IContentStream contentStream,
            VersioningState? versioningState, IList<string> policies, IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            UrlBuilder url = (folderId != null) ? GetObjectUrl(repositoryId, folderId) : GetRepositoryUrl(repositoryId);

            FormDataWriter formData = new FormDataWriter(BrowserConstants.ActionCreateDocument, contentStream);
            formData.AddPropertiesParameters(properties);
            formData.AddParameter(Parameters.ParamVersioningState, versioningState);
            formData.AddPoliciesParameters(policies);
            formData.AddAddAcesParameters(addAces);
            formData.AddRemoveAcesParameters(removeAces);
            formData.AddSuccinctFlag(Succinct);

            HttpUtils.Response resp = Post(
                url,
                formData.GetContentType(),
                (Stream stream) =>
                {
                    formData.Write(stream);
                });
            JObject json = ParseObject(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            IObjectData newObject = BrowserConverter.ConvertObjectData(json, typeCache);
            return (newObject == null) ? null : newObject.Id;
        }

        public string CreateDocumentFromSource(string repositoryId, string sourceId, IProperties properties, string folderId,
            VersioningState? versioningState, IList<string> policies, IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("CreateDocumentFromSource");
        }

        public string CreateFolder(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, folderId);

            FormDataWriter formData = new FormDataWriter(BrowserConstants.ActionCreateFolder);
            formData.AddPropertiesParameters(properties);
            formData.AddPoliciesParameters(policies);
            formData.AddAddAcesParameters(addAces);
            formData.AddRemoveAcesParameters(removeAces);
            formData.AddSuccinctFlag(Succinct);

            HttpUtils.Response resp = Post(
                url,
                formData.GetContentType(),
                (Stream stream) =>
                {
                    formData.Write(stream);
                });
            JObject json = ParseObject(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            IObjectData newObject = BrowserConverter.ConvertObjectData(json, typeCache);
            return (newObject == null) ? null : newObject.Id;
        }

        public string CreateRelationship(string repositoryId, IProperties properties, IList<string> policies, IAcl addAces,
            IAcl removeAces, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("CreateRelationship");
        }

        public string CreatePolicy(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("CreatePolicy");
        }

        public string CreateItem(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("CreateItem");
        }

        public IAllowableActions GetAllowableActions(string repositoryId, string objectId, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetAllowableActions");
        }

        public IProperties GetProperties(string repositoryId, string objectId, string filter, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetProperties");
        }

        public IList<IRenditionData> GetRenditions(string repositoryId, string objectId, string renditionFilter,
            long? maxItems, long? skipCount, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetRenditions");
        }

        public IObjectData GetObject(string repositoryId, string objectId, string filter, bool? includeAllowableActions,
            IncludeRelationshipsFlag? includeRelationships, string renditionFilter, bool? includePolicyIds, bool? includeAcl,
            IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BrowserConstants.SelectorObject);
            url.AddParameter(Parameters.ParamFilter, filter);
            url.AddParameter(Parameters.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(Parameters.ParamRelationships, includeRelationships);
            url.AddParameter(Parameters.ParamRenditionFilter, renditionFilter);
            url.AddParameter(Parameters.ParamPolicyIds, includePolicyIds);
            url.AddParameter(Parameters.ParamACL, includeAcl);
            url.AddParameter(Parameters.ParamSuccinct, GetSuccinctParameter());

            HttpUtils.Response resp = Read(url);
            JObject json = ParseObject(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            return BrowserConverter.ConvertObjectData(json, typeCache);
        }

        public IObjectData GetObjectByPath(string repositoryId, string path, string filter, bool? includeAllowableActions,
            IncludeRelationshipsFlag? includeRelationships, string renditionFilter, bool? includePolicyIds, bool? includeAcl,
            IExtensionsData extension)
        {
            UrlBuilder url = GetPathUrl(repositoryId, path, BrowserConstants.SelectorObject);
            url.AddParameter(Parameters.ParamFilter, filter);
            url.AddParameter(Parameters.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(Parameters.ParamRelationships, includeRelationships);
            url.AddParameter(Parameters.ParamRenditionFilter, renditionFilter);
            url.AddParameter(Parameters.ParamPolicyIds, includePolicyIds);
            url.AddParameter(Parameters.ParamACL, includeAcl);
            url.AddParameter(Parameters.ParamSuccinct, GetSuccinctParameter());

            HttpUtils.Response resp = Read(url);
            JObject json = ParseObject(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            return BrowserConverter.ConvertObjectData(json,typeCache);
        }

        public IContentStream GetContentStream(string repositoryId, string objectId, string streamId, long? offset, long? length,
            IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BrowserConstants.SelectorContent);
            url.AddParameter(Parameters.ParamStreamId, streamId);

            HttpUtils.Response resp = HttpUtils.InvokeGET(url, Session, offset, length);
            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.PartialContent)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            string filename = null;
            string[] contentDisposition = resp.Headers[MimeHelper.ContentDisposition];
            if (contentDisposition != null && contentDisposition.Length > 0)
            {
                Dictionary<string,string> parameters = new Dictionary<string,string>();
                MimeHelper.DecodeContentDisposition(contentDisposition[0], parameters);
                parameters.TryGetValue(MimeHelper.DispositionFilename, out filename);
            }

            ContentStream result = new ContentStream();
            result.FileName = filename;
            result.Length = resp.ContentLength;
            result.MimeType = resp.ContentType;
            result.Stream = resp.Stream;

            return result;
        }

        public void SetContentStream(string repositoryId, ref string objectId, bool? overwriteFlag, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension)
        {
            if ((objectId == null) || (objectId.Length == 0))
            {
                throw new CmisInvalidArgumentException("Object id must be set!");
            }

            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            FormDataWriter formData = new FormDataWriter(BrowserConstants.ActionSetContent, contentStream);
            formData.AddParameter(Parameters.ParamOverwriteFlag,overwriteFlag);
            formData.AddParameter(Parameters.ParamChangeToken, (changeToken == null ? null : changeToken));
            formData.AddSuccinctFlag(Succinct);

            HttpUtils.Response resp = Post(
                url,
                formData.GetContentType(),
                (Stream stream) =>
                {
                    formData.Write(stream);
                });

            JObject json = ParseObject(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            IObjectData newObject = BrowserConverter.ConvertObjectData(json, typeCache);
            objectId = ((newObject == null) ? null : newObject.Id);
            if (changeToken != null && newObject.Properties != null)
            {
                Object ct = newObject.Properties[PropertyIds.ChangeToken];
                changeToken = (ct == null ? null : ct.ToString());
            }
        }

        public void DeleteContentStream(string repositoryId, ref string objectId, ref string changeToken, IExtensionsData extension)
        {
            if ((objectId == null) || (objectId.Length == 0))
            {
                throw new CmisInvalidArgumentException("Object id must be set!");
            }

            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            FormDataWriter formData = new FormDataWriter(BrowserConstants.ActionDeleteContent);
            formData.AddParameter(Parameters.ParamChangeToken, (changeToken == null ? null : changeToken));
            formData.AddSuccinctFlag(Succinct);

            HttpUtils.Response resp = Post(
                url,
                formData.GetContentType(),
                (Stream stream) =>
                {
                    formData.Write(stream);
                });

            JObject json = ParseObject(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            IObjectData newObject = BrowserConverter.ConvertObjectData(json, typeCache);
            objectId = ((newObject == null) ? null : newObject.Id);
            if (changeToken != null && newObject.Properties != null)
            {
                Object ct = newObject.Properties[PropertyIds.ChangeToken];
                changeToken = (ct == null ? null : ct.ToString());
            }
        }

        public void AppendContentStream(string repositoryId, ref string objectId, bool? isLastChunk, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension)
        {
            if ((objectId == null) || (objectId.Length == 0))
            {
                throw new CmisInvalidArgumentException("Object id must be set!");
            }

            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            FormDataWriter formData = new FormDataWriter(BrowserConstants.ActionAppendContent, contentStream);
            formData.AddParameter(BrowserConstants.ControlIsLastChunk, isLastChunk);
            formData.AddParameter(Parameters.ParamChangeToken, (changeToken == null ? null : changeToken));
            formData.AddSuccinctFlag(Succinct);

            HttpUtils.Response resp = Post(
                url,
                formData.GetContentType(),
                (Stream stream) =>
                {
                    formData.Write(stream);
                });

            JObject json = ParseObject(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            IObjectData newObject = BrowserConverter.ConvertObjectData(json, typeCache);
            objectId = ((newObject == null) ? null : newObject.Id);
            if (changeToken != null && newObject.Properties != null)
            {
                Object ct = newObject.Properties[PropertyIds.ChangeToken];
                changeToken = (ct == null ? null : ct.ToString());
            }
        }

        public void UpdateProperties(string repositoryId, ref string objectId, ref string changeToken, IProperties properties,
            IExtensionsData extension)
        {
            if ((objectId == null) || (objectId.Length == 0))
            {
                throw new CmisInvalidArgumentException("Object id must be set!");
            }
            
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            FormDataWriter formData = new FormDataWriter(BrowserConstants.ActionUpdateProperties);
            formData.AddPropertiesParameters(properties);
            formData.AddParameter(Parameters.ParamChangeToken, (changeToken == null ? null : changeToken));
            formData.AddSuccinctFlag(Succinct);

            HttpUtils.Response resp = Post(
                url,
                formData.GetContentType(),
                (Stream stream) =>
                {
                    formData.Write(stream);
                });
            JObject json = ParseObject(resp.Stream);
            ClientTypeCache typeCache = new ClientTypeCache(repositoryId, this);
            IObjectData newObject = BrowserConverter.ConvertObjectData(json, typeCache);
            objectId = ((newObject == null) ? null : newObject.Id);
            if (changeToken != null && newObject.Properties != null)
            {
                Object ct = newObject.Properties[PropertyIds.ChangeToken];
                changeToken = (ct == null ? null : ct.ToString());
            }
        }

        public void MoveObject(string repositoryId, ref string objectId, string targetFolderId, string sourceFolderId,
            IExtensionsData extension)
        {
            throw new CmisNotSupportedException("MoveObject");
        }

        public void DeleteObject(string repositoryId, string objectId, bool? allVersions, IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            FormDataWriter formData = new FormDataWriter(BrowserConstants.ActionDelete);
            formData.AddParameter(Parameters.ParamAllVersions, allVersions);

            PostAndConsume(
                url,
                formData.GetContentType(),
                (Stream stream) =>
                {
                    formData.Write(stream);
                });
        }

        public IFailedToDeleteData DeleteTree(string repositoryId, string folderId, bool? allVersions, UnfileObject? unfileObjects,
            bool? continueOnFailure, ExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, folderId);

            FormDataWriter formData = new FormDataWriter(BrowserConstants.ActionDeleteTree);
            formData.AddParameter(Parameters.ParamAllVersions, allVersions);
            formData.AddParameter(Parameters.ParamUnfildeObjects, unfileObjects);
            formData.AddParameter(Parameters.ParamContinueOnFailure, continueOnFailure);

            HttpUtils.Response resp = Post(
                url,
                formData.GetContentType(),
                (Stream stream) =>
                {
                    formData.Write(stream);
                });
            if (resp.Stream != null && resp.Stream.CanRead)
            {
                try
                {
                    JObject json = ParseObject(resp.Stream);
                    return BrowserConverter.ConvertFailedToDelete(json);
                }
                catch (IOException e)
                {
                    throw new CmisConnectionException("Cannot read response!", e);
                }
                catch (Exception)
                {
                    //  empty stream
                }
            }

            return new FailedToDeleteData();
        }
    }

    internal class VersioningService : AbstractBrowserService, IVersioningService
    {
        public VersioningService(BindingSession session)
            : base(session)
        {
        }

        public void CheckOut(string repositoryId, ref string objectId, IExtensionsData extension, out bool? contentCopied)
        {
            throw new CmisNotSupportedException("CheckOut");
        }

        public void CancelCheckOut(string repositoryId, string objectId, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("CancelCheckOut");
        }

        public void CheckIn(string repositoryId, ref string objectId, bool? major, IProperties properties,
            IContentStream contentStream, string checkinComment, IList<string> policies, IAcl addAces, IAcl removeAces,
            IExtensionsData extension)
        {
            throw new CmisNotSupportedException("CheckIn");
        }

        public IObjectData GetObjectOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool major,
            string filter, bool? includeAllowableActions, IncludeRelationshipsFlag? includeRelationships,
            string renditionFilter, bool? includePolicyIds, bool? includeAcl, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetObjectOfLatestVersion");
        }

        public IProperties GetPropertiesOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool major,
            string filter, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetPropertiesOfLatestVersion");
        }

        public IList<IObjectData> GetAllVersions(string repositoryId, string objectId, string versionSeriesId, string filter,
            bool? includeAllowableActions, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetAllVersions");
        }
    }

    internal class RelationshipService : AbstractBrowserService, IRelationshipService
    {
        public RelationshipService(BindingSession session)
            : base(session)
        {
        }

        public IObjectList GetObjectRelationships(string repositoryId, string objectId, bool? includeSubRelationshipTypes,
            RelationshipDirection? relationshipDirection, string typeId, string filter, bool? includeAllowableActions,
            long? maxItems, long? skipCount, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetObjectRelationships");
        }
    }

    internal class DiscoveryService : AbstractBrowserService, IDiscoveryService
    {
        public DiscoveryService(BindingSession session)
            : base(session)
        {
        }

        public IObjectList Query(string repositoryId, string statement, bool? searchAllVersions,
            bool? includeAllowableActions, IncludeRelationshipsFlag? includeRelationships, string renditionFilter,
            long? maxItems, long? skipCount, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("Query");
        }

        public IObjectList GetContentChanges(string repositoryId, ref string changeLogToken, bool? includeProperties,
           string filter, bool? includePolicyIds, bool? includeAcl, long? maxItems, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetContentChanges");
        }
    }

    internal class MultiFilingService : AbstractBrowserService, IMultiFilingService
    {
        public MultiFilingService(BindingSession session)
            : base(session)
        {
        }

        public void AddObjectToFolder(string repositoryId, string objectId, string folderId, bool? allVersions, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("AddObjectToFolder");
        }

        public void RemoveObjectFromFolder(string repositoryId, string objectId, string folderId, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("RemoveObjectFromFolder");
        }
    }

    internal class AclService : AbstractBrowserService, IAclService
    {
        public AclService(BindingSession session)
            : base(session)
        {
        }

        public IAcl GetAcl(string repositoryId, string objectId, bool? onlyBasicPermissions, IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BrowserConstants.SelectorAcl);
            url.AddParameter(Parameters.ParamOnlyBasicPermissions, onlyBasicPermissions);

            HttpUtils.Response resp = Read(url);
            JObject json = ParseObject(resp.Stream);
            return BrowserConverter.ConvertAcl(json);
        }

        public IAcl ApplyAcl(string repositoryId, string objectId, IAcl addAces, IAcl removeAces, AclPropagation? aclPropagation,
            IExtensionsData extension)
        {
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);
            FormDataWriter formData = new FormDataWriter(BrowserConstants.ActionApplyACL);
            formData.AddAddAcesParameters(addAces);
            formData.AddRemoveAcesParameters(addAces);
            formData.AddParameter(Parameters.ParamACLPropagation, aclPropagation);

            HttpUtils.Response resp = Post(
                url,
                formData.GetContentType(),
                (Stream stream) =>
                {
                    formData.Write(stream);
                });
            JObject json = ParseObject(resp.Stream);
            return BrowserConverter.ConvertAcl(json);
        }
    }

    internal class PolicyService : AbstractBrowserService, IPolicyService
    {
        public PolicyService(BindingSession session)
            : base(session)
        {
        }

        public void ApplyPolicy(string repositoryId, string policyId, string objectId, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("ApplyPolicy");
        }

        public void RemovePolicy(string repositoryId, string policyId, string objectId, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("RemovePolicy");
        }

        public IList<IObjectData> GetAppliedPolicies(string repositoryId, string objectId, string filter, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("GetAppliedPolicies");
        }
    }
}
