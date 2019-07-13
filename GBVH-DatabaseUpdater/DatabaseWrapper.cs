using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Net.Mime;

namespace GBVH_DatabaseUpdater
{
    /// <summary>
    /// Базовая обертка для работы с базой
    /// </summary>
    public class DatabaseWrapper
    {
        private string _dbPath;
        private static SQLiteConnection _connection = null;


        /// <summary>
        /// Выполняет запрос без ответа
        /// </summary>
        /// <param name="query">Запрос к базе</param>
        public void ExecuteQueryWithoutAnswer(string query)
        {
            OpenConnection();
            var command = new SQLiteCommand(_connection) {CommandText = query};
            command.ExecuteNonQuery();
            CloseConnection();
        }
        
        /// <summary> Этот метод выполняет запрос query и возвращает ответ запроса. </summary>
        /// <param name="query"> Собственно запрос. </param>
        /// <returns> Возвращает значение 1 строки 1 столбца, если оно имеется. </returns>
        public string ExecuteQueryWithAnswer(string query)
        {
            OpenConnection();
            var command = new SQLiteCommand {CommandText = query, Connection = _connection};
            var answer = command.ExecuteScalar();
            CloseConnection();
            return answer?.ToString();
        }

        /// <summary> Этот метод возвращает таблицу, которая является результатом выборки запроса query. </summary>
        /// <param name="query"> Собственно запрос. </param>
        public DataTable GetTable(string query)
        {
            OpenConnection();

            var adapter = new SQLiteDataAdapter(query, _connection);

            var ds = new DataSet();
            adapter.Fill(ds);
            adapter.Dispose();  

            CloseConnection();

            return ds.Tables[0];
        }

        /// <summary>
        /// Открываем соединение с базой
        /// </summary>
        private void OpenConnection()
        {
             _connection.Open();
        }


        /// <summary>
        /// Инитиализация соединения с базой и подписывается на событие закрытия игры
        /// </summary>
        public DatabaseWrapper(string dbPath)
        {
            _dbPath = dbPath;
            _connection = new SQLiteConnection("Data Source=" + _dbPath);
            Console.WriteLine($"Connected to {_dbPath}");
        }

        /// <summary>
        /// Закрывает соединение с базой, если оно есть
        /// </summary>
        private void CloseConnection()
        {
            _connection.Close();
        }

    }
}