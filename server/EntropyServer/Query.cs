using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace EntropyServer
{
    class Query
    {
        public DataSet result { get; set; }
        private SqlCeDataAdapter adapter;

        #region Constructor
        public Query(string user_query)
        {
            adapter = new SqlCeDataAdapter( user_query, ConnectionManager.SQLConnection);
            result = new DataSet();
            adapter.Fill(result);
        }
        #endregion

        public void Update()
        {
            adapter.Update(result);
        }
    }
}
