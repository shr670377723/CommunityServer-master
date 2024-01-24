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
using System.Collections.Generic;
using System.Runtime.Serialization;

using ASC.Core;
using ASC.Core.Users;
using ASC.Specific;
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "todo", Namespace = "")]
    public class TodoWrapper
    {
        private readonly TimeZoneInfo _timeZone;

        public Guid UserId { get; private set; }

        protected ITodo _baseTodo;

        private readonly DateTime _utcStartDate = DateTime.MinValue;
        private readonly DateTime _utcCompletedDate = DateTime.MinValue;

        private TodoWrapper(ITodo baseTodo, Guid userId, TimeZoneInfo timeZone, DateTime utcStartDate)
            : this(baseTodo, userId, timeZone)
        {
            _utcStartDate = utcStartDate;
        }

        public TodoWrapper(ITodo baseTodo, Guid userId, TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
            _baseTodo = baseTodo;
            this.UserId = userId;
        }

        public List<TodoWrapper> GetList()
        {
            var list = new List<TodoWrapper>();

            list.Add(new TodoWrapper(_baseTodo, this.UserId, _timeZone));

            return list;
        }
        ///<example name="objectId">1</example>
        ///<order>0</order>
        [DataMember(Name = "objectId", Order = 0)]
        public string Id { get { return _baseTodo.Id; } }

        ///<example name="uniqueId">123wda</example>
        ///<order>140</order>
        [DataMember(Name = "uniqueId", Order = 140)]
        public string Uid { get { return _baseTodo.Uid; } }

        ///<example type="int">1</example>
        public int TenantId { get; set; }

        ///<example name="sourceId">calendarID</example>
        ///<order>10</order>
        [DataMember(Name = "sourceId", Order = 10)]
        public string CalendarId { get { return _baseTodo.CalendarId; } }

        ///<example name="title">Todo Name</example>
        ///<order>20</order>
        [DataMember(Name = "title", Order = 20)]
        public string Name { get { return _baseTodo.Name; } }

        ///<example name="description">Todo Description</example>
        ///<order>30</order>
        [DataMember(Name = "description", Order = 30)]
        public string Description { get { return _baseTodo.Description; } }

        ///<example name="start">2020-12-01T06:36:10.8645482Z</example>
        ///<order>40</order>
        [DataMember(Name = "start", Order = 40)]
        public ApiDateTime Start
        {
            get
            {
                var startD = _utcStartDate != DateTime.MinValue ? _utcStartDate : _baseTodo.UtcStartDate;
                startD = new DateTime(startD.Ticks, DateTimeKind.Utc);

                return new ApiDateTime(startD, _timeZone);
            }
        }

        ///<example name="completed">2020-12-01T06:36:10.8645482Z</example>
        ///<order>110</order>
        [DataMember(Name = "completed", Order = 110)]
        public ApiDateTime Completed
        {
            get
            {
                var completedD = _utcCompletedDate != DateTime.MinValue ? _utcCompletedDate : _baseTodo.Completed;
                completedD = new DateTime(completedD.Ticks, DateTimeKind.Utc);
                return new ApiDateTime(completedD, _timeZone);
            }
        }

        ///<type name="owner">ASC.Api.Calendar.Wrappers.UserParams, ASC.Api.Calendar</type>
        ///<order>120</order>
        [DataMember(Name = "owner", Order = 120)]
        public UserParams Owner
        {
            get
            {
                var owner = new UserParams() { Id = _baseTodo.OwnerId, Name = "" };
                if (_baseTodo.OwnerId != Guid.Empty)
                    owner.Name = CoreContext.UserManager.GetUsers(_baseTodo.OwnerId).DisplayUserName();

                return owner;
            }
        }


        public static object GetSample()
        {
            return new
            {
                owner = UserParams.GetSample(),
                start = new ApiDateTime(DateTime.Now.AddDays(1), TimeZoneInfo.Utc),
                description = "Todo Description",
                title = "Todo Name",
                objectId = "1",
                sourceId = "calendarID",
                completed = new ApiDateTime(DateTime.MinValue, TimeZoneInfo.Utc)
            };
        }
    }
}
