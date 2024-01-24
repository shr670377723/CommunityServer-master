﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using AppLimit.CloudComputing.SharpBox.Common.Net.Json;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive
{
    [Serializable]
    public class SkyDriveParserException : Exception
    {
        private readonly string _message;

        public override string Message
        {
            get { return _message; }
        }

        public SkyDriveParserException()
            : this(string.Empty)
        {
        }

        public SkyDriveParserException(string message)
        {
            _message = message;
        }

        protected SkyDriveParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    internal static class SkyDriveJsonParser
    {
        public static ICloudFileSystemEntry ParseSingleEntry(IStorageProviderSession session, string json)
        {
            return ParseSingleEntry(session, json, null);
        }

        private static ICloudFileSystemEntry ParseSingleEntry(IStorageProviderSession session, string json, JsonHelper parser)
        {
            if (json == null)
                return null;

            if (parser == null)
                parser = CreateParser(json);

            if (ContainsError(json, false, parser))
                return null;

            BaseFileEntry entry;

            var type = parser.GetProperty("type");

            if (!IsFolderType(type) && !IsFileType(type))
                return null;

            var id = parser.GetProperty("id");
            var name = parser.GetProperty("name");
            var parentID = parser.GetProperty("parent_id");
            var uploadLocation = parser.GetProperty("upload_location");
            var updatedTime = Convert.ToDateTime(parser.GetProperty("updated_time")).ToUniversalTime();

            if (IsFolderType(type))
            {
                var count = parser.GetPropertyInt("count");
                entry = new BaseDirectoryEntry(name, count, updatedTime, session.Service, session) { Id = id };
            }
            else
            {
                var size = Convert.ToInt64(parser.GetProperty("size"));
                entry = new BaseFileEntry(name, size, updatedTime, session.Service, session) { Id = id };
            }
            entry[SkyDriveConstants.UploadLocationKey] = uploadLocation;

            if (!string.IsNullOrEmpty(parentID))
            {
                entry.ParentID = SkyDriveConstants.RootIDRegex.IsMatch(parentID) ? "/" : parentID;
            }
            else
            {
                entry.Name = "/";
                entry.Id = "/";
            }

            entry[SkyDriveConstants.InnerIDKey] = id;
            entry[SkyDriveConstants.TimestampKey] = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

            return entry;
        }

        public static string ParseEntryID(string json)
        {
            var parser = CreateParser(json);
            return parser.GetProperty("id");
        }

        public static IEnumerable<ICloudFileSystemEntry> ParseListOfEntries(IStorageProviderSession session, string json)
        {
            return ParseListOfEntries(session, json, null);
        }

        private static IEnumerable<ICloudFileSystemEntry> ParseListOfEntries(IStorageProviderSession session, string json, JsonHelper parser)
        {
            if (json == null)
                return null;

            if (parser == null)
                parser = CreateParser(json);

            if (ContainsError(json, false, parser))
                return null;

            return parser.GetListProperty("data").Select(jsonEntry => ParseSingleEntry(session, jsonEntry)).Where(entry => entry != null);
        }

        public static bool ContainsError(string json, bool throwIfError)
        {
            return ContainsError(json, throwIfError, null);
        }

        private static bool ContainsError(string json, bool throwIfError, JsonHelper parser)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            if (parser == null)
                parser = CreateParser(json);

            var error = parser.GetProperty("error");

            if (!string.IsNullOrEmpty(error))
            {
                if (throwIfError)
                    throw new SkyDriveParserException(
                        string.Format("The returned JSON message is describing the error. The message contained the following: {0}", error));

                return true;
            }

            return false;
        }

        private static JsonHelper CreateParser(string json)
        {
            var parser = new JsonHelper();
            if (!parser.ParseJsonMessage(json))
                throw new SkyDriveParserException("Can not parse this JSON message");
            return parser;
        }

        private static bool IsFolderType(string type)
        {
            return type.Equals("folder") || type.Equals("album");
        }

        private static bool IsFileType(string type)
        {
            return type.Equals("file") || type.Equals("photo");
        }
    }
}