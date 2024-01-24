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


using System.Collections.Generic;

namespace ASC.Migration.OwnCloud.Models
{
    public class OCStorages
    {
        public int NumericId { get; set; }
        public string Id { get; set; }
        public List<OCFileCache> FileCache { get; set; }
    }

    public class OCFileCache
    {
        public int FileId { get; set; }
        public string Path { get; set; }
        public List<OCShare> Share { get; set; }
    }

    public class OCShare
    {
        public int Id { get; set; }
        public string ShareWith { get; set; }
        public int Premissions { get; set; }
    }
}
