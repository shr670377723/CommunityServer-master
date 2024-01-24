/*
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

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "NotificationAddress")]
    public class ServerNotificationAddressData
    {
        ///<example>Email</example>
        [DataMember(IsRequired = true)]
        public string Email { get; set; }

        ///<example name="smtp_server">smtp_server</example>
        [DataMember(Name = "smtp_server")]
        public string SmtpServer { get; set; }

        ///<example type="int" name="smtp_port">33</example>
        [DataMember(Name = "smtp_port")]
        public int SmtpPort { get; set; }

        ///<example name="smtp_account">smtp_account</example>
        [DataMember(Name = "smtp_account")]
        public string SmtpAccount { get; set; }

        ///<example name="smtp_auth">true</example>
        [DataMember(Name = "smtp_auth")]
        public bool SmtpAuth { get; set; }

        ///<example name="smtp_encryption_type">smtp_encryption_type</example>
        [DataMember(Name = "smtp_encryption_type")]
        public string SmptEncryptionType { get; set; }

        ///<example name="smtp_auth_type">smtp_auth_type</example>
        [DataMember(Name = "smtp_auth_type")]
        public string SmtpAuthenticationType { get; set; }
    }
}
