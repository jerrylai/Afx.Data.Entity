using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#if NETCOREAPP || NETSTANDARD
using Microsoft.EntityFrameworkCore;
using IndexAttribute = System.ComponentModel.DataAnnotations.IndexAttribute;
#else
using System.Data.Entity;
#endif

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 表结构接口
    /// </summary>
    public abstract class TableSchema : ITableSchema
    {
        /// <summary>
        /// 执行sql logs
        /// </summary>
        public abstract Action<string> Log { get; set; }

        /// <summary>
        /// 获取数据库所有表名
        /// </summary>
        /// <returns>数据库所有表名</returns>
        public abstract List<TableInfoModel> GetTables();

        /// <summary>
        /// 创建数据库表
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="columns">列信息</param>
        /// <returns>是否成功</returns>
        public abstract bool CreateTable(TableInfoModel table, List<ColumnInfoModel> columns);

        //public abstract bool DeleteTable(string table);

        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="column">列信息</param>
        /// <returns>是否成功</returns>
        public abstract bool AddColumn(string table, ColumnInfoModel column);

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="indexs">索引列信息</param>
        public abstract void AddIndex(string table, List<IndexModel> indexs);

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="index">索引列信息</param>
        /// <returns>是否成功</returns>
        public abstract bool AddIndex(string table, IndexModel index);

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="indexName">索引名称</param>
        /// <param name="isUnique">是否唯一索引</param>
        /// <param name="columns">列名</param>
        /// <returns>是否成功</returns>
        public abstract bool AddIndex(string table, string indexName, bool isUnique, List<string> columns);

        //public abstract bool DeleteColumn(string table, ColumnInfoModel column);

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="index">索引名称</param>
        /// <returns>是否成功</returns>
        public abstract bool DeleteIndex(string table, string index);

        //public abstract bool AlterColumn(string table, ColumnInfoModel column);

        //public abstract bool EqualColumn(ColumnInfoModel column1, ColumnInfoModel column2);

        /// <summary>
        /// 获取表列信息
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>列信息</returns>
        public abstract List<ColumnInfoModel> GetTableColumns(string table);

        /// <summary>
        /// 获取所有model type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual List<Type> GetModelType<T>() where T : EntityContext
        {
            var dbContextType = typeof(T);
            var arr = dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<Type> list = new List<Type>(arr != null ? arr.Length : 0);
            var dbsettype = typeof(DbSet<>);
            foreach (var p in arr)
            {
                if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == dbsettype)
                {
                    var mt = p.PropertyType.GetGenericArguments();
                    if (mt != null && mt.Length == 1)
                    {
                        list.Add(mt[0]);
                    }
                }
            }

            return list;
        }

        private Dictionary<string, System.Xml.XmlDocument> dicDoc = new Dictionary<string, System.Xml.XmlDocument>();
        protected virtual string GetComment(Assembly assembly, string name)
        {
            string comment = string.Empty;
            System.Xml.XmlDocument xml = null;
            var xmlFile = assembly.Location;
            int i = xmlFile.LastIndexOf('.');
            if(i > 0)
            {
                xmlFile = xmlFile.Substring(0, i) + ".xml";
                if (!dicDoc.TryGetValue(xmlFile, out xml))
                {
                    if (System.IO.File.Exists(xmlFile))
                    {
                        using (var fs = System.IO.File.Open(xmlFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                        {
                            xml = new System.Xml.XmlDocument();
                            xml.Load(fs);
                        }
                    }
                }
            }

            if(xml != null)
            {
                var nodes = xml.SelectNodes("/doc/members");
                foreach (System.Xml.XmlNode node in nodes)
                {
                    if (node is System.Xml.XmlElement)
                    {
                        var el = node as System.Xml.XmlElement;
                        if (el.GetAttribute("name") == name)
                        {
                            var cn = el.SelectSingleNode("summary");
                            if (cn != null)
                            {
                                comment = cn.InnerText ?? string.Empty;
                            }
                            break;
                        }
                    }
                }
            }

            return comment;
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="modelType">model type</param>
        /// <returns>表名</returns>
        public virtual TableInfoModel GetTableName(Type modelType)
        {
            if (modelType == null) throw new ArgumentNullException("modelType");
            string result = modelType.Name;
            var attrs = modelType.GetCustomAttributes(typeof(TableAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                var att = attrs[0] as TableAttribute;
                if (!string.IsNullOrEmpty(att.Name))
                    result = att.Name;
            }

            return new TableInfoModel() { Name = result, Comment = GetComment(modelType.Assembly,"T:" + modelType.FullName) };
        }

        /// <summary>
        /// 获取model 属性列信息
        /// </summary>
        /// <param name="modelType">model type</param>
        /// <param name="table">表名</param>
        /// <returns>列信息</returns>
        public virtual List<ColumnInfoModel> GetModelColumns(Type modelType, string table)
        {
            if (modelType == null) throw new ArgumentNullException("modelType");
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            var propertys = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<ColumnInfoModel> list = new List<ColumnInfoModel>(propertys != null ? propertys.Length : 0);
            if (propertys != null && propertys.Length > 0)
            {
                foreach (var p in propertys)
                {
                    var column = this.GetColumnInfo(p, table);
                    if (column != null)
                        list.Add(column);
                }
            }

            return list;
        }
        
        private void GetColumnInLength(PropertyInfo property, out int maxLength, out int minLength)
        {
            maxLength = 0;
            minLength = 0;
            if (typeof(string) == property.PropertyType
                || typeof(byte[]) == property.PropertyType
                || typeof(char[]) == property.PropertyType)
            {
                var atts = property.GetCustomAttributes(typeof(MaxLengthAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    var att = atts[0] as MaxLengthAttribute;
                    maxLength = att.Length;
                }
                else if (typeof(string) == property.PropertyType)
                {
                    atts = property.GetCustomAttributes(typeof(StringLengthAttribute), false);
                    if (atts != null && atts.Length > 0)
                    {
                        var att = atts[0] as StringLengthAttribute;
                        maxLength = att.MaximumLength;
                        minLength = att.MinimumLength;
                    }
                }
            }
            else if (typeof(decimal) == property.PropertyType)
            {
                var atts = property.GetCustomAttributes(typeof(DecimalAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    var att = atts[0] as DecimalAttribute;
                    maxLength = att.Precision;
                    minLength = att.Scale;
                }
            }
        }

        /// <summary>
        /// 获取列信息
        /// </summary>
        /// <param name="property">model 属性</param>
        /// <param name="table">表名</param>
        /// <returns>列信息</returns>
        public virtual ColumnInfoModel GetColumnInfo(PropertyInfo property, string table)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            ColumnInfoModel m = new ColumnInfoModel();
            m.Name = property.Name;
            int maxlength = 0;
            int minlength = 0;
            GetColumnInLength(property, out maxlength, out minlength);
            Type propertyType = property.PropertyType;
            m.IsNullable = false;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                m.IsNullable = true;
                propertyType = propertyType.GetGenericArguments()[0];
            }
            string dataType = this.GetColumnType(propertyType, maxlength, minlength);
            if (string.IsNullOrEmpty(dataType))
            {
                throw new Exception("未找到" + property.PropertyType.FullName + "对应的数据库类型!");
            }
            m.DataType = dataType;
            m.MaxLength = maxlength;
            m.MinLength = minlength;
            var atts = property.GetCustomAttributes(typeof(ColumnAttribute), false);
            if (atts != null && atts.Length > 0)
            {
                var att = atts[0] as ColumnAttribute;
                if (!string.IsNullOrEmpty(att.Name))
                    m.Name = att.Name;
            }

            m.IsAutoIncrement = false;
            if (propertyType == typeof(int) || propertyType == typeof(uint)
                || propertyType == typeof(long) || propertyType == typeof(ulong))
            {
                atts = property.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    var att = atts[0] as DatabaseGeneratedAttribute;
                    if (att.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                        m.IsAutoIncrement = true;
                }
            }

            m.IsKey = false;
            m.IsNonClustered = false;
            atts = property.GetCustomAttributes(typeof(KeyAttribute), false);
            if (atts != null && atts.Length > 0)
            {
                m.IsKey = true;
                atts = property.GetCustomAttributes(typeof(NonClusteredAttribute), false);
                m.IsNonClustered = atts != null && atts.Length > 0;
            }
            else
            {
                if (!propertyType.IsValueType)
                {
                    m.IsNullable = true;
                    atts = property.GetCustomAttributes(typeof(RequiredAttribute), false);
                    if (atts != null && atts.Length > 0)
                    {
                        m.IsNullable = false;
                    }
                }

                atts = property.GetCustomAttributes(typeof(IndexAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    m.Indexs = new List<IndexModel>(atts.Length);
                    foreach (var o in atts)
                    {
                        var att = o as IndexAttribute;
                        var index = new IndexModel();
                        m.Indexs.Add(index);
                        index.ColumnName = m.Name;
                        index.IsUnique = att.IsUnique;
                        if (!string.IsNullOrEmpty(att.Name))
                            index.Name = att.Name;
                        else
                            m.Name = string.Format("IX_{0}_{1}", table, m.Name);
                    }
                }
            }

            m.Comment = this.GetComment(property.DeclaringType.Assembly, "P:" + property.DeclaringType.FullName + "." + property.Name) ?? string.Empty;

            return m;
        }

        /// <summary>
        /// 获取列数据库类型
        /// </summary>
        /// <param name="propertyType">model 属性类型</param>
        /// <param name="maxLength">类型最大长度</param>
        /// <param name="minLength">类型最小长度</param>
        /// <returns>列数据库类型</returns>
        public abstract string GetColumnType(Type propertyType, int maxLength, int minLength);

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
           if(this.dicDoc!= null) dicDoc.Clear();
            dicDoc = null;
        }
    }
}
