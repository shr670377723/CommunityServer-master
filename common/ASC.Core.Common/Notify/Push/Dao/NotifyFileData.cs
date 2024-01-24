﻿/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ASC.Core.Common.Notify.FireBase.Dao
{
    [Serializable]
    public class NotifyFileData
    {
        [JsonProperty("id")]
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [JsonProperty("extension")]
        [DataMember(Name = "extension")]
        public string Extension { get; set; }

    }
}
