﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BoxNet;
using AppLimit.CloudComputing.SharpBox.StorageProvider.CIFS;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.FTP;
using AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs;
using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav;


namespace AppLimit.CloudComputing.SharpBox
{
    /// <summary>
    /// The class CloudStorage implements all needed methods to get access
    /// to a supported cloud storage infrastructure. The following vendors
    /// are currently supported:
    /// 
    ///  - DropBox
    ///  
    /// </summary>
    public partial class CloudStorage : ICloudStoragePublicAPI
    {
        // some information for token storage
        internal const string TokenProviderConfigurationType = "TokenProvConfigType";
        internal const string TokenCredentialType = "TokenCredType";
        internal const string TokenMetadataPrefix = "TokenMetadata";

        #region Member Declarations

        private ICloudStorageProviderInternal _provider;
        private ICloudStorageConfiguration _configuration;
        private readonly Dictionary<string, Type> _configurationProviderMap;

        #endregion

        #region Constructure and logistics

        /// <summary>
        /// returns the currently setted access token 
        /// </summary>
        public ICloudStorageAccessToken CurrentAccessToken
        {
            get
            {
                if (_provider == null)
                    return null;
                return _provider.CurrentAccessToken;
            }
        }

        /// <summary>
        /// Allows access to the current configuration which was used in the open call
        /// </summary>
        public ICloudStorageConfiguration CurrentConfiguration
        {
            get { return _configuration; }
        }

        /// <summary>
        /// The default constructure for a cloudstorage 
        /// </summary>
        public CloudStorage()
        {
            // build config provider
            _configurationProviderMap = new Dictionary<string, Type>();

            // register provider
            RegisterStorageProvider(typeof(DropBoxConfiguration), typeof(DropBoxStorageProvider));
            RegisterStorageProvider(typeof(WebDavConfiguration), typeof(WebDavStorageProvider));
            RegisterStorageProvider(typeof(BoxNetConfiguration), typeof(BoxNetStorageProvider));
            RegisterStorageProvider(typeof(FtpConfiguration), typeof(FtpStorageProvider));
            RegisterStorageProvider(typeof(CIFSConfiguration), typeof(CIFSStorageProvider));
            RegisterStorageProvider(typeof(GoogleDocsConfiguration), typeof(GoogleDocsStorageProvider));
            RegisterStorageProvider(typeof(StorageProvider.SkyDrive.SkyDriveConfiguration), typeof(StorageProvider.SkyDrive.Logic.SkyDriveStorageProvider));
        }

        /// <summary>
        /// copy ctor
        /// </summary>
        /// <param name="src"></param>
        public CloudStorage(CloudStorage src)
            : this(src, true)
        {
        }

        /// <summary>
        /// copy ctor 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="openIfSourceWasOpen"></param>
        public CloudStorage(CloudStorage src, bool openIfSourceWasOpen)
            : this()
        {
            // copy all registered provider from src
            _configurationProviderMap = src._configurationProviderMap;

            // open the provider
            if (src.IsOpened && openIfSourceWasOpen)
                Open(src._configuration, src.CurrentAccessToken);
            else
                _configuration = src._configuration;
        }

        /// <summary>
        /// This method allows to register a storage provider for a specific configuration
        /// type
        /// </summary>
        /// <param name="configurationType">
        /// A <see cref="Type"/>
        /// </param>
        /// <param name="storageProviderType">
        /// A <see cref="Type"/>
        /// </param>
        /// <returns>
        /// A <see cref="bool"/>
        /// </returns>
        public bool RegisterStorageProvider(Type configurationType, Type storageProviderType)
        {
            // do double check
            if (_configurationProviderMap.ContainsKey(configurationType.FullName))
                return false;

            // register
            _configurationProviderMap.Add(configurationType.FullName, storageProviderType);

            // go ahead
            return true;
        }

        /// <summary>
        /// Ignores all invalid ssl certs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private static bool ValidateAllServerCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #endregion

        #region Base Functions

        /// <summary>
        /// True when a the connection to the Cloudstorage will be established
        /// </summary>
        public bool IsOpened { get; private set; }

        /// <summary>
        /// Calling this method with vendor specific configuration settings and token based credentials
        /// to get access to the cloud storage. The following exceptions are possible:
        /// 
        /// - System.UnauthorizedAccessException when the user provides wrong credentials
        /// - AppLimit.CloudComputing.SharpBox.Exceptions.SharpBoxException when something 
        ///   happens during cloud communication
        /// 
        /// </summary>
        /// <param name="configuration">Vendor specific configuration of the cloud storage</param>
        /// <param name="token">Vendor specific authorization token</param>        
        /// <returns>A valid access token or null</returns>
        public ICloudStorageAccessToken Open(ICloudStorageConfiguration configuration, ICloudStorageAccessToken token)
        {
            // check state
            if (IsOpened)
                return null;

            // ensures that the right provider will be used
            SetProviderByConfiguration(configuration);

            // save the configuration
            _configuration = configuration;

            // verify the ssl config
            if (configuration.TrustUnsecureSSLConnections)
                System.Net.ServicePointManager.ServerCertificateValidationCallback = ValidateAllServerCertificates;

            // update the max connection settings 
            ServicePointManager.DefaultConnectionLimit = 250;

            // disable the not well implementes Expected100 header settings
            ServicePointManager.Expect100Continue = false;

            // open the cloud connection                                    
            token = _provider.Open(configuration, token);

            // ok without Exception every is good
            IsOpened = true;

            // return the token
            return token;
        }

        /// <summary>
        /// This method will close the connection to the cloud storage system
        /// </summary>
        public void Close()
        {
            if (_provider == null)
                return;

            _provider.Close();

            IsOpened = false;
        }

        /// <summary>
        /// This method returns access to the root folder of the storage system
        /// </summary>
        /// <returns>Reference to the root folder of the storage system</returns>
        public ICloudDirectoryEntry GetRoot()
        {
            return _provider == null ? null : _provider.GetRoot();
        }

        /// <summary>
        /// This method returns access ro a specific subfolder or file addressed via 
        /// unix like file system path, e.g. /Public/SubFolder/SubSub
        /// 
        /// Valid Exceptions are:
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorFileNotFound);
        /// </summary>
        /// <param name="name">The path to the target subfolder</param>
        /// <param name="parent">The startfolder for the searchpath</param>
        /// <returns></returns>
        public ICloudFileSystemEntry GetFileSystemObject(string name, ICloudDirectoryEntry parent)
        {
            return _provider == null ? null : _provider.GetFileSystemObject(name, parent);
        }

        /// <summary>
        /// This method creates a child folder in the give folder
        /// of the cloud storage
        /// </summary>
        /// <param name="name">Name of the new folder</param>
        /// <param name="parent">Parent folder</param>
        /// <returns>Reference to the created folder</returns>
        public ICloudDirectoryEntry CreateFolder(string name, ICloudDirectoryEntry parent)
        {
            return _provider == null ? null : _provider.CreateFolder(name, parent);
        }

        /// <summary>
        /// This method removes a file or a directory from 
        /// the cloud storage
        /// </summary>
        /// <param name="fsentry">Reference to the filesystem object which has to be removed</param>
        /// <returns>Returns true or false</returns>
        public bool DeleteFileSystemEntry(ICloudFileSystemEntry fsentry)
        {
            return _provider != null && _provider.DeleteFileSystemEntry(fsentry);
        }

        /// <summary>
        /// This method moves a file or a directory from its current
        /// location to a new onw
        /// </summary>
        /// <param name="fsentry">Filesystem object which has to be moved</param>
        /// <param name="newParent">The new location of the targeted filesystem object</param>
        /// <returns></returns>
        public bool MoveFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            return _provider == null ? false : _provider.MoveFileSystemEntry(fsentry, newParent);
        }

        /// <summary>
        ///  This method copy a file or a directory from its current
        /// location to a new onw
        /// </summary>
        /// <param name="fsentry"></param>
        /// <param name="newParent"></param>
        /// <returns></returns>
        public bool CopyFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            return _provider == null ? false : _provider.CopyFileSystemEntry(fsentry, newParent);
        }

        /// <summary>
        /// This mehtod allows to perform a server side renam operation which is basicly the same
        /// then a move operation in the same directory
        /// </summary>
        /// <param name="fsentry"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool RenameFileSystemEntry(ICloudFileSystemEntry fsentry, string newName)
        {
            return _provider == null ? false : _provider.RenameFileSystemEntry(fsentry, newName);
        }

        /// <summary> 
        /// This method creates a new file object in the cloud storage. Use the GetContentStream method to 
        /// get a .net stream which usable in the same way then local stream are usable.
        /// </summary>
        /// <param name="parent">Link to the parent container, null means the root directory</param>
        /// <param name="name">The name of the targeted file</param>        
        /// <returns></returns>
        public ICloudFileSystemEntry CreateFile(ICloudDirectoryEntry parent, string name)
        {
            // pass through the provider
            return _provider == null ? null : _provider.CreateFile(parent, name);
        }

        /// <summary>
        /// This method returns the direct URL to a specific file system object,
        /// e.g. a file or folder
        /// </summary>
        /// <param name="path">A relative path to the file</param>
        /// <param name="parent">A reference to the parent of the path</param>
        /// <returns></returns>
        public Uri GetFileSystemObjectUrl(string path, ICloudDirectoryEntry parent)
        {
            // pass through the provider
            return _provider == null ? null : _provider.GetFileSystemObjectUrl(path, parent);
        }

        /// <summary>
        /// Returns the path of the targeted object
        /// </summary>
        /// <param name="fsObject"></param>
        /// <returns></returns>
        public string GetFileSystemObjectPath(ICloudFileSystemEntry fsObject)
        {
            // pass through the provider
            return _provider == null ? null : _provider.GetFileSystemObjectPath(fsObject);
        }

        #endregion

        #region AccessTokenHandling

        /// <summary>
        /// This method allows to store a security token into a serialization stream
        /// </summary>        
        /// <param name="token"></param>
        /// <returns></returns>
        public Stream SerializeSecurityToken(ICloudStorageAccessToken token)
        {
            return SerializeSecurityToken(token, null);
        }

        /// <summary>
        /// This method allows to store a security token into a serialization stream and 
        /// makes it possible to store a couple of meta data as well
        /// </summary>
        /// <param name="token"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public Stream SerializeSecurityToken(ICloudStorageAccessToken token, Dictionary<string, string> additionalMetaData)
        {
            if (!IsOpened)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorOpenedConnectionNeeded);

            return SerializeSecurityTokenEx(token, _configuration.GetType(), additionalMetaData);
        }

        /// <summary>
        /// This method can be used for serialize a token without have the connection opened :-)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="configurationType"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public Stream SerializeSecurityTokenEx(ICloudStorageAccessToken token, Type configurationType, Dictionary<string, string> additionalMetaData)
        {
            var items = new Dictionary<string, string>();
            var stream = new MemoryStream();
            var serializer = new DataContractSerializer(items.GetType());

            // add the metadata 
            if (additionalMetaData != null)
            {
                foreach (KeyValuePair<String, string> kvp in additionalMetaData)
                {
                    items.Add(TokenMetadataPrefix + kvp.Key, kvp.Value);
                }
            }

            // save the token into our list
            StoreToken(items, token, configurationType);

            // write the data to stream
            serializer.WriteObject(stream, items);

            // go to start
            stream.Seek(0, SeekOrigin.Begin);

            // go ahead
            return stream;
        }

        /// <summary>
        /// This method stores a token into a file
        /// </summary>
        /// <param name="token"></param>
        /// <param name="configurationType"></param>
        /// <param name="additionalMetaData"></param>
        /// <param name="fileName"></param>
        public void SerializeSecurityTokenEx(ICloudStorageAccessToken token, Type configurationType, Dictionary<string, string> additionalMetaData, string fileName)
        {
            using (var fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var ts = SerializeSecurityTokenEx(token, configurationType, additionalMetaData))
                {
                    // copy
                    StreamHelper.CopyStreamData(this, ts, fs, null, null);
                }

                // flush
                fs.Flush();

                // close
                fs.Close();
            }
        }

        /// <summary>
        /// This method allows to serialize a security token into a base64 string 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="configurationType"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public string SerializeSecurityTokenToBase64Ex(ICloudStorageAccessToken token, Type configurationType, Dictionary<string, string> additionalMetaData)
        {
            using (var tokenStream = SerializeSecurityTokenEx(token, configurationType, additionalMetaData))
            {
                using (var memStream = new MemoryStream())
                {
                    // copy to memory
                    StreamHelper.CopyStreamData(this, tokenStream, memStream, null, null);

                    // reset
                    memStream.Position = 0;

                    // convert 
                    return Convert.ToBase64String(memStream.ToArray());
                }
            }
        }

        /// <summary>
        /// This method allows to load a token from a previously generated stream
        /// </summary>
        /// <param name="tokenStream"></param>        
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityToken(Stream tokenStream)
        {
            Dictionary<string, string> metadata;
            return DeserializeSecurityToken(tokenStream, out metadata);
        }

        /// <summary>
        /// This method restores a tokene from file 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityTokenEx(string fileName)
        {
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return DeserializeSecurityToken(fs);
            }
        }

        /// <summary>
        /// This method allows to load a token from a previously generated stream 
        /// and the attached metadata
        /// </summary>
        /// <param name="tokenStream"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityToken(Stream tokenStream, out Dictionary<string, string> additionalMetaData)
        {
            // load the data in our list            
            var serializer = new DataContractSerializer(typeof(Dictionary<string, string>));

            // check the type
            var obj = serializer.ReadObject(tokenStream);
            if (!obj.GetType().Equals(typeof(Dictionary<string, string>)))
                throw new InvalidDataException("A List<String> was expected");

            // evaluate the right provider
            var tokendata = (Dictionary<string, string>)obj;

            // find the right provider by typename
            var provider = GetProviderByConfigurationTypeName(tokendata[TokenProviderConfigurationType]);

            // set the output parameter
            additionalMetaData = new Dictionary<string, string>();

            // fill the metadata
            foreach (var kvp in tokendata)
            {
                if (kvp.Key.StartsWith(TokenMetadataPrefix))
                {
                    additionalMetaData.Add(kvp.Key.Remove(0, TokenMetadataPrefix.Length), kvp.Value);
                }
            }

            // build the token            
            return provider.LoadToken(tokendata);
        }

        /// <summary>
        ///  This methd allows to deserialize a base64 tokenstring
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityTokenFromBase64(string tokenString)
        {
            Dictionary<string, string> additionalMetaData;
            return DeserializeSecurityTokenFromBase64(tokenString, out additionalMetaData);
        }

        /// <summary>        
        ///  This methd allows to deserialize a base64 tokenstring
        /// </summary>
        /// <param name="tokenString"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityTokenFromBase64(string tokenString, out Dictionary<string, string> additionalMetaData)
        {
            // convert base64 to byte array
            var data = Convert.FromBase64String(tokenString);

            // create a token stream 
            using (var tokenStream = new MemoryStream(data))
            {
                // read the token stream
                return DeserializeSecurityToken(tokenStream, out additionalMetaData);
            }
        }

        /// <summary>
        /// This method stores the content of an access token in to 
        /// a list of string. This list can be serialized.
        /// </summary>
        /// <param name="tokendata">Target list</param>
        /// <param name="token">the token</param>
        /// <param name="configurationType">type of configguration which is responsable for the token</param>
        internal void StoreToken(Dictionary<string, string> tokendata, ICloudStorageAccessToken token, Type configurationType)
        {
            // add the configuration information into the 
            tokendata.Add(TokenProviderConfigurationType, configurationType.FullName);
            tokendata.Add(TokenCredentialType, token.GetType().FullName);

            // get the provider by toke
            var provider = GetProviderByConfigurationTypeName(configurationType.FullName);

            // store all the other information to tokendata
            provider.StoreToken(tokendata, token);
        }

        /// <summary>
        /// This method generated a access token from the given data 
        /// string list
        /// </summary>
        /// <param name="tokendata">the string list</param>
        /// <returns>The unserialized token</returns>
        internal ICloudStorageAccessToken LoadToken(Dictionary<string, string> tokendata)
        {
            return _provider.LoadToken(tokendata);
        }

        #endregion

        #region Helper

        private static ICloudStorageProviderInternal CreateProviderByType(Type providerType)
        {
            ICloudStorageProviderInternal provider;

            try
            {
                provider = Activator.CreateInstance(providerType) as ICloudStorageProviderInternal;
            }
            catch (Exception e)
            {
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCreateProviderInstanceFailed, e);
            }

            if (provider == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCreateProviderInstanceFailed);

            return provider;
        }

        private void SetProviderByConfiguration(ICloudStorageConfiguration configuration)
        {
            // check
            if (configuration == null && _provider == null)
                throw new InvalidOperationException("It's only allowed to set the configuration parameter to null when a provider was set before");

            // read out the right provider type
            SetProviderByConfigurationTypeName(configuration.GetType().FullName);
        }

        private void SetProviderByConfigurationTypeName(string typeName)
        {
            _provider = GetProviderByConfigurationTypeName(typeName);
        }

        private ICloudStorageProviderInternal GetProviderByConfigurationTypeName(string typeName)
        {
            // read out the right provider type
            Type providerType;
            if (!_configurationProviderMap.TryGetValue(typeName, out providerType))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorNoValidProviderFound);

            // build up the provider
            return CreateProviderByType(providerType);
        }

        #endregion
    }
}