// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using NLog.Internal;

    /// <summary>
    /// Represents SQL command parameter converter.
    /// </summary>
    public class DatabaseParameterTypeSetter
    {
        private bool _defaultDbProperty;

        /// <summary>
        /// SQL Command Parameter DbType Property
        /// </summary>
        private PropertyInfo DbTypeProperty { get; set; }
        /// <summary>
        /// SQL Command Parameter instance DbType Property Values
        /// </summary>
        private Dictionary<DatabaseParameterInfo, object> PropertyDbTypeValues { get; set; }
        /// <summary>
        /// Resolve Parameter DbType Property and DbType Value
        /// </summary>
        /// <docgen category='Parameter Options' order='10' />
        public void Resolve(IDbDataParameter p, string dbTypePropertyName, IList<DatabaseParameterInfo> parameters)
        {
            Type propertyType;
            if (string.IsNullOrEmpty(dbTypePropertyName) ||
                dbTypePropertyName.Equals("dbbType", StringComparison.OrdinalIgnoreCase))
            {
                _defaultDbProperty = true;
                propertyType = typeof(DbType);
            }
            else
            {

                if (!PropertyHelper.TryGetPropertyInfo(p, dbTypePropertyName, out var dbTypeProperty))
                {
                    throw new NLogConfigurationException(
                        "Type '" + p.GetType().Name + "' has no property '" + dbTypePropertyName + "'.");
                }

                this.DbTypeProperty = dbTypeProperty;
              
                propertyType = dbTypeProperty.PropertyType;
            }

            this.PropertyDbTypeValues = new Dictionary<DatabaseParameterInfo, object>();
            foreach (var par in parameters)
            {
                if (string.IsNullOrEmpty(par.DbType)) continue;
             
                var dbTypeValue = Enum.Parse(propertyType, par.DbType);
                this.PropertyDbTypeValues[par] = dbTypeValue;
            }
        }
        /// <summary>
        /// Set Parameter DbType
        /// </summary>
        public void SetParameterDbType(IDbDataParameter p, DatabaseParameterInfo par)
        {
            if (PropertyDbTypeValues.TryGetValue(par, out var propertyDbTypeValue))
            {
                if (_defaultDbProperty)
                {
                    p.DbType = (DbType)propertyDbTypeValue;
                }
                else
                {
                    this.DbTypeProperty.SetValue(p, propertyDbTypeValue, null);
                }
            }
        }


    }
}
#endif