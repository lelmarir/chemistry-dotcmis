using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using DotCMIS.Exceptions;
using DotCMIS.Enums;
using DotCMIS.Data;
using DotCMIS.Data.Extensions;
using DotCMIS.Data.Impl;

using Newtonsoft.Json.Linq;


namespace DotCMIS.Binding.Browser
{
    internal class BrowserConverter
    {
        internal static IRepositoryInfo ConvertRepositoryInfo(JToken json, out string repositoryUrl, out string rootUrl)
        {
            repositoryUrl = (string)json[BrowserConstants.RepoInfoRepositoryUrl];
            rootUrl = (string)json[BrowserConstants.RepoInfoRootFolderUrl];

            RepositoryInfo result = new RepositoryInfo();
            result.Id = (string)json[BrowserConstants.RepoInfoId];
            result.Name = (string)json[BrowserConstants.RepoInfoName];
            result.Description = (string)json[BrowserConstants.RepoInfoDescription];
            result.VendorName = (string)json[BrowserConstants.RepoInfoVendor];
            result.ProductName = (string)json[BrowserConstants.RepoInfoProduct];
            result.ProductVersion = (string)json[BrowserConstants.RepoInfoProductVersion];
            result.RootFolderId = (string)json[BrowserConstants.RepoInfoRootFolderId];
            result.Capabilities = ConvertRepositoryCapabilities(json[BrowserConstants.RepoInfoCapabilities]);
            result.AclCapabilities = ConvertAclCapabilities(json[BrowserConstants.RepoInfoAclCapabilities]);
            result.LatestChangeLogToken = (string)json[BrowserConstants.RepoInfoChangeLogToken];
            result.CmisVersionSupported = (string)json[BrowserConstants.RepoInfoCmisVersionSupported];
            result.ThinClientUri = (string)json[BrowserConstants.RepoInfoThinClientUri];
            //  TODO ChangesIncomplete
            //result.ChangesIncomplete = (string)json[BrowserConstants.RepoInfoChangesIncomplete];
            //  TODO ChangesOnType
            //result.ChangesOnType = (string)json[BrowserConstants.RepoInfoChangesOnType];
            result.PrincipalIdAnonymous = (string)json[BrowserConstants.RepoInfoPrincipalIdAnonymous];
            result.PrincipalIdAnyone = (string)json[BrowserConstants.RepoInfoPrincipalIdAnyone];
            result.Extensions = null; //  TODO Extension

            return result;
        }

        internal static IRepositoryCapabilities ConvertRepositoryCapabilities(JToken json)
        {
            //  TODO ConvertRepositoryCapabilities
            return null;
        }

        internal static IAclCapabilities ConvertAclCapabilities(JToken json)
        {
            //  TODO ConvertAclCapabilities
            return null;
        }

        internal static ITypeDefinition ConvertTypeDefinition(JToken json)
        {
            AbstractTypeDefinition result = null;
            string id = (string)json[BrowserConstants.TypeId];
            string baseId = (string)json[BrowserConstants.TypeBaseId];
            BaseTypeId baseType = CmisValue.GetCmisEnum<BaseTypeId>(baseId);
            switch (baseType)
            {
                case BaseTypeId.CmisDocument:
                    result = new DocumentTypeDefinition();

                    ((DocumentTypeDefinition)result).IsVersionable = (bool?)json[BrowserConstants.TypeVersionable];

                    string contentStreamAllowed = (string)json[BrowserConstants.TypeContentStreamAllowed];
                    ((DocumentTypeDefinition)result).ContentStreamAllowed = (ContentStreamAllowed?)CmisValue.GetCmisEnum<ContentStreamAllowed>(contentStreamAllowed);

                    break;

                case BaseTypeId.CmisFolder:
                    result = new FolderTypeDefinition();
                    break;

                case BaseTypeId.CmisRelationship:
                    result = new RelationshipTypeDefinition();

                    object sourceTypes = json[BrowserConstants.TypeAllowedSourceTypes];
                    if (sourceTypes is IList<object>)
                    {
                        List<string> types = new List<string>();
                        foreach (object sourceType in sourceTypes as IList<object>)
                        {
                            types.Add(sourceType.ToString());
                        }
                        ((RelationshipTypeDefinition)result).AllowedSourceTypeIds = types;
                    }

                    object targetTypes = json[BrowserConstants.TypeAllowedTargetTypes];
                    if (targetTypes is IList<object>)
                    {
                        List<string> types = new List<string>();
                        foreach (object targetType in targetTypes as IList<object>)
                        {
                            types.Add(targetType.ToString());
                        }
                        ((RelationshipTypeDefinition)result).AllowedTargetTypeIds = types;
                    }

                    break;

                case BaseTypeId.CmisPolicy:
                    result = new PolicyTypeDefinition();
                    break;

                case BaseTypeId.CmisItem:
                    result = new ItemTypeDefinition();
                    break;

                case BaseTypeId.CmisSecondary:
                    result = new SecondaryTypeDefinition();
                    break;

                default:
                    throw new CmisRuntimeException("Type '" + baseId + "' does not match a base type!");
            }

            result.BaseTypeId = baseType;
            result.Description = (string)json[BrowserConstants.TypeDescription];
            result.DisplayName = (string)json[BrowserConstants.TypeDisplayName];
            result.Id = id;
            result.IsControllableAcl = (bool?)json[BrowserConstants.TypeControllableAcl];
            result.IsControllablePolicy = (bool?)json[BrowserConstants.TypeControllablePolicy];
            result.IsCreatable = (bool?)json[BrowserConstants.TypeCreatable];
            result.IsFileable = (bool?)json[BrowserConstants.TypeFileable];
            result.IsFulltextIndexed = (bool?)json[BrowserConstants.TypeFulltextIndexed];
            result.IsIncludedInSupertypeQuery = (bool?)json[BrowserConstants.TypeIncludeInSupertypeQuery];
            result.IsQueryable = (bool?)json[BrowserConstants.TypeQueryable];
            result.LocalName = (string)json[BrowserConstants.TypeLocalName];
            result.LocalNamespace = (string)json[BrowserConstants.TypeLocalNamespace];
            result.ParentTypeId = (string)json[BrowserConstants.TypeParentId];
            result.QueryName = (string)json[BrowserConstants.TypeQueryName];

            JObject jsonPropertyDefinitions = json[BrowserConstants.TypePropertyDefinitions] as JObject;
            if (jsonPropertyDefinitions != null)
            {
                foreach (JToken jsonPropertyDefinition in jsonPropertyDefinitions.PropertyValues())
                {
                    result.AddPropertyDefinition(ConvertPropertyDefinition(jsonPropertyDefinition));
                }
            }

            ConvertExtensionData(json, result, BrowserConstants.TypeKeys);

            return result;
        }

        internal static void ConvertExtensionData(JToken json, IExtensionsData target, HashSet<string> cmisKeys)
        {
            IList<ICmisExtensionElement> result = null;

            foreach (JProperty property in (json as JObject).Properties())
            {
                if (cmisKeys.Contains(property.Name))
                {
                    continue;
                }
                if (result == null)
                {
                    result = new List<ICmisExtensionElement>();
                }

                JObject jsonProperty = new JObject();
                jsonProperty.Add(property.Name, property.Value);
                foreach (ICmisExtensionElement extension in ConvertExtension(jsonProperty))
                {
                    result.Add(extension);
                }
            }

            target.Extensions = result;
        }

        private static IList<ICmisExtensionElement> ConvertExtension(JObject json)
        {
            if (json == null)
            {
                return null;
            }

            IList<ICmisExtensionElement> result = new List<ICmisExtensionElement>();
            foreach (JProperty property in json.Properties())
            {
                CmisExtensionElement extension = new CmisExtensionElement();
                extension.Name = property.Name;
                if (property.Value is JObject)
                {
                    extension.Children = ConvertExtension(property.Value as JObject);
                    result.Add(extension);
                }
                else if (property.Value is JArray)
                {
                    foreach (ICmisExtensionElement childExtension in ConvertExtension(property.Name, property.Value as JArray))
                    {
                        result.Add(childExtension);
                    }
                }
                else
                {
                    extension.Value = property.Value.ToString();
                    result.Add(extension);
                }
            }
            return result;
        }

        private static IList<ICmisExtensionElement> ConvertExtension(string name, JArray json)
        {
            if (json == null)
            {
                return null;
            }

            IList<ICmisExtensionElement> result = new List<ICmisExtensionElement>();
            foreach (JToken child in json.Children())
            {
                CmisExtensionElement extension = new CmisExtensionElement();
                extension.Name = name;
                if (child is JObject)
                {
                    extension.Children = ConvertExtension(child as JObject);
                    result.Add(extension);
                }
                else if (child is JArray)
                {
                    foreach (ICmisExtensionElement childExtension in ConvertExtension(name, child as JArray))
                    {
                        result.Add(childExtension);
                    }
                }
                else
                {
                    extension.Value = child.ToString();
                    result.Add(extension);
                }
            }
            return result;
        }

        internal static IPropertyDefinition ConvertPropertyDefinition(JToken json)
        {
            if (json == null)
            {
                return null;
            }

            string id = (string)json[BrowserConstants.PropertyTypeId];
            PropertyType propertyType = CmisValue.GetCmisEnum<PropertyType>((string)json[BrowserConstants.PropertyTypePropertyType]);
            Cardinality cardinality = CmisValue.GetCmisEnum<Cardinality>((string)json[BrowserConstants.PropertyTypeCardinality]);

            PropertyDefinition result = null;
            switch (propertyType)
            {
                case PropertyType.String:
                    result = new PropertyStringDefinition();
                    ((PropertyStringDefinition)result).MaxLength = (long?)json[BrowserConstants.PropertyTypeMaxLength];
                    ((PropertyStringDefinition)result).Choices = ConvertChoicesString(json[BrowserConstants.PropertyTypeChoice]);
                    break;
                case PropertyType.Id:
                    result = new PropertyIdDefinition();
                    ((PropertyIdDefinition)result).Choices = ConvertChoicesString(json[BrowserConstants.PropertyTypeChoice]);
                    break;
                case PropertyType.Boolean:
                    result = new PropertyBooleanDefinition();
                    ((PropertyBooleanDefinition)result).Choices = ConvertChoicesBoolean(json[BrowserConstants.PropertyTypeChoice]);
                    break;
                case PropertyType.Integer:
                    result = new PropertyIntegerDefinition();
                    ((PropertyIntegerDefinition)result).MaxValue = (long?)json[BrowserConstants.PropertyTypeMaxValue];
                    ((PropertyIntegerDefinition)result).MinValue = (long?)json[BrowserConstants.PropertyTypeMaxValue];
                    ((PropertyIntegerDefinition)result).Choices = ConvertChoicesLong(json[BrowserConstants.PropertyTypeChoice]);
                    break;
                case PropertyType.DateTime:
                    result = new PropertyDateTimeDefinition();
                    ((PropertyDateTimeDefinition)result).DateTimeResolution = CmisValue.GetCmisEnum<DateTimeResolution>((string)json[BrowserConstants.PropertyTypeResolution]);
                    ((PropertyDateTimeDefinition)result).Choices = ConvertChoicesDateTime(json[BrowserConstants.PropertyTypeChoice]);
                    break;
                case PropertyType.Decimal:
                    result = new PropertyDecimalDefinition();
                    ((PropertyDecimalDefinition)result).MaxValue = (decimal?)json[BrowserConstants.PropertyTypeMaxValue];
                    ((PropertyDecimalDefinition)result).MinValue = (decimal?)json[BrowserConstants.PropertyTypeMaxValue];
                    //  TODO Precision
                    //((PropertyDecimalDefinition)result).Precision = (DecimalPrecision?)json[BrowserConstants.PropertyTypePrecision];
                    ((PropertyDecimalDefinition)result).Choices = ConvertChoicesDecimal(json[BrowserConstants.PropertyTypeChoice]);
                    break;
                case PropertyType.Html:
                    result = new PropertyHtmlDefinition();
                    ((PropertyHtmlDefinition)result).Choices = ConvertChoicesString(json[BrowserConstants.PropertyTypeChoice]);
                    break;
                case PropertyType.Uri:
                    result = new PropertyUriDefinition();
                    ((PropertyUriDefinition)result).Choices = ConvertChoicesString(json[BrowserConstants.PropertyTypeChoice]);
                    break;
                default:
                    throw new CmisRuntimeException("Property type '" + id + "' does not match a data type!");
            }
          
            //  TODO default value

            //  generic
            result.Id = id;
            result.PropertyType = propertyType;
            result.Cardinality = cardinality;
            result.LocalName = (string)json[BrowserConstants.PropertyTypeLocalName];
            result.LocalNamespace = (string)json[BrowserConstants.PropertyTypeLocalNamespace];
            result.QueryName = (string)json[BrowserConstants.PropertyTypeQueryName];
            result.Description = (string)json[BrowserConstants.PropertyTypeDescription];
            result.DisplayName = (string)json[BrowserConstants.PropertyTypeDisplayName];
            result.IsInherited = (bool?)json[BrowserConstants.PropertyTypeInherited];
            result.IsOpenChoice = (bool?)json[BrowserConstants.PropertyTypeOpenChoice];
            result.IsOrderable = (bool?)json[BrowserConstants.PropertyTypeOrderable];
            result.IsQueryable = (bool?)json[BrowserConstants.PropertyTypeQueryable];
            result.IsRequired = (bool?)json[BrowserConstants.PropertyTypeRequired];
            result.Updatability = CmisValue.GetCmisEnum<Updatability>((string)json[BrowserConstants.PropertyTypeUpdatability]);

            //  TODO extensions

            return result;
        }

        internal static IObjectInFolderList ConvertObjectInFolderList(JToken json, ClientTypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            ObjectInFolderList result = new ObjectInFolderList();

            JArray jsonChildren = json[BrowserConstants.ObjectInFolderListObjects] as JArray;
            List<IObjectInFolderData> objects = new List<IObjectInFolderData>();
            if (jsonChildren != null)
            {
                foreach (JToken jsonChild in jsonChildren)
                {
                    objects.Add(ConvertObjectInFolder(jsonChild, typeCache));
                }
            }
            result.Objects = objects;

            result.HasMoreItems = (bool?)json[BrowserConstants.ObjectInFolderListHasMoreItems];
            result.NumItems = (long?)json[BrowserConstants.ObjectInFolderListNumItems];

            ConvertExtensionData(json, result, BrowserConstants.ObjectInFolderListKeys);

            return result;
        }

        internal static IList<IObjectInFolderContainer> ConvertDescendants(JToken json, ClientTypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            IList<IObjectInFolderContainer> result = new List<IObjectInFolderContainer>();

            foreach (JToken jsonChild in json.Children())
            {
                result.Add(ConvertDescendant(jsonChild, typeCache));
            }

            return result;
        }

        internal static IObjectInFolderContainer ConvertDescendant(JToken json, ClientTypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            ObjectInFolderContainer result = new ObjectInFolderContainer();

            result.Object = ConvertObjectInFolder(json[BrowserConstants.ObjectInFolderContainer], typeCache);
            IList<IObjectInFolderContainer> containerList = new List<IObjectInFolderContainer>();
            JArray jsonContainerList = json[BrowserConstants.ObjectInFolderContainerChildren] as JArray;
            if (jsonContainerList != null)
            {
                foreach (JToken jsonChild in jsonContainerList.Children())
                {
                    containerList.Add(ConvertDescendant(jsonChild, typeCache));
                }
            }

            result.Children = containerList;

            ConvertExtensionData(json, result, BrowserConstants.ObjectInFolderContainerKeys);

            return result;
        }

        internal static IList<IObjectParentData> ConvertObjectParents(JToken json, ClientTypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            IList<IObjectParentData> result = new List<IObjectParentData>();
            foreach (JToken child in json.Children())
            {
                ObjectParentData data = new ObjectParentData();
                data.Object = ConvertObjectData(child[BrowserConstants.ObjectParentsObject], typeCache);
                data.RelativePathSegment = (string)child[BrowserConstants.ObjectParentsRelavivePathSegment];
                ConvertExtensionData(child, data, BrowserConstants.ObjectParentsKeys);
                result.Add(data);
            }

            return result;
        }

        internal static IObjectInFolderData ConvertObjectInFolder(JToken json, ClientTypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            ObjectInFolderData result = new ObjectInFolderData();
            result.Object = ConvertObjectData(json[BrowserConstants.ObjectInFolderObject], typeCache);
            result.PathSegment = (string)json[BrowserConstants.ObjectInFolderPathSegment];
            ConvertExtensionData(json, result, BrowserConstants.ObjectInFolderKeys);
            return result;
        }

        internal static IObjectData ConvertObjectData(JToken json, ClientTypeCache typeCache)
        {
            ObjectData result = new ObjectData();

            result.Acl = ConvertAcl(json[BrowserConstants.ObjectAcl]);
            result.AllowableActions = ConvertAllowableActions(json[BrowserConstants.ObjectAllowableActions]);
            //  TODO ChangeEventInfo
            result.ChangeEventInfo = null;
            //  TODO IsExactAcl
            result.IsExactAcl = null;
            //  TODO PolicyIds
            result.PolicyIds = null;
            //  TODO Relationships
            result.Relationships = null;
            //  TODO Renditions
            result.Renditions = null;

            //  TODO Properties
            JToken jsonProperties = json[BrowserConstants.ObjectSuccinctProperties];
            if (jsonProperties != null)
            {
                result.Properties = ConvertSuccinctProperties(
                    jsonProperties,
                    json[BrowserConstants.ObjectPropertiesExtension],
                    typeCache);
            }
            else
            {
                jsonProperties = json[BrowserConstants.ObjectProperties];
                //  TODO Properties
                result.Properties = null;
            }

            ConvertExtensionData(json, result, BrowserConstants.ObjectKeys);

            return result;
        }

        private static IProperties ConvertSuccinctProperties(JToken json, JToken jsonExtension, ClientTypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            ITypeDefinition typeDefinition = null;
            string objectTypeId = (string)json[PropertyIds.ObjectTypeId];
            if (!string.IsNullOrEmpty(objectTypeId))
            {
                typeDefinition = typeCache.GetTypeDefinition(objectTypeId);
            }

            IList<ITypeDefinition> secondaryTypeDefinitions = new List<ITypeDefinition>();
            JArray jsonSecondaryTypeIds = json[PropertyIds.SecondaryObjectTypeIds] as JArray;
            if (jsonSecondaryTypeIds != null && jsonSecondaryTypeIds.Count > 0)
            {
                foreach (JToken value in jsonSecondaryTypeIds.Values())
                {
                    string typeId = (string)value;
                    if (typeId != null)
                    {
                        secondaryTypeDefinitions.Add(typeCache.GetTypeDefinition(typeId));
                    }
                }
            }

            Properties result = new Properties();

            foreach (JProperty property in ((JObject)json).Properties())
            {
                string id = property.Name;
                IPropertyDefinition propertyDefinition = null;
                if (typeDefinition != null)
                {
                    propertyDefinition = typeDefinition[id];
                }
                if (propertyDefinition == null && secondaryTypeDefinitions != null)
                {
                    foreach (ITypeDefinition secondaryTypeDefinition in secondaryTypeDefinitions)
                    {
                        propertyDefinition = secondaryTypeDefinition[id];
                        if (propertyDefinition != null)
                        {
                            break;
                        }
                    }
                }
                if (propertyDefinition == null)
                {
                    propertyDefinition = typeCache.GetTypeDefinition(BaseTypeId.CmisDocument.GetCmisValue())[id];
                }
                if (propertyDefinition == null)
                {
                    propertyDefinition = typeCache.GetTypeDefinition(BaseTypeId.CmisFolder.GetCmisValue())[id];
                }

                List<object> values = new List<object>();
                if (property.Value is JArray)
                {
                    foreach (JToken value in property.Value as JArray)
                    {
                        if (value is JValue)
                        {
                            values.Add(((JValue)value).Value);
                        }
                        else
                        {
                            throw new CmisRuntimeException("Invalid JSON value: " + value);
                        }
                    }
                }
                else
                {
                    if (property.Value is JValue)
                    {
                        values.Add(((JValue)property.Value).Value);
                    }
                    else
                    {
                        throw new CmisRuntimeException("Invalid JSON value: " + property.Value);
                    }
                }

                PropertyData propertyData = null;
                if (propertyDefinition != null)
                {
                    propertyData = new PropertyData(propertyDefinition.PropertyType);
                }
                else
                {
                    // this else block should only be reached in rare circumstances
                    // it may return incorrect types
                    if (values.Count == 0)
                    {
                        propertyData = new PropertyData(PropertyType.String);
                        propertyData.Values = null;
                    }
                    else
                    {
                        object firstValue = values[0];
                        if (firstValue == null || firstValue is string)
                        {
                            propertyData = new PropertyData(PropertyType.String);
                        }
                        else if (firstValue is bool)
                        {
                            propertyData = new PropertyData(PropertyType.Boolean);
                        }
                        else if (firstValue is decimal)
                        {
                            propertyData = new PropertyData(PropertyType.Decimal);
                        }
                        else if (firstValue is DateTime)
                        {
                            propertyData = new PropertyData(PropertyType.DateTime);
                        }
                        else
                        {
                            propertyData = new PropertyData(PropertyType.Integer);
                        }
                    }
                }

                foreach (object value in values)
                {
                    if (value == null)
                    {
                        continue;
                    }
                    object convertedValue = value;
                    if (propertyData.PropertyType == PropertyType.DateTime && value is long)
                    {
                        convertedValue = new DateTime((long)value);
                    }
                    try
                    {
                        propertyData.CheckValue(convertedValue);
                    }
                    catch (Exception)
                    {
                        throw new CmisRuntimeException("Invalid property value: " + convertedValue);
                    }
                    propertyData.AddValue(convertedValue);
                }

                propertyData.Id = id;
                if (propertyDefinition != null)
                {
                    propertyData.DisplayName = propertyDefinition.DisplayName;
                    propertyData.QueryName = propertyDefinition.QueryName;
                    propertyData.LocalName = propertyDefinition.LocalName;
                }
                else
                {
                    propertyData.DisplayName = id;
                    propertyData.QueryName = null;
                    propertyData.LocalName = null;
                }

                result.AddProperty(propertyData);
            }

            if (jsonExtension != null)
            {
                ConvertExtensionData(jsonExtension, result, new HashSet<string>());
            }

            return result;
        }

        internal static IAcl ConvertAcl(JToken json)
        {
            if (json == null)
            {
                return null;
            }

            Acl result = new Acl();
            result.Aces = new List<IAce>();
            JArray jsonAces = json[BrowserConstants.AclAces] as JArray;
            if (jsonAces != null)
            {
                foreach (JToken jsonAce in jsonAces)
                {
                    Ace ace = new Ace();
                    ace.IsDirect = ((bool?)jsonAce[BrowserConstants.AceIsDirect]).GetValueOrDefault();
                    JArray jsonPermissions = (jsonAce[BrowserConstants.AcePermissions] as JArray);
                    if (jsonPermissions != null)
                    {
                        ace.Permissions = new List<string>();
                        foreach (JToken jsonPermission in jsonPermissions)
                        {
                            ace.Permissions.Add(jsonPermission.Value<string>());
                        }
                    }
                    JToken jsonPrincipal = jsonAce[BrowserConstants.AcePrincipal];
                    Principal principal = new Principal();
                    principal.Id = (string)jsonPrincipal[BrowserConstants.AcePrincipalId];
                    ConvertExtensionData(jsonPrincipal, principal, BrowserConstants.AcePrincipalKeys);
                    ace.Principal = principal;
                    ConvertExtensionData(jsonAce, ace, BrowserConstants.AceKeys);
                    result.Aces.Add(ace);
                }
            }
            result.IsExact = (bool?)json[BrowserConstants.AclIsExact];
            ConvertExtensionData(json, result, BrowserConstants.AclKeys);

            return result;
        }

        internal static IAllowableActions ConvertAllowableActions(JToken json)
        {
            if (json == null)
            {
                return null;
            }

            AllowableActions result = new AllowableActions();
            result.Actions = new HashSet<string>();
            foreach (JProperty property in json)
            {
                if (property.Value.Value<bool>())
                {
                    result.Actions.Add(property.Name);
                }
            }

            return result;
        }

        internal static IFailedToDeleteData ConvertFailedToDelete(JToken json)
        {
            if (json == null)
            {
                return null;
            }

            FailedToDeleteData result = new FailedToDeleteData();

            IList<string> ids = new List<string>();
            JArray jsonIds = json[BrowserConstants.FailedToDeleteId] as JArray;
            if (jsonIds != null)
            {
                foreach (JToken jsonChild in jsonIds.Children())
                {
                    ids.Add(jsonChild.Value<string>());
                }
            }
            result.Ids = ids;

            ConvertExtensionData(json, result, BrowserConstants.FailedToDeleteKeys);

            return result;
        }

        internal static IList<IChoice<bool>> ConvertChoicesBoolean(JToken json)
        {
            //  TODO ConvertChoicesBoolean
            return null;
        }

        internal static IList<IChoice<long>> ConvertChoicesLong(JToken json)
        {
            //  TODO ConvertChoicesLong
            return null;
        }

        internal static IList<IChoice<decimal>> ConvertChoicesDecimal(JToken json)
        {
            //  TODO ConvertChoicesDateTime
            return null;
        }

        internal static IList<IChoice<string>> ConvertChoicesString(JToken json)
        {
            //  TODO ConvertChoicesString
            return null;
        }

        internal static IList<IChoice<DateTime>> ConvertChoicesDateTime(JToken json)
        {
            //  TODO ConvertChoicesDateTime
            return null;
        }

        internal static int? GetInteger(JValue json)
        {
            if (json == null)
            {
                return null;
            }
            return json.Value<int>();
        }
    }
}
