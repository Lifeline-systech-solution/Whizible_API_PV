using AutoMapper;
using AutoMapper.Data;
using Dapper;
using InitiativeNextGen.Infra.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.General;

namespace WhizibleTeams.Application.Repository
{
    public abstract partial class BaseRepository<T> : IRepository<T> where T : class
    {

        private readonly string _connectionString;
        protected readonly MSSQL _db;
        public static string? _iPAddress { get; set; }
        public static string? _userName { get; set; }


        public BaseRepository(string connectionString)
        {
            //string connectionString = ConnectionStringManager.Instance.ConnectionString;
            _connectionString = connectionString; // ?? throw new ArgumentNullException(nameof(connectionString));
            _db = new MSSQL();
        }

        public BaseRepository()
        {
            string connectionString = ConnectionStringManager.Instance.ConnectionString;
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _db = new MSSQL();
        }

        public BaseRepository(string ipAddress, string userName)
        {
            string connectionString = ConnectionStringManager.Instance.ConnectionString;
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _db = new MSSQL();
            _iPAddress = ipAddress;
            _userName = userName;
        }




    }


    public abstract partial class BaseRepository<T>
    {
        //Added By Dipali V On 10th Jan 2026 For Get Data in datatable 
        public async Task<DataSet> GetDataSetAsync(string storedProcedure, List<SqlParameter> parameters)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(storedProcedure, con)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Count > 0)
                cmd.Parameters.AddRange(parameters.ToArray());

            var ds = new DataSet();
            using var da = new SqlDataAdapter(cmd);

            await Task.Run(() => da.Fill(ds));
            return ds;
        }
        //End of Added By Dipali V On 10th Jan 2026 For Get Data in datatable 


        internal void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        internal String[] GetUniqueName(String[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                int duplicatecount = 2;
                for (int j = i + 1; j < names.Length; j++)
                {
                    if (names[i] == names[j])
                    {
                        names[j] = names[j] + duplicatecount.ToString();
                        duplicatecount = duplicatecount + 1;
                    }
                }
            }
            return names;
        }
    }

    public abstract partial class BaseRepository<T>
    {
        public async Task<dynamic> GetAsyncSP<T1>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            try
            {
                List<T1> t1 = null;
                var config = new MapperConfiguration(cfg => {
                    cfg.CreateMap<IDataRecord, T1>();
                    cfg.AddDataReaderMapping();
                }
                );
                var mapper = new Mapper(config);
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
                {
                    int count = 0;
                    bool HasResult = true;
                    while (HasResult)
                    {
                        var readerschema = reader.GetSchemaTable();
                        //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                        //Debug.WriteLine("-" + tableName);
                        count = count + 1;
                        switch (count)
                        {
                            case 1:
                                t1 = mapper.Map<IDataReader, List<T1>>(reader);
                                break;
                        }
                        if (reader.NextResult())
                        {
                            HasResult = true;
                        }
                        else
                        {

                            HasResult = false;
                        }
                    }
                }
                dynamic retval = new ExpandoObject();
                AddProperty(retval, typeof(T1).Name, t1);
                return retval;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<dynamic> GetAsyncSP<T1, T2>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    //var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            dynamic retval = new ExpandoObject();
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name });
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            dynamic retval = new ExpandoObject();
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name });
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            dynamic retval = new ExpandoObject();
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name });
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            dynamic retval = new ExpandoObject();
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name });
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            List<T12> t12 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.CreateMap<IDataRecord, T12>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                        case 12:
                            t12 = mapper.Map<IDataReader, List<T12>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name, typeof(T12).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            AddProperty(retval, dynamicNames[11], t12);
            return retval;
        }


        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            List<T12> t12 = null;
            List<T13> t13 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.CreateMap<IDataRecord, T12>();
                cfg.CreateMap<IDataRecord, T13>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                        case 12:
                            t12 = mapper.Map<IDataReader, List<T12>>(reader);
                            break;
                        case 13:
                            t13 = mapper.Map<IDataReader, List<T13>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name, typeof(T12).Name, typeof(T13).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            AddProperty(retval, dynamicNames[11], t12);
            AddProperty(retval, dynamicNames[12], t13);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            List<T12> t12 = null;
            List<T13> t13 = null;
            List<T14> t14 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.CreateMap<IDataRecord, T12>();
                cfg.CreateMap<IDataRecord, T13>();
                cfg.CreateMap<IDataRecord, T14>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                        case 12:
                            t12 = mapper.Map<IDataReader, List<T12>>(reader);
                            break;
                        case 13:
                            t13 = mapper.Map<IDataReader, List<T13>>(reader);
                            break;
                        case 14:
                            t14 = mapper.Map<IDataReader, List<T14>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name, typeof(T12).Name, typeof(T13).Name, typeof(T14).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            AddProperty(retval, dynamicNames[11], t12);
            AddProperty(retval, dynamicNames[12], t13);
            AddProperty(retval, dynamicNames[13], t14);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            List<T12> t12 = null;
            List<T13> t13 = null;
            List<T14> t14 = null;
            List<T15> t15 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.CreateMap<IDataRecord, T12>();
                cfg.CreateMap<IDataRecord, T13>();
                cfg.CreateMap<IDataRecord, T14>();
                cfg.CreateMap<IDataRecord, T15>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                        case 12:
                            t12 = mapper.Map<IDataReader, List<T12>>(reader);
                            break;
                        case 13:
                            t13 = mapper.Map<IDataReader, List<T13>>(reader);
                            break;
                        case 14:
                            t14 = mapper.Map<IDataReader, List<T14>>(reader);
                            break;
                        case 15:
                            t15 = mapper.Map<IDataReader, List<T15>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name, typeof(T12).Name, typeof(T13).Name, typeof(T14).Name, typeof(T15).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            AddProperty(retval, dynamicNames[11], t12);
            AddProperty(retval, dynamicNames[12], t13);
            AddProperty(retval, dynamicNames[13], t14);
            AddProperty(retval, dynamicNames[14], t15);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            List<T12> t12 = null;
            List<T13> t13 = null;
            List<T14> t14 = null;
            List<T15> t15 = null;
            List<T16> t16 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.CreateMap<IDataRecord, T12>();
                cfg.CreateMap<IDataRecord, T13>();
                cfg.CreateMap<IDataRecord, T14>();
                cfg.CreateMap<IDataRecord, T15>();
                cfg.CreateMap<IDataRecord, T16>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                        case 12:
                            t12 = mapper.Map<IDataReader, List<T12>>(reader);
                            break;
                        case 13:
                            t13 = mapper.Map<IDataReader, List<T13>>(reader);
                            break;
                        case 14:
                            t14 = mapper.Map<IDataReader, List<T14>>(reader);
                            break;
                        case 15:
                            t15 = mapper.Map<IDataReader, List<T15>>(reader);
                            break;
                        case 16:
                            t16 = mapper.Map<IDataReader, List<T16>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name, typeof(T12).Name, typeof(T13).Name, typeof(T14).Name, typeof(T15).Name, typeof(T16).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            AddProperty(retval, dynamicNames[11], t12);
            AddProperty(retval, dynamicNames[12], t13);
            AddProperty(retval, dynamicNames[13], t14);
            AddProperty(retval, dynamicNames[14], t15);
            AddProperty(retval, dynamicNames[15], t16);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            List<T12> t12 = null;
            List<T13> t13 = null;
            List<T14> t14 = null;
            List<T15> t15 = null;
            List<T16> t16 = null;
            List<T17> t17 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.CreateMap<IDataRecord, T12>();
                cfg.CreateMap<IDataRecord, T13>();
                cfg.CreateMap<IDataRecord, T14>();
                cfg.CreateMap<IDataRecord, T15>();
                cfg.CreateMap<IDataRecord, T16>();
                cfg.CreateMap<IDataRecord, T17>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                        case 12:
                            t12 = mapper.Map<IDataReader, List<T12>>(reader);
                            break;
                        case 13:
                            t13 = mapper.Map<IDataReader, List<T13>>(reader);
                            break;
                        case 14:
                            t14 = mapper.Map<IDataReader, List<T14>>(reader);
                            break;
                        case 15:
                            t15 = mapper.Map<IDataReader, List<T15>>(reader);
                            break;
                        case 16:
                            t16 = mapper.Map<IDataReader, List<T16>>(reader);
                            break;
                        case 17:
                            t17 = mapper.Map<IDataReader, List<T17>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name, typeof(T12).Name, typeof(T13).Name, typeof(T14).Name, typeof(T15).Name, typeof(T16).Name, typeof(T17).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            AddProperty(retval, dynamicNames[11], t12);
            AddProperty(retval, dynamicNames[12], t13);
            AddProperty(retval, dynamicNames[13], t14);
            AddProperty(retval, dynamicNames[14], t15);
            AddProperty(retval, dynamicNames[15], t16);
            AddProperty(retval, dynamicNames[16], t17);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            List<T12> t12 = null;
            List<T13> t13 = null;
            List<T14> t14 = null;
            List<T15> t15 = null;
            List<T16> t16 = null;
            List<T17> t17 = null;
            List<T18> t18 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.CreateMap<IDataRecord, T12>();
                cfg.CreateMap<IDataRecord, T13>();
                cfg.CreateMap<IDataRecord, T14>();
                cfg.CreateMap<IDataRecord, T15>();
                cfg.CreateMap<IDataRecord, T16>();
                cfg.CreateMap<IDataRecord, T17>();
                cfg.CreateMap<IDataRecord, T18>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                        case 12:
                            t12 = mapper.Map<IDataReader, List<T12>>(reader);
                            break;
                        case 13:
                            t13 = mapper.Map<IDataReader, List<T13>>(reader);
                            break;
                        case 14:
                            t14 = mapper.Map<IDataReader, List<T14>>(reader);
                            break;
                        case 15:
                            t15 = mapper.Map<IDataReader, List<T15>>(reader);
                            break;
                        case 16:
                            t16 = mapper.Map<IDataReader, List<T16>>(reader);
                            break;
                        case 17:
                            t17 = mapper.Map<IDataReader, List<T17>>(reader);
                            break;
                        case 18:
                            t18 = mapper.Map<IDataReader, List<T18>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name, typeof(T12).Name, typeof(T13).Name, typeof(T14).Name, typeof(T15).Name, typeof(T16).Name, typeof(T17).Name, typeof(T18).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            AddProperty(retval, dynamicNames[11], t12);
            AddProperty(retval, dynamicNames[12], t13);
            AddProperty(retval, dynamicNames[13], t14);
            AddProperty(retval, dynamicNames[14], t15);
            AddProperty(retval, dynamicNames[15], t16);
            AddProperty(retval, dynamicNames[16], t17);
            AddProperty(retval, dynamicNames[17], t18);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            List<T12> t12 = null;
            List<T13> t13 = null;
            List<T14> t14 = null;
            List<T15> t15 = null;
            List<T16> t16 = null;
            List<T17> t17 = null;
            List<T18> t18 = null;
            List<T19> t19 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.CreateMap<IDataRecord, T12>();
                cfg.CreateMap<IDataRecord, T13>();
                cfg.CreateMap<IDataRecord, T14>();
                cfg.CreateMap<IDataRecord, T15>();
                cfg.CreateMap<IDataRecord, T16>();
                cfg.CreateMap<IDataRecord, T17>();
                cfg.CreateMap<IDataRecord, T18>();
                cfg.CreateMap<IDataRecord, T19>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                        case 12:
                            t12 = mapper.Map<IDataReader, List<T12>>(reader);
                            break;
                        case 13:
                            t13 = mapper.Map<IDataReader, List<T13>>(reader);
                            break;
                        case 14:
                            t14 = mapper.Map<IDataReader, List<T14>>(reader);
                            break;
                        case 15:
                            t15 = mapper.Map<IDataReader, List<T15>>(reader);
                            break;
                        case 16:
                            t16 = mapper.Map<IDataReader, List<T16>>(reader);
                            break;
                        case 17:
                            t17 = mapper.Map<IDataReader, List<T17>>(reader);
                            break;
                        case 18:
                            t18 = mapper.Map<IDataReader, List<T18>>(reader);
                            break;
                        case 19:
                            t19 = mapper.Map<IDataReader, List<T19>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name, typeof(T12).Name, typeof(T13).Name, typeof(T14).Name, typeof(T15).Name, typeof(T16).Name, typeof(T17).Name, typeof(T18).Name, typeof(T19).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            AddProperty(retval, dynamicNames[11], t12);
            AddProperty(retval, dynamicNames[12], t13);
            AddProperty(retval, dynamicNames[13], t14);
            AddProperty(retval, dynamicNames[14], t15);
            AddProperty(retval, dynamicNames[15], t16);
            AddProperty(retval, dynamicNames[16], t17);
            AddProperty(retval, dynamicNames[17], t18);
            AddProperty(retval, dynamicNames[18], t19);
            return retval;
        }

        public async Task<dynamic> GetAsyncSP<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            List<T1> t1 = null;
            List<T2> t2 = null;
            List<T3> t3 = null;
            List<T4> t4 = null;
            List<T5> t5 = null;
            List<T6> t6 = null;
            List<T7> t7 = null;
            List<T8> t8 = null;
            List<T9> t9 = null;
            List<T10> t10 = null;
            List<T11> t11 = null;
            List<T12> t12 = null;
            List<T13> t13 = null;
            List<T14> t14 = null;
            List<T15> t15 = null;
            List<T16> t16 = null;
            List<T17> t17 = null;
            List<T18> t18 = null;
            List<T19> t19 = null;
            List<T20> t20 = null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.CreateMap<IDataRecord, T2>();
                cfg.CreateMap<IDataRecord, T3>();
                cfg.CreateMap<IDataRecord, T4>();
                cfg.CreateMap<IDataRecord, T5>();
                cfg.CreateMap<IDataRecord, T6>();
                cfg.CreateMap<IDataRecord, T7>();
                cfg.CreateMap<IDataRecord, T8>();
                cfg.CreateMap<IDataRecord, T9>();
                cfg.CreateMap<IDataRecord, T10>();
                cfg.CreateMap<IDataRecord, T11>();
                cfg.CreateMap<IDataRecord, T12>();
                cfg.CreateMap<IDataRecord, T13>();
                cfg.CreateMap<IDataRecord, T14>();
                cfg.CreateMap<IDataRecord, T15>();
                cfg.CreateMap<IDataRecord, T16>();
                cfg.CreateMap<IDataRecord, T17>();
                cfg.CreateMap<IDataRecord, T18>();
                cfg.CreateMap<IDataRecord, T19>();
                cfg.CreateMap<IDataRecord, T20>();
                cfg.AddDataReaderMapping();
            }
            );
            var mapper = new Mapper(config);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
            {
                int count = 0;
                bool HasResult = true;
                while (HasResult)
                {
                    var readerschema = reader.GetSchemaTable();
                    //string tableName = (string)readerschema.Rows[0]["BaseTableName"];
                    //Debug.WriteLine("-" + tableName);
                    count = count + 1;
                    switch (count)
                    {
                        case 1:
                            t1 = mapper.Map<IDataReader, List<T1>>(reader);
                            break;
                        case 2:
                            t2 = mapper.Map<IDataReader, List<T2>>(reader);
                            break;
                        case 3:
                            t3 = mapper.Map<IDataReader, List<T3>>(reader);
                            break;
                        case 4:
                            t4 = mapper.Map<IDataReader, List<T4>>(reader);
                            break;
                        case 5:
                            t5 = mapper.Map<IDataReader, List<T5>>(reader);
                            break;
                        case 6:
                            t6 = mapper.Map<IDataReader, List<T6>>(reader);
                            break;
                        case 7:
                            t7 = mapper.Map<IDataReader, List<T7>>(reader);
                            break;
                        case 8:
                            t8 = mapper.Map<IDataReader, List<T8>>(reader);
                            break;
                        case 9:
                            t9 = mapper.Map<IDataReader, List<T9>>(reader);
                            break;
                        case 10:
                            t10 = mapper.Map<IDataReader, List<T10>>(reader);
                            break;
                        case 11:
                            t11 = mapper.Map<IDataReader, List<T11>>(reader);
                            break;
                        case 12:
                            t12 = mapper.Map<IDataReader, List<T12>>(reader);
                            break;
                        case 13:
                            t13 = mapper.Map<IDataReader, List<T13>>(reader);
                            break;
                        case 14:
                            t14 = mapper.Map<IDataReader, List<T14>>(reader);
                            break;
                        case 15:
                            t15 = mapper.Map<IDataReader, List<T15>>(reader);
                            break;
                        case 16:
                            t16 = mapper.Map<IDataReader, List<T16>>(reader);
                            break;
                        case 17:
                            t17 = mapper.Map<IDataReader, List<T17>>(reader);
                            break;
                        case 18:
                            t18 = mapper.Map<IDataReader, List<T18>>(reader);
                            break;
                        case 19:
                            t19 = mapper.Map<IDataReader, List<T19>>(reader);
                            break;
                        case 20:
                            t20 = mapper.Map<IDataReader, List<T20>>(reader);
                            break;
                    }
                    if (reader.NextResult())
                    {
                        HasResult = true;
                    }
                    else
                    {

                        HasResult = false;
                    }
                }
            }
            String[] dynamicNames = GetUniqueName(new String[] { typeof(T1).Name, typeof(T2).Name, typeof(T3).Name, typeof(T4).Name, typeof(T5).Name, typeof(T6).Name, typeof(T7).Name, typeof(T8).Name, typeof(T9).Name, typeof(T10).Name, typeof(T11).Name, typeof(T12).Name, typeof(T13).Name, typeof(T14).Name, typeof(T15).Name, typeof(T16).Name, typeof(T17).Name, typeof(T18).Name, typeof(T19).Name, typeof(T20).Name });
            dynamic retval = new ExpandoObject();
            AddProperty(retval, dynamicNames[0], t1);
            AddProperty(retval, dynamicNames[1], t2);
            AddProperty(retval, dynamicNames[2], t3);
            AddProperty(retval, dynamicNames[3], t4);
            AddProperty(retval, dynamicNames[4], t5);
            AddProperty(retval, dynamicNames[5], t6);
            AddProperty(retval, dynamicNames[6], t7);
            AddProperty(retval, dynamicNames[7], t8);
            AddProperty(retval, dynamicNames[8], t9);
            AddProperty(retval, dynamicNames[9], t10);
            AddProperty(retval, dynamicNames[10], t11);
            AddProperty(retval, dynamicNames[11], t12);
            AddProperty(retval, dynamicNames[12], t13);
            AddProperty(retval, dynamicNames[13], t14);
            AddProperty(retval, dynamicNames[14], t15);
            AddProperty(retval, dynamicNames[15], t16);
            AddProperty(retval, dynamicNames[16], t17);
            AddProperty(retval, dynamicNames[17], t18);
            AddProperty(retval, dynamicNames[18], t19);
            AddProperty(retval, dynamicNames[19], t20);
            return retval;
        }


    }


    public abstract partial class BaseRepository<T> : IRepository<T> where T : class
    {
        public async Task<T1?> GetByIdAsyncSP<T1>(string storedProcedureName, Guid id)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>{
                new SqlParameter("@pId", id)
            };

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<IDataRecord, T1>();
                cfg.AddDataReaderMapping();
            });

            var mapper = new Mapper(config);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlDataReader reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, storedProcedureName, sqlParameters))
                {
                    // Read only the first result set (assuming it contains the records you want)
                    if (reader.HasRows)
                    {
                        var results = mapper.Map<IDataReader, List<T1>>(reader);
                        return results.FirstOrDefault(); // Return the first item or null if no items.
                    }
                }
            }
            return default(T1); // Return null if there were no results or any other issues.
        }

        public async Task<T1> GetScalarAsyncSP<T1>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();

            foreach (var param in sqlParameters)
            {
                parameters.Add(param.ParameterName, param.Value, direction: ParameterDirection.Input);
            }

            return await connection.ExecuteScalarAsync<T1>(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
        }

        public virtual async Task<int> AddAsyncSP(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            //sqlParameters.Add(new SqlParameter("@pUserName", _userName ?? ""));
            //sqlParameters.Add(new SqlParameter("@pIPAddress", _iPAddress ?? ""));
            var retval = await new MSSQL().ExecuteNonQuerySPAsync(_connectionString, storedProcedureName, sqlParameters);
            return retval;
        }

        public virtual async Task<int> UpdateAsyncSP(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            //sqlParameters.Add(new SqlParameter("@pUserName", _userName ?? ""));
            //sqlParameters.Add(new SqlParameter("@pIPAddress", _iPAddress ?? ""));
            var retval = await new MSSQL().ExecuteNonQuerySPAsync(_connectionString, storedProcedureName, sqlParameters);
            return retval;
        }

        public virtual async Task<int> DeleteByIdAsyncSP(string storedProcedureName, object id)
        {
            var retval = await new MSSQL().ExecuteNonQuerySPAsync(_connectionString,
                storedProcedureName,
                new List<SqlParameter>
                {
                    new SqlParameter("@pId", id)
                });
            return retval;
        }

        public virtual async Task<int> DeleteAsyncSP(string storedProcedureName)
        {
            var retval = await new MSSQL().ExecuteNonQuerySPAsync(_connectionString,
                storedProcedureName,
                new List<SqlParameter>
                {
                });
            return retval;
        }

        public virtual async Task<int> DeletesAsyncSP(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            var retval = await new MSSQL().ExecuteNonQuerySPAsync(_connectionString,
                storedProcedureName,
                sqlParameters);
            return retval;
        }

        public virtual async Task<int> UpdateCellAsyncSP(string storedProcedureName, Guid id, string tableName, string columnName, string newValue)
        {
            var retval = await new MSSQL().ExecuteNonQuerySPAsync(_connectionString, storedProcedureName, new List<SqlParameter>
            {
                new SqlParameter("@pId", id),
                new SqlParameter("@pTableName", tableName ?? ""),
                new SqlParameter("@pColumnName", columnName ?? ""),
                new SqlParameter("@newValue", newValue),
                new SqlParameter("@pUserName", _userName ?? ""),
                new SqlParameter("@pIPAddress", _iPAddress ?? "")
            });
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure without return value (INSERT/UPDATE/DELETE operations)
        /// </summary>
        public virtual async Task<int> ExecuteSPAsync(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            var retval = await new MSSQL().ExecuteNonQuerySPAsync(_connectionString, storedProcedureName, sqlParameters);
            return retval;
        }
    public async Task<dynamic> GetDynamicAsyncSP(string storedProcedureName, List<SqlParameter> sqlParameters)

        {

            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            foreach (var param in sqlParameters)

            {

                parameters.Add(

                    param.ParameterName,

                    param.Value,

                    direction: ParameterDirection.Input);

            }

            using var multi = await connection.QueryMultipleAsync(

                storedProcedureName,

                parameters,

                commandType: CommandType.StoredProcedure);

            var data = (await multi.ReadAsync())

                .Select(x => (IDictionary<string, object>)x)

                .ToList();

            var result = new ExpandoObject() as IDictionary<string, object>;

            result.Add("Data", data);

            List<IDictionary<string, object>> pagination = null;

            while (!multi.IsConsumed)
            {
                var table = (await multi.ReadAsync())
                    .Select(x => (IDictionary<string, object>)x)
                    .ToList();

                if (table.Count > 0 &&
                    table[0].ContainsKey("CurrentPage") &&
                    table[0].ContainsKey("PageSize") &&
                    table[0].ContainsKey("TotalRecords"))
                {
                    pagination = table;
                    break;
                }
            }

            result.Add("Pagination", pagination ?? new List<IDictionary<string, object>>());

            return result;

        } 

     }
}
