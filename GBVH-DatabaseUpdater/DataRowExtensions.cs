using System;
using System.Data;

namespace GBVH_DatabaseUpdater
{
    public static class DataRowExtensions
    {
        public static int GetInt(this DataRow row, int column)
        {
            return Convert.ToInt32(row[column]);
        }

        public static string GetString(this DataRow row, int column)
        {
            return Convert.ToString(row[column]);
        }
        
        public static float GetFloat(this DataRow row, int column)
        {
            return Convert.ToSingle(row[column]);
        }
        public static int GetInt(this DataRow row, string column)
        {
            return Convert.ToInt32(row[column]);
        }

        public static string GetString(this DataRow row, string column)
        {
            return Convert.ToString(row[column]);
        }
        
        public static float GetFloat(this DataRow row, string column)
        {
            return Convert.ToSingle(row[column]);
        }
    }
}