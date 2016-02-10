﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System.Collections.Generic;


namespace DotCMIS
{
    public static class SessionParameter
    {
        // ---- general parameter ----
        public const string User = "org.apache.chemistry.dotcmis.user";
        public const string Password = "org.apache.chemistry.dotcmis.password";

        // ---- provider parameter ----
        // Predefined binding types
        public const string BindingType = "org.apache.chemistry.dotcmis.binding.spi.type";

        // Class name of the binding class.
        public const string BindingSpiClass = "org.apache.chemistry.dotcmis.binding.spi.classname";

        // URL of the AtomPub service document.
        public const string AtomPubUrl = "org.apache.chemistry.dotcmis.binding.atompub.url";

        // WSDL URLs for Web Services.
        public const string WebServicesRepositoryService = "org.apache.chemistry.dotcmis.binding.webservices.RepositoryService";
        public const string WebServicesNavigationService = "org.apache.chemistry.dotcmis.binding.webservices.NavigationService";
        public const string WebServicesObjectService = "org.apache.chemistry.dotcmis.binding.webservices.ObjectService";
        public const string WebServicesVersioningService = "org.apache.chemistry.dotcmis.binding.webservices.VersioningService";
        public const string WebServicesDiscoveryService = "org.apache.chemistry.dotcmis.binding.webservices.DiscoveryService";
        public const string WebServicesRelationshipService = "org.apache.chemistry.dotcmis.binding.webservices.RelationshipService";
        public const string WebServicesMultifilingService = "org.apache.chemistry.dotcmis.binding.webservices.MultiFilingService";
        public const string WebServicesPolicyService = "org.apache.chemistry.dotcmis.binding.webservices.PolicyService";
        public const string WebServicesAclService = "org.apache.chemistry.dotcmis.binding.webservices.ACLService";

        public const string WebServicesWCFBinding = "org.apache.chemistry.dotcmis.binding.webservices.wcfbinding";
        public const string WebServicesOpenTimeout = "org.apache.chemistry.dotcmis.binding.webservices.opentimeout";
        public const string WebServicesCloseTimeout = "org.apache.chemistry.dotcmis.binding.webservices.closetimeout";
        public const string WebServicesSendTimeout = "org.apache.chemistry.dotcmis.binding.webservices.sendtimeout";
        public const string WebServicesReceiveTimeout = "org.apache.chemistry.dotcmis.binding.webservices.receivetimeout";

        public const string WebServicesEnableUnsecuredResponse = "org.apache.chemistry.dotcmis.binding.webservices.enableUnsecuredResponse"; // requires hotfix 971493 or the .NET framework 4 

        // URL of the Browser service
        public const string BrowserUrl = "org.apache.chemistry.dotcmis.binding.browser.url";

        // succinct flag for Browser service
        public const string BrowserSuccinct = "org.apache.chemistry.dotcmis.binding.browser.succinct";

        // authentication provider
        public const string AuthenticationProviderClass = "org.apache.chemistry.dotcmis.binding.auth.classname";

        // compression flag
        public const string Compression = "org.apache.chemistry.dotcmis.binding.compression";

        // timeouts
        public const string ConnectTimeout = "org.apache.chemistry.dotcmis.binding.connecttimeout";
        public const string ReadTimeout = "org.apache.chemistry.dotcmis.binding.readtimeout";

        // binding caches
        public const string CacheSizeRepositories = "org.apache.chemistry.dotcmis.binding.cache.repositories.size";
        public const string CacheSizeTypes = "org.apache.chemistry.dotcmis.binding.cache.types.size";
        public const string CacheSizeLinks = "org.apache.chemistry.dotcmis.binding.cache.links.size";

        // message size
        public const string MessageSize = "org.apache.chemistry.dotcmis.binding.message.size";

        // device management
        public const string DeviceIdentifier = "org.apache.chemistry.dotcmis.devicemanagement.identifier";

        // http user agent
        public const string UserAgent = "org.apache.chemistry.dotcmis.http.useragent";

        // http stability tweek
        public const string MaximumRequestRetries = "org.apache.chemistry.dotcmis.http.maximumretries";

        // session parameter
        public const string ObjectFactoryClass = "org.apache.chemistry.dotcmis.objectfactory.classname";
        public const string CacheClass = "org.apache.chemistry.dotcmis.cache.classname";
        public const string RepositoryId = "org.apache.chemistry.dotcmis.session.repository.id";

        public const string CacheSizeObjects = "org.apache.chemistry.dotcmis.cache.objects.size";
        public const string CacheTTLObjects = "org.apache.chemistry.dotcmis.cache.objects.ttl";
        public const string CacheSizePathToId = "org.apache.chemistry.dotcmis.cache.pathtoid.size";
        public const string CacheTTLPathToId = "org.apache.chemistry.dotcmis.cache.pathtoid.ttl";
        public const string CachePathOmit = "org.apache.chemistry.dotcmis.cache.path.omit";
    }

    public static class BindingType
    {
        public const string AtomPub = "atompub";
        public const string WebServices = "webservices";
        public const string Browser = "browser";
        public const string Custom = "custom";
    }

    public static class PropertyIds
    {
        // ---- base ----
        public const string Name = "cmis:name";
        public const string ObjectId = "cmis:objectId";
        public const string ObjectTypeId = "cmis:objectTypeId";
        public const string BaseTypeId = "cmis:baseTypeId";
        public const string CreatedBy = "cmis:createdBy";
        public const string CreationDate = "cmis:creationDate";
        public const string LastModifiedBy = "cmis:lastModifiedBy";
        public const string LastModificationDate = "cmis:lastModificationDate";
        public const string ChangeToken = "cmis:changeToken";
        public const string SecondaryObjectTypeIds = "cmis:secondaryObjectTypeIds";

        // ---- document ----
        public const string IsImmutable = "cmis:isImmutable";
        public const string IsLatestVersion = "cmis:isLatestVersion";
        public const string IsMajorVersion = "cmis:isMajorVersion";
        public const string IsLatestMajorVersion = "cmis:isLatestMajorVersion";
        public const string IsPrivateWorkingCopy = "cmis:isPrivateWorkingCopy";
        public const string VersionLabel = "cmis:versionLabel";
        public const string VersionSeriesId = "cmis:versionSeriesId";
        public const string IsVersionSeriesCheckedOut = "cmis:isVersionSeriesCheckedOut";
        public const string VersionSeriesCheckedOutBy = "cmis:versionSeriesCheckedOutBy";
        public const string VersionSeriesCheckedOutId = "cmis:versionSeriesCheckedOutId";
        public const string CheckinComment = "cmis:checkinComment";
        public const string ContentStreamLength = "cmis:contentStreamLength";
        public const string ContentStreamMimeType = "cmis:contentStreamMimeType";
        public const string ContentStreamFileName = "cmis:contentStreamFileName";
        public const string ContentStreamId = "cmis:contentStreamId";

        // ---- folder ----
        public const string ParentId = "cmis:parentId";
        public const string AllowedChildObjectTypeIds = "cmis:allowedChildObjectTypeIds";
        public const string Path = "cmis:path";

        // ---- relationship ----
        public const string SourceId = "cmis:sourceId";
        public const string TargetId = "cmis:targetId";

        // ---- policy ----
        public const string PolicyText = "cmis:policyText";
    }

    public static class BasicPermissions
    {
        public const string Read = "cmis:read";
        public const string Write = "cmis:write";
        public const string All = "cmis:all";
    }

    public static class PermissionMappingKeys
    {
        public const string CanGetDescendentsFolder = "canGetDescendents.Folder";
        public const string CanGetChildrenFolder = "canGetChildren.Folder";
        public const string CanGetParentsFolder = "canGetParents.Folder";
        public const string CanGetFolderParentObject = "canGetFolderParent.Object";
        public const string CanCreateDocumentFolder = "canCreateDocument.Folder";
        public const string CanCreateFolderFolder = "canCreateFolder.Folder";
        public const string CanCreateRelationshipSource = "canCreateRelationship.Source";
        public const string CanCreateRelationshipTarget = "canCreateRelationship.Target";
        public const string CanGetPropertiesObject = "canGetProperties.Object";
        public const string CanViewContentObject = "canViewContent.Object";
        public const string CanUpdatePropertiesObject = "canUpdateProperties.Object";
        public const string CanMoveObject = "canMove.Object";
        public const string CanMoveTarget = "canMove.Target";
        public const string CanMoveSource = "canMove.Source";
        public const string CanDeleteObject = "canDelete.Object";
        public const string CanDeleteTreeFolder = "canDeleteTree.Folder";
        public const string CanSetContentDocument = "canSetContent.Document";
        public const string CanDeleteContentDocument = "canDeleteContent.Document";
        public const string CanAddToFolderObject = "canAddToFolder.Object";
        public const string CanAddToFolderFolder = "canAddToFolder.Folder";
        public const string CanRemoveFromFolderObject = "canRemoveFromFolder.Object";
        public const string CanRemoveFromFolderFolder = "canRemoveFromFolder.Folder";
        public const string CanCheckoutDocument = "canCheckout.Document";
        public const string CanCancelCheckoutDocument = "canCancelCheckout.Document";
        public const string CanCheckinDocument = "canCheckin.Document";
        public const string CanGetAllVersionsVersionSeries = "canGetAllVersions.VersionSeries";
        public const string CanGetObjectRelationshipSObject = "canGetObjectRelationships.Object";
        public const string CanAddPolicyObject = "canAddPolicy.Object";
        public const string CanAddPolicyPolicy = "canAddPolicy.Policy";
        public const string CanRemovePolicyObject = "canRemovePolicy.Object";
        public const string CanRemovePolicyPolicy = "canRemovePolicy.Policy";
        public const string CanGetAppliesPoliciesObject = "canGetAppliedPolicies.Object";
        public const string CanGetAclObject = "canGetAcl.Object";
        public const string CanApplyAclObject = "canApplyAcl.Object";
    }

    public static class Actions
    {
        public const string CanDeleteObject = "canDeleteObject";
        public const string CanUpdateProperties = "canUpdateProperties";
        public const string CanGetProperties = "canGetProperties";
        public const string CanGetObjectRelationships = "canGetObjectRelationships";
        public const string CanGetObjectParents = "canGetObjectParents";
        public const string CanGetFolderParent = "canGetFolderParent";
        public const string CanGetFolderTree = "canGetFolderTree";
        public const string CanGetDescendants = "canGetDescendants";
        public const string CanMoveObject = "canMoveObject";
        public const string CanDeleteContentStream = "canDeleteContentStream";
        public const string CanCheckOut = "canCheckOut";
        public const string CanCancelCheckOut = "canCancelCheckOut";
        public const string CanCheckIn = "canCheckIn";
        public const string CanSetContentStream = "canSetContentStream";
        public const string CanGetAllVersions = "canGetAllVersions";
        public const string CanAddObjectToFolder = "canAddObjectToFolder";
        public const string CanRemoveObjectFromFolder = "canRemoveObjectFromFolder";
        public const string CanGetContentStream = "canGetContentStream";
        public const string CanApplyPolicy = "canApplyPolicy";
        public const string CanGetAppliedPolicies = "canGetAppliedPolicies";
        public const string CanRemovePolicy = "canRemovePolicy";
        public const string CanGetChildren = "canGetChildren";
        public const string CanCreateDocument = "canCreateDocument";
        public const string CanCreateFolder = "canCreateFolder";
        public const string CanCreateRelationship = "canCreateRelationship";
        public const string CanDeleteTree = "canDeleteTree";
        public const string CanGetRenditions = "canGetRenditions";
        public const string CanGetAcl = "canGetACL";
        public const string CanApplyAcl = "canApplyACL";
    }

    internal static class Parameters
    {
        // parameter
        public const string ParamACL = "includeACL";
        public const string ParamAllowableActions = "includeAllowableActions";
        public const string ParamAllVersions = "allVersions";
        public const string ParamAppend = "append";
        public const string ParamChangeLogToken = "changeLogToken";
        public const string ParamChangeToken = "changeToken";
        public const string ParamCheckinComment = "checkinComment";
        public const string ParamCheckIn = "checkin";
        public const string ParamChildTypes = "childTypes";
        public const string ParamContinueOnFailure = "continueOnFailure";
        public const string ParamDepth = "depth";
        public const string ParamFilter = "filter";
        public const string ParamSuccinct = "succinct";
        public const string ParamFolderId = "folderId";
        public const string ParamId = "id";
        public const string ParamIsLastChunk = "isLastChunk";
        public const string ParamMajor = "major";
        public const string ParamMaxItems = "maxItems";
        public const string ParamObjectId = "objectId";
        public const string ParamOnlyBasicPermissions = "onlyBasicPermissions";
        public const string ParamOrderBy = "orderBy";
        public const string ParamOverwriteFlag = "overwriteFlag";
        public const string ParamPath = "path";
        public const string ParamPathSegment = "includePathSegment";
        public const string ParamPolicyId = "policyId";
        public const string ParamPolicyIds = "includePolicyIds";
        public const string ParamProperties = "includeProperties";
        public const string ParamPropertyDefinitions = "includePropertyDefinitions";
        public const string ParamRelationships = "includeRelationships";
        public const string ParamRelationshipDirection = "relationshipDirection";
        public const string ParamRelativePathSegment = "includeRelativePathSegment";
        public const string ParamRemoveFrom = "removeFrom";
        public const string ParamRenditionFilter = "renditionFilter";
        public const string ParamRepositoryId = "repositoryId";
        public const string ParamReturnVersion = "returnVersion";
        public const string ParamSkipCount = "skipCount";
        public const string ParamSourceFolderId = "sourceFolderId";
        public const string ParamTargetFolderId = "targetFolderId";
        public const string ParamStreamId = "streamId";
        public const string ParamSubRelationshipTypes = "includeSubRelationshipTypes";
        public const string ParamTypeId = "typeId";
        public const string ParamUnfildeObjects = "unfileObjects";
        public const string ParamVersioningState = "versioningState";
        public const string ParamQ = "q";
        public const string ParamStatement = "statement";
        public const string ParamSearchAllVersions = "searchAllVersions";
        public const string ParamACLPropagation = "ACLPropagation";
        public const string ParamSelector = "cmisselector";
    }

    internal static class AtomPubConstants
    {
        // namespaces
        public const string NamespaceCMIS = "http://docs.oasis-open.org/ns/cmis/core/200908/";
        public const string NamespaceAtom = "http://www.w3.org/2005/Atom";
        public const string NamespaceAPP = "http://www.w3.org/2007/app";
        public const string NamespaceRestAtom = "http://docs.oasis-open.org/ns/cmis/restatom/200908/";
        public const string NamespaceXSI = "http://www.w3.org/2001/XMLSchema-instance";
        public const string NamespaceApacheChemistry = "http://chemistry.apache.org/";

        // media types
        public const string MediatypeService = "application/atomsvc+xml";
        public const string MediatypeFeed = "application/atom+xml;type=feed";
        public const string MediatypeEntry = "application/atom+xml;type=entry";
        public const string MediatypeChildren = MediatypeFeed;
        public const string MediatypeDescendants = "application/cmistree+xml";
        public const string MediatypeQuery = "application/cmisquery+xml";
        public const string MediatypeAllowableAction = "application/cmisallowableactions+xml";
        public const string MediatypeACL = "application/cmisacl+xml";
        public const string MediatypeCMISAtom = "application/cmisatom+xml";
        public const string MediatypeOctetStream = "application/octet-stream";

        // collections
        public const string CollectionRoot = "root";
        public const string CollectionTypes = "types";
        public const string CollectionQuery = "query";
        public const string CollectionCheckedout = "checkedout";
        public const string CollectionUnfiled = "unfiled";

        // URI templates
        public const string TemplateObjectById = "objectbyid";
        public const string TemplateObjectByPath = "objectbypath";
        public const string TemplateTypeById = "typebyid";
        public const string TemplateQuery = "query";

        // Link rel
        public const string RelSelf = "self";
        public const string RelEnclosure = "enclosure";
        public const string RelService = "service";
        public const string RelDescribedBy = "describedby";
        public const string RelAlternate = "alternate";
        public const string RelDown = "down";
        public const string RelUp = "up";
        public const string RelFirst = "first";
        public const string RelLast = "last";
        public const string RelPrev = "previous";
        public const string RelNext = "next";
        public const string RelVia = "via";
        public const string RelEdit = "edit";
        public const string RelEditMedia = "edit-media";
        public const string RelVersionHistory = "version-history";
        public const string RelCurrentVersion = "current-version";
        public const string RelWorkingCopy = "working-copy";
        public const string RelFolderTree = "http://docs.oasis-open.org/ns/cmis/link/200908/foldertree";
        public const string RelAllowableActions = "http://docs.oasis-open.org/ns/cmis/link/200908/allowableactions";
        public const string RelACL = "http://docs.oasis-open.org/ns/cmis/link/200908/acl";
        public const string RelSource = "http://docs.oasis-open.org/ns/cmis/link/200908/source";
        public const string RelTarget = "http://docs.oasis-open.org/ns/cmis/link/200908/target";
        public const string RelRelationships = "http://docs.oasis-open.org/ns/cmis/link/200908/relationships";
        public const string RelPolicies = "http://docs.oasis-open.org/ns/cmis/link/200908/policies";

        public const string RepRelTypeDesc = "http://docs.oasis-open.org/ns/cmis/link/200908/typedescendants";
        public const string RepRelFolderTree = "http://docs.oasis-open.org/ns/cmis/link/200908/foldertree";
        public const string RepRelRootDesc = "http://docs.oasis-open.org/ns/cmis/link/200908/rootdescendants";
        public const string RepRelChanges = "http://docs.oasis-open.org/ns/cmis/link/200908/changes";

        // rendition filter
        public const string RenditionNone = "cmis:none";

        // service doc
        public const string TagService = "service";
        public const string TagWorkspace = "workspace";
        public const string TagRepositoryInfo = "repositoryInfo";
        public const string TagCollection = "collection";
        public const string TagCollectionType = "collectionType";
        public const string TagUriTemplate = "uritemplate";
        public const string TagTemplateTemplate = "template";
        public const string TagTemplateType = "type";
        public const string TagLink = "link";

        // atom
        public const string TagAtomId = "id";
        public const string TagAtomTitle = "title";
        public const string TagAtomUpdated = "updated";

        // feed
        public const string TagFeed = "feed";

        // entry
        public const string TagEntry = "entry";
        public const string TagObject = "object";
        public const string TagNumItems = "numItems";
        public const string TagPathSegment = "pathSegment";
        public const string TagRelativePathSegment = "relativePathSegment";
        public const string TagType = "type";
        public const string TagChildren = "children";
        public const string TagContent = "content";
        public const string TagContentMediatype = "mediatype";
        public const string TagContentBase64 = "base64";
        public const string TagContentFilename = "filename";

        // allowable actions
        public const string TagAllowableActions = "allowableActions";

        // ACL
        public const string TagACL = "acl";

        // query
        public const string TagQuery = "query";
        public const string TagStatement = "statement";
        public const string TagSearchAllVersions = "searchAllVersions";
        public const string TagIncludeAllowableActions = "includeAllowableActions";
        public const string TagRenditionFilter = "renditionFilter";
        public const string TagIncludeRelationships = "includeRelationships";
        public const string TagMaxItems = "maxItems";
        public const string TagSkipCount = "skipCount";

        // links
        public const string LinkRel = "rel";
        public const string LinkHref = "href";
        public const string LinkType = "type";
        public const string ContentSrc = "src";
        public const string LinkRelContent = "@@content@@";
    }

    internal static class BrowserConstants
    {
        //  repository info
        public const string RepoInfoId = "repositoryId";
        public const string RepoInfoName = "repositoryName";
        public const string RepoInfoDescription = "repositoryDescription";
        public const string RepoInfoVendor = "vendorName";
        public const string RepoInfoProduct = "productName";
        public const string RepoInfoProductVersion = "productVersion";
        public const string RepoInfoRootFolderId = "rootFolderId";
        public const string RepoInfoCapabilities = "capabilities";
        public const string RepoInfoAclCapabilities = "aclCapabilities";
        public const string RepoInfoChangeLogToken = "latestChangeLogToken";
        public const string RepoInfoCmisVersionSupported = "cmisVersionSupported";
        public const string RepoInfoThinClientUri = "thinClientURI";
        public const string RepoInfoChangesIncomplete = "changesIncomplete";
        public const string RepoInfoChangesOnType = "changesOnType";
        public const string RepoInfoPrincipalIdAnonymous = "principalIdAnonymous";
        public const string RepoInfoPrincipalIdAnyone = "principalIdAnyone";
        public const string RepoInfoExtendedFeatures = "extendedFeatures";
        public const string RepoInfoRepositoryUrl = "repositoryUrl";
        public const string RepoInfoRootFolderUrl = "rootFolderUrl";

        public static readonly HashSet<string> RepoInfoKeys = new HashSet<string>()
        {
            RepoInfoId,
            RepoInfoName,
            RepoInfoDescription,
            RepoInfoVendor,
            RepoInfoProduct,
            RepoInfoProductVersion,
            RepoInfoRootFolderId,
            RepoInfoCapabilities,
            RepoInfoAclCapabilities,
            RepoInfoChangeLogToken,
            RepoInfoCmisVersionSupported,
            RepoInfoThinClientUri,
            RepoInfoChangesIncomplete,
            RepoInfoChangesOnType,
            RepoInfoPrincipalIdAnonymous,
            RepoInfoPrincipalIdAnyone,
            RepoInfoExtendedFeatures,
            RepoInfoRepositoryUrl,
            RepoInfoRootFolderUrl
        };


        //  selectors
        public const string SelectorLastResult = "lastResult";
        public const string SelectorRepositoryInfo = "repositoryInfo";
        public const string SelectorTypeChildren = "typeChildren";
        public const string SelectorTypeDescendants = "typeDescendants";
        public const string SelectorTypeDefinition = "typeDefinition";
        public const string SelectorContent = "content";
        public const string SelectorObject = "object";
        public const string SelectorProperties = "properties";
        public const string SelectorAllowableActions = "allowableActions";
        public const string SelectorRenditions = "renditions";
        public const string SelectorChildren = "children";
        public const string SelectorDescendants = "descendants";
        public const string SelectorParents = "parents";
        public const string SelectorParent = "parent";
        public const string SelectorFolderTree = "folder";
        public const string SelectorQuery = "query";
        public const string SelectorVersions = "versions";
        public const string SelectorRelationships = "relationships";
        public const string SelectorCheckout = "checkedout";
        public const string SelectorPolicies = "policies";
        public const string SelectorAcl = "acl";
        public const string SelectorContentChanges = "contentChanges";

        //  type
        public const string TypeId = "id";
        public const string TypeBaseId = "baseId";
        public const string TypeDescription = "description";
        public const string TypeDisplayName = "displayName";
        public const string TypeControllableAcl = "controllableACL";
        public const string TypeControllablePolicy = "controllablePolicy";
        public const string TypeCreatable = "creatable";
        public const string TypeFileable = "fileable";
        public const string TypeFulltextIndexed = "fulltextIndexed";
        public const string TypeIncludeInSupertypeQuery = "includedInSupertypeQuery";
        public const string TypeQueryable = "queryable";
        public const string TypeLocalName = "localName";
        public const string TypeLocalNamespace = "localNamespace";
        public const string TypeParentId = "parentId";
        public const string TypeQueryName = "queryName";
        public const string TypePropertyDefinitions = "propertyDefinitions";
        public const string TypeVersionable = "versionable";    //  document
        public const string TypeContentStreamAllowed = "contentStreamAllowed";  //  document
        public const string TypeAllowedSourceTypes = "allowedSourceTypes";  //  relationship
        public const string TypeAllowedTargetTypes = "allowedTargetTypes";  //  relationship

        public static readonly HashSet<string> TypeKeys = new HashSet<string>()
        {
            TypeId,
            TypeBaseId,
            TypeDescription,
            TypeDisplayName,
            TypeControllableAcl,
            TypeControllablePolicy,
            TypeCreatable,
            TypeFileable,
            TypeFulltextIndexed,
            TypeIncludeInSupertypeQuery,
            TypeQueryable,
            TypeLocalName,
            TypeLocalNamespace,
            TypeParentId,
            TypeQueryName,
            TypePropertyDefinitions,
            TypeVersionable,
            TypeContentStreamAllowed,
            TypeAllowedSourceTypes,
            TypeAllowedTargetTypes,
        };

        //  object
        public const string ObjectProperties = "properties";
        public const string ObjectSuccinctProperties = "succinctProperties";
        public const string ObjectPropertiesExtension = "propertiesExtension";
        public const string ObjectAllowableActions = "allowableActions";
        public const string ObjectRelationships = "relationships";
        public const string ObjectChangeEventInfo = "changeEventInfo";
        public const string ObjectAcl = "acl";
        public const string ObjectExactAcl = "exactACL";
        public const string ObjectPolicyIds = "policyIds";
        public const string ObjectPolicyIdsIds = "ids";
        public const string ObjectRenditions = "renditions";

        public static readonly HashSet<string> ObjectKeys = new HashSet<string>()
        {
            ObjectProperties,
            ObjectSuccinctProperties,
            ObjectPropertiesExtension,
            ObjectAllowableActions,
            ObjectRelationships,
            ObjectChangeEventInfo,
            ObjectAcl,
            ObjectExactAcl,
            ObjectPolicyIds,
            ObjectRenditions
        };

        //  object in folder list
        public const string ObjectInFolderListObjects = "objects";
        public const string ObjectInFolderListHasMoreItems = "hasMoreItems";
        public const string ObjectInFolderListNumItems = "numItems";

        public static readonly HashSet<string> ObjectInFolderListKeys = new HashSet<string>()
        {
            ObjectInFolderListObjects,
            ObjectInFolderListHasMoreItems,
            ObjectInFolderListNumItems
        };

        //  object in folder
        public const string ObjectInFolderObject = "object";
        public const string ObjectInFolderPathSegment = "pathSegment";

        public static readonly HashSet<string> ObjectInFolderKeys = new HashSet<string>()
        {
            ObjectInFolderObject,
            ObjectInFolderPathSegment
        };

        //  object in folder container
        public const string ObjectInFolderContainer = "object";
        public const string ObjectInFolderContainerChildren = "children";

        public static readonly HashSet<string> ObjectInFolderContainerKeys = new HashSet<string>()
        {
            ObjectInFolderContainer,
            ObjectInFolderContainerChildren
        };

        // parent object
        public const string ObjectParentsObject = "object";
        public const string ObjectParentsRelavivePathSegment = "relativePathSegment";

        public static readonly HashSet<string> ObjectParentsKeys = new HashSet<string>()
        {
            ObjectParentsObject,
            ObjectParentsRelavivePathSegment
        };

        public const string TypesContainerType = "type";
        public const string TypesContainerChildren = "children";

        public static readonly HashSet<string> TypesContainerKeys = new HashSet<string>()
        {
            TypesContainerType,
            TypesContainerChildren
        };

        public const string TypesListTypes = "types";
        public const string TypesListHasMoreItems = "hasMoreItems";
        public const string TypesListNumItems = "numItems";

        public static readonly HashSet<string> TypesListKeys = new HashSet<string>()
        {
            TypesListTypes,
            TypesListHasMoreItems,
            TypesListNumItems
        };

        public const string QueryResultListResults = "results";
        public const string QueryResultListHasMoreItems = "hasMoreItems";
        public const string QueryResultListNumItems = "numItems";

        public static readonly HashSet<string> QueryResultListKeys = new HashSet<string>()
        {
            QueryResultListResults,
            QueryResultListHasMoreItems,
            QueryResultListNumItems
        };

        public const string ObjectListObjects = "objects";
        public const string ObjectListObject = "object";
        public const string ObjectListHasMoreItems = "hasMoreItems";
        public const string ObjectListNumItems = "numItems";
        public const string ObjectListChangeLogToken = "changeLogToken";

        public static readonly HashSet<string> ObjectListKeys = new HashSet<string>()
        {
            ObjectListObjects,
            ObjectListHasMoreItems,
            ObjectListNumItems,
            ObjectListChangeLogToken
        };

        public const string ChangeEventType = "changeType";
        public const string ChangeEventTime = "changeTime";

        public static readonly HashSet<string> ChangeEventKeys = new HashSet<string>()
        {
            ChangeEventType,
            ChangeEventTime
        };

        public const string CapContentStreamUpdatability = "capabilityContentStreamUpdatability";
        public const string CapChanges = "capabilityChanges";
        public const string CapRenditions = "capabilityRenditions";
        public const string CapGetDescendants = "capabilityGetDescendants";
        public const string CapGetFolderTree = "capabilityGetFolderTree";
        public const string CapMultifiling = "capabilityMultifiling";
        public const string CapUnfiling = "capabilityUnfiling";
        public const string CapVersionSpecificFiling = "capabilityVersionSpecificFiling";
        public const string CapPwcSearchable = "capabilityPWCSearchable";
        public const string CapPwcUpdateble = "capabilityPWCUpdatable";
        public const string CapAllVersionsSearchable = "capabilityAllVersionsSearchable";
        public const string CapQuery = "capabilityQuery";
        public const string CapJoin = "capabilityJoin";
        public const string CapAcl = "capabilityACL";


        public static readonly HashSet<string> CapKeys = new HashSet<string>()
        {
            CapContentStreamUpdatability,
            CapChanges,
            CapRenditions,
            CapGetDescendants,
            CapGetFolderTree,
            CapMultifiling,
            CapUnfiling,
            CapVersionSpecificFiling,
            CapPwcSearchable,
            CapPwcUpdateble,
            CapAllVersionsSearchable,
            CapQuery,
            CapJoin,
            CapAcl
        };

        //  property type
        public const string PropertyTypeId = "id";
        public const string PropertyTypeLocalName = "localName";
        public const string PropertyTypeLocalNamespace = "localNamespace";
        public const string PropertyTypeDisplayName = "displayName";
        public const string PropertyTypeQueryName = "queryName";
        public const string PropertyTypeDescription = "description";
        public const string PropertyTypePropertyType = "propertyType";
        public const string PropertyTypeCardinality = "cardinality";
        public const string PropertyTypeUpdatability = "updatability";
        public const string PropertyTypeInherited = "inherited";
        public const string PropertyTypeRequired = "required";
        public const string PropertyTypeQueryable = "queryable";
        public const string PropertyTypeOrderable = "orderable";
        public const string PropertyTypeOpenChoice = "openChoice";

        public const string PropertyTypeDefaultValue = "defaultValue";

        public const string PropertyTypeMaxLength = "maxLength";
        public const string PropertyTypeMinValue = "minValue";
        public const string PropertyTypeMaxValue = "maxValue";
        public const string PropertyTypePrecision = "precision";
        public const string PropertyTypeResolution = "resolution";

        public const string PropertyTypeChoice = "choice";
        public const string PropertyTypeChoiceDisplayName = "displayName";
        public const string PropertyTypeChoiceValue = "value";
        public const string PropertyTypeChoiceChoice = "choice";

        public static readonly HashSet<string> PropertyTypeKeys = new HashSet<string>()
        {
            PropertyTypeId,
            PropertyTypeLocalName,
            PropertyTypeLocalNamespace,
            PropertyTypeDisplayName,
            PropertyTypeQueryName,
            PropertyTypeDescription,
            PropertyTypePropertyType,
            PropertyTypeCardinality,
            PropertyTypeUpdatability,
            PropertyTypeInherited,
            PropertyTypeRequired,
            PropertyTypeQueryable,
            PropertyTypeOrderable,
            PropertyTypeOpenChoice,
            PropertyTypeDefaultValue,
            PropertyTypeMaxLength,
            PropertyTypeMinValue,
            PropertyTypeMaxValue,
            PropertyTypePrecision,
            PropertyTypeResolution,
            PropertyTypeChoice
        };

        //  property
        public const string PropertyId = "id";
        public const string PropertyLocalName = "localName";
        public const string PropertyDisplayName = "displayName";
        public const string PropertyQueryName = "queryName";
        public const string PropertyValue = "value";
        public const string PropertyDataType = "type";
        public const string PropertyCardinality = "cardinality";

        public static readonly HashSet<string> PropertyKeys = new HashSet<string>(){
            PropertyId,
            PropertyLocalName,
            PropertyDisplayName,
            PropertyQueryName,
            PropertyValue,
            PropertyDataType,
            PropertyCardinality
        };
        
        //  control
        public const string ControlCmisAction = "cmisaction";
        public const string ControlSuccinct = "succinct";
        public const string ControlPropertyId = "propertyId";
        public const string ControlPropertyValue = "propertyValue";
        public const string ControlIsLastChunk = "isLastChunk";
        public const string ControlPolicy = "policy";
        public const string ControlAddAcePrincipal = "addACEPrincipal";
        public const string ControlAddAcePermission = "addACEPermission";
        public const string ControlRemoveAcePrincipal = "removeACEPrincipal";
        public const string ControlRemoveAcePermission = "removeACEPermission";

        //  action
        public const string ActionCreateDocument = "createDocument";
        public const string ActionSetContent = "setContent";
        public const string ActionUpdateProperties = "update";
        public const string ActionDeleteContent = "deleteContent";
        public const string ActionAppendContent = "appendContent";
        public const string ActionDelete = "delete";
        public const string ActionDeleteTree = "deleteTree";
        public const string ActionCreateFolder = "createFolder";
        public const string ActionCreateDocumentFromSource = "createDocumentFromSource";
        public const string ActionMove = "move";
        public const string ActionApplyACL = "applyACL";
        public const string ActionCheckOut = "checkOut";
        public const string ActionCancelCheckOut = "cancelCheckOut";
        public const string ActionCheckIn = "checkIn";
        public const string ActionCreateRelationship = "createRelationship";
        public const string ActionCreateItem = "createItem";
        public const string ActionCreatePolicy = "createPolicy";
        public const string ActionQuery = "query";
        public const string ActionApplyPolicy = "applyPolicy";
        public const string ActionRemovePolicy = "removePolicy";
        public const string ActionAddObjectToFolder = "addObjectToFolder";
        public const string ActionRemoveObjectToFolder = "removeObjectToFolder";


        //  acl
        public const string AclAces = "aces";
        public const string AclIsExact = "isExact";

        public static readonly HashSet<string> AclKeys = new HashSet<string>()
        {
            AclAces,
            AclIsExact
        };

        public const string AclCapSupportedPermissions = "supportedPermissions";
        public const string AclCapAclPropagation = "propagation";
        public const string AclCapPermissions = "permissions";
        public const string AclCapPermissionMapping = "permissionMapping";

        public static readonly HashSet<string> AclCapKeys = new HashSet<string>()
        {
            AclCapSupportedPermissions,
            AclCapAclPropagation,
            AclCapPermissions,
            AclCapPermissionMapping
        };

        public const string AclCapPermissionPermission = "permission";
        public const string AclCAPPermissionDescription = "description";

        public static readonly HashSet<string> AclCapPermissionKeys = new HashSet<string>()
        {
            AclCapPermissionPermission,
            AclCAPPermissionDescription
        };

        public const string AclCapMappingKey = "key";
        public const string AclCapMappingPermission = "permission";

        public static readonly HashSet<string> AclCapMappingKeys = new HashSet<string>()
        {
            AclCapMappingKey,
            AclCapMappingPermission
        };

        //  ace
        public const string AcePrincipal = "principal";
        public const string AcePrincipalId = "principalId";
        public const string AcePermissions = "permissions";
        public const string AceIsDirect = "isDirect";

        public static readonly HashSet<string> AceKeys = new HashSet<string>()
        {
            AcePrincipal,
            AcePrincipalId,
            AcePermissions,
            AceIsDirect
        };

        public static readonly HashSet<string> AcePrincipalKeys = new HashSet<string>()
        {
            AcePrincipalId
        };

        //  failed to delete
        public const string FailedToDeleteId = "ids";

        public static readonly HashSet<string> FailedToDeleteKeys = new HashSet<string>()
        {
            FailedToDeleteId
        };

        //  rendition
        public const string RenditionStreamId = "streamId";
        public const string RenditionMimeType = "mimeType";
        public const string RenditionLength = "length";
        public const string RenditionKind = "kind";
        public const string RenditionTitle = "title";
        public const string RenditionHeight = "height";
        public const string RenditionWidth = "width";
        public const string RenditionDocumentId = "renditionDocumentId";

        public static readonly HashSet<string> RenditionKeys = new HashSet<string>()
        {
            RenditionStreamId,
            RenditionMimeType,
            RenditionLength,
            RenditionKind,
            RenditionTitle,
            RenditionHeight,
            RenditionWidth,
            RenditionDocumentId
        };
    }
}
