/*  New BSD License
-------------------------------------------------------------------------------
Copyright (c) 2006-2012, EntitySpaces, LLC
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the EntitySpaces, LLC nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL EntitySpaces, LLC BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EffiProz;

using EntitySpaces.Interfaces;

namespace EntitySpaces.EffiProzProvider
{
    class Cache
    {
        static public Dictionary<string, EfzParameter> GetParameters(esDataRequest request)
        {
            return GetParameters(request.DataID, request.ProviderMetadata, request.Columns);
        }

        static public Dictionary<string, EfzParameter> GetParameters(Guid dataID, 
            esProviderSpecificMetadata providerMetadata, esColumnMetadataCollection columns)
        {
            lock (parameterCache)
            {
                if (!parameterCache.ContainsKey(dataID))
                {
                    // The Parameters for this Table haven't been cached yet, this is a one time operation
                    Dictionary<string, EfzParameter> types = new Dictionary<string, EfzParameter>();

                    EfzParameter param1;
                    foreach (esColumnMetadata col in columns)
                    {
                        esTypeMap typeMap = providerMetadata.GetTypeMap(col.PropertyName);
                        if (typeMap != null)
                        {
                            string nativeType = typeMap.NativeType;
                            EfzType dbType = Cache.NativeTypeToDbType(nativeType);

                            param1 = new EfzParameter(Delimiters.Param + col.PropertyName, dbType, col.PropertyName);
                            param1.SourceColumn = col.Name;

                            switch (dbType)
                            {
                                // TODO:
                                //case SqlDbType.BigInt:
                                //case SqlDbType.Decimal:
                                //case SqlDbType.Float:
                                //case SqlDbType.Int:
                                //case SqlDbType.Money:
                                //case SqlDbType.Real:
                                //case SqlDbType.SmallMoney:
                                //case SqlDbType.TinyInt:
                                //case SqlDbType.SmallInt:

                                //    param1.Size = (int)col.CharacterMaxLength;
                                //    param1.Precision = (byte)col.NumericPrecision;
                                //    param1.Scale = (byte)col.NumericScale;
                                //    break;

                                //case SqlDbType.DateTime:

                                //    param1.Precision = 23;
                                //    param1.Scale = 3;
                                //    break;

                                //case SqlDbType.SmallDateTime:

                                //    param1.Precision = 16;
                                //    break;

                                //case SqlDbType.Udt:

                                //    SetUdtTypeNameToAvoidMonoError(param1, typeMap);
                                //    break;

                            }
                            types[col.Name] = param1;
                        }
                    }

                    parameterCache[dataID] = types;
                }
            }

            return parameterCache[dataID];
        }

        static private EfzType NativeTypeToDbType(string nativeType)
        {
            switch(nativeType)
            {
                case "BOOLEAN": return EfzType.Boolean;
                case "BIGINT": return EfzType.BigInt;
                case "BINARY": return EfzType.Binary;
                case "BLOB": return EfzType.Blob;
                case "CHAR": return EfzType.Char;
                case "CHAR(1)": return EfzType.Char;
                case "CLOB": return EfzType.Clob;
                case "DATE": return EfzType.Date;
                case "DOUBLE": return EfzType.Double;
                case "DECIMAL": return EfzType.Decimal;
                case "INT": return EfzType.Int;
                case "INTEGER": return EfzType.Int;
                case "NUMBER": return EfzType.Decimal;
                case "NUMERIC": return EfzType.Decimal;
                case "SMALLINT": return EfzType.SmallInt;
                case "TINYINT": return EfzType.TinyInt;
                case "TIMESTAMP": return EfzType.TimeStamp;
                case "UNIQUEIDENTIFIER": return EfzType.UniqueIdentifier;
                case "VARCHAR": return EfzType.VarChar;
                case "VARCHAR2": return EfzType.VarChar;
                case "VARBINARY": return EfzType.VarBinary;

                default:
                    return EfzType.Variant;
            }
        }

        static public EfzParameter CloneParameter(EfzParameter p)
        {
            ICloneable param = p as ICloneable;
            return param.Clone() as EfzParameter;
        }

        static private Dictionary<Guid, Dictionary<string, EfzParameter>> parameterCache
            = new Dictionary<Guid, Dictionary<string, EfzParameter>>();
    }
}
