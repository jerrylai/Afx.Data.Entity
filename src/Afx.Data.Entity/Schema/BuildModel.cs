using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 根据数据库结构生成 EntityContext 与 model .cs 文件
    /// </summary>
    public class BuildModel : IBuildModel
    {
        private IDatabaseSchema databaseSchema;
        private ITableSchema tableSchema;
        private IDbContextSchema dbContexSchema;
        private IModelSchema modelSchema;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="databaseSchema"></param>
        /// <param name="tableSchema"></param>
        /// <param name="dbContextSchema"></param>
        /// <param name="modelSchema"></param>
        public BuildModel(IDatabaseSchema databaseSchema, ITableSchema tableSchema,
            IDbContextSchema dbContextSchema, IModelSchema modelSchema)
        {
            if (databaseSchema == null) throw new ArgumentNullException("databaseSchema");
            if (tableSchema == null) throw new ArgumentNullException("tableSchema");
            if (dbContextSchema == null) throw new ArgumentNullException("dbContextSchema");
            if (modelSchema == null) throw new ArgumentNullException("modelSchema");
            this.databaseSchema = databaseSchema;
            this.tableSchema = tableSchema;
            this.dbContexSchema = dbContextSchema;
            this.modelSchema = modelSchema;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="databaseSchema"></param>
        /// <param name="tableSchema"></param>
        /// <param name="modelSchema"></param>
        public BuildModel(IDatabaseSchema databaseSchema, ITableSchema tableSchema, IModelSchema modelSchema)
            : this(databaseSchema, tableSchema, new DbContextSchema(), modelSchema)
        {
        }

        /// <summary>
        /// 生成 model .cs 文件
        /// </summary>
        /// <param name="namespase">.cs 文件命名空间</param>
        /// <param name="dir">存放目录</param>
        public virtual void Build(string @namespase, string dir)
        {
            if (string.IsNullOrEmpty(@namespase)) throw new ArgumentNullException("namespase");
            if (string.IsNullOrEmpty(dir)) throw new ArgumentNullException("dir");
            //if (string.IsNullOrEmpty(dir)) dir = Path.Combine(Path.GetPathRoot(Directory.GetCurrentDirectory()), "BuildModels");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            List<TableInfoModel> tables = tableSchema.GetTables();
            List<string> models = new List<string>();
            string modeldir = Path.Combine(dir, "Models");
            if (!Directory.Exists(modeldir)) Directory.CreateDirectory(modeldir);
            foreach (var t in tables)
            {
                var columnInfos = tableSchema.GetTableColumns(t.Name);
                if (columnInfos.Count > 0)
                {
                    string modelName = modelSchema.GetModelName(t.Name);
                    models.Add(modelName);
                    string code = modelSchema.GetModelCode(t, columnInfos, @namespase);
                    string csfile = Path.Combine(modeldir, modelName + ".cs");
                    File.WriteAllText(csfile, code, Encoding.UTF8);
                }

                string dbname = databaseSchema.GetDatabase();
                string dbcode = dbContexSchema.GetContextCode(dbname, models, @namespase);
                string dbcsfile = Path.Combine(dir, dbname + "Context.cs");
                File.WriteAllText(dbcsfile, dbcode, Encoding.UTF8);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (databaseSchema != null) databaseSchema.Dispose();
            if (dbContexSchema != null) dbContexSchema.Dispose();
            if (modelSchema != null) modelSchema.Dispose();
            if (tableSchema != null) tableSchema.Dispose();

            this.databaseSchema = null;
            this.dbContexSchema = null;
            this.modelSchema = null;
            this.tableSchema = null;
        }
    }
}
