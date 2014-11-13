//-----------------------------------------------------------------------
// <copyright file="browser-writer.cs" company="GRAU DATA AG">
//
//   Licensed to the Apache Software Foundation (ASF) under one
//   or more contributor license agreements.  See the NOTICE file
//   distributed with this work for additional information
//   regarding copyright ownership.  The ASF licenses this file
//   to you under the Apache License, Version 2.0 (the
//   "License"); you may not use this file except in compliance
//   with the License.  You may obtain a copy of the License at
//   
//   http://www.apache.org/licenses/LICENSE-2.0
//   
//   Unless required by applicable law or agreed to in writing,
//   software distributed under the License is distributed on an
//   "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//   KIND, either express or implied.  See the License for the
//   specific language governing permissions and limitations
//   under the License.
//
// </copyright>
//-----------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using DotCMIS;
using DotCMIS.Data;
using DotCMIS.Data.Impl;
using DotCMIS.Binding.Impl;


namespace DotCMIS.Binding.Browser
{
    internal class FormDataWriter
    {
        private const string ContentTypeUrlEncoded = "application/x-www-form-urlencoded;charset=utf-8";
        private const string ContentTypeFormData = "multipart/form-data; boundary=";
        private const string CRLF = "\r\n";
        private const int BufferSize = 64 * 1024;

        private string Boundary;
        private Dictionary<string,string> Parameters = new Dictionary<string,string>();
        private IContentStream ContentStream;

        public FormDataWriter(string action)
            : this(action, null)
        {
        }

        public FormDataWriter(string action, IContentStream contentStream)
        {
            AddParameter(BrowserConstants.ControlCmisAction, action);
            ContentStream = contentStream;
            Boundary = "aPacHeCheMIStrydOTcmiS" + action.GetHashCode().ToString() + action
                + System.DateTime.Now.Ticks + this.GetHashCode().ToString();
        }

        public string GetContentType()
        {
            if (ContentStream == null)
            {
                return ContentTypeUrlEncoded;
            }
            else
            {
                return ContentTypeFormData + Boundary;
            }
        }

        public void AddParameter(string name, object value)
        {
            if (name == null || value == null)
            {
                return;
            }
            Parameters[name] = UrlBuilder.NormalizeParameter(value);
        }

        public void AddPropertiesParameters(IProperties properties)
        {
            if (properties == null)
            {
                return;
            }

            int index = 0;
            foreach (IPropertyData property in properties.PropertyList)
            {
                if (property == null)
                {
                    continue;
                }

                string indexString = "[" + index.ToString() + "]";
                AddParameter(BrowserConstants.ControlPropertyId + indexString, property.Id);

                if (property.Values != null && property.Values.Count > 0)
                {
                    if (property.Values.Count == 1)
                    {
                        AddParameter(BrowserConstants.ControlPropertyValue + indexString, ConvertPropertyValue(property.FirstValue));
                    }
                    else
                    {
                        int vIndex = 0;
                        foreach (object value in property.Values)
                        {
                            string vIndexString = "[" + vIndex.ToString() + "]";
                            AddParameter(BrowserConstants.ControlPropertyValue + indexString + vIndexString, ConvertPropertyValue(value));
                            vIndex++;
                        }
                    }
                }

                index++;
            }
        }

        private string ConvertPropertyValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is DateTime)
            {
                return BrowserConverter.ConvertDateTimeString((DateTime)value);
            }

            return value.ToString();
        }

        public void AddPoliciesParameters(IList<string> policies)
        {
            if (policies == null)
            {
                return;
            }

            int idx = 0;
            foreach(object policy in policies)
            {
                if (policy != null)
                {
                    string idxStr = "[" + idx.ToString() + "]";
                    AddParameter(BrowserConstants.ControlPolicy + idxStr, policy);
                    idx++;
                }
            }
        }

        public void AddAddAcesParameters(IAcl acl)
        {

            AddAcesParameters(acl, BrowserConstants.ControlAddAcePrincipal, BrowserConstants.ControlAddAcePermission);
        }

        public void AddRemoveAcesParameters(IAcl acl)
        {
            AddAcesParameters(acl, BrowserConstants.ControlRemoveAcePrincipal, BrowserConstants.ControlRemoveAcePermission);
        }

        public void AddAcesParameters(IAcl acl,String principalControl,String permissionControl)
        {
            if (acl == null || acl.Aces == null)
            {
                return;
            }

            int idx = 0;
            foreach (Ace ace in acl.Aces)
            {
                if (ace.PrincipalId != null && ace.Permissions != null && ace.Permissions.Count > 0)
                {
                    string idxStr = "[" + idx.ToString() + "]";
                    AddParameter(principalControl + idxStr, ace.PrincipalId);
                    int permIdx = 0;
                    foreach (string perm in ace.Permissions)
                    {
                        if (perm != null)
                        {
                            string permIdxStr = "[" + permIdx.ToString() + "]";
                            AddParameter(permissionControl + idxStr + permIdxStr, perm);
                            permIdx++;
                        }
                    }
                    idx++;
                }
            }
        }

        public void AddSuccinctFlag(bool succinct)
        {
            if (succinct)
            {
                AddParameter(BrowserConstants.ControlSuccinct, "true");
            }
        }

        public void Write(Stream output)
        {
            if (ContentStream == null || ContentStream.Stream == null)
            {
                Boolean first = true;
                string content = string.Empty;
                foreach (KeyValuePair<string, string> pair in Parameters)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        content += "&";
                    }

                    content += pair.Key + "=" + Uri.EscapeDataString(pair.Value);
                }
                byte[] buffer = UTF8Encoding.UTF8.GetBytes(content);
                output.Write(buffer, 0, buffer.Length);
            }
            else
            {
                //  TODO Write optimization: combine multiple WriteLine() to one block
                WriteLine(output);

                //  parameters
                foreach (KeyValuePair<string, string> pair in Parameters)
                {
                    WriteLine(output, "--" + Boundary);
                    WriteLine(output, "Content-Disposition: form-data; name=\"" + pair.Key + "\"");
                    WriteLine(output, "Content-Type: text/plain; charset=utf-8");
                    WriteLine(output);
                    WriteLine(output, pair.Value);
                }

                //  content
                string filename = ContentStream.FileName;
                if (string.IsNullOrEmpty(filename))
                {
                    filename = "content";
                }

                string mediaType = ContentStream.MimeType;
                if (string.IsNullOrEmpty(mediaType) || mediaType.IndexOf('/') < 1 || mediaType.IndexOf('\n') > -1 || mediaType.IndexOf('\r') > -1)
                {
                    mediaType = AtomPubConstants.MediatypeOctetStream;
                }

                WriteLine(output, "--" + Boundary);
                WriteLine(output,
                    "Content-Disposition: "
                    + MimeHelper.EncodeContentDisposition(MimeHelper.DispositionFormDataContent, filename));
                WriteLine(output, "Content-Type: " + mediaType);
                WriteLine(output, "Content-Transfer-Encoding: binary");
                WriteLine(output);

                int length;
                byte[] buffer = new byte[BufferSize];
                do
                {
                    length = ContentStream.Stream.Read(buffer, 0, BufferSize);
                    if (length <= 0)
                    {
                        break;
                    }
                    output.Write(buffer, 0, length);
                } while (true);

                WriteLine(output);
                WriteLine(output, "--" + Boundary + "--");
            }
        }

        private void WriteLine(Stream output)
        {
            WriteLine(output, null);
        }

        private void WriteLine(Stream output, string s)
        {
            s = (s == null ? CRLF : s + CRLF);
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(s);
            output.Write(buffer, 0, buffer.Length);
        }
    }
}
