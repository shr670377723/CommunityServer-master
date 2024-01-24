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

using ASC.Core;

namespace ASC.Common.Logging
{
    public class LogManager
    {
        private static bool CoreContextEnabled { get; set; }

        static LogManager()
        {
            try
            {
                var tenantManager = CoreContext.TenantManager;
                if (tenantManager != null)
                {
                    CoreContextEnabled = true;
                }
            }
            catch (Exception)
            {
                CoreContextEnabled = false;
            }
        }

        public static ILog GetLogger(string name)
        {
            return BaseLogManager.GetLogger(name, () =>
            {
                if (!CoreContextEnabled) return null;

                var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                return tenant == null ? null : tenant.TenantAlias;
            });
        }
    }
}
