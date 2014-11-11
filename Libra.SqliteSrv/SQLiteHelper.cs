using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Data;

namespace DBUtility
{


    /// 
    /// SQLiteHelper 的摘要说明 
    /// 
    public class SQLiteHelper
    {
        private SQLiteHelper()
        {
            // 
            // TODO: 在此处添加构造函数逻辑 
            // 
        }

        #region 静态私有方法
        /// 
        /// 附加参数 
        /// 
        /// 
        /// 
        private static void AttachParameters(SQLiteCommand command, SQLiteParameter[] commandParameters)
        {
            command.Parameters.Clear();
            foreach (SQLiteParameter p in commandParameters)
            {
                if (p.Direction == ParameterDirection.InputOutput && p.Value == null)
                    p.Value = DBNull.Value;
                command.Parameters.Add(p);
            }
        }
        /// 
        /// 分配参数值 
        /// 
        /// 
        /// 
        private static void AssignParameterValues(SQLiteParameter[] commandParameters, object[] parameterValues)
        {
            if (commandParameters == null || parameterValues == null)
                return;
            if (commandParameters.Length != parameterValues.Length)
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }
        /// 
        /// 预备执行command命令 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        private static void PrepareCommand(SQLiteCommand command,
        SQLiteConnection connection, SQLiteTransaction transaction,
        CommandType commandType, string commandText, SQLiteParameter[] commandParameters
        )
        {
            if (commandType == CommandType.StoredProcedure)
            {
                throw new ArgumentException("SQLite 暂时不支持存储过程");
            }
            if (connection.State != ConnectionState.Open)
                connection.Open();
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
                command.Transaction = transaction;
            command.CommandType = commandType;
            if (commandParameters != null)
                AttachParameters(command, commandParameters);
            return;
        }
        #endregion

        #region 加密
        public static void ChangePassword(string connectionString, string password) 
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString)) 
            {
                connection.Open();
                connection.ChangePassword(password);
                connection.Close();
            }          
        }
        #endregion

        #region ExecuteNonQuery 执行SQL命令，返回影响行数
        /// 
        /// 执行SQL命名 
        /// 
        /// 
        /// 
        /// 
        /// 
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connectionString, commandType, commandText, (SQLiteParameter[])null);
        }
        /// 
        /// 不支持存储过程，但可以参数化查询 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SQLiteParameter[] commandParameters)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                return ExecuteNonQuery(conn, commandType, commandText, commandParameters);
            }
        }
        public static int ExecuteNonQuery(SQLiteConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connection, commandType, commandText, (SQLiteParameter[])null);
        }
        public static int ExecuteNonQuery(SQLiteConnection connection, CommandType commandType, string commandText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            PrepareCommand(cmd, connection, (SQLiteTransaction)null, commandType, commandText, commandParameters);
            int retval = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return retval;
        }

        public static int ExecuteNonQuery(SQLiteTransaction trans, CommandType commandType, string commandText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            PrepareCommand(cmd, trans.Connection, trans, commandType, commandText, commandParameters);
            int retval = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return retval;
        }

        public static SQLiteTransaction GetTransaction(string connection) 
        {
            SQLiteConnection conn = new SQLiteConnection(connection);            
            conn.Open();            
            return conn.BeginTransaction();
        }
        #endregion

        #region ExecuteDataSet 执行SQL查询，并将返回数据填充到DataSet
        public static DataSet ExecuteDataset(SQLiteConnection connection, CommandType commandType, string commandText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            PrepareCommand(cmd, connection, (SQLiteTransaction)null, commandType, commandText, commandParameters);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            cmd.Parameters.Clear();
            return ds;
        }
        public static DataSet ExecuteDataset(SQLiteConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connection, commandType, commandText, (SQLiteParameter[])null);
        }
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SQLiteParameter[] commandParameters)
        {

            using (SQLiteConnection cn = new SQLiteConnection(connectionString))
            {
                cn.Open();

                return ExecuteDataset(cn, commandType, commandText, commandParameters);
            }
        }
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connectionString, commandType, commandText, (SQLiteParameter[])null);
        }
        #endregion

        #region ExecuteReader 执行SQL查询,返回DbDataReader
        public static SQLiteDataReader ExecuteReader(SQLiteConnection connection, SQLiteTransaction transaction, CommandType commandType, string commandText, SQLiteParameter[] commandParameters, DbConnectionOwnership connectionOwnership)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);
            SQLiteDataReader dr;
            if (connectionOwnership == DbConnectionOwnership.External)
                dr = cmd.ExecuteReader();
            else
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return dr;
        }
        /// 
        ///读取数据后将自动关闭连接 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        public static SQLiteDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SQLiteParameter[] commandParameters)
        {
            SQLiteConnection cn = new SQLiteConnection(connectionString);
            cn.Open();
            try
            {
                return ExecuteReader(cn, null, commandType, commandText, commandParameters, DbConnectionOwnership.Internal);
            }
            catch
            {
                cn.Close();
                throw;
            }
        }
        /// 
        /// 读取数据后将自动关闭连接 
        /// 
        /// 
        /// 
        /// 
        /// 
        public static SQLiteDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteReader(connectionString, commandType, commandText, (SQLiteParameter[])null);
        }
        /// 
        /// 读取数据以后需要自行关闭连接 
        /// 
        /// 
        /// 
        /// 
        /// 
        public static SQLiteDataReader ExecuteReader(SQLiteConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(connection, commandType, commandText, (SQLiteParameter[])null);
        }
        /// 
        /// 读取数据以后需要自行关闭连接 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        public static SQLiteDataReader ExecuteReader(SQLiteConnection connection, CommandType commandType, string commandText, params SQLiteParameter[] commandParameters)
        {
            return ExecuteReader(connection, (SQLiteTransaction)null, commandType, commandText, commandParameters, DbConnectionOwnership.External);
        }

        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connectionString, commandType, commandText, (SQLiteParameter[])null);
        }
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SQLiteParameter[] commandParameters)
        {
            SQLiteConnection cn = new SQLiteConnection(connectionString);
            cn.Open();
            try
            {
                return ExecuteScalar(cn, commandType, commandText, commandParameters);
            }
            catch
            {

                return 0; ;
            }
            finally
            {
                cn.Close();
            }

        }
        public static object ExecuteScalar(SQLiteConnection connection, CommandType commandType, string commandText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            PrepareCommand(cmd, connection, (SQLiteTransaction)null, commandType, commandText, commandParameters);
            var obj = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return obj;
        }
        #endregion
    }


    /// <SUMMARY></SUMMARY> 
    /// DbConnectionOwnership DataReader以后是否自动关闭连接 
    /// 
    public enum DbConnectionOwnership
    {
        /// <SUMMARY></SUMMARY> 
        /// 自动关闭 
        /// 
        Internal,
        /// <SUMMARY></SUMMARY> 
        /// 手动关闭 
        /// 
        External,
    }
}
