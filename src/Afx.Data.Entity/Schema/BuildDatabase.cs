using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if NETCOREAPP || NETSTANDARD
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 用于更新数据库结构类型
    /// </summary>
    public class BuildDatabase : IBuildDatabase
    {
        private IDatabaseSchema databaseSchema;
        private ITableSchema tableSchema;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="databaseSchema">获取数据库结构信息接口</param>
        /// <param name="tableSchema">表架构接口</param>
        public BuildDatabase(IDatabaseSchema databaseSchema, ITableSchema tableSchema)
        {
            if (databaseSchema == null) throw new ArgumentNullException("databaseSchema");
            if(tableSchema == null) throw new ArgumentNullException("tableSchema");
            this.databaseSchema = databaseSchema;
            this.tableSchema = tableSchema;
        }

        /// <summary>
        /// 创建、更新数据库结构，添加 更新、删除索引， 列只增加，不修改，不删除
        /// </summary>
        /// <typeparam name="T">EntityContext</typeparam>
        public virtual void Build<T>() where T : EntityContext
        {
            bool count = databaseSchema.Exist();
            if (count == false)
                databaseSchema.CreateDatabase();

            List<TableInfoModel> tables = tableSchema.GetTables();
            List<Type> modelTypes = tableSchema.GetModelType<T>();
            foreach (var t in modelTypes)
            {
                var tb = tableSchema.GetTableName(t);
                if (!tables.Exists(q=>string.Equals(q.Name, tb.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    var columns = tableSchema.GetModelColumns(t, tb.Name);
                    tableSchema.CreateTable(tb, columns);
                }
                else
                {
                    var tableColumns = tableSchema.GetTableColumns(tb.Name);
                    var modelColumns = tableSchema.GetModelColumns(t, tb.Name);
                    List<IndexModel> addIndexs = new List<IndexModel>();
                    List<string> delIndexs = new List<string>();
                    foreach (var mColumn in modelColumns)
                    {
                        var tColumn = tableColumns.Find(q => q.Name.Equals(mColumn.Name, StringComparison.OrdinalIgnoreCase));
                        if (tColumn == null)
                        {
                            tableSchema.AddColumn(tb.Name, mColumn);
                            if(mColumn.Indexs != null && mColumn.Indexs.Count > 0)
                            {
                                foreach(var index in mColumn.Indexs)
                                {
                                    if (string.IsNullOrEmpty(index.Name)) continue;
                                    if (string.IsNullOrEmpty(index.ColumnName)) index.ColumnName = mColumn.Name;
                                    addIndexs.Add(index);
                                }
                            }
                        }
                        else if (mColumn.Indexs != null && mColumn.Indexs.Count > 0)
                        {
                            // 添加数据库不存在索引
                            foreach(var index in mColumn.Indexs)
                            {
                                if (string.IsNullOrEmpty(index.Name)) continue;
                                if (string.IsNullOrEmpty(index.ColumnName)) index.ColumnName = mColumn.Name;

                                if(tColumn.Indexs == null || tColumn.Indexs.Count == 0)
                                {
                                    addIndexs.Add(index);
                                }
                                else
                                {
                                    var tindex = tColumn.Indexs.Find(q => index.Name.Equals(q.Name, StringComparison.OrdinalIgnoreCase));
                                    if(tindex == null || tindex.IsUnique != index.IsUnique)
                                    {
                                        addIndexs.Add(index);
                                    }
                                }
                            }

                            // 删除数据库多余索引
                            if (tColumn.Indexs != null && tColumn.Indexs.Count > 0)
                            {
                                foreach (var tindex in tColumn.Indexs)
                                {
                                    if (mColumn.Indexs == null || mColumn.Indexs.Count == 0)
                                    {
                                        if (!delIndexs.Contains(tindex.Name, StringComparer.OrdinalIgnoreCase))
                                            delIndexs.Add(tindex.Name);
                                    }
                                    else
                                    {
                                        var index = mColumn.Indexs.Find(q => tindex.Name.Equals(q.Name, StringComparison.OrdinalIgnoreCase));
                                        if (index == null || tindex.IsUnique != index.IsUnique)
                                        {
                                            if (!delIndexs.Contains(tindex.Name, StringComparer.OrdinalIgnoreCase))
                                                delIndexs.Add(tindex.Name);
                                        }
                                    }
                                }
                            }
                        }

                    }
                    foreach(var indexs in delIndexs)
                    {
                        tableSchema.DeleteIndex(tb.Name, indexs);
                    }

                    if(addIndexs.Count > 0) tableSchema.AddIndex(tb.Name, addIndexs);
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (databaseSchema != null) databaseSchema.Dispose();
            if (tableSchema != null) tableSchema.Dispose();

            this.databaseSchema = null;
            this.tableSchema = null;
        }
    }
}
