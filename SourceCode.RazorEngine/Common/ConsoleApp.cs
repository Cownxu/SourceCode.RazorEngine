using FreeSql;
using FreeSql.DatabaseModel;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Console = Colorful.Console;

namespace SourceCode.RazorEngine.Common
{
    public class ConsoleApp
    {
        string ArgsRazorRaw { get; }
        string ArgsRazor { get; }
        bool[] ArgsNameOptions { get; }
        string ArgsNameSpace { get; }
        DataType ArgsDbType { get; }
        string ArgsConnectionString { get; }
        string ArgsFilter { get; }
        string ArgsMatch { get; }
        string ArgsJson { get; }
        string ArgsFileName { get; }
        bool ArgsReadKey { get; }
        internal string ArgsOutput { get; private set; }

        public ConsoleApp(string[] args, ManualResetEvent wait)
        {
            System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string copyRightTemplatePath = @"Templates\实体类.cshtml";
            var copyRightTemplate = File.ReadAllText(copyRightTemplatePath);
            var serviceTemplateStr = File.ReadAllText(@"Templates\服务层.cshtml");
            var dalTemplateStr1 = File.ReadAllText(@"Templates\数据层1.cshtml");
            var dalTemplateStr2 = File.ReadAllText(@"Templates\数据层2.cshtml");
            var version = "v" + string.Join(".", typeof(ConsoleApp).Assembly.GetName().Version.ToString().Split('.').Where((a, b) => b <= 2));
            Console.WriteFormatted(@"
  # Github # {0} {1}
", Color.SlateGray,
new Colorful.Formatter("https://github.com/Cownxu", Color.DeepSkyBlue),
new Colorful.Formatter("v" + string.Join(".", typeof(ConsoleApp).Assembly.GetName().Version.ToString().Split('.').Where((a, b) => b <= 2)), Color.SlateGray));

            ArgsRazorRaw = "1";
            ArgsRazor = copyRightTemplate;
            ArgsNameOptions = new[] { false, false, false, false };
            ArgsNameSpace = "MyProject";
            ArgsFilter = "";
            ArgsMatch = "";
            ArgsJson = "Newtonsoft.Json";
            ArgsFileName = "{name}.cs";
            ArgsReadKey = true;
            Action<string> setArgsOutput = value =>
            {
                ArgsOutput = value;
                ArgsOutput = ArgsOutput.Trim().TrimEnd('/', '\\');
                ArgsOutput += ArgsOutput.Contains("\\") ? "\\" : "/";
                if (!Directory.Exists(ArgsOutput))
                    Directory.CreateDirectory(ArgsOutput);
            };
            setArgsOutput(Directory.GetCurrentDirectory());

            string args0 = args[0].Trim().ToLower();
            if (args[0] == "?" || args0 == "--help" || args0 == "-help")
            {

                Console.WriteFormatted(@"
    {0}

    更新工具：dotnet tool update -g SourceCode.RazorEngine


  # 快速开始 #

  > {1} {2} 1 {3} 0,0,0,0 {4} MyProject {5} 

     -Razor 1                  * 选择模板：实体类+特性
     -Razor ""d:\diy.cshtml""    * 自定义模板文件，如乱码请修改为UTF8(不带BOM)编码格式

     -NameOptions              * 4个布尔值对应：
                                 首字母大写
                                 首字母大写，其他小写
                                 全部小写
                                 下划线转驼峰

     -NameSpace                * 命名空间

     -DB ""{6},data source=.;integrated security=True;initial catalog=数据库;pooling=true;max pool size=2""
     -DB ""{7},host=192.168.164.10;port=5432;username=postgres;password=123456;database=数据库;pooling=true;maximum pool size=2""
     -DB ""{8},user id=user1;password=123456;data source=//127.0.0.1:1521/XE;pooling=true;max pool size=2""
     -DB ""{9},data source=document.db""
     -DB ""{10},database=localhost:D:\fbdata\EXAMPLES.fdb;user=sysdba;password=123456;max pool size=2""
     -DB ""{11},server=127.0.0.1;port=5236;user id=2user;password=123456789;database=2user;poolsize=2""
     -DB ""{12},server=127.0.0.1;port=54321;uid=USER2;pwd=123456789;database=数据库""
     -DB ""{13},host=192.168.164.10;port=2003;database=数据库;username=SYSDBA;password=szoscar55;maxpoolsize=2""
                               * {10}(达梦数据库)、{11}(人大金仓数据库)、{12}(神舟通用数据库)

     -Filter                   Table

     -Match                    表名或正则表达式，只生成匹配的表，如：dbo\.TB_.+
     -Json                     NTJ、STJ、NONE
                               Newtonsoft.Json、System.Text.Json、不生成

     -FileName                 文件名，默认：{name}.cs
     -Output                   保存路径，默认为当前 shell 所在目录
                               {14}

", Color.SlateGray,
new Colorful.Formatter("SourceCode.RazorEngine 快速生成数据库的实体类", Color.White),
new Colorful.Formatter("SourceCode.RazorEngine", Color.White),
new Colorful.Formatter("-Razor", Color.ForestGreen),
new Colorful.Formatter("-NameOptions", Color.ForestGreen),
new Colorful.Formatter("-NameSpace", Color.ForestGreen),
new Colorful.Formatter("-DB", Color.ForestGreen),
new Colorful.Formatter("SqlServer", Color.Yellow),
new Colorful.Formatter("PostgreSQL", Color.Yellow),
new Colorful.Formatter("Oracle", Color.Yellow),
new Colorful.Formatter("Sqlite", Color.Yellow),
new Colorful.Formatter("Dameng", Color.Yellow),
new Colorful.Formatter("KingbaseES", Color.Yellow),
new Colorful.Formatter("ShenTong", Color.Yellow),
new Colorful.Formatter("Firebird", Color.Yellow),
new Colorful.Formatter("推荐在实体类目录创建 gen.bat，双击它重新所有实体类", Color.ForestGreen)
);
                wait.Set();
                return;
            }
            for (int a = 0; a < args.Length; a++)
            {
                switch (args[a].Trim().ToLower())
                {
                    case "-razor":
                        ArgsRazorRaw = args[a + 1].Trim();
                        switch (ArgsRazorRaw)
                        {
                            case "1":

                                ArgsRazor = copyRightTemplate; break;
                            default: ArgsRazor = File.ReadAllText(args[a + 1], System.Text.Encoding.UTF8); break;
                        }
                        a++;
                        break;

                    case "-nameoptions":
                        ArgsNameOptions = args[a + 1].Split(',').Select(opt => opt == "1").ToArray();
                        if (ArgsNameOptions.Length != 4) throw new ArgumentException(CoreStrings.S_NameOptions_Incorrect);
                        a++;
                        break;
                    case "-namespace":
                        ArgsNameSpace = args[a + 1];
                        a++;
                        break;
                    case "-db":
                        var dbargs = args[a + 1].Split(',', 2);
                        if (dbargs.Length != 2) throw new ArgumentException(CoreStrings.S_DB_ParameterError);

                        switch (dbargs[0].Trim().ToLower())
                        {
                            case "sqlserver": ArgsDbType = DataType.SqlServer; break;
                            case "postgresql": ArgsDbType = DataType.PostgreSQL; break;
                            case "oracle": ArgsDbType = DataType.Oracle; break;
                            case "sqlite": ArgsDbType = DataType.Sqlite; break;
                            case "firebird": ArgsDbType = DataType.Firebird; break;
                            case "dameng": ArgsDbType = DataType.Dameng; break;
                            case "kingbasees": ArgsDbType = DataType.KingbaseES; break;
                            case "shentong": ArgsDbType = DataType.ShenTong; break;
                            case "clickhouse": ArgsDbType = DataType.ClickHouse; break;
                            default: throw new ArgumentException(CoreStrings.S_DB_ParameterError_UnsupportedType(dbargs[0]));
                        }
                        ArgsConnectionString = dbargs[1].Trim();
                        a++;
                        break;
                    case "-filter":
                        ArgsFilter = args[a + 1];
                        a++;
                        break;
                    case "-match":
                        ArgsMatch = args[a + 1];
                        if (Regex.IsMatch("", ArgsMatch)) { } //throw
                        a++;
                        break;
                    case "-json":
                        switch (args[a + 1].Trim().ToLower())
                        {
                            case "none":
                                ArgsJson = "";
                                break;
                            case "stj":
                                ArgsJson = "System.Text.Json";
                                break;
                        }
                        a++;
                        break;
                    case "-filename":
                        ArgsFileName = args[a + 1];
                        a++;
                        break;
                    case "-readkey":
                        ArgsReadKey = args[a + 1].Trim() == "1";
                        a++;
                        break;
                    case "-output":
                        setArgsOutput(args[a + 1]);
                        a++;
                        break;
                    default:
                        throw new ArgumentException(CoreStrings.S_WrongParameter(args[a]));
                }
            }

            if (string.IsNullOrEmpty(ArgsConnectionString)) throw new ArgumentException(CoreStrings.S_DB_Parameter_Error_NoConnectionString);

            Engine.Razor = RazorEngineService.Create(new TemplateServiceConfiguration
            {
                EncodedStringFactory = new RawStringFactory()
            });
            var razorId = Guid.NewGuid().ToString("N");
            Engine.Razor.Compile(ArgsRazor, razorId);

            var outputCounter = 0;
            using (IFreeSql fsql = new FreeSqlBuilder()
                .UseConnectionString(ArgsDbType, ArgsConnectionString, typeof(FreeSql.SqlServer.SqlServerProvider<>))
                .UseAutoSyncStructure(false)
                .UseMonitorCommand(cmd => Console.WriteFormatted(cmd.CommandText + "\r\n", Color.SlateGray))
                .Build())
            {
                List<DbTableInfo> tables = new List<DbTableInfo>();
                if (string.IsNullOrEmpty(ArgsMatch) == false)
                {
                    try
                    {
                        var matchTable = fsql.DbFirst.GetTableByName(ArgsMatch);
                        if (matchTable != null) tables.Add(matchTable);
                    }
                    catch { }
                }
                if (tables.Any() == false)
                    tables = fsql.DbFirst.GetTablesByDatabase();
                var outputTables = tables;

                //开始生成操作
                foreach (var table in outputTables)
                {
                    if (string.IsNullOrEmpty(ArgsMatch) == false)
                    {
                        if (Regex.IsMatch($"{table.Schema}.{table.Name}".TrimStart('.'), ArgsMatch) == false) continue;
                    }
                    if (table.Type != DbTableType.TABLE)
                    {
                        if (ArgsFilter.Contains("Table", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteFormatted(" Ignore Table -> " + table.Name + "\r\n", Color.DarkSlateGray);
                            continue;
                        }
                    }
                    
                    List<DbColumnInfo> columns = new List<DbColumnInfo>();
                    foreach (var item in table.Columns)
                    {
                        if (!new string[] { "Id", "CreateUser", "CreateTime", "ModifyUser", "ModifyTime", "RowVersion", "LogAction" }.Contains(item.Name))
                        {
                            columns.Add(item);
                        }
                    }
                    table.Columns = columns.OrderBy(x => x.Position).ToList();
                    var sw = new StringWriter();
                    var model = new RazorModel(new TaskBuild
                    {
                        NamespaceName = ArgsNameSpace,
                        //OptionsEntity04 = true,

                    }, tables, table);//ArgsNameOptions
                    model.fsql = fsql;
                    Engine.Razor.Run(razorId, sw, null, model);

                    StringBuilder plus = new StringBuilder();
                    plus.Append(sw.ToString());
                    plus.AppendLine();

                    var outputFile = $"{ArgsOutput}{ArgsFileName.Replace("{name}", model.GetCsName(table.Name))}";
                    File.WriteAllText(outputFile, plus.ToString(), System.Text.Encoding.UTF8);
                    switch (table.Type)
                    {
                        case DbTableType.TABLE:
                            Console.WriteFormatted(" OUT Table -> " + outputFile + "\r\n", Color.DeepSkyBlue);
                            break;
                    }
                    ++outputCounter;
                    var service = ArgsOutput + ArgsMatch.Replace("_", "") + "Service.cs";

                    if (File.Exists(@"Templates\服务层.cshtml") && !File.Exists(service))
                    {

                        var tempId = Guid.NewGuid().ToString("N");
                        Engine.Razor.Compile(serviceTemplateStr, tempId);
                        var tempSw = new StringWriter();
                        var tempModel = new RazorModel(new TaskBuild
                        {
                            NamespaceName = "UFX.SCM.BLL",

                        }, tables, table);//ArgsNameOptions
                        tempModel.fsql = fsql;
                        Engine.Razor.Run(tempId, tempSw, null, tempModel);

                        StringBuilder tempPlus = new StringBuilder();
                        tempPlus.Append(tempSw.ToString());
                        tempPlus.AppendLine();

                        var tempOutputFile = $"{ArgsOutput}{ArgsMatch.Replace("_", "") + "Service.cs"}";
                        File.WriteAllText(tempOutputFile, tempPlus.ToString(), System.Text.Encoding.UTF8);
                        Console.WriteFormatted(" OUT Service -> " + tempOutputFile + "\r\n", Color.DeepSkyBlue);
                        ++outputCounter;
                    }
                    var idal = ArgsOutput + $"{"I" + ArgsMatch.Replace("_", "") + "DAL.cs"}";
                    if (File.Exists(@"Templates\数据层1.cshtml") && !File.Exists(idal))
                    {

                        var tempId = Guid.NewGuid().ToString("N");
                        Engine.Razor.Compile(dalTemplateStr1, tempId);
                        var tempSw = new StringWriter();
                        var tempModel = new RazorModel(new TaskBuild
                        {
                            NamespaceName = "UFX.SCM.IDAL",

                        }, tables, table);//ArgsNameOptions
                        tempModel.fsql = fsql;
                        Engine.Razor.Run(tempId, tempSw, null, tempModel);

                        StringBuilder tempPlus = new StringBuilder();
                        tempPlus.Append(tempSw.ToString());
                        tempPlus.AppendLine();

                        var tempOutputFile = $"{ArgsOutput}{"I" + ArgsMatch.Replace("_", "") + "DAL.cs"}";
                        File.WriteAllText(tempOutputFile, tempPlus.ToString(), System.Text.Encoding.UTF8);
                        Console.WriteFormatted(" OUT IDAL -> " + tempOutputFile + "\r\n", Color.DeepSkyBlue);
                        ++outputCounter;
                    }
                    var dal = ArgsOutput + $"{ArgsMatch.Replace("_", "") + "DAL.cs"}";
                    if (File.Exists(@"Templates\数据层2.cshtml") && !File.Exists(dal))
                    {

                        var tempId = Guid.NewGuid().ToString("N");
                        Engine.Razor.Compile(dalTemplateStr2, tempId);
                        var tempSw = new StringWriter();
                        var tempModel = new RazorModel(new TaskBuild
                        {
                            NamespaceName = "UFX.SCM.DAL",

                        }, tables, table);//ArgsNameOptions
                        tempModel.fsql = fsql;
                        Engine.Razor.Run(tempId, tempSw, null, tempModel);

                        StringBuilder tempPlus = new StringBuilder();
                        tempPlus.Append(tempSw.ToString());
                        tempPlus.AppendLine();

                        var tempOutputFile = $"{ArgsOutput}{ArgsMatch.Replace("_", "") + "DAL.cs"}";
                        File.WriteAllText(tempOutputFile, tempPlus.ToString(), System.Text.Encoding.UTF8);
                        Console.WriteFormatted(" OUT DAL -> " + tempOutputFile + "\r\n", Color.DeepSkyBlue);
                        ++outputCounter;
                    }
                }


            }

            var rebuildBat = ArgsOutput + "__重新生成.bat";
            if (File.Exists(rebuildBat) == false)
            {
                var razorCshtml = ArgsOutput + "__razor.cshtml.txt";
                if (File.Exists(razorCshtml) == false)
                {
                    File.WriteAllText(razorCshtml, ArgsRazor, System.Text.Encoding.UTF8);
                    Console.WriteFormatted(" OUT -> " + razorCshtml + "    (以后) 编辑它自定义模板生成\r\n", Color.Magenta);
                    ++outputCounter;
                }

                File.WriteAllText(rebuildBat, $@"
SourceCode.RazorEngine -Razor ""__razor.cshtml.txt"" -NameOptions {string.Join(",", ArgsNameOptions.Select(a => a ? 1 : 0))} -NameSpace {ArgsNameSpace} -DB ""{ArgsDbType},{ArgsConnectionString}""{(string.IsNullOrEmpty(ArgsFilter) ? "" : $" -Filter \"{ArgsFilter}\"")}{(string.IsNullOrEmpty(ArgsMatch) ? "" : $" -Match \"{ArgsMatch}\"")} -FileName ""{ArgsFileName}""
", System.Text.Encoding.UTF8);
                Console.WriteFormatted(" OUT -> " + rebuildBat + "    (以后) 双击它重新生成实体\r\n", Color.Magenta);
                ++outputCounter;
            }

            Console.WriteFormatted($"\r\n[{DateTime.Now.ToString("MM-dd HH:mm:ss")}] 生成完毕，总共生成了 {outputCounter} 个文件，目录：\"{ArgsOutput}\"\r\n", Color.DarkGreen);

            if (ArgsReadKey)
                Console.ReadKey();
            wait.Set();
        }
    }
}
