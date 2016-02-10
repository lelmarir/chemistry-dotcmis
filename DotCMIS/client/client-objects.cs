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
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DotCMIS.Binding;
using DotCMIS.Binding.Services;
using DotCMIS.Data;
using DotCMIS.Data.Extensions;
using DotCMIS.Data.Impl;
using DotCMIS.Enums;
using DotCMIS.Exceptions;

namespace DotCMIS.Client.Impl
{
    /// <summary>
    /// CMIS object base class.
    /// </summary>
    public abstract class AbstractCmisObject : ICmisObject
    {
        protected ISession Session { get; private set; }
        protected string RepositoryId { get { return Session.RepositoryInfo.Id; } }
        protected ICmisBinding Binding { get { return Session.Binding; } }

        private IObjectType objectType;
        public virtual IObjectType ObjectType
        {
            get
            {
                lock (objectLock)
                {
                    return objectType;
                }
            }
        }

        public virtual IList<ISecondaryType> SecondaryTypes
        {
            get
            {
                lock(objectLock)
                {
                    return secondaryTypes;
                }
            }
        }

        protected virtual string ObjectId
        {
            get
            {
                string objectId = Id;
                if (objectId == null)
                {
                    throw new CmisRuntimeException("Object Id is unknown!");
                }

                return objectId;
            }
        }

        protected IOperationContext CreationContext { get; private set; }

        private IDictionary<string, IProperty> properties;
        private IAllowableActions allowableActions;
        private IList<IRendition> renditions;
        private IAcl acl;
        private IList<IPolicy> policies;
        private IList<IRelationship> relationships;
        private IDictionary<ExtensionLevel, IList<ICmisExtensionElement>> extensions;
        private IList<ISecondaryType> secondaryTypes;
        protected object objectLock = new object();

        protected virtual void Initialize(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            if (objectType.PropertyDefinitions == null || objectType.PropertyDefinitions.Count < 9)
            {
                // there must be at least the 9 standard properties that all objects have
                throw new ArgumentException("Object type must have property definitions!");
            }

            this.Session = session;
            this.objectType = objectType;
            this.extensions = new Dictionary<ExtensionLevel, IList<ICmisExtensionElement>>();
            this.CreationContext = new OperationContext(context);
            this.RefreshTimestamp = DateTime.UtcNow;

            IObjectFactory of = Session.ObjectFactory;

            if (objectData != null)
            {
                // handle properties
                if (objectData.Properties != null)
                {
                    // search secondaryObjectTypes
                    foreach(IPropertyData property in objectData.Properties.PropertyList)
                    {
                        if(property.Id == PropertyIds.SecondaryObjectTypeIds)
                        {
                            IList<object> stids = property.Values as IList<object>;
                            if(stids != null && stids.Count > 0)
                            {
                                secondaryTypes = new List<ISecondaryType>();
                                foreach(object stid in stids) {
                                    IObjectType type = Session.GetTypeDefinition(stid.ToString());
                                    if( type is ISecondaryType)
                                        secondaryTypes.Add(type as ISecondaryType);
                                }
                            }
                            break;
                        }
                    }

                    properties = of.ConvertProperties(objectType, secondaryTypes, objectData.Properties);
                    extensions[ExtensionLevel.Properties] = objectData.Properties.Extensions;
                }

                // handle allowable actions
                if (objectData.AllowableActions != null)
                {
                    allowableActions = objectData.AllowableActions;
                    extensions[ExtensionLevel.AllowableActions] = objectData.AllowableActions.Extensions;
                }

                // handle renditions
                if (objectData.Renditions != null)
                {
                    renditions = new List<IRendition>();
                    foreach (IRenditionData rd in objectData.Renditions)
                    {
                        renditions.Add(of.ConvertRendition(Id, rd));
                    }
                }

                // handle ACL
                if (objectData.Acl != null)
                {
                    acl = objectData.Acl;
                    extensions[ExtensionLevel.Acl] = objectData.Acl.Extensions;
                }

                // handle policies
                if (objectData.PolicyIds != null && objectData.PolicyIds.PolicyIds != null)
                {
                    policies = new List<IPolicy>();
                    foreach (string pid in objectData.PolicyIds.PolicyIds)
                    {
                        IPolicy policy = Session.GetObject(Session.CreateObjectId(pid)) as IPolicy;
                        if (policy != null)
                        {
                            policies.Add(policy);
                        }
                    }
                    extensions[ExtensionLevel.Policies] = objectData.PolicyIds.Extensions;
                }

                // handle relationships
                if (objectData.Relationships != null)
                {
                    relationships = new List<IRelationship>();
                    foreach (IObjectData rod in objectData.Relationships)
                    {
                        IRelationship relationship = of.ConvertObject(rod, CreationContext) as IRelationship;
                        if (relationship != null)
                        {
                            relationships.Add(relationship);
                        }
                    }
                }

                extensions[ExtensionLevel.Object] = objectData.Extensions;
            }
        }

        protected virtual string GetPropertyQueryName(string propertyId)
        {
            lock (objectLock)
            {
                IPropertyDefinition propDef = objectType[propertyId];
                if (propDef == null)
                {
                    return null;
                }

                return propDef.QueryName;
            }
        }

        // --- object ---

        public virtual void Delete(bool allVersions)
        {
            lock (objectLock)
            {
                Session.Delete(this, allVersions);
            }
        }

        public virtual ICmisObject UpdateProperties(IDictionary<string, object> properties)
        {
            IObjectId objectId = UpdateProperties(properties, true);
            if (objectId == null)
            {
                return null;
            }

            if (ObjectId != objectId.Id)
            {
                return Session.GetObject(objectId, CreationContext);
            }

            return this;
        }

        public virtual IObjectId UpdateProperties(IDictionary<String, object> properties, bool refresh)
        {
            if (properties == null || properties.Count == 0)
            {
                throw new ArgumentException("Properties must not be empty!");
            }

            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                string changeToken = ChangeToken;

                HashSet<Updatability> updatebility = new HashSet<Updatability>();
                updatebility.Add(Updatability.ReadWrite);

                // check if checked out
                bool? isCheckedOut = GetPropertyValue(PropertyIds.IsVersionSeriesCheckedOut) as bool?;
                if (isCheckedOut.HasValue && isCheckedOut.Value)
                {
                    updatebility.Add(Updatability.WhenCheckedOut);
                }

                // it's time to update
                Binding.GetObjectService().UpdateProperties(RepositoryId, ref objectId, ref changeToken,
                        Session.ObjectFactory.ConvertProperties(properties, this.objectType, this.secondaryTypes, updatebility), null);

                newObjectId = objectId;
            }

            if (refresh)
            {
                Refresh();
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        public virtual ICmisObject Rename(string newName)
        {
            if (newName == null || newName.Length == 0)
            {
                throw new ArgumentException("New name must not be empty!");
            }

            IDictionary<string, object> prop = new Dictionary<string, object>();
            prop[PropertyIds.Name] = newName;

            return UpdateProperties(prop);
        }

        public virtual IObjectId Rename(string newName, bool refresh)
        {
            IDictionary<string, object> prop = new Dictionary<string, object>();
            prop[PropertyIds.Name] = newName;

            return UpdateProperties(prop, refresh);
        }

        // --- properties ---

        public virtual IObjectType BaseType { get { return Session.GetTypeDefinition(GetPropertyValue(PropertyIds.BaseTypeId) as string); } }

        public virtual BaseTypeId BaseTypeId
        {
            get
            {
                string baseType = GetPropertyValue(PropertyIds.BaseTypeId) as string;
                if (baseType == null) { throw new CmisRuntimeException("Base type not set!"); }

                return baseType.GetCmisEnum<BaseTypeId>();
            }
        }

        public virtual string Id { get { return GetPropertyValue(PropertyIds.ObjectId) as string; } }

        public virtual string Name { get { return GetPropertyValue(PropertyIds.Name) as string; } }

        public virtual string CreatedBy { get { return GetPropertyValue(PropertyIds.CreatedBy) as string; } }

        public virtual DateTime? CreationDate { get { return GetPropertyValue(PropertyIds.CreationDate) as DateTime?; } }

        public virtual string LastModifiedBy { get { return GetPropertyValue(PropertyIds.LastModifiedBy) as string; } }

        public virtual DateTime? LastModificationDate { get { return GetPropertyValue(PropertyIds.LastModificationDate) as DateTime?; } }

        public virtual string ChangeToken { get { return GetPropertyValue(PropertyIds.ChangeToken) as string; } }

        public virtual IList<IProperty> Properties
        {
            get
            {
                lock (objectLock)
                {
                    return new List<IProperty>(properties.Values);
                }
            }
        }

        public virtual IProperty this[string propertyId]
        {
            get
            {
                if (propertyId == null)
                {
                    throw new ArgumentNullException("propertyId");
                }

                lock (objectLock)
                {
                    IProperty property;
                    if (properties.TryGetValue(propertyId, out property))
                    {
                        return property;
                    }
                    return null;
                }
            }
        }

        public virtual object GetPropertyValue(string propertyId)
        {
            IProperty property = this[propertyId];
            if (property == null) { return null; }

            return property.Value;
        }

        // --- allowable actions ---

        public virtual IAllowableActions AllowableActions
        {
            get
            {
                lock (objectLock)
                {
                    return allowableActions;
                }
            }
        }

        // --- renditions ---

        public virtual IList<IRendition> Renditions
        {
            get
            {
                lock (objectLock)
                {
                    return renditions;
                }
            }
        }

        // --- ACL ---

        public virtual IAcl getAcl(bool onlyBasicPermissions)
        {
            return Binding.GetAclService().GetAcl(RepositoryId, ObjectId, onlyBasicPermissions, null);
        }

        public virtual IAcl ApplyAcl(IList<IAce> addAces, IList<IAce> removeAces, AclPropagation? aclPropagation)
        {
            IAcl result = Session.ApplyAcl(this, addAces, removeAces, aclPropagation);

            Refresh();

            return result;
        }

        public virtual IAcl AddAcl(IList<IAce> addAces, AclPropagation? aclPropagation)
        {
            return ApplyAcl(addAces, null, aclPropagation);
        }

        public virtual IAcl RemoveAcl(IList<IAce> removeAces, AclPropagation? aclPropagation)
        {
            return ApplyAcl(null, removeAces, aclPropagation);
        }

        public virtual IAcl Acl
        {
            get
            {
                lock (objectLock)
                {
                    return acl;
                }
            }
        }

        // --- policies ---

        public virtual void ApplyPolicy(params IObjectId[] policyId)
        {
            lock (objectLock)
            {
                Session.ApplyPolicy(this, policyId);
            }

            Refresh();
        }

        public virtual void RemovePolicy(params IObjectId[] policyId)
        {
            lock (objectLock)
            {
                Session.RemovePolicy(this, policyId);
            }

            Refresh();
        }

        public virtual IList<IPolicy> Policies
        {
            get
            {
                lock (objectLock)
                {
                    return policies;
                }
            }
        }

        // --- relationships ---

        public virtual IList<IRelationship> Relationships
        {
            get
            {
                lock (objectLock)
                {
                    return relationships;
                }
            }
        }

        // --- extensions ---

        public virtual IList<ICmisExtensionElement> GetExtensions(ExtensionLevel level)
        {
            IList<ICmisExtensionElement> ext;
            if (extensions.TryGetValue(level, out ext))
            {
                return ext;
            }

            return null;
        }

        // --- other ---

        public virtual DateTime RefreshTimestamp { get; private set; }

        public void Refresh()
        {
            lock (objectLock)
            {
                IOperationContext oc = CreationContext;

                // get the latest data from the repository
                IObjectData objectData = Binding.GetObjectService().GetObject(RepositoryId, ObjectId, oc.FilterString, oc.IncludeAllowableActions,
                    oc.IncludeRelationships, oc.RenditionFilterString, oc.IncludePolicies, oc.IncludeAcls, null);

                // reset this object
                Initialize(Session, ObjectType, objectData, CreationContext);
            }
        }

        public virtual void RefreshIfOld(long durationInMillis)
        {
            lock (objectLock)
            {
                if (((DateTime.UtcNow - RefreshTimestamp).Ticks / 10000) > durationInMillis)
                {
                    Refresh();
                }
            }
        }
    }

    /// <summary>
    /// Fileable object base class.
    /// </summary>
    public abstract class AbstractFileableCmisObject : AbstractCmisObject, IFileableCmisObject
    {
        public virtual IFileableCmisObject Move(IObjectId sourceFolderId, IObjectId targetFolderId)
        {
            string objectId = ObjectId;

            if (sourceFolderId == null || sourceFolderId.Id == null)
            {
                throw new ArgumentException("Source folder id must be set!");
            }

            if (targetFolderId == null || targetFolderId.Id == null)
            {
                throw new ArgumentException("Target folder id must be set!");
            }

            Binding.GetObjectService().MoveObject(RepositoryId, ref objectId, targetFolderId.Id, sourceFolderId.Id, null);

            if (objectId == null)
            {
                return null;
            }

            IFileableCmisObject movedObject = Session.GetObject(Session.CreateObjectId(objectId)) as IFileableCmisObject;
            if (movedObject == null)
            {
                throw new CmisRuntimeException("Moved object is invalid!");
            }

            return movedObject;
        }

        public virtual IList<IFolder> Parents
        {
            get
            {
                // get object ids of the parent folders
                IList<IObjectParentData> bindingParents = Binding.GetNavigationService().GetObjectParents(RepositoryId, ObjectId,
                    GetPropertyQueryName(PropertyIds.ObjectId), false, IncludeRelationshipsFlag.None, null, false, null);

                IList<IFolder> parents = new List<IFolder>();

                foreach (IObjectParentData p in bindingParents)
                {
                    if (p == null || p.Object == null || p.Object.Properties == null)
                    {
                        // should not happen...
                        throw new CmisRuntimeException("Repository sent invalid data!");
                    }

                    // get id property
                    IPropertyData idProperty = p.Object.Properties[PropertyIds.ObjectId];
                    if (idProperty == null || idProperty.PropertyType != PropertyType.Id)
                    {
                        // the repository sent an object without a valid object id...
                        throw new CmisRuntimeException("Repository sent invalid data! No object id!");
                    }

                    // fetch the object and make sure it is a folder
                    IObjectId parentId = Session.CreateObjectId(idProperty.FirstValue as string);
                    IFolder parentFolder = Session.GetObject(parentId) as IFolder;
                    if (parentFolder == null)
                    {
                        // the repository sent an object that is not a folder...
                        throw new CmisRuntimeException("Repository sent invalid data! Object is not a folder!");
                    }

                    parents.Add(parentFolder);
                }

                return parents;
            }
        }

        public virtual IList<string> Paths
        {
            get
            {
                // get object paths of the parent folders
                IList<IObjectParentData> parents = Binding.GetNavigationService().GetObjectParents(
                        RepositoryId, ObjectId, GetPropertyQueryName(PropertyIds.Path), false, IncludeRelationshipsFlag.None,
                        null, true, null);

                IList<string> paths = new List<string>();

                foreach (IObjectParentData p in parents)
                {
                    if (p == null || p.Object == null || p.Object.Properties == null)
                    {
                        // should not happen...
                        throw new CmisRuntimeException("Repository sent invalid data!");
                    }

                    // get path property
                    IPropertyData pathProperty = p.Object.Properties[PropertyIds.Path];
                    if (pathProperty == null || pathProperty.PropertyType != PropertyType.String)
                    {
                        // the repository sent a folder without a valid path...
                        throw new CmisRuntimeException("Repository sent invalid data! No path property!");
                    }

                    if (p.RelativePathSegment == null)
                    {
                        // the repository didn't send a relative path segment
                        throw new CmisRuntimeException("Repository sent invalid data! No relative path segement!");
                    }

                    string folderPath = pathProperty.FirstValue as string;
                    paths.Add(folderPath + (folderPath.EndsWith("/") ? "" : "/") + p.RelativePathSegment);
                }

                return paths;
            }
        }

        public virtual void AddToFolder(IObjectId folderId, bool allVersions)
        {
            if (folderId == null || folderId.Id == null)
            {
                throw new ArgumentException("Folder Id must be set!");
            }

            Binding.GetMultiFilingService().AddObjectToFolder(RepositoryId, ObjectId, folderId.Id, allVersions, null);
        }

        public virtual void RemoveFromFolder(IObjectId folderId)
        {
            Binding.GetMultiFilingService().RemoveObjectFromFolder(RepositoryId, ObjectId, folderId == null ? null : folderId.Id, null);
        }
    }

    /// <summary>
    /// Document implemetation.
    /// </summary>
    public class Document : AbstractFileableCmisObject, IDocument
    {
        public Document(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }

        // properties

        public virtual bool? IsImmutable { get { return GetPropertyValue(PropertyIds.IsImmutable) as bool?; } }

        public virtual bool? IsLatestVersion { get { return GetPropertyValue(PropertyIds.IsLatestVersion) as bool?; } }

        public virtual bool? IsMajorVersion { get { return GetPropertyValue(PropertyIds.IsMajorVersion) as bool?; } }

        public virtual bool? IsLatestMajorVersion { get { return GetPropertyValue(PropertyIds.IsLatestMajorVersion) as bool?; } }

        public virtual bool? IsPrivateWorkingCopy { get { return GetPropertyValue(PropertyIds.IsPrivateWorkingCopy) as bool?; } }

        public virtual string VersionLabel { get { return GetPropertyValue(PropertyIds.VersionLabel) as string; } }

        public virtual string VersionSeriesId { get { return GetPropertyValue(PropertyIds.VersionSeriesId) as string; } }

        public virtual bool? IsVersionSeriesCheckedOut { get { return GetPropertyValue(PropertyIds.IsVersionSeriesCheckedOut) as bool?; } }

        public virtual string VersionSeriesCheckedOutBy { get { return GetPropertyValue(PropertyIds.VersionSeriesCheckedOutBy) as string; } }

        public virtual string VersionSeriesCheckedOutId { get { return GetPropertyValue(PropertyIds.VersionSeriesCheckedOutId) as string; } }

        public virtual string CheckinComment { get { return GetPropertyValue(PropertyIds.CheckinComment) as string; } }

        public virtual long? ContentStreamLength { get { return GetPropertyValue(PropertyIds.ContentStreamLength) as long?; } }

        public virtual string ContentStreamMimeType { get { return GetPropertyValue(PropertyIds.ContentStreamMimeType) as string; } }

        public virtual string ContentStreamFileName { get { return GetPropertyValue(PropertyIds.ContentStreamFileName) as string; } }

        public virtual string ContentStreamId { get { return GetPropertyValue(PropertyIds.ContentStreamId) as string; } }

        // operations

        public virtual IDocument Copy(IObjectId targetFolderId, IDictionary<string, object> properties, VersioningState? versioningState,
                IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {

            IObjectId newId = Session.CreateDocumentFromSource(this, properties, targetFolderId, versioningState, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }
            // get the new object
            IDocument newDoc = Session.GetObject(newId, context) as IDocument;
            if (newDoc == null)
            {
                throw new CmisRuntimeException("Newly created object is not a document! New id: " + newId);
            }

            return newDoc;
        }

        public virtual IDocument Copy(IObjectId targetFolderId)
        {
            return Copy(targetFolderId, null, null, null, null, null, Session.DefaultContext);
        }

        public virtual void DeleteAllVersions()
        {
            Delete(true);
        }

        // versioning

        public virtual IObjectId CheckOut()
        {
            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                bool? contentCopied;

                Binding.GetVersioningService().CheckOut(RepositoryId, ref objectId, null, out contentCopied);
                newObjectId = objectId;
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        public virtual void CancelCheckOut()
        {
            Binding.GetVersioningService().CancelCheckOut(RepositoryId, ObjectId, null);
        }

        public virtual IObjectId CheckIn(bool major, IDictionary<string, object> properties, IContentStream contentStream,
                string checkinComment, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces)
        {
            String newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;

                IObjectFactory of = Session.ObjectFactory;

                HashSet<Updatability> updatebility = new HashSet<Updatability>();
                updatebility.Add(Updatability.ReadWrite);
                updatebility.Add(Updatability.WhenCheckedOut);

                Binding.GetVersioningService().CheckIn(RepositoryId, ref objectId, major, of.ConvertProperties(properties, ObjectType, SecondaryTypes ,updatebility),
                    contentStream, checkinComment, of.ConvertPolicies(policies), of.ConvertAces(addAces), of.ConvertAces(removeAces), null);

                newObjectId = objectId;
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);

        }

        public virtual IList<IDocument> GetAllVersions()
        {
            return GetAllVersions(Session.DefaultContext);
        }

        public virtual IList<IDocument> GetAllVersions(IOperationContext context)
        {
            string objectId;
            string versionSeriesId;

            lock (objectLock)
            {
                objectId = ObjectId;
                versionSeriesId = VersionSeriesId;
            }

            IList<IObjectData> versions = Binding.GetVersioningService().GetAllVersions(RepositoryId, objectId, versionSeriesId,
                context.FilterString, context.IncludeAllowableActions, null);

            IObjectFactory of = Session.ObjectFactory;

            IList<IDocument> result = new List<IDocument>();
            if (versions != null)
            {
                foreach (IObjectData objectData in versions)
                {
                    IDocument doc = of.ConvertObject(objectData, context) as IDocument;
                    if (doc == null)
                    {
                        // should not happen...
                        continue;
                    }

                    result.Add(doc);
                }
            }

            return result;
        }

        public virtual IDocument GetObjectOfLatestVersion(bool major)
        {
            return GetObjectOfLatestVersion(major, Session.DefaultContext);
        }

        public virtual IDocument GetObjectOfLatestVersion(bool major, IOperationContext context)
        {
            return Session.GetLatestDocumentVersion(this, major, context);
        }

        // content operations

        public virtual IContentStream GetContentStream()
        {
            return GetContentStream(null);
        }

        public virtual IContentStream GetContentStream(string streamId)
        {
            return GetContentStream(streamId, null, null);
        }

        public virtual IContentStream GetContentStream(string streamId, long? offset, long? length)
        {
            IContentStream contentStream = Session.GetContentStream(this, streamId, offset, length);
            if (contentStream == null)
            {
                // no content stream
                return null;
            }

            // the AtomPub binding doesn't return a file name
            // -> get the file name from properties, if present
            if (contentStream.FileName == null && ContentStreamFileName != null)
            {
                ContentStream newContentStream = new ContentStream();
                newContentStream.FileName = ContentStreamFileName;
                newContentStream.Length = contentStream.Length;
                newContentStream.MimeType = contentStream.MimeType;
                newContentStream.Stream = contentStream.Stream;
                newContentStream.Extensions = contentStream.Extensions;

                contentStream = newContentStream;
            }

            return contentStream;
        }

        public virtual IDocument SetContentStream(IContentStream contentStream, bool overwrite)
        {
            IObjectId objectId = SetContentStream(contentStream, overwrite, true);
            if (objectId == null)
            {
                return null;
            }

            if (ObjectId != objectId.Id)
            {
                return (IDocument)Session.GetObject(objectId, CreationContext);
            }

            return this;
        }

        public virtual IObjectId SetContentStream(IContentStream contentStream, bool overwrite, bool refresh)
        {
            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                string changeToken = ChangeToken;

                Binding.GetObjectService().SetContentStream(RepositoryId, ref objectId, overwrite, ref changeToken, contentStream, null);

                newObjectId = objectId;
            }

            if (refresh)
            {
                Refresh();
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        public virtual IDocument AppendContentStream(IContentStream contentStream, bool isLastTrunk)
        {
            IObjectId objectId = AppendContentStream(contentStream, isLastTrunk, true);
            if (objectId == null)
            {
                return null;
            }

            if (ObjectId != objectId.Id)
            {
                IDocument newDoc = Session.GetObject(objectId, CreationContext) as IDocument;
                newDoc.Refresh();
                return newDoc;
            }

            return this;
        }

        public IObjectId AppendContentStream(IContentStream contentStream, bool isLastTrunk, bool refresh)
        {
            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                string changeToken = ChangeToken;

                Binding.GetObjectService().AppendContentStream(RepositoryId, ref objectId, isLastTrunk, ref changeToken, contentStream, null);

                newObjectId = objectId;
            }

            if (!(newObjectId != null && this.ObjectId != newObjectId) && refresh) {
                Refresh();
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        public virtual IDocument DeleteContentStream()
        {
            IObjectId objectId = DeleteContentStream(true);
            if (objectId == null)
            {
                return null;
            }

            if (ObjectId != objectId.Id)
            {
                return (IDocument)Session.GetObject(objectId, CreationContext);
            }

            return this;
        }

        public virtual IObjectId DeleteContentStream(bool refresh)
        {
            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                string changeToken = ChangeToken;

                Binding.GetObjectService().DeleteContentStream(RepositoryId, ref objectId, ref changeToken, null);

                newObjectId = objectId;
            }

            if (refresh)
            {
                Refresh();
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        public virtual IObjectId CheckIn(bool major, IDictionary<String, object> properties, IContentStream contentStream, string checkinComment)
        {
            return this.CheckIn(major, properties, contentStream, checkinComment, null, null, null);
        }
    }

    /// <summary>
    /// Folder implemetation.
    /// </summary>
    public class Folder : AbstractFileableCmisObject, IFolder
    {
        public Folder(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }

        public virtual IDocument CreateDocument(IDictionary<string, object> properties, IContentStream contentStream, VersioningState? versioningState,
            IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId = Session.CreateDocument(properties, this, contentStream, versioningState, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }

            // get the new object
            IDocument newDoc = Session.GetObject(newId, context) as IDocument;
            if (newDoc == null)
            {
                throw new CmisRuntimeException("Newly created object is not a document! New id: " + newId);
            }

            return newDoc;
        }

        public virtual IDocument CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, VersioningState? versioningState,
            IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId = Session.CreateDocumentFromSource(source, properties, this, versioningState, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }

            // get the new object
            IDocument newDoc = Session.GetObject(newId, context) as IDocument;
            if (newDoc == null)
            {
                throw new CmisRuntimeException("Newly created object is not a document! New id: " + newId);
            }

            return newDoc;
        }

        public virtual IFolder CreateFolder(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId = Session.CreateFolder(properties, this, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }

            // get the new object
            IFolder newFolder = Session.GetObject(newId, context) as IFolder;
            if (newFolder == null)
            {
                throw new CmisRuntimeException("Newly created object is not a folder! New id: " + newId);
            }

            return newFolder;
        }

        public virtual IPolicy CreatePolicy(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId = Session.CreatePolicy(properties, this, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }

            // get the new object
            IPolicy newPolicy = Session.GetObject(newId, context) as IPolicy;
            if (newPolicy == null)
            {
                throw new CmisRuntimeException("Newly created object is not a policy! New id: " + newId);
            }

            return newPolicy;
        }

        public virtual IList<string> DeleteTree(bool allVersions, UnfileObject? unfile, bool continueOnFailure)
        {
            IFailedToDeleteData failed = Binding.GetObjectService().DeleteTree(RepositoryId, ObjectId, allVersions, unfile, continueOnFailure, null);
            return failed.Ids;
        }

        public virtual string ParentId { get { return GetPropertyValue(PropertyIds.ParentId) as string; } }

        public virtual IList<IObjectType> AllowedChildObjectTypes
        {
            get
            {
                IList<IObjectType> result = new List<IObjectType>();

                lock (objectLock)
                {
                    IList<string> otids = GetPropertyValue(PropertyIds.AllowedChildObjectTypeIds) as IList<string>;
                    if (otids == null)
                    {
                        return result;
                    }

                    foreach (string otid in otids)
                    {
                        result.Add(Session.GetTypeDefinition(otid));
                    }
                }

                return result;
            }
        }

        public virtual IItemEnumerable<IDocument> GetCheckedOutDocs()
        {
            return GetCheckedOutDocs(Session.DefaultContext);
        }

        public virtual IItemEnumerable<IDocument> GetCheckedOutDocs(IOperationContext context)
        {
            string objectId = ObjectId;
            INavigationService service = Binding.GetNavigationService();
            IObjectFactory of = Session.ObjectFactory;
            IOperationContext ctxt = new OperationContext(context);

            PageFetcher<IDocument>.FetchPage fetchPageDelegate = delegate(long maxNumItems, long skipCount)
            {
                // get checked out documents for this folder
                IObjectList checkedOutDocs = service.GetCheckedOutDocs(RepositoryId, objectId, ctxt.FilterString, ctxt.OrderBy, ctxt.IncludeAllowableActions,
                    ctxt.IncludeRelationships, ctxt.RenditionFilterString, maxNumItems, skipCount, null);

                IList<IDocument> page = new List<IDocument>();
                if (checkedOutDocs.Objects != null)
                {
                    foreach (IObjectData objectData in checkedOutDocs.Objects)
                    {
                        IDocument doc = of.ConvertObject(objectData, ctxt) as IDocument;
                        if (doc == null)
                        {
                            // should not happen...
                            continue;
                        }

                        page.Add(doc);
                    }
                }


                return new PageFetcher<IDocument>.Page<IDocument>(page, checkedOutDocs.NumItems, checkedOutDocs.HasMoreItems);
            };

            return new CollectionEnumerable<IDocument>(new PageFetcher<IDocument>(ctxt.MaxItemsPerPage, fetchPageDelegate));
        }

        public virtual IItemEnumerable<ICmisObject> GetChildren()
        {
            return GetChildren(Session.DefaultContext);
        }

        public virtual IItemEnumerable<ICmisObject> GetChildren(IOperationContext context)
        {
            string objectId = ObjectId;
            INavigationService service = Binding.GetNavigationService();
            IObjectFactory of = Session.ObjectFactory;
            IOperationContext ctxt = new OperationContext(context);

            PageFetcher<ICmisObject>.FetchPage fetchPageDelegate = delegate(long maxNumItems, long skipCount)
            {
                // get the children
                IObjectInFolderList children = service.GetChildren(RepositoryId, objectId, ctxt.FilterString, ctxt.OrderBy, ctxt.IncludeAllowableActions,
                    ctxt.IncludeRelationships, ctxt.RenditionFilterString, ctxt.IncludePathSegments, maxNumItems, skipCount, null);

                // convert objects
                IList<ICmisObject> page = new List<ICmisObject>();
                if (children.Objects != null)
                {
                    foreach (IObjectInFolderData objectData in children.Objects)
                    {
                        if (objectData.Object != null)
                        {
                            page.Add(of.ConvertObject(objectData.Object, ctxt));
                        }
                    }
                }

                return new PageFetcher<ICmisObject>.Page<ICmisObject>(page, children.NumItems, children.HasMoreItems);
            };

            return new CollectionEnumerable<ICmisObject>(new PageFetcher<ICmisObject>(ctxt.MaxItemsPerPage, fetchPageDelegate));
        }

        public virtual IList<ITree<IFileableCmisObject>> GetDescendants(int depth)
        {
            return GetDescendants(depth, Session.DefaultContext);
        }

        public virtual IList<ITree<IFileableCmisObject>> GetDescendants(int depth, IOperationContext context)
        {
            IList<IObjectInFolderContainer> bindingContainerList = Binding.GetNavigationService().GetDescendants(RepositoryId, ObjectId, depth,
                context.FilterString, context.IncludeAllowableActions, context.IncludeRelationships, context.RenditionFilterString,
                context.IncludePathSegments, null);

            return ConvertProviderContainer(bindingContainerList, context);
        }

        public virtual IList<ITree<IFileableCmisObject>> GetFolderTree(int depth)
        {
            return GetFolderTree(depth, Session.DefaultContext);
        }

        public virtual IList<ITree<IFileableCmisObject>> GetFolderTree(int depth, IOperationContext context)
        {
            IList<IObjectInFolderContainer> bindingContainerList = Binding.GetNavigationService().GetFolderTree(RepositoryId, ObjectId, depth,
                context.FilterString, context.IncludeAllowableActions, context.IncludeRelationships, context.RenditionFilterString,
                context.IncludePathSegments, null);

            return ConvertProviderContainer(bindingContainerList, context);
        }

        private IList<ITree<IFileableCmisObject>> ConvertProviderContainer(IList<IObjectInFolderContainer> bindingContainerList, IOperationContext context)
        {
            if (bindingContainerList == null || bindingContainerList.Count == 0)
            {
                return null;
            }

            IList<ITree<IFileableCmisObject>> result = new List<ITree<IFileableCmisObject>>();
            foreach (IObjectInFolderContainer oifc in bindingContainerList)
            {
                if (oifc.Object == null || oifc.Object.Object == null)
                {
                    // shouldn't happen ...
                    continue;
                }

                // convert the object
                IFileableCmisObject cmisObject = Session.ObjectFactory.ConvertObject(oifc.Object.Object, context) as IFileableCmisObject;
                if (cmisObject == null)
                {
                    // the repository must not return objects that are not fileable, but you never know...
                    continue;
                }

                // convert the children
                IList<ITree<IFileableCmisObject>> children = ConvertProviderContainer(oifc.Children, context);

                // add both to current container
                Tree<IFileableCmisObject> tree = new Tree<IFileableCmisObject>();
                tree.Item = cmisObject;
                tree.Children = children;

                result.Add(tree);
            }

            return result;
        }

        public virtual bool IsRootFolder { get { return ObjectId == Session.RepositoryInfo.RootFolderId; } }

        public virtual IFolder FolderParent
        {
            get
            {
                if (IsRootFolder)
                {
                    return null;
                }

                IList<IFolder> parents = Parents;
                if (parents == null || parents.Count == 0)
                {
                    return null;
                }

                return parents[0];
            }
        }

        public virtual string Path
        {
            get
            {
                string path;

                lock (objectLock)
                {
                    // get the path property
                    path = GetPropertyValue(PropertyIds.Path) as string;

                    // if the path property isn't set, get it
                    if (path == null)
                    {
                        IObjectData objectData = Binding.GetObjectService().GetObject(RepositoryId, ObjectId,
                                GetPropertyQueryName(PropertyIds.Path), false, IncludeRelationshipsFlag.None, "cmis:none", false,
                                false, null);

                        if (objectData.Properties != null)
                        {
                            IPropertyData pathProperty = objectData.Properties[PropertyIds.Path];
                            if (pathProperty != null && pathProperty.PropertyType == PropertyType.String)
                            {
                                path = pathProperty.FirstValue as string;
                            }
                        }
                    }
                }

                // we still don't know the path ... it's not a CMIS compliant repository
                if (path == null)
                {
                    throw new CmisRuntimeException("Repository didn't return " + PropertyIds.Path + "!");
                }

                return path;
            }
        }

        public override IList<string> Paths
        {
            get
            {
                IList<string> result = new List<string>();
                result.Add(Path);

                return result;
            }
        }

        public virtual IDocument CreateDocument(IDictionary<string, object> properties, IContentStream contentStream, VersioningState? versioningState)
        {
            return CreateDocument(properties, contentStream, versioningState, null, null, null, Session.DefaultContext);
        }

        public virtual IDocument CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, VersioningState? versioningState)
        {
            return CreateDocumentFromSource(source, properties, versioningState, null, null, null, Session.DefaultContext);
        }

        public virtual IFolder CreateFolder(IDictionary<string, object> properties)
        {
            return CreateFolder(properties, null, null, null, Session.DefaultContext);
        }

        public virtual IPolicy CreatePolicy(IDictionary<string, object> properties)
        {
            return CreatePolicy(properties, null, null, null, Session.DefaultContext);
        }
    }

    /// <summary>
    /// Policy implemetation.
    /// </summary>
    public class Policy : AbstractFileableCmisObject, IPolicy
    {
        public Policy(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }

        public virtual string PolicyText { get { return GetPropertyValue(PropertyIds.PolicyText) as string; } }
    }

    /// <summary>
    /// Item implemetation.
    /// </summary>
    public class Item : AbstractFileableCmisObject
    {
        public Item(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }
    }

    /// <summary>
    /// Relationship implemetation.
    /// </summary>
    public class Relationship : AbstractCmisObject, IRelationship
    {

        public Relationship(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }

        public virtual ICmisObject GetSource()
        {
            return GetSource(Session.DefaultContext);
        }

        public virtual ICmisObject GetSource(IOperationContext context)
        {
            lock (objectLock)
            {
                IObjectId sourceId = SourceId;
                if (sourceId == null)
                {
                    return null;
                }

                return Session.GetObject(sourceId, context);
            }
        }

        public virtual IObjectId SourceId
        {
            get
            {
                string sourceId = GetPropertyValue(PropertyIds.SourceId) as string;
                if (sourceId == null || sourceId.Length == 0)
                {
                    return null;
                }

                return Session.CreateObjectId(sourceId);
            }
        }

        public virtual ICmisObject GetTarget()
        {
            return GetTarget(Session.DefaultContext);
        }

        public virtual ICmisObject GetTarget(IOperationContext context)
        {
            lock (objectLock)
            {
                IObjectId targetId = TargetId;
                if (targetId == null)
                {
                    return null;
                }

                return Session.GetObject(targetId, context);
            }
        }

        public virtual IObjectId TargetId
        {
            get
            {
                string targetId = GetPropertyValue(PropertyIds.TargetId) as string;
                if (targetId == null || targetId.Length == 0)
                {
                    return null;
                }

                return Session.CreateObjectId(targetId);
            }
        }
    }

    public class Property : IProperty
    {
        public Property(IPropertyDefinition propertyDefinition, IList<object> values)
        {
            PropertyDefinition = propertyDefinition;
            Values = values;
        }

        public virtual string Id { get { return PropertyDefinition.Id; } }

        public virtual string LocalName { get { return PropertyDefinition.LocalName; } }

        public virtual string DisplayName { get { return PropertyDefinition.DisplayName; } }

        public virtual string QueryName { get { return PropertyDefinition.QueryName; } }

        public virtual bool IsMultiValued { get { return PropertyDefinition.Cardinality == Cardinality.Multi; } }

        public virtual PropertyType? PropertyType { get { return PropertyDefinition.PropertyType; } }

        public virtual IPropertyDefinition PropertyDefinition { get; protected set; }

        public virtual object Value
        {
            get
            {
                if (PropertyDefinition.Cardinality == Cardinality.Single)
                {
                    return Values == null || Values.Count == 0 ? null : Values[0];
                }
                else
                {
                    return Values;
                }
            }
        }

        public virtual IList<object> Values { get; protected set; }

        public virtual object FirstValue { get { return Values == null || Values.Count == 0 ? null : Values[0]; } }

        public virtual string ValueAsString { get { return FormatValue(FirstValue); } }

        public virtual string ValuesAsString
        {
            get
            {
                if (Values == null)
                {
                    return "[]";
                }
                else
                {
                    StringBuilder result = new StringBuilder();
                    foreach (object value in Values)
                    {
                        if (result.Length > 0)
                        {
                            result.Append(", ");
                        }

                        result.Append(FormatValue(value));
                    }

                    return "[" + result.ToString() + "]";
                }
            }
        }

        private string FormatValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            // for future formating

            return value.ToString();
        }
    }

    public class Rendition : RenditionData, IRendition
    {
        private ISession session;
        private string objectId;

        public Rendition(ISession session, string objectId, string streamId, string mimeType, long? length, string kind,
            string title, long? height, long? width, string renditionDocumentId)
        {
            this.session = session;
            this.objectId = objectId;

            StreamId = streamId;
            MimeType = mimeType;
            Length = length;
            Kind = kind;
            Title = title;
            Height = height;
            Width = width;
            RenditionDocumentId = renditionDocumentId;
        }

        public virtual IDocument GetRenditionDocument()
        {
            return GetRenditionDocument(session.DefaultContext);
        }

        public virtual IDocument GetRenditionDocument(IOperationContext context)
        {
            if (RenditionDocumentId == null)
            {
                return null;
            }

            return session.GetObject(session.CreateObjectId(RenditionDocumentId), context) as IDocument;
        }

        public virtual IContentStream GetContentStream()
        {
            if (objectId == null || StreamId == null)
            {
                return null;
            }

            return session.Binding.GetObjectService().GetContentStream(session.RepositoryInfo.Id, objectId, StreamId, null, null, null);
        }
    }

    public class QueryResult : IQueryResult
    {
        private IDictionary<string, IPropertyData> propertiesById;
        private IDictionary<string, IPropertyData> propertiesByQueryName;

        public QueryResult(ISession session, IObjectData objectData)
        {
            if (objectData != null)
            {
                IObjectFactory of = session.ObjectFactory;

                // handle properties
                if (objectData.Properties != null)
                {
                    Properties = new List<IPropertyData>();
                    propertiesById = new Dictionary<string, IPropertyData>();
                    propertiesByQueryName = new Dictionary<string, IPropertyData>();

                    IList<IPropertyData> queryProperties = of.ConvertQueryProperties(objectData.Properties);

                    foreach (IPropertyData property in queryProperties)
                    {
                        Properties.Add(property);
                        if (property.Id != null)
                        {
                            propertiesById[property.Id] = property;
                        }
                        if (property.QueryName != null)
                        {
                            propertiesByQueryName[property.QueryName] = property;
                        }
                    }
                }

                // handle allowable actions
                AllowableActions = objectData.AllowableActions;

                // handle relationships
                if (objectData.Relationships != null)
                {
                    Relationships = new List<IRelationship>();
                    foreach (IObjectData rod in objectData.Relationships)
                    {
                        IRelationship relationship = of.ConvertObject(rod, session.DefaultContext) as IRelationship;
                        if (relationship != null)
                        {
                            Relationships.Add(relationship);
                        }
                    }
                }

                // handle renditions
                if (objectData.Renditions != null)
                {
                    Renditions = new List<IRendition>();
                    foreach (IRenditionData rd in objectData.Renditions)
                    {
                        Renditions.Add(of.ConvertRendition(null, rd));
                    }
                }
            }
        }

        public virtual IPropertyData this[string queryName]
        {
            get
            {
                if (queryName == null)
                {
                    return null;
                }

                IPropertyData result;
                if (propertiesByQueryName.TryGetValue(queryName, out result))
                {
                    return result;
                }

                return null;
            }
        }

        public virtual IList<IPropertyData> Properties { get; protected set; }

        public virtual IPropertyData GetPropertyById(string propertyId)
        {
            if (propertyId == null)
            {
                return null;
            }

            IPropertyData result;
            if (propertiesById.TryGetValue(propertyId, out result))
            {
                return result;
            }

            return null;
        }

        public virtual object GetPropertyValueByQueryName(string queryName)
        {
            IPropertyData property = this[queryName];
            if (property == null)
            {
                return null;
            }

            return property.FirstValue;
        }

        public virtual object GetPropertyValueById(string propertyId)
        {
            IPropertyData property = GetPropertyById(propertyId);
            if (property == null)
            {
                return null;
            }

            return property.FirstValue;
        }

        public virtual IList<object> GetPropertyMultivalueByQueryName(string queryName)
        {
            IPropertyData property = this[queryName];
            if (property == null)
            {
                return null;
            }

            return property.Values;
        }

        public virtual IList<object> GetPropertyMultivalueById(string propertyId)
        {
            IPropertyData property = GetPropertyById(propertyId);
            if (property == null)
            {
                return null;
            }

            return property.Values;
        }

        public virtual IAllowableActions AllowableActions { get; protected set; }

        public virtual IList<IRelationship> Relationships { get; protected set; }

        public virtual IList<IRendition> Renditions { get; protected set; }
    }

    public class ChangeEvent : ChangeEventInfo, IChangeEvent
    {
        public virtual string ObjectId { get; set; }

        public virtual IDictionary<string, IList<object>> Properties { get; set; }

        public virtual IList<string> PolicyIds { get; set; }

        public virtual IAcl Acl { get; set; }
    }

    public class ChangeEvents : IChangeEvents
    {
        public virtual string LatestChangeLogToken { get; set; }

        public virtual IList<IChangeEvent> ChangeEventList { get; set; }

        public virtual bool? HasMoreItems { get; set; }

        public virtual long? TotalNumItems { get; set; }
    }
}
