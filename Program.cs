﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;
using LitJson;
using System.Diagnostics;
using ShellProgressBar;

public static class ProgressBarExtensions
{
    public static IEnumerable<T> WithProgressBar<T>(this IEnumerable<T> enumerable, string message = "Processing...")
    {
        int totalCount = enumerable.Count();
        int processedItems = 0;

        var stopwatch = Stopwatch.StartNew();

        var progressBar = new ShellProgressBar.ProgressBar(totalCount, message, new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = true
        });

        foreach (var item in enumerable)
        {
            yield return item;

            processedItems++;
            progressBar.Tick();

            if (processedItems > 0)
            {
                var elapsed = stopwatch.Elapsed;
                var estimatedTotalTime = TimeSpan.FromSeconds(elapsed.TotalSeconds / processedItems * totalCount);
                var estimatedTimeRemaining = estimatedTotalTime - elapsed;
                progressBar.Message = $"{message} ({processedItems}/{totalCount}) Estimated time remaining: {estimatedTimeRemaining}";
            }
        }

        stopwatch.Stop();
        progressBar.Message = $"{message}已完成，总用时：{stopwatch.Elapsed}";
        progressBar.Dispose();
    }
}


public class LinkInfo
{
    public string UserID { get; set; }
    public string ItemID { get; set; }

    public string VoterID { get; set; }
    // 其他字段...
}

public class UserIDInfo
{
    // 设为double形是为了后面更好的取平均值
    public int Level { get; set; }
    public int Gender { get; set; }

    public int Age { get; set; }

    // 性别的独热编码
    public double[] GenderOneHot { get; set; }

    // 该用户分享过的用户
    public Dictionary<string, List<DateTime>> Neighbor { get; set; }
    public Dictionary<string, List<DateTime>> NewNeighbor { get; set; }
    public Dictionary<int, Dictionary<string, double>> Ratio { get; set; }
    public List<DateTime> ResponesTime { get; set; }
    public HashSet<string> ItemID { get; set; }
    public Dictionary<int, double> responseTimeZone { get; set; }
    public Dictionary<string, double> StaticSimUsers { get; set; }
    public Dictionary<string, double> SimLusers;
    public Dictionary<string, double> SimLLusers { get; set; }
    public Dictionary<string, double> SimFFusers { get; set; }
    public Dictionary<string, int> ItemPath { get; set; }
    public Dictionary<string, double> SimFusers { get; set; }
    public Dictionary<int, HashSet<string>> ItemPart { get; set; }
}

public class SubmitInfo
{
    public string triple_id { get; set; }

    public string[] candidate_voter_list { get; set; }
    // 其他字段...
}

public class ItemInfo
{
    public string ShopId { get; set; }
    public string BrandId { get; set; }
    public string CateId { get; set; }

    public string CateLevelOneId { get; set; }
    // 其他字段...
}

namespace tianchi
{
    internal class Program : Form
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var program = new Program();
            program.NewSumbit();
        }
        private void NewSumbit()
        {
            var shareDataPath = @"./data/item_share_train_info_09.json"; //定义商品分享数据文件的路径
            var itemDataPath = @"./data/item_info.json"; //定义商品信息数据文件的路径
            var userDataPath = @"./data/user_info.json"; //定义用户信息数据文件的路径
            var testDataPath = @"./data/item_share_train_info_test_01.json"; //定义测试数据文件的路径
            var jsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(shareDataPath)); //读取并解析商品分享数据文件
            var itemjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(itemDataPath)); //读取并解析商品信息数据文件
            var userjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(userDataPath)); //读取并解析用户信息数据文件
            var testjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(testDataPath)); //读取并解析测试数据文件
            var data = new SortedDictionary<DateTime, List<LinkInfo>>(); //初始化一个根据日期排序的商品分享链路信息字典
            DateTime dt; //定义一个日期时间变量dt
            LinkInfo lk; //定义一个链路信息变量lk
            int n; //定义一个整型变量n
            string itemid; //定义一个字符串变量itemid，表示商品id
            string id1, id2; //定义两个字符串变量id1和id2
            var users = new Dictionary<string, UserIDInfo>(); //初始化一个用户信息字典，Key为用户id，Value为用户信息
            var itemsinfo = new Dictionary<string, ItemInfo>(); //初始化一个商品信息字典，Key为商品id，Value为商品信息
            var itemreceive = new Dictionary<string, Dictionary<string, HashSet<string>>>(); //初始化一个嵌套字典结构，记录商品被接收的信息
            var netrelation = new Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<string, double>>>>(); //初始化一个复杂嵌套字典结构，记录网络关系信息
            var responseitems = new Dictionary<string, Dictionary<string, HashSet<string>>>(); //初始化一个嵌套字典结构，记录用户响应商品的信息
            var sharerank = new Dictionary<string, Dictionary<string, double>>(); //初始化一个嵌套字典结构，记录商品分享的排名信息
            string item; //定义一个字符串变量item，表示商品
            var ranks = new Dictionary<string, Dictionary<string, SortedDictionary<DateTime, List<string>>>>(); //初始化一个嵌套字典结构，记录商品的排名信息
            Dictionary<string, HashSet<string>> sharenum = new Dictionary<string, HashSet<string>>(), responseall = new Dictionary<string, HashSet<string>>(); //初始化两个字典，分别记录商品分享次数和所有的响应信息
            var responsenum = new Dictionary<string, Dictionary<string, HashSet<string>>>(); //初始化一个嵌套字典结构，记录用户响应次数信息
            var activenum = new Dictionary<string, double>(); //初始化一个字典，记录用户活跃数信息
            var allNeigbors = new Dictionary<string, Dictionary<string, HashSet<string>>>(); //初始化一个嵌套字典结构，记录所有邻居信息
            var sharetimes = new Dictionary<string, int>(); //初始化一个字典，记录商品分享次数信息
            var activefrenq = new Dictionary<string, double>(); //初始化一个字典，记录活跃频率信息
            var userresptime = new Dictionary<string, List<DateTime>>(); //初始化一个字典，记录用户响应时间信息
            Dictionary<string, Dictionary<string, SortedSet<DateTime>>> shareusers = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(), responstimeAllitems = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(), sharetrainitem = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(); //初始化三个嵌套字典，记录分享用户信息、所有商品的响应时间信息和分享商品信息
            var userclassinfo = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>(); //初始化一个嵌套字典结构，记录用户类别信息
            var itemuserclassinfo = new Dictionary<string, Dictionary<string, HashSet<string>>>(); //初始化一个嵌套字典结构，记录商品的用户类别信息
            var shareresponse = new Dictionary<string, Dictionary<string, List<double>>>(); //初始化一个嵌套字典结构，记录分享和响应信息
            var classtype = new Dictionary<string, string>(); //初始化一个字典，记录类别类型信息
            var classtypeweight = new Dictionary<string, double>(); //初始化一个字典，记录类别权重信息
            var classtypekey = new List<string>() { "Brand", "CateID", "CateOne", "Shop" }; //初始化一个列表，记录类别关键字信息
            var weight = new List<double>() { 0.1, 0.1, 0.5, 0.3 }; //初始化一个列表，记录权重信息
            n = 0; //重置n的值为0
            foreach (var xid in classtypekey.ToList().WithProgressBar("Processing 1...")) //遍历类别关键字列表，执行以下操作：
            {
                classtype.Add(xid, ""); //将当前关键字和空字符串添加到类别类型字典中
                classtypeweight.Add(xid, weight[n++]); //将当前关键字和对应的权重添加到类别权重字典中
            }
            n = 0; //重置n的值为0
            foreach (var xid in classtype.Keys.WithProgressBar("Processing 2...")) //遍历类别类型字典的键，执行以下操作：
            {
                userclassinfo.Add(xid, new Dictionary<string, Dictionary<string, int>>()); //将当前键和一个新的嵌套字典添加到用户类别信息字典中
                itemuserclassinfo.Add(xid, new Dictionary<string, HashSet<string>>()); //将当前键和一个新的嵌套字典添加到商品的用户类别信息字典中
            }


            // 这段代码的主要工作是遍历用户数据文件，并为每个用户创建和初始化各种属性和统计数据。这些属性和数据包括：
            // 用户ID、等级、性别、年龄、邻居、新邻居、比率、响应时间、项目ID、响应时间区、静态相似用户、网络关系、响应项目、分享排名、分享数量、
            // 响应数量、活跃数量、所有邻居、活跃频率以及用户响应时间。这段代码的策略主要是通过构建各种字典和集合，
            // 对用户的属性和行为进行详细的追踪和统计，从而为之后的预测工作提供依据。
            foreach (JsonData temp in userjsonData.Cast<JsonData>().ToList().WithProgressBar("Processing 用户数据...")) //遍历用户数据json文件
            {
                var user_id = temp[0]; //提取用户id
                var level = temp[3]; //提取用户等级
                id1 = user_id.ToString(); //将用户id转换为字符串
                users.Add(id1, new UserIDInfo()); //将新用户添加到users字典中，并为其创建新的UserIDInfo实例
                users[id1].SimLLusers = new Dictionary<string, double>(); //初始化字典来记录每个用户分享回流item的次数（如果分享一次，折算为2）
                users[id1].SimFFusers = new Dictionary<string, double>(); //初始化字典来记录是否已分享，已分享记为1；
                responstimeAllitems.Add(id1, new Dictionary<string, SortedSet<DateTime>>()); //初始化嵌套字典，存储用户的所有响应时间
                responseall.Add(id1, new HashSet<string>()); //初始化集合，存储用户的所有响应
                shareresponse.Add(id1, new Dictionary<string, List<double>>()); //初始化嵌套字典，存储用户分享和响应的信息
                sharetrainitem.Add(id1, new Dictionary<string, SortedSet<DateTime>>()); //初始化嵌套字典，存储用户分享商品的信息
                users[id1].ItemPath = new Dictionary<string, int>(); //初始化字典，存储用户的项目路径
                users[id1].Level = int.Parse(level.ToString()); //将用户等级转换为整数并存储
                users[id1].Gender = int.Parse(temp[1].ToString()); //将用户性别转换为整数并存储
                users[id1].Age = int.Parse(temp[2].ToString()); //将用户年龄转换为整数并存储
                users[id1].Neighbor = new Dictionary<string, List<DateTime>>(); //初始化字典，存储用户的邻居信息
                users[id1].NewNeighbor = new Dictionary<string, List<DateTime>>(); //初始化字典，存储用户的新邻居信息
                users[id1].Ratio = new Dictionary<int, Dictionary<string, double>>(); //初始化嵌套字典，存储用户的比率信息
                users[id1].ResponesTime = new List<DateTime>(); //初始化列表，存储用户的响应时间信息
                users[id1].ItemID = new HashSet<string>(); //初始化集合，存储用户的项目id信息
                users[id1].responseTimeZone = new Dictionary<int, double>(); //初始化字典，存储用户的响应时间区信息
                users[id1].StaticSimUsers = new Dictionary<string, double>(); //初始化字典，存储用户的静态相似用户信息
                netrelation.Add(id1, new Dictionary<string, Dictionary<int, Dictionary<string, double>>>()); //初始化嵌套字典，存储网络关系信息
                responseitems.Add(id1, new Dictionary<string, HashSet<string>>()); //初始化嵌套字典，存储响应项目信息
                sharerank.Add(id1, new Dictionary<string, double>()); //初始化字典，存储分享排名信息
                sharenum.Add(id1, new HashSet<string>()); //初始化集合，存储分享数量信息
                responsenum.Add(id1, new Dictionary<string, HashSet<string>>()); //初始化嵌套字典，存储响应数量信息
                activenum.Add(id1, 0); //初始化字典，存储活跃数量信息，先将其设置为0
                allNeigbors.Add(id1, new Dictionary<string, HashSet<string>>()); //初始化嵌套字典，存储所有邻居信息
                allNeigbors[id1].Add("F", new HashSet<string>()); //初始化集合，存储一阶邻居信息
                allNeigbors[id1].Add("L", new HashSet<string>()); //初始化集合，存储一阶邻居信息
                allNeigbors[id1].Add("FF", new HashSet<string>()); //初始化集合，存储一阶邻居信息
                allNeigbors[id1].Add("LF", new HashSet<string>()); //初始化集合，存储二阶邻居信息
                allNeigbors[id1].Add("XF", new HashSet<string>()); //初始化集合，存储三阶邻居信息
                activefrenq.Add(id1, 0); //初始化字典，存储活跃频率信息，先将其设置为0
                userresptime.Add(id1, new List<DateTime>()); //初始化列表，存储用户响应时间信息
            }

            // 这段代码的主要工作是遍历商品数据文件，并为每个商品创建和初始化各种属性和统计数据。这些属性和数据包括：
            // 商品ID、商店ID、品牌ID、分类ID和等级ID。与之前的用户数据处理类似，
            // 这段代码的策略主要是通过构建字典来跟踪和统计商品的属性，从而为之后的预测工作提供依据。
            foreach (JsonData temp in itemjsonData.Cast<JsonData>().ToList().WithProgressBar("Processing 商品数据...")) //遍历商品数据json文件
            {
                var item_id = temp[0]; //提取商品id
                var cate_id = temp[1]; //提取分类id
                var level_id = temp[2]; //提取等级id
                var brandid = temp[3]; //提取品牌id
                var shopid = temp[4]; //提取商店id
                itemid = item_id.ToString(); //将商品id转换为字符串
                itemsinfo.Add(itemid, new ItemInfo()); //将新商品添加到itemsinfo字典中，并为其创建新的ItemInfo实例
                itemsinfo[itemid].ShopId = shopid.ToString(); //将商店id转换为字符串并存储
                itemsinfo[itemid].BrandId = brandid.ToString(); //将品牌id转换为字符串并存储
                itemsinfo[itemid].CateId = cate_id.ToString(); //将分类id转换为字符串并存储
                itemsinfo[itemid].CateLevelOneId = level_id.ToString(); //将等级id转换为字符串并存储
                itemreceive.Add(itemid, new Dictionary<string, HashSet<string>>()); //初始化嵌套字典，存储商品接收信息
                //ItemAll.Add(itemid,new SortedDictionary<DateTime, List<Tuple<string, string>>>()); //此行代码被注释掉，本来用于初始化嵌套字典，存储商品所有信息
            }

            // 此段代码主要用于处理训练数据。在遍历训练数据的过程中，它会提取每个JSON对象中的user_id, item_id, voter_id, 和timestamp，
            // 并使用这些信息来创建LinkInfo对象。然后，根据这些信息，它会更新多个字典结构，这些字典结构用于保存不同类型的用户和商品信息，
            // 如回应时间，排名，分享数，回应数，活跃数和分类信息。这段代码使用了特征工程的方法，
            // 通过提取和组合原始数据中的信息来生成用于预测voter_id的特征。在遍历结束后，每个用户和商品都会有一个与其相关的详细信息集，
            // 这些信息将用于预测阶段。
            foreach (JsonData temp in jsonData.Cast<JsonData>().ToList().WithProgressBar("Processing 训练数据...")) //遍历训练数据
            {
                var user_id = temp[0]; //提取用户id
                var item_id = temp[1]; //提取物品id
                var voter_id = temp[2]; //提取投票者id
                var timestamp = temp[3]; //提取时间戳
                lk = new LinkInfo(); //创建新的LinkInfo实例
                id1 = lk.UserID = user_id.ToString(); //将用户id转换为字符串并赋值给UserID
                item = lk.ItemID = item_id.ToString(); //将物品id转换为字符串并赋值给ItemID
                id2 = lk.VoterID = voter_id.ToString(); //将投票者id转换为字符串并赋值给VoterID
                dt = DateTime.Parse(timestamp.ToString()); //将时间戳转换为DateTime对象
                if (!responstimeAllitems[id2].ContainsKey(item))
                    responstimeAllitems[id2].Add(item, new SortedSet<DateTime>()); //如果回应时间列表中没有该项，就添加
                responstimeAllitems[id2][item].Add(dt); //在回应时间列表中添加时间戳
                if (!data.ContainsKey(dt)) data.Add(dt, new List<LinkInfo>()); //如果数据中没有该时间，就添加
                data[dt].Add(lk); //在数据中添加LinkInfo对象

                if (!ranks.ContainsKey(id1))
                    ranks.Add(id1, new Dictionary<string, SortedDictionary<DateTime, List<string>>>()); //如果排名中没有该用户，就添加
                if (!ranks[id1].ContainsKey(item)) ranks[id1].Add(item, new SortedDictionary<DateTime, List<string>>()); //如果用户中没有该物品，就添加
                if (!ranks[id1][item].ContainsKey(dt)) ranks[id1][item].Add(dt, new List<string>()); //如果物品中没有该时间，就添加
                ranks[id1][item][dt].Add(id2); //在指定时间和物品中添加投票者id
                sharenum[id1].Add(item); //在分享数中添加物品id
                if (!responsenum[id1].ContainsKey(id2)) responsenum[id1].Add(id2, new HashSet<string>()); //如果回应数中没有该投票者，就添加
                responsenum[id1][id2].Add(item); //在指定投票者中添加物品id
                activenum[id2] += 1; //增加投票者活动数

                classtype["Brand"] = itemsinfo[item].BrandId; //获取商品的品牌id
                classtype["CateID"] = itemsinfo[item].CateId; //获取商品的分类id
                classtype["CateOne"] = itemsinfo[item].CateLevelOneId; //获取商品的一级分类id
                classtype["Shop"] = itemsinfo[item].ShopId; //获取商品的商店id
                foreach (var xid in classtype.Keys) //遍历分类类型
                {
                    if (!userclassinfo[xid].ContainsKey(id1))
                        userclassinfo[xid].Add(id1, new Dictionary<string, int>()); //如果用户分类信息中没有该用户，就添加
                    if (!userclassinfo[xid][id1].ContainsKey(classtype[xid]))
                        userclassinfo[xid][id1].Add(classtype[xid], 0); //如果用户分类信息中没有该分类，就添加
                    userclassinfo[xid][id1][classtype[xid]]++; //增加指定分类的计数

                    if (!userclassinfo[xid].ContainsKey(id2))
                        userclassinfo[xid].Add(id2, new Dictionary<string, int>()); //如果用户分类信息中没有该投票者，就添加
                    if (!userclassinfo[xid][id2].ContainsKey(classtype[xid]))
                        userclassinfo[xid][id2].Add(classtype[xid], 0); //如果投票者分类信息中没有该分类，就添加
                    userclassinfo[xid][id2][classtype[xid]]++; //增加指定分类的计数

                    if (!itemuserclassinfo[xid].ContainsKey(classtype[xid]))
                        itemuserclassinfo[xid].Add(classtype[xid], new HashSet<string>()); //如果物品用户分类信息中没有该分类，就添加
                    itemuserclassinfo[xid][classtype[xid]].Add(id1); //在指定分类中添加用户id
                    itemuserclassinfo[xid][classtype[xid]].Add(id2); //在指定分类中添加投票者id
                }

                ++n; //增加计数器
            }


            jsonData.Clear();
            userjsonData.Clear();
            itemjsonData.Clear();

            // 此代码段主要用于计算每个用户对每个物品的分享排名。它遍历ranks字典，该字典保存了每个用户对每个物品在不同日期的投票情况。
            // 然后，对于每个用户、物品和日期，计算一个排名分数，该分数为1除以ii和tt的乘积，其中ii为指定用户、物品和日期的投票者数量，tt始终为1。
            // 这个分数加入到sharerank字典中，该字典保存了每个用户对每个投票者的总排名分数。然后，它初始化了几个字典，这些字典用于保存后续需要使用的数据。
            // 最后，它计算了data字典中的最大和最小日期。
            foreach (var id in ranks.Keys.WithProgressBar("Processing 3...")) //遍历所有的用户id
            {
                double tt = 1; //初始化tt值为1
                foreach (var fid in ranks[id].Keys) //遍历指定用户id对应的所有物品id
                {
                    double ii = 1; //初始化ii值为1

                    foreach (var d in ranks[id][fid].Keys) //遍历指定用户id和物品id对应的所有日期
                    {
                        foreach (var xid in ranks[id][fid][d]) //遍历指定用户id、物品id和日期对应的所有投票者id
                            if (!sharerank[id].ContainsKey(xid)) sharerank[id].Add(xid, 1.0 / ii / tt); //如果分享排名中没有指定的投票者id，就添加并初始化评分
                            else sharerank[id][xid] += 1.0 / ii / tt; //如果分享排名中已有指定的投票者id，就累加评分
                        ii += ranks[id][fid][d].Count; //累加指定用户id、物品id和日期对应的投票者数量
                    }
                }
            }

            Dictionary<string, Dictionary<string, HashSet<string>>> dataLF = new Dictionary<string, Dictionary<string, HashSet<string>>>(), //初始化dataLF，键为用户id，值为一个字典，该字典的键为物品id，值为投票者id的集合
                dataItemLF = new Dictionary<string, Dictionary<string, HashSet<string>>>(); //初始化dataItemLF，键为物品id，值为一个字典，该字典的键为用户id，值为投票者id的集合

            var items = new Dictionary<string, List<DateTime>>(); //初始化items，键为物品id，值为日期的列表
            var itemusers = new Dictionary<string, HashSet<string>>(); //初始化itemusers，键为物品id，值为用户id的集合
            DateTime dtmax = data.Keys.Max(), dtmin = data.Keys.Min(); //找出data中的最大和最小日期

            // 此段代码主要在处理数据集中的所有链接信息，并将处理的结果存储到多个字典中。这些字典包括：用户的响应时间、用户的响应物品、用户的分享训练物品、用户的邻居、
            // 用户的网络关系、用户的物品ID、物品的日期、物品的用户、物品的接收者、用户的活动频率、用户的分享响应、用户的响应物品以及用户的所有邻居。

            //此代码使用了几种数据结构：SortedSet < DateTime >（排序的日期集合）、Dictionary<int, Dictionary<string, double>>（用户之间的网络关系），以及一些其他用于存储用户、物品和日期的字典。
            //在处理过程中，代码对每种物品信息（如类别一级ID、类别ID、商店ID和品牌ID）的出现次数进行了统计，并计算了分享响应的衰减函数。此外，代码还对投票者在过去30天内的活动频率进行了统计。

            //此代码的核心策略是通过遍历数据集中的所有链接信息，提取并统计各种信息，以便于后续的预测任务。
            foreach (var d in data.Keys.WithProgressBar("Processing 4...")) //遍历所有的日期
            {
                foreach (var llk in data[d]) //遍历指定日期的所有链接信息
                {
                    itemid = llk.ItemID; //获取物品ID
                    id1 = llk.UserID; //获取用户ID
                    id2 = llk.VoterID; //获取投票者ID
                    userresptime[id2].Add(d); //将日期添加到指定投票者的响应时间

                    responseall[id2].Add(itemid); //将物品ID添加到指定投票者的响应集合中
                    if (!sharetrainitem[id1].ContainsKey(itemid)) //如果分享训练物品字典中指定用户对应的字典没有指定物品ID
                        sharetrainitem[id1].Add(itemid, new SortedSet<DateTime>()); //则添加物品ID并初始化日期集合
                    sharetrainitem[id1][itemid].Add(d); //将日期添加到指定用户和物品ID的分享训练物品集合中
                    if (!users[id1].Neighbor.ContainsKey(id2)) //如果指定用户的邻居字典没有指定投票者ID
                    {
                        netrelation[id1].Add(id2, new Dictionary<int, Dictionary<string, double>>()); //在网络关系字典中添加投票者ID并初始化内层字典
                        users[id1].Neighbor.Add(id2, new List<DateTime>()); //在用户字典的邻居字典中添加投票者ID并初始化日期列表
                    }

                    for (var ii = 0; ii < 4; ++ii) //遍历0到3的所有数字
                        if (!netrelation[id1][id2].ContainsKey(ii)) //如果网络关系字典中指定用户和投票者对应的字典没有指定数字
                            netrelation[id1][id2].Add(ii, new Dictionary<string, double>()); //则添加数字并初始化内层字典

                    //以下四个if-else结构对应四种物品信息：类别一级ID、类别ID、商店ID和品牌ID，用于在网络关系字典中统计各类物品信息的出现次数
                    if (!netrelation[id1][id2][0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                        netrelation[id1][id2][0].Add(itemsinfo[itemid].CateLevelOneId, 1);
                    else netrelation[id1][id2][0][itemsinfo[itemid].CateLevelOneId]++;
                    if (!netrelation[id1][id2][1].ContainsKey(itemsinfo[itemid].CateId))
                        netrelation[id1][id2][1].Add(itemsinfo[itemid].CateId, 1);
                    else netrelation[id1][id2][1][itemsinfo[itemid].CateId]++;
                    if (!netrelation[id1][id2][2].ContainsKey(itemsinfo[itemid].ShopId))
                        netrelation[id1][id2][2].Add(itemsinfo[itemid].ShopId, 1);
                    else netrelation[id1][id2][2][itemsinfo[itemid].ShopId]++;
                    if (!netrelation[id1][id2][3].ContainsKey(itemsinfo[itemid].BrandId))
                        netrelation[id1][id2][3].Add(itemsinfo[itemid].BrandId, 1);
                    else netrelation[id1][id2][3][itemsinfo[itemid].BrandId]++;

                    //添加日期到指定用户和投票者的邻居日期列表，添加日期到指定投票者的响应时间列表，添加物品ID到指定用户和投票者的物品ID列表
                    users[id1].Neighbor[id2].Add(d);
                    users[id2].ResponesTime.Add(d);
                    users[id1].ItemID.Add(llk.ItemID);
                    users[id2].ItemID.Add(llk.ItemID);

                    if (!items.ContainsKey(llk.ItemID)) //如果物品字典没有指定物品ID
                        items.Add(llk.ItemID, new List<DateTime>()); //则添加物品ID并初始化日期列表
                    items[llk.ItemID].Add(d); //将日期添加到指定物品ID的日期列表中

                    //以下三个if结构对应物品用户字典、物品接收字典和响应物品字典，用于将指定用户和/或投票者添加到指定物品ID的相关字典中
                    if (!itemusers.ContainsKey(itemid)) itemusers.Add(itemid, new HashSet<string>());
                    itemusers[itemid].Add(id1);
                    itemusers[itemid].Add(id2);
                    if (!itemreceive[itemid].ContainsKey(id1)) itemreceive[itemid].Add(id1, new HashSet<string>());
                    if ((dtmax - d).TotalDays < 30) activefrenq[id2]++;
                    itemreceive[itemid][id1].Add(id2);
                    if (!responseitems[id2].ContainsKey(id1)) responseitems[id2].Add(id1, new HashSet<string>());
                    if (!shareresponse[id2].ContainsKey(id1)) shareresponse[id2].Add(id1, new List<double>());
                    shareresponse[id2][id1].Add(Math.Exp(-0.1 * (d - sharetrainitem[id1][itemid].Min()).TotalHours));
                    responseitems[id2][id1].Add(itemid);
                    allNeigbors[id1]["F"].Add(id2);
                    allNeigbors[id2]["L"].Add(id1);
                }
            }

            // 该段代码主要做了两件事。首先，它归一化了所有用户的活动频率。这是通过获取最大活动频率并将每个用户的活动频率除以最大活动频率实现的。
            // 其次，代码遍历了网络关系字典，对其中的所有用户-邻居-物品信息进行归一化处理。它计算了指定用户和邻居用户的所有物品信息的总值，并将每个物品的信息除以总值，从而实现归一化。

            // 在数据预处理的阶段，归一化是一种常见的方法，用来消除不同数据之间的量纲和尺度差异，使其落在同一数量级，便于进行后续的分析和处理。

            // 创建一个列表用于存储所有在活跃度字典中出现的用户ID
            var strlist = activefrenq.Keys.ToList();
            // 找到活跃度字典中的最大值
            var actmax = activefrenq.Values.Max();
            // 遍历活跃度字典中的所有用户ID，并对每个用户的活跃度进行归一化处理
            foreach (var iid in strlist.WithProgressBar("Processing 5..."))
                activefrenq[iid] = (activefrenq[iid] + 0.001) / actmax;
            // 遍历网络关系字典中的所有用户ID
            foreach (var iid in netrelation.Keys.WithProgressBar("Processing 6..."))
            {
                // 遍历指定用户的所有邻居用户
                foreach (var fid in netrelation[iid].Keys)
                {
                    // 遍历指定用户和邻居用户的所有物品信息
                    foreach (var xid in netrelation[iid][fid].Keys)
                    {
                        // 计算指定用户和邻居用户的所有物品信息的总值
                        var yy = netrelation[iid][fid][xid].Values.Sum();
                        // 创建一个列表用于存储所有在物品信息中出现的物品ID
                        var tmparr = new List<string>(netrelation[iid][fid][xid].Keys);
                        // 遍历物品信息中的所有物品ID，并对每个物品的信息进行归一化处理
                        foreach (var mid in tmparr)
                            netrelation[iid][fid][xid][mid] = netrelation[iid][fid][xid][mid] * 1.0 / yy;
                    }
                }
            }

            // 声明两个整型变量 k1 和 k2，以及一个双精度浮点型变量 sim
            int k1, k2;
            double sim;

            // 这段代码主要实现的功能是找出每个用户的二阶邻居和三阶邻居，其中"F"代表一阶邻居，"L"代表二阶邻居，"FF"代表二阶邻居，"LF"也代表二阶邻居，
            // "XF"代表三阶邻居。具体操作如下：对于每个用户，遍历其所有一阶邻居，然后遍历这些一阶邻居的所有一阶邻居，得到的就是用户的二阶邻居，
            // 分别存储在"FF"和"LF"下，然后从二阶邻居中删除所有的一阶邻居；接着，遍历二阶邻居的所有一阶邻居，得到的就是用户的三阶邻居，存储在"XF"下，
            // 最后从三阶邻居中删除所有的一阶和二阶邻居。这样做的目的是将网络中的节点按照与给定用户的关系远近进行分类，为下一步的特征工程提供了基础。
            // 遍历所有的邻居列表
            foreach (var id in allNeigbors.Keys.WithProgressBar("Processing 7..."))
            {
                // 遍历当前用户的所有一阶邻居
                foreach (var fid in allNeigbors[id]["F"])
                {
                    // 从当前用户的一阶邻居中删除该邻居
                    allNeigbors[id]["L"].Remove(fid);

                    // 遍历该一阶邻居的所有邻居，并将其添加到当前用户的二阶邻居中
                    foreach (var xid in allNeigbors[fid]["F"]) allNeigbors[id]["FF"].Add(xid);
                    foreach (var xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
                }

                // 为当前用户查找二阶邻居
                foreach (var fid in allNeigbors[id]["L"])
                {
                    // 遍历该一阶邻居的所有邻居，并将其添加到当前用户的二阶邻居中
                    foreach (var xid in allNeigbors[fid]["F"]) allNeigbors[id]["LF"].Add(xid);
                    foreach (var xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
                }

                // 从当前用户的二阶邻居中删除所有的一阶邻居
                foreach (var fid in allNeigbors[id]["F"])
                {
                    allNeigbors[id]["LF"].Remove(fid);
                    allNeigbors[id]["FF"].Remove(fid);
                }

                // 从当前用户的二阶邻居中删除所有的一阶邻居
                foreach (var fid in allNeigbors[id]["L"])
                    allNeigbors[id]["LF"].Remove(fid);

                // 遍历当前用户的所有二阶邻居
                foreach (var fid in allNeigbors[id]["FF"])
                {
                    // 遍历该二阶邻居的所有邻居，并将其添加到当前用户的三阶邻居中
                    foreach (var xid in allNeigbors[fid]["F"]) allNeigbors[id]["XF"].Add(xid);
                    foreach (var xid in allNeigbors[fid]["L"]) allNeigbors[id]["XF"].Add(xid);
                }

                // 为当前用户查找三阶邻居
                foreach (var fid in allNeigbors[id]["LF"])
                {
                    // 遍历该二阶邻居的所有邻居，并将其添加到当前用户的三阶邻居中
                    foreach (var xid in allNeigbors[fid]["F"]) allNeigbors[id]["XF"].Add(xid);
                    foreach (var xid in allNeigbors[fid]["L"]) allNeigbors[id]["XF"].Add(xid);
                }

                // 从当前用户的三阶邻居中删除所有的一阶和二阶邻居
                foreach (var fid in allNeigbors[id]["F"])
                    allNeigbors[id]["XF"].Remove(fid);
                foreach (var fid in allNeigbors[id]["L"])
                    allNeigbors[id]["XF"].Remove(fid);
                foreach (var fid in allNeigbors[id]["LF"])
                    allNeigbors[id]["XF"].Remove(fid);
                foreach (var fid in allNeigbors[id]["FF"])
                    allNeigbors[id]["XF"].Remove(fid);
            }


            //double mmr = 0, ktotal = 0;
            // 定义变量
            double tsp, rsim, ritem;
            // 定义接收数据字典，用于存储字符串和HashSet<string>
            var receivedata = new Dictionary<string, HashSet<string>>();
            // 定义提交结果列表，用于存储提交信息
            var submitres = new List<SubmitInfo>();
            // 定义提交信息临时变量
            SubmitInfo subtemp;
            // 创建一个StreamWriter用于写入"testres.txt"文件
            var ppr = new System.IO.StreamWriter("testres.txt");
            // 定义共享项字典
            var shareitems = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            // 定义共享详情字典
            var sharedetails = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            // 定义共享ID字典
            var shareIDs = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            // 定义最后分享的字典，存储字符串和DateTime
            var lastshare = new Dictionary<string, DateTime>();
            // 定义共享次数字典
            var shareNumber = new Dictionary<string, Dictionary<string, int>>();
            // 定义测试ID的HashSet
            var IDTest = new HashSet<string>();

            // 这段代码主要用于处理测试数据。它读取测试数据，然后提取出各个数据项（如id1，itemid，testdate等），
            // 并将这些数据项添加到相应的数据结构中，如shareitems，shareIDs，lastshare，sharetimes，shareusers，sharedetails等。
            // 这些数据结构用于在后续代码中进行进一步的处理和分析。这段代码的主要策略就是通过将测试数据存储在多个数据结构中，
            // 以便能够从多个角度对数据进行分析，这样可以更好地进行特征工程。
            // 遍历测试数据
            foreach (JsonData testdata in testjsonData.Cast<JsonData>().ToList().WithProgressBar("Processing 测试数据..."))
            {
                id1 = testdata[1].ToString(); // 获取id1
                itemid = testdata[2].ToString(); // 获取itemid
                var testdate = DateTime.Parse(testdata[3].ToString()); // 获取并解析日期
                                                                       // 如果shareitems字典不包含id1，则添加
                if (!shareitems.ContainsKey(id1)) shareitems.Add(id1, new Dictionary<string, List<DateTime>>());
                // 如果shareitems[id1]不包含itemid，则添加
                if (!shareitems[id1].ContainsKey(itemid)) shareitems[id1].Add(itemid, new List<DateTime>());
                // 将测试日期添加到shareitems[id1][itemid]的列表中
                shareitems[id1][itemid].Add(testdate);
                // 添加id1到IDTest HashSet中
                IDTest.Add(id1);
                // 如果shareIDs字典不包含itemid，则添加
                if (!shareIDs.ContainsKey(itemid))
                {
                    shareIDs.Add(itemid, new Dictionary<string, List<DateTime>>());
                    shareNumber.Add(itemid, new Dictionary<string, int>());
                }
                // 如果shareIDs[itemid]不包含id1，则添加
                if (!shareIDs[itemid].ContainsKey(id1))
                {
                    shareIDs[itemid].Add(id1, new List<DateTime>());
                    shareNumber[itemid].Add(id1, 0);
                }
                // 将测试日期添加到shareIDs[itemid][id1]的列表中，并增加shareNumber[itemid][id1]的次数
                shareIDs[itemid][id1].Add(testdate);
                shareNumber[itemid][id1]++;
                // 如果lastshare字典不包含itemid，则添加，否则更新lastshare[itemid]为测试日期
                if (!lastshare.ContainsKey(itemid)) lastshare.Add(itemid, testdate);
                else lastshare[itemid] = testdate;
                // 如果sharetimes字典不包含itemid，则添加，否则增加sharetimes[itemid]的次数
                if (!sharetimes.ContainsKey(itemid)) sharetimes.Add(itemid, 1);
                else sharetimes[itemid]++;
                // 如果shareusers字典不包含itemid，则添加
                if (!shareusers.ContainsKey(itemid))
                    shareusers.Add(itemid, new Dictionary<string, SortedSet<DateTime>>());
                // 如果shareusers[itemid]不包含id1，则添加
                if (!shareusers[itemid].ContainsKey(id1))
                    shareusers[itemid].Add(id1, new SortedSet<DateTime>());
                // 将测试日期添加到shareusers[itemid][id1]的SortedSet中
                shareusers[itemid][id1].Add(testdate);
                // 如果sharedetails字典不包含itemid，则添加
                if (!sharedetails.ContainsKey(itemid))
                    sharedetails.Add(itemid, new Dictionary<string, List<DateTime>>());
                // 如果sharedetails[itemid]不包含id1，则添加
                if (!sharedetails[itemid].ContainsKey(id1)) sharedetails[itemid].Add(id1, new List<DateTime>());
                // 将测试日期添加到sharedetails[itemid][id1]的列表中
                sharedetails[itemid][id1].Add(testdate);
            }


            // 这段代码主要是进行了一些特征工程的工作，对用户的行为和特性进行了一些统计和计算，主要包括计算用户的响应频率、计算用户在近30天内的响应次数，统计用户在不同类别上的行为分布并计算了比例。
            // 并且对用户频率和用户时间进行了归一化处理。这些特征都可能对预测voter_id有所帮助。

            // 定义用户频率和用户时间字典
            Dictionary<string, double> userfreq = new Dictionary<string, double>(), usertimes = new Dictionary<string, double>();

            // 遍历用户
            foreach (var iid in users.Keys.WithProgressBar("Processing 8..."))
            {
                // 如果用户响应时间有值，将差距最大的天数添加到用户频率字典中
                if (userresptime[iid].Count > 0)
                    userfreq.Add(iid, (dtmax - userresptime[iid].Max()).TotalDays);
                // 否则将最大和最小日期之间的天数添加到用户频率字典中
                else userfreq.Add(iid, (dtmax - dtmin).TotalDays);

                // 将0添加到用户时间字典中
                usertimes.Add(iid, 0);
                // 遍历用户响应时间，如果最大日期和响应日期之间的天数小于30，用户时间增加1
                foreach (var dx in userresptime[iid])
                    if ((dtmax - dx).TotalDays < 30)
                        usertimes[iid]++;

                // 定义类别计数字典
                var catenum = new Dictionary<int, Dictionary<string, int>>();
                for (var ii = 0; ii < 4; ++ii)
                {
                    // 对每个类别添加字典
                    catenum.Add(ii, new Dictionary<string, int>());
                    users[iid].Ratio.Add(ii, new Dictionary<string, double>());
                }

                // 遍历用户的ItemID
                foreach (var fid in users[iid].ItemID)
                {
                    // 对每个类别的项进行计数，如果不存在则添加
                    if (!catenum[0].ContainsKey(itemsinfo[fid].CateLevelOneId))
                        catenum[0].Add(itemsinfo[fid].CateLevelOneId, 1);
                    else catenum[0][itemsinfo[fid].CateLevelOneId]++;
                    if (!catenum[1].ContainsKey(itemsinfo[fid].CateId))
                        catenum[1].Add(itemsinfo[fid].CateId, 1);
                    else catenum[1][itemsinfo[fid].CateId]++;
                    if (!catenum[2].ContainsKey(itemsinfo[fid].ShopId))
                        catenum[2].Add(itemsinfo[fid].ShopId, 1);
                    else catenum[2][itemsinfo[fid].ShopId]++;
                    if (!catenum[3].ContainsKey(itemsinfo[fid].BrandId))
                        catenum[3].Add(itemsinfo[fid].BrandId, 1);
                    else catenum[3][itemsinfo[fid].BrandId]++;
                }

                // 定义临时变量tt用于计算比例
                double tt;
                for (var ii = 0; ii < 4; ++ii)
                {
                    // 计算总数
                    tt = catenum[ii].Values.Sum();
                    // 计算每个类别的比例并添加到用户的比例中
                    foreach (var xx in catenum[ii].Keys) users[iid].Ratio[ii].Add(xx, catenum[ii][xx] * 1.0 / tt);
                }
            }

            // 计算用户频率和用户时间的最大值
            double freqmax = userfreq.Values.Max(), timesmax = usertimes.Values.Max();

            // 用最大值来归一化用户频率和用户时间
            foreach (var iid in users.Keys.WithProgressBar("Processing 9..."))
            {
                userfreq[iid] /= freqmax;
                usertimes[iid] /= timesmax;
            }

            // 定义两个变量用于后续操作
            int kx = 0, kn = users.Count;


            // 这段代码主要执行了用户相似性的计算，主要基于用户的行为和行为交集进行相似度计算。
            // 对于每个测试用户，它首先找到与之交互过相同项目的所有用户，并根据公式计算了与这些用户的相似度。
            // 另外，如果两个用户是邻居，则通过另一个公式计算其相似度。
            // 这个计算过程包含了项目交集、邻居交集等多个因素，使得相似度更能反映用户的实际相似情况。
            // 通过这样的特征工程，模型可能更好地预测voter_id。
            // 定义一个存储用户之间相似性的字典
            var SimNeigbor = new Dictionary<string, Dictionary<string, double>>();

            // 创建一个临时的HashSet用于存储交集
            var NTemp = new HashSet<string>();

            // 遍历测试ID列表
            foreach (var iid in IDTest.ToList().WithProgressBar("Processing 10..."))
            {
                //if (!users.ContainsKey(iid))
                //{
                //    Console.WriteLine(iid+":error 549");
                //    continue;
                //}

                // 更新进度条
                if (++kx % 100 == 0) Text = kx.ToString() + "  " + kn.ToString();

                // 刷新应用
                Application.DoEvents();

                // 为当前用户初始化相似的用户集
                users[iid].SimLusers = new Dictionary<string, double>();
                users[iid].SimFusers = new Dictionary<string, double>();

                // 创建一个排序字典用于存储用户之间的相似度
                var simUser = new SortedDictionary<double, List<string>>();

                // 创建一个HashSet用于存储相关的用户
                var backusers = new HashSet<string>();
                foreach (var itemiid in users[iid].ItemID)
                    foreach (var zid in itemusers[itemiid])
                        backusers.Add(zid);

                // 循环遍历相关用户
                foreach (var fid in backusers)
                {
                    // 如果用户ID相同则跳过
                    if (fid == iid) continue;

                    // 计算用户间共享的项目数量
                    NTemp = users[iid].ItemID.Intersect(users[fid].ItemID).ToHashSet();
                    k2 = NTemp.Count();
                    if (k2 == 0) continue;

                    int k21 = 0, k22 = 0;
                    // 计算与邻居的交集数量
                    if (allNeigbors[iid].ContainsKey("F"))
                        k21 = NTemp.Intersect(allNeigbors[iid]["F"]).Count();
                    if (allNeigbors[iid].ContainsKey("L"))
                        k22 = NTemp.Intersect(allNeigbors[iid]["L"]).Count();

                    // 计算用户间总的项目数量
                    k1 = users[iid].ItemID.Union(users[fid].ItemID).Count();

                    // 计算相似度
                    sim = -(k21 * 1.2 + k22 * 0.2 + (k2 - k21 - k22) * 0.1) / k1 * (1 - 0.5 / Math.Sqrt(k1));

                    // 如果两个用户是邻居
                    if (users[iid].Neighbor.ContainsKey(fid))
                    {
                        k1 = sharenum[iid].Count;
                        k2 = responsenum[iid][fid].Count;
                        if (!SimNeigbor.ContainsKey(iid)) SimNeigbor.Add(iid, new Dictionary<string, double>());

                        // 计算并存储邻居间的相似度
                        var sk = k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));
                        SimNeigbor[iid].Add(fid, sk);
                    }

                    // 将相似度和用户添加到字典中
                    if (!simUser.ContainsKey(sim)) simUser.Add(sim, new List<string>());
                    simUser[sim].Add(fid);
                }

                // 遍历simUser字典并添加到SimLusers字典中
                foreach (var dd in simUser.Keys)
                    foreach (var fid in simUser[dd])
                        users[iid].SimLusers.Add(fid, -dd);
            }


            // 这段代码主要在进行准备工作，为预测模型准备所需的各种数据结构，
            // 包括记录不同数量的字典、添加数量的字典、订阅内容的字典、已经添加的内容的字典、
            // 回溯添加的内容的字典、已经添加的数量的字典、用户分类数量的字典等。
            // 同时，它还为每个类别类型统计了用户的分类信息的数量。
            // 通过对用户的分类信息进行详细的统计和记录，可以为预测模型提供更全面的数据，进而提高预测的准确性。
            // 创建一个用于存储不同数量的字典
            var diffnum = new Dictionary<int, int>();
            // 设置候选数量为6
            var candNum = 6;
            // 创建一个记录添加数量的字典
            var AddNum = new Dictionary<string, Dictionary<string, int>>();
            // 对于每个候选数量，初始化diffnum字典
            for (var ii = 0; ii < candNum; ++ii) diffnum.Add(ii, 0);
            // 创建一个新的字典，用于存储订阅内容
            var subnew = new Dictionary<string, List<string>>();
            // 创建字典，用于存储已经添加的内容和回溯添加的内容
            Dictionary<string, Dictionary<string, HashSet<string>>> haveadded =
                    new Dictionary<string, Dictionary<string, HashSet<string>>>(),
                backadded = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            // 创建一个字典用于记录已经添加的数量
            var AddedNumber = new Dictionary<string, Dictionary<string, int>>();
            // 创建一个字典用于存储用户分类数量
            var usersClassNum = new Dictionary<string, Dictionary<string, int>>();
            // 遍历所有的类别类型
            foreach (var xid in classtype.Keys.WithProgressBar("Processing 11..."))
            {
                // 对于每个类别，记录用户的分类信息的数量
                usersClassNum.Add(xid, new Dictionary<string, int>());
                foreach (var id in userclassinfo[xid].Keys)
                    usersClassNum[xid].Add(id, userclassinfo[xid][id].Values.Sum());
            }
            // 创建一个变量ch
            var ch = 0;
            // 创建一个新的HashSet用于存储布尔用户
            var boolusers = new HashSet<string>();


            data.Clear();
            // 这段代码首先遍历了测试数据集，对于每一条测试数据，它都会解析出相应的用户ID和物品ID，然后在几个关键字典（例如AddNum和receivedata）中为这两个ID添加或更新记录。
            // 接着，如果用户字典（users）中存在这个用户ID，那么它会为这个用户初始化几个用于存储各种得分的排序字典。
            // 最后，它会计算并设置一个名为att的属性，它依赖于AddNum字典中某用户ID和项目ID的数量。这部分代码的
            // 遍历测试数据集
            foreach (JsonData testdata in testjsonData.Cast<JsonData>().ToList().WithProgressBar("Processing 结果输出..."))
            {
                // 每处理100条数据更新一次显示的文本
                if (++ch % 100 == 0) Text = ch.ToString();
                // 进行界面更新
                Application.DoEvents();
                // 创建一个结果列表
                var res = new List<string>();
                // 创建一个链接信息的对象
                var linfo = new LinkInfo();
                // 设置对象的用户ID和物品ID
                id1 = linfo.UserID = testdata[1].ToString();
                itemid = linfo.ItemID = testdata[2].ToString();
                // 解析测试日期
                var testdate = DateTime.Parse(testdata[3].ToString());
                // 在AddNum字典中为该用户ID添加新的项目ID键，如果该用户ID不存在，则先添加该用户ID
                if (!AddNum.ContainsKey(id1)) AddNum.Add(id1, new Dictionary<string, int>());
                if (!AddNum[id1].ContainsKey(itemid)) AddNum[id1].Add(itemid, 1);
                else AddNum[id1][itemid]++;
                // 在接收数据字典中为该项目ID添加新的用户ID，如果该项目ID不存在，则先添加该项目ID
                if (!receivedata.ContainsKey(itemid)) receivedata.Add(itemid, new HashSet<string>());
                // 如果用户字典中不包含此用户ID，则跳过此次循环
                if (!users.ContainsKey(id1)) continue;
                // 初始化排序字典，用于存储各种得分
                SortedDictionary<double, List<string>> scor = new SortedDictionary<double, List<string>>(),
                    scorF = new SortedDictionary<double, List<string>>(),
                    scorFF = new SortedDictionary<double, List<string>>(),
                    scorL = new SortedDictionary<double, List<string>>(),
                    scorX = new SortedDictionary<double, List<string>>(),
                    scorXF = new SortedDictionary<double, List<string>>(),
                    scorT1 = new SortedDictionary<double, List<string>>(),
                    scorT2 = new SortedDictionary<double, List<string>>(),
                    scorT3 = new SortedDictionary<double, List<string>>();
                // 计算并设置属性att，它依赖于AddNum字典中某用户ID和项目ID的数量
                var att = AddNum[id1][itemid] / 5;
                if (AddNum[id1][itemid] % 5 == 0) att *= 5;
                else att = att * 5 + 5;
                // 这段代码中，为用户找到了相似的其他用户，并用权重来衡量这种相似性。
                // 首先检查了用户的已知相似用户数量是否小于预定值（5或att的值）并且该用户是否尚未处理。
                // 然后对用户的类别进行遍历，找出该类别中和当前用户有相似特征的用户（称之为反向用户），并将他们的相似性（根据他们在这个类别下的共享特征计算得出）添加到反向用户字典中。
                // 最后，对反向用户进行遍历，计算他们的加权相似度（根据类别的权重计算得出），并将这个值添加到用户的相似用户字典中。
                // 这个过程是一个特征工程的过程，为预测模型创建和添加了新的特征。
                // 检查用户的相似用户数量是否小于5和属性att中的较大值，以及这个用户ID是否还没有被处理过
                if (users[id1].SimLusers.Count < Math.Max(5, att) && !boolusers.Contains(id1))
                {
                    // 将用户ID添加到处理过的用户集合中
                    boolusers.Add(id1);
                    // 进行界面更新
                    Application.DoEvents();
                    // 创建一个字典用于保存反向用户信息
                    var backuser = new Dictionary<string, Dictionary<string, double>>();
                    // 遍历每个类别
                    foreach (var mid in classtype.Keys)
                    {
                        // 如果用户类别信息中没有这个类别和用户ID的键值对，就添加进去
                        if (!userclassinfo[mid].ContainsKey(id1))
                        {
                            userclassinfo[mid].Add(id1, new Dictionary<string, int>());
                        }
                        // 创建一个HashSet用于保存反向临时用户
                        var backtmp = new HashSet<string>();
                        // 遍历每个用户ID在某一类别下的所有项目ID，然后添加到反向临时用户集合中
                        foreach (var xid in userclassinfo[mid][id1].Keys)
                            foreach (var fid in itemuserclassinfo[mid][xid])
                                backtmp.Add(fid);
                        // 从反向临时用户集合中移除当前用户已经有的相似用户，然后移除当前用户自己
                        backtmp = new HashSet<string>(backtmp.Except(users[id1].SimLusers.Keys));
                        backtmp.Remove(id1);
                        // 遍历反向临时用户集合
                        foreach (var fid in backtmp)
                        {
                            // 如果反向用户字典中没有这个用户ID，就添加进去
                            if (!backuser.ContainsKey(fid)) backuser.Add(fid, new Dictionary<string, double>());
                            // 计算两个用户在某一类别下的相同项目数和总项目数，然后计算他们的相似度
                            k2 = userclassinfo[mid][id1].Keys.Intersect(userclassinfo[mid][fid].Keys).Count();
                            if (k2 == 0) continue;
                            k1 = userclassinfo[mid][id1].Keys.Union(userclassinfo[mid][fid].Keys).Count();
                            sim = k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));

                            // 将类别和相似度添加到反向用户字典中
                            backuser[fid].Add(mid, sim);
                        }
                    }
                    // 遍历反向用户字典
                    foreach (var fid in backuser.Keys)
                    {
                        // 计算加权后的相似度
                        double w = 0;
                        foreach (var mid in backuser[fid].Keys) w += backuser[fid][mid] * classtypeweight[mid];
                        // 将加权后的相似度添加到用户的相似用户字典中
                        users[id1].SimFusers.Add(fid, w);
                    }
                }

                // 这段代码主要在计算用户id1和他的相似用户们之间的预测值，并根据不同的情况将这个预测值和相关数据放入不同的字典中。
                // 这个预测值的计算是基于各种不同的因素，包括用户的信息，用户和他的相似用户的关系，以及用户的行为等等。
                // 这些因素通过一系列复杂的计算和调整，被整合到一个预测值中，以进行后续的预测任务
                // 这个预测策略是基于用户的行为数据，和用户之间的关系网络，以及对数据的一些基本假设
                // （比如一些数值通过指数函数转换，以减小数值的影响）来进行的。
                foreach (var fid in users[id1].SimLusers.Keys)
                {
                    // 开始遍历用户id1的相似用户列表

                    // 初始化一些变量
                    double r = 0, sir = 1, fir = 1, kir = 0, expN = 0.1;

                    if (dataLF.ContainsKey(id1) && dataLF[id1].ContainsKey(fid))
                    {
                        // 如果dataLF字典中存在以id1为键的键值对，且以fid为键的键值对，进行下一步操作

                        foreach (var ssitem in dataLF[id1][fid])
                        {
                            // 对于dataLF[id1][fid]中的每个项目(ssitem)，进行下面的操作

                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                            {
                                // 如果dataItemLF中存在以fid为键的键值对，并且以ssitem为键的键值对，进行下面的操作

                                kir += dataItemLF[fid][ssitem].Count; // 计算每一个项目的二次转发次数
                            }
                        }
                    }
                    kir = Math.Exp(0.01 * kir); // 对kir应用指数函数进行转换，使得大的kir值不会对后续计算产生过大影响

                    if (users[fid].Ratio[0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                    {
                        // 如果用户fid的Ratio的第一项包含itemsinfo中的项目的CateLevelOneId，进行下面的操作

                        sir *= 1 + 6 * users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId]; // 更新sir值
                    }

                    // 同上，处理其他三个类别的数据
                    if (users[fid].Ratio[1].ContainsKey(itemsinfo[itemid].CateId))
                        sir *= 1 + 1 * users[fid].Ratio[1][itemsinfo[itemid].CateId];
                    if (users[fid].Ratio[2].ContainsKey(itemsinfo[itemid].ShopId))
                        sir *= 1 + 2 * users[fid].Ratio[2][itemsinfo[itemid].ShopId];
                    if (users[fid].Ratio[3].ContainsKey(itemsinfo[itemid].BrandId))
                        sir *= 1 + users[fid].Ratio[3][itemsinfo[itemid].BrandId];

                    rsim = 0; // 初始化rsim
                    double rneighbor = 0, resneigbor = 0, rkdt = 0; // 初始化rneighbor, resneigbor, rkdt
                    // 这一段代码是在计算预测值的过程中，进行更详细的处理。
                    // 它处理了用户和他的相似用户们之间的网络关系，以及用户的回应行为等数据，并根据这些数据调整了预测值。
                    // 对于不同的数据类型和情况，这段代码使用了不同的计算和加权策略，以尽可能准确地计算预测值。
                    if (netrelation[id1].ContainsKey(fid))
                    {
                        // 如果netrelation字典中包含以id1和fid为键的键值对，进行下面的操作

                        if (netrelation[id1][fid][0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                            fir *= 1 + 5 * netrelation[id1][fid][0][itemsinfo[itemid].CateLevelOneId];
                        // 如果netrelation中对应项包含itemsinfo中的项目的CateLevelOneId，则更新fir值，使用网络关系的信息进行加权

                        if (netrelation[id1][fid][1].ContainsKey(itemsinfo[itemid].CateId))
                            fir *= 1 + 1 * netrelation[id1][fid][1][itemsinfo[itemid].CateId];
                        // 同上，处理其他三个类别的数据

                        if (netrelation[id1][fid][2].ContainsKey(itemsinfo[itemid].ShopId))
                            fir *= 1 + 2 * netrelation[id1][fid][2][itemsinfo[itemid].ShopId];

                        if (netrelation[id1][fid][3].ContainsKey(itemsinfo[itemid].BrandId))
                            fir *= 1 + netrelation[id1][fid][3][itemsinfo[itemid].BrandId];
                    }

                    if (users[id1].Neighbor.ContainsKey(fid))
                    {
                        // 如果用户id1的邻居列表中包含fid，进行下面的操作

                        int kk1 = users[id1].Neighbor[fid].Count, kk2 = users[fid].ResponesTime.Count;
                        rneighbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));
                        // 计算rneighbor，基于用户的相互回应时间的计数

                        kk1 = responsenum[id1][fid].Count;
                        kk2 = responseall[fid].Count;
                        resneigbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));
                        // 计算resneigbor，基于所有回应的计数

                        rkdt = Math.Exp((users[id1].Neighbor[fid].Count - kk1) / kk1);
                        // 计算rkdt，基于用户的邻居计数和kk1的差值

                        foreach (var it in users[id1].Neighbor[fid])
                        {
                            tsp = (testdate - it).TotalDays;
                            rsim += Math.Exp(-0.02 * tsp);
                            // 计算rsim，基于测试日期和it之间的时间差
                        }
                    }
                    else if (users[fid].Neighbor.ContainsKey(id1))
                    {
                        // 如果用户fid的邻居列表中包含id1，进行类似的操作

                        int kk1 = users[fid].Neighbor[id1].Count, kk2 = users[id1].ResponesTime.Count;
                        rneighbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));

                        kk1 = responsenum[fid][id1].Count;
                        kk2 = responseall[id1].Count;
                        resneigbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));

                        foreach (var it in users[fid].Neighbor[id1])
                        {
                            tsp = (testdate - it).TotalDays;
                            rsim += Math.Exp(-0.10 * tsp);
                            // 注意这里的权重是0.10，而上面是0.02
                        }
                    }

                    ritem = 0;
                    foreach (var it in users[fid].ResponesTime)
                    {
                        tsp = (testdate - it).TotalDays;
                        ritem += Math.Exp(-0.06 * tsp);
                        // 计算ritem，基于测试日期和it之间的时间差，权重为0.06
                    }


                    r = sir * ritem * users[id1].SimLusers[fid] * users[fid].Level * 0.1; // 计算一个预测值，根据各种已经计算出的变量
                    r = r * fir * kir; // 继续根据其他变量调整预测值
                    if (sharerank[id1].ContainsKey(fid)) r *= sharerank[id1][fid]; // 如果sharerank字典中包含对应键值对，继续调整预测值
                    else r *= 0.000000001; // 如果不包含，则乘以一个极小的值

                    // 调整预测值，根据userfreq和usertimes
                    r *= Math.Exp(-2 * userfreq[fid]) * Math.Exp(2 * usertimes[fid]);
                    //r *= Math.Exp(1.0 / responseitems[fid].Count) * Math.Log(users[fid].ResponesTime.Count + 1);
                    //if (users[id1].StaticSimUsers.ContainsKey(fid)) r *= Math.Exp(0.1 * users[id1].StaticSimUsers[fid]);//按圈层加权
                    // 这一段代码根据id1的所有邻居是否包含fid来对r进行不同的更新，并将结果添加到相应的字典中。
                    // 各种类型的邻居对应不同的处理和权重。
                    if (allNeigbors[id1]["F"].Contains(fid))
                    {
                        // 如果id1的所有邻居中，类别"F"包含fid，进行下面的操作

                        r *= rsim * resneigbor * rkdt; // 使用rsim, resneigbor和rkdt值来更新r
                        r *= Math.Exp(1.0 / responseitems[fid].Count); // 使用responseitems中fid的计数来更新r

                        if (!scorF.ContainsKey(-r)) scorF.Add(-r, new List<string>());
                        // 如果scorF中没有-r这个键，就在scorF中添加这个键并为它关联一个空的字符串列表

                        scorF[-r].Add(fid);
                        // 在scorF中-r这个键所关联的字符串列表中添加fid
                    }
                    else if (allNeigbors[id1]["L"].Contains(fid))
                    {
                        // 如果id1的所有邻居中，类别"L"包含fid，进行类似的操作，但这次不包含rkdt

                        r *= rsim * resneigbor;
                        r *=expN * responseitems[fid].Count;

                        if (!scorL.ContainsKey(-r)) scorL.Add(-r, new List<string>());
                        scorL[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["FF"].Contains(fid) || allNeigbors[id1]["LF"].Contains(fid))
                    {
                        // 如果id1的所有邻居中，类别"FF"或"LF"包含fid，进行类似的操作，但这次不包含rsim

                        r *= expN * responseitems[fid].Count;

                        if (!scorX.ContainsKey(-r)) scorX.Add(-r, new List<string>());
                        scorX[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["XF"].Contains(fid))
                    {
                        // 如果id1的所有邻居中，类别"XF"包含fid，进行类似的操作，这次也不包含rsim

                        r *= expN * responseitems[fid].Count;

                        if (!scorXF.ContainsKey(-r)) scorXF.Add(-r, new List<string>());
                        scorXF[-r].Add(fid);
                    }
                    else
                    {
                        // 如果id1的所有邻居中没有包含fid，进行类似的操作，这次也不包含rsim

                        r *= expN * responseitems[fid].Count;

                        if (!scor.ContainsKey(-r)) scor.Add(-r, new List<string>());
                        scor[-r].Add(fid);
                    }
                }

                // 这段代码主要是对每个与给定用户id1相似的用户进行操作，基于多个因素计算一个预测结果r，然后根据id1的邻居类型将r添加到相应的字典中。
                // 计算r的因素包括用户间的相似度、商品信息、网络关系、用户响应时间和用户等级等。各因素通过特定的公式进行加权和调整，以获得最终的预测结果。
                // 这种方法考虑了多方面的信息，旨在提高预测的准确性。
                foreach (var fid in users[id1].SimFusers.Keys)
                {
                    // 对每个在id1的相似用户列表中的用户fid进行操作

                    double r = 0, sir = 1, fir = 1, kir = 0, expN = 0.1;
                    // 初始化一些将要使用的变量，r用来存储最后的预测结果，sir和fir将用来存储商品信息和网络关系的加权值，kir存储邻居二次转发的信息，expN是指数分布的参数

                    if (dataLF.ContainsKey(id1) && dataLF[id1].ContainsKey(fid))
                        // 检查dataLF中是否包含id1和fid的信息
                        foreach (var ssitem in dataLF[id1][fid])
                            // 遍历dataLF中id1和fid关联的所有项
                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                                // 检查dataItemLF中是否包含fid和ssitem的信息
                                kir += dataItemLF[fid][ssitem].Count;
                    // 对每一个项目，累加邻居的二次转发计数到kir

                    kir = Math.Exp(0.001 * kir);
                    // 对kir进行指数运算，调节kir的大小
                    if (users[fid].Ratio[0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                        // 检查用户fid的Ratio[0]中是否包含项目的一级分类信息
                        sir *= 1 + 6 * users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId];
                    // 更新sir，将用户对一级分类的比率加权后加入sir
                    // 接下来的几行做的操作与上面类似，只是检查的是其他分类信息，包括二级分类、商店ID和品牌ID，并更新sir
                    // else sir *= 0.000001;
                    if (users[fid].Ratio[1].ContainsKey(itemsinfo[itemid].CateId))
                        sir *= 1 + users[fid].Ratio[1][itemsinfo[itemid].CateId];
                    // else sir *= 0.000001;
                    if (users[fid].Ratio[2].ContainsKey(itemsinfo[itemid].ShopId))
                        sir *= 1 + 2 * users[fid].Ratio[2][itemsinfo[itemid].ShopId];
                    //  else sir *= 0.000001;
                    if (users[fid].Ratio[3].ContainsKey(itemsinfo[itemid].BrandId))
                        sir *= 1 + users[fid].Ratio[3][itemsinfo[itemid].BrandId];
                    // else sir *= 0.000001;
                    rsim = 0;
                    // 初始化rsim为0，rsim将用来存储相似度信息

                    if (netrelation[id1].ContainsKey(fid))
                    // 检查网络关系中是否包含id1和fid的信息
                    {
                        // 下面的几行代码类似于更新sir的操作，只是它是在网络关系存在的情况下，对fir进行更新，检查的是项目的一级分类、二级分类、商店ID和品牌ID
                        if (netrelation[id1][fid][0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                            fir *= 1 + 5 * netrelation[id1][fid][0][itemsinfo[itemid].CateLevelOneId];
                        // else sir *= 0.000001;
                        if (netrelation[id1][fid][1].ContainsKey(itemsinfo[itemid].CateId))
                            fir *= 1 + netrelation[id1][fid][1][itemsinfo[itemid].CateId];
                        // else sir *= 0.000001;
                        if (netrelation[id1][fid][2].ContainsKey(itemsinfo[itemid].ShopId))
                            fir *= 1 + 2 * netrelation[id1][fid][2][itemsinfo[itemid].ShopId];
                        //  else sir *= 0.000001;
                        if (netrelation[id1][fid][3].ContainsKey(itemsinfo[itemid].BrandId))
                            fir *= 1 + netrelation[id1][fid][3][itemsinfo[itemid].BrandId];
                    }

                    ritem = 0;
                    foreach (var it in users[fid].ResponesTime)
                    // 遍历用户fid的所有响应时间
                    {
                        tsp = (testdate - it).TotalDays;
                        // 计算测试日期与响应时间的差值（天数）

                        ritem += Math.Exp(-0.06 * tsp);
                        // 使用指数函数调整差值，累加到ritem，该操作将最近的响应时间赋予更大的权重
                    }

                    r = sir * ritem * users[id1].SimFusers[fid] * users[fid].Level * 0.1;
                    // 计算预测结果r，其中包含了sir，ritem，用户之间的相似度，用户fid的级别等信息

                    r = r * fir * kir;
                    // 将r与fir和kir进行乘积操作，调整r的大小

                    // 下面的几行代码在调整r的大小后，将r添加到相应的字典中，字典的键是-r，值是一个包含fid的列表，添加到的字典取决于id1的邻居类型
                    r *= Math.Exp(-2 * userfreq[fid]) * Math.Exp(2 * usertimes[fid]);
                    r *= expN * responseitems[fid].Count;
                    if (allNeigbors[id1]["LF"].Contains(fid) || allNeigbors[id1]["FF"].Contains(fid))
                    {
                        //rsim=0
                        if (!scorT1.ContainsKey(-r)) scorT1.Add(-r, new List<string>());
                        scorT1[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["XF"].Contains(fid))
                    {
                        //rsim=0
                        if (!scorT2.ContainsKey(-r)) scorT2.Add(-r, new List<string>());
                        scorT2[-r].Add(fid);
                    }
                    else

                    {
                        //rsim=0
                        if (!scorT3.ContainsKey(-r)) scorT3.Add(-r, new List<string>());
                        scorT3[-r].Add(fid);
                    }
                }

                // 这段代码主要是在给定的用户id1和项目itemid的上下文中，遍历所有的预测结果分数以及对应的用户。
                // 在遍历过程中，如果满足一些特定的条件，就将该用户添加到候选用户列表中。
                // 这个过程主要考虑了用户对项目的回应、已添加列表和候选用户数量等信息，根据这些信息筛选出可能的候选用户。
                var wrt = id1 + "\t" + allNeigbors[id1]["F"].Count.ToString() + "\t" +
                      shareitems[id1][itemid].Count
                          .ToString();
                // 创建一个字符串wrt，其中包含用户id1、用户id1的"F"类型邻居数量以及用户id1与特定项目itemid共享的次数
                if (!AddedNumber.ContainsKey(itemid)) AddedNumber.Add(itemid, new Dictionary<string, int>());
                // 检查字典AddedNumber中是否包含itemid，如果不包含则添加一个新的条目，键是itemid，值是一个新的字典，存储该项目的附加编号
                if (!AddedNumber[itemid].ContainsKey(id1)) AddedNumber[itemid].Add(id1, 0);
                // 检查AddedNumber中对应itemid的字典中是否包含id1，如果不包含则添加一个新的条目，键是id1，值是0
                AddedNumber[itemid][id1]++;
                // 将AddedNumber中对应itemid和id1的值递增，表示该用户对应的项目的添加编号增加
                foreach (var xx in scorF.Keys)
                {
                    // 遍历scorF字典的所有键，即预测结果的分数
                    foreach (var yid in scorF[xx])
                    {
                        // 遍历给定分数的所有用户
                        if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                            continue;
                        // 如果itemid对应的用户id1已经接收了用户yid，就跳过本次循环
                        if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                        // 如果用户yid对应的回应项目中包含用户id1和项目itemid，就跳过本次循环
                        if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) &&
                            haveadded[itemid][id1].Contains(yid)) continue;
                        // 如果已添加列表中包含项目itemid、用户id1和用户yid，就跳过本次循环
                        if (res.Count < candNum)
                            res.Add(yid);
                        // 如果候选用户列表的数量少于给定的数量，就添加用户yid到列表中
                        else break;
                        // 否则，结束循环
                    }
                    if (res.Count == candNum) break;
                    // 如果候选用户列表的数量达到了给定的数量，就结束循环
                }

                // 这段代码同样是在筛选候选用户，但它通过迭代三个不同的字典（scorL、scorX、scorT1）并对它们进行相同的处理，来在不同的数据源上应用相同的策略。
                // 它仍然是在给定的用户id1和项目itemid的上下文中，检查用户的接收情况、回应项目和已添加列表，来筛选出可能的候选用户。
                // 如果候选用户列表的数量达到了给定的数量，就会结束遍历。
                if (res.Count < candNum)
                    //如果当前候选用户列表的数量小于给定数量
                    foreach (var xx in scorL.Keys)
                    {
                        // 遍历scorL字典的所有键，即预测结果的分数
                        foreach (var yid in scorL[xx])
                        {
                            // 遍历给定分数的所有用户
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                                continue;
                            // 如果itemid对应的用户id1已经接收了用户yid，就跳过本次循环
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid))
                                continue;
                            // 如果用户yid对应的回应项目中包含用户id1和项目itemid，就跳过本次循环
                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) &&
                                haveadded[itemid][id1].Contains(yid)) continue;
                            // 如果已添加列表中包含项目itemid、用户id1和用户yid，就跳过本次循环
                            if (res.Count < candNum)
                                res.Add(yid);
                            // 如果候选用户列表的数量少于给定的数量，就添加用户yid到列表中
                            else break;
                            // 否则，结束循环
                        }
                        if (res.Count == candNum) break;
                        // 如果候选用户列表的数量达到了给定的数量，就结束循环
                    }
                //以下的两段代码与上述代码类似，但是是对不同的字典（scorX, scorT1）进行处理。相同的策略被应用于不同的数据源。
                if (res.Count < candNum)
                    //二阶
                    foreach (var xx in scorX.Keys)
                    {
                        foreach (var yid in scorX[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                                continue; //同一item，同一分享用户，回流的用户只会回流一次（train）
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid))
                                continue;
                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) &&
                                haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)

                                res.Add(yid);

                            else break;
                        }

                        if (res.Count == candNum) break;
                    }

                if (res.Count < candNum)
                    //二阶
                    foreach (var xx in scorT1.Keys)
                    {
                        foreach (var yid in scorT1[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                                continue; //同一item，同一分享用户，回流的用户只会回流一次（train）
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid))
                                continue;
                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) &&
                                haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)
                                res.Add(yid);

                            else break;
                        }

                        if (res.Count == candNum) break;
                    }
                // 三阶
                // 这段代码延续了前面的策略，通过迭代四个不同的字典（scorXF、scorT2、scor、scorT3）并对它们进行相同的处理，来在不同的数据源上应用相同的策略。
                // 它仍然是在给定的用户id1和项目itemid的上下文中，检查用户的接收情况、回应项目和已添加列表，来筛选出可能的候选用户。
                // 如果候选用户列表的数量达到了给定的数量，就会结束遍历。
                // 尽管处理逻辑在每个字典上都相同，但这些字典可能代表了不同的特征或评分，
                // 因此遍历它们能够使模型在更广泛的数据源中寻找可能的候选用户。
                if (res.Count < candNum)
                    // 如果当前候选用户列表的数量小于给定数量
                    foreach (var xx in scorXF.Keys)
                    {
                        // 遍历scorXF字典的所有键，即预测结果的分数
                        foreach (var yid in scorXF[xx])
                        {
                            // 遍历给定分数的所有用户
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                                continue;
                            // 如果itemid对应的用户id1已经接收了用户yid，就跳过本次循环
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid))
                                continue;
                            // 如果用户yid对应的回应项目中包含用户id1和项目itemid，就跳过本次循环
                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) &&
                                haveadded[itemid][id1].Contains(yid)) continue;
                            // 如果已添加列表中包含项目itemid、用户id1和用户yid，就跳过本次循环
                            if (res.Count < candNum)
                                res.Add(yid);
                            // 如果候选用户列表的数量少于给定的数量，就添加用户yid到列表中
                            else break;
                            // 否则，结束循环
                        }
                        if (res.Count == candNum) break;
                        // 如果候选用户列表的数量达到了给定的数量，就结束循环
                    }

                //以下的三段代码与上述代码类似，但是是对不同的字典（scorT2, scor, scorT3）进行处理。相同的策略被应用于不同的数据源。
                if (res.Count < candNum)
                    //三阶
                    foreach (var xx in scorT2.Keys)
                    {
                        foreach (var yid in scorT2[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                                continue; //同一item，同一分享用户，回流的用户只会回流一次（train）
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid))
                                continue;
                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) &&
                                haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)
                                res.Add(yid);

                            else break;
                        }

                        if (res.Count == candNum) break;
                    }

                if (res.Count < candNum)
                    //其他
                    foreach (var xx in scor.Keys)
                    {
                        foreach (var yid in scor[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                                continue; //同一item，同一分享用户，回流的用户只会回流一次（train）
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid))
                                continue;
                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) &&
                                haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)
                                res.Add(yid);

                            else break;
                        }

                        if (res.Count == candNum) break;
                    }

                if (res.Count < candNum)
                    //其他
                    foreach (var xx in scorT3.Keys)
                    {
                        foreach (var yid in scorT3[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                                continue; //同一item，同一分享用户，回流的用户只会回流一次（train）
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid))
                                continue;
                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) &&
                                haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)
                                res.Add(yid);

                            else break;
                        }

                        if (res.Count == candNum) break;
                    }
                //if (res.Count < candNum)
                //{//列表不够
                //    foreach (string id in IDTest)
                //    {
                //        if (++k % 100 == 0) this.Text = k.ToString() + " / " + IDTest.Count.ToString();
                //        Application.DoEvents();
                //        Dictionary<string, Dictionary<string, double>> backuser = new Dictionary<string, Dictionary<string, double>>();
                //        foreach (string mid in classtype.Keys)
                //        {
                //            HashSet<string> backitem, backtmp = new HashSet<string>();
                //            //backuser.Add(mid, new Dictionary<string, double>());
                //            foreach (string xid in userclassinfo[mid][id].Keys)
                //            {
                //                foreach (string fid in itemuserclassinfo[mid][xid]) backtmp.Add(fid);
                //            }
                //            foreach (string fid in backtmp)
                //            {
                //                if (fid == id) continue;
                //                if (!users[id].SimLusers.ContainsKey(fid)) continue;
                //                if (!backuser.ContainsKey(fid)) backuser.Add(fid, new Dictionary<string, double>());
                //                backitem = new HashSet<string>(userclassinfo[mid][id].Keys.Intersect(userclassinfo[mid][fid].Keys));
                //                //if (backitem.Count < 6) continue;
                //                k2 = 0; k1 = 0;
                //                foreach (string xid in backitem)
                //                {
                //                    k2 += Math.Min(userclassinfo[mid][id][xid], userclassinfo[mid][fid][xid]);
                //                    // k1+= Math.Max(userclassinfo[id][xid], userclassinfo[fid][xid]);
                //                }
                //                k1 = usersClassNum[mid][id] + usersClassNum[mid][fid] - k2;// userclassinfo[id].Values.Sum() + userclassinfo[fid].Values.Sum() - k2;

                //                sim = k2 * 1.0 / k1 * (1 - 1.0 / Math.Sqrt(k1));
                //                backuser[fid].Add(mid, sim);
                //            }
                //            //if (users[id].SimLusers.ContainsKey(fid)) users[id].SimLusers[fid] += sim * 0.1;
                //            //else users[id].SimLusers.Add(fid, sim * 0.1);

                //        }

                //        foreach (string fid in backuser.Keys)
                //        {
                //            double w = 0;
                //            foreach (string mid in backuser[fid].Keys)
                //            {
                //                w += backuser[fid][mid] * classtypeweight[mid];
                //            }
                //            users[id].SimLusers[fid] += w;
                //        }
                //    }

                //}
                // 这段代码主要做了两件事情：
                //遍历shareIDs中与itemid关联的每个用户ID及其分享时间列表。通过计算分享时间与测试日期的小时数差值，筛选出在最近12小时内分享的用户。
                //对于筛选出的用户，如果他们已经在结果列表中，则把他们移动到结果列表的首位，并从shareIDs中删除对应记录；如果结果列表中的用户数量少于5，则把他们添加到结果列表中；否则，把他们添加到结果列表的第五个位置。
                //该策略看起来主要是基于用户的最近活跃度来预测可能的候选用户，即近期分享过的用户更可能是候选用户。
                var uid = shareIDs[itemid].Keys.ToList();
                // 获取shareIDs中itemid对应的所有用户ID，并转换为列表
                var shflag = false;
                // 定义一个布尔变量shflag，用于判断是否找到符合条件的用户
                foreach (var xid in uid)
                {
                    // 遍历每一个用户ID
                    if (xid == id1) continue;
                    // 如果当前用户ID等于id1，就跳过本次循环
                    var ld = shareIDs[itemid][xid].ToList();
                    // 获取itemid对应用户xid的分享时间列表
                    foreach (var dx in ld)
                    {
                        // 遍历该用户的每一个分享时间
                        var days = (dx - testdate).TotalHours;
                        // 计算分享时间与测试日期的小时数差值
                        if (days > 0 && days < 12)
                        {
                            // 如果小时数差值在0到12小时之间
                            if (res.Contains(xid))
                            {
                                // 如果候选用户列表已经包含用户xid
                                var nnk = res.Count;
                                // 获取候选用户列表的数量
                                for (var ii = 0; ii < nnk; ++ii)
                                    // 遍历候选用户列表
                                    if (res[ii] == xid)
                                    {
                                        // 如果当前候选用户等于xid
                                        res[ii] = res[0];
                                        // 把当前候选用户的位置用列表中的第一个用户填充
                                        res[0] = xid;
                                        // 把列表中的第一个位置用xid填充
                                        shareIDs[itemid].Remove(xid);
                                        // 从shareIDs中移除用户xid的记录
                                        break;
                                        // 结束循环
                                    }
                            }
                            else if (res.Count < 5)
                            {
                                // 否则，如果候选用户列表的数量小于5
                                res.Add(xid);
                                // 就把用户xid添加到候选用户列表中
                            }
                            else
                            {
                                // 否则
                                res[4] = xid;
                                // 把候选用户列表中的第五个位置用xid填充
                            }
                            shflag = true;
                            // 设置shflag为true
                            break;
                            // 结束循环
                        }
                    }
                    if (shflag) break;
                    // 如果shflag为true，就结束循环
                }


                if (res.Count == candNum || (users[id1].SimLusers.Count == 5 && res.Count == candNum - 1))
                // 检查候选者列表res是否已满，或者用户id1的相似用户列表是否已满且res的数量比预定的候选者数量少1

                {
                    if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1)
                                                          && haveadded[itemid][id1].Contains(res[0]))
                        res.Remove(res[0]);
                    // 如果itemid、id1和res中的第一个元素都存在于haveadded中，则从res中移除res[0]

                    else if (res.Count == candNum) res.Remove(res[candNum - 1]);
                    // 如果候选者列表res已满，移除res的最后一个元素

                    if (backadded.ContainsKey(itemid) && backadded[itemid].ContainsKey(id1))
                    // 检查backadded是否包含了itemid和id1

                    {
                        var nnk = res.Count;
                        // 获取res的元素数量

                        var rtmp = new List<string>();
                        // 定义一个新的列表rtmp

                        for (var ii = nnk - 1; ii >= 0; --ii)
                            // 从后向前遍历res

                            if (backadded[itemid][id1].Contains(res[ii]))
                            // 如果backadded包含了res[ii]

                            {
                                rtmp.Add(res[ii]);
                                // 将res[ii]添加到rtmp中

                                res.Remove(res[ii]);
                                // 从res中移除res[ii]
                            }

                        nnk = res.Count;
                        // 更新res的元素数量

                        if (nnk < candNum - 1)
                        // 如果res的元素数量比预定的候选者数量少1

                        {
                            var ttk = rtmp.Count;
                            // 获取rtmp的元素数量

                            for (var ii = 0; ii < ttk; ++ii) res.Add("");
                            // 在res中添加rtmp数量的空字符串

                            for (var ii = candNum - 2; ii >= ttk + 1; --ii) res[ii] = res[ii - ttk];
                            // 将res中剩余未进入rtmp的元素移动到最后

                            if (nnk == 0)
                                // 如果res为空

                                for (int ii = 0, jj = 0; jj < ttk; ++ii, ++jj)
                                    res[ii] = rtmp[jj];
                            // 将rtmp中的元素添加到res中，从头开始添加

                            else
                                for (int ii = 1, jj = 0; jj < ttk; ++ii, ++jj)
                                    res[ii] = rtmp[jj];
                            // 将rtmp中的元素添加到res中，从第二个元素开始添加
                        }
                    }

                    // 这段代码的主要任务是更新候选用户列表res。当res的数量达到候选者数量，或者用户id1的相似用户列表已满且res的数量比候选者数量少1时，会触发更新。
                    // 更新的方式包括从res中移除存在于haveadded的元素，或者当res已满时移除最后一个元素。此外，还会将存在于backadded的元素移动到res中，并将backadded的相关元素移动到haveadded中，如果满足一定的条件。
                    // 在每一次迭代的最后，会清空backadded[itemid][id1]。这些操作的目的都是为了精选和更新候选用户列表，从而增加预测的准确度。
                    if (!backadded.ContainsKey(itemid))
                        backadded.Add(itemid, new Dictionary<string, HashSet<string>>());
                    // 如果backadded中不包含itemid，就在backadded中添加一个新的键值对，键为itemid，值为一个新的字典
                    if (!backadded[itemid].ContainsKey(id1)) backadded[itemid].Add(id1, new HashSet<string>());
                    // 如果backadded[itemid]中不包含id1，就在backadded[itemid]中添加一个新的键值对，键为id1，值为一个新的哈希集合
                    backadded[itemid][id1].Add(res[0]);
                    // 在backadded[itemid][id1]中添加res的第一个元素
                    if (AddedNumber[itemid][id1] % (candNum - 1) == 0)
                    // 如果AddedNumber[itemid][id1]模(candNum - 1)等于0
                    {
                        if (!haveadded.ContainsKey(itemid))
                            haveadded.Add(itemid, new Dictionary<string, HashSet<string>>());
                        // 如果haveadded中不包含itemid，就在haveadded中添加一个新的键值对，键为itemid，值为一个新的字典
                        if (!haveadded[itemid].ContainsKey(id1)) haveadded[itemid].Add(id1, new HashSet<string>());
                        // 如果haveadded[itemid]中不包含id1，就在haveadded[itemid]中添加一个新的键值对，键为id1，值为一个新的哈希集合
                        foreach (var xmid in backadded[itemid][id1])
                            haveadded[itemid][id1].Add(xmid);
                        // 将backadded[itemid][id1]中的每一个元素都添加到haveadded[itemid][id1]中
                        backadded[itemid][id1].Clear();
                        // 清空backadded[itemid][id1]
                    }
                }

                // 这段代码主要完成了对res列表的更新和操作。当res列表包含元素时，如果backadded中存在对应的itemid和id1，
                // 就会将res中存在于backadded的元素移入rtmp，并在res的前部或者后部（取决于res是否为空）加入rtmp的元素。
                // 然后，会在backadded中的相应位置添加res的首个元素。接下来，代码从shareIDs中移除了itemid对应的id1，
                // 然后在wrt字符串中加入id1、itemid以及res中各元素在responsenum[id1]中的数量。最后，代码将wrt字符串写入文件，
                // 并在subnew中添加一个键值对，键为testdata[0]的字符串形式，值为res。同时，还将diffnum中res数量对应的值增加1。
                // 这段代码通过这些操作，进一步更新了候选用户列表并进行了记录。
                else if (res.Count > 0)
                // 检查res列表是否有元素，如果有则执行以下操作
                {
                    var mk = res.Count;
                    // 获取res的元素数量
                    if (backadded.ContainsKey(itemid) && backadded[itemid].ContainsKey(id1))
                    // 检查backadded是否包含了itemid和id1
                    {
                        var nnk = mk;
                        // 获取res的元素数量
                        var rtmp = new List<string>();
                        // 定义一个新的列表rtmp
                        for (var ii = nnk - 1; ii >= 0; --ii)
                            // 从后向前遍历res
                            if (backadded[itemid][id1].Contains(res[ii]))
                            // 如果backadded包含了res[ii]
                            {
                                rtmp.Add(res[ii]);
                                // 将res[ii]添加到rtmp中
                                res.Remove(res[ii]);
                                // 从res中移除res[ii]
                            }
                        nnk = res.Count;
                        // 更新res的元素数量
                        var ttk = rtmp.Count;
                        // 获取rtmp的元素数量
                        for (var ii = 0; ii < ttk; ++ii) res.Add("");
                        // 在res中添加rtmp数量的空字符串
                        for (var ii = mk - 1; ii >= ttk + 1; --ii) res[ii] = res[ii - ttk];
                        // 将res中剩余未进入rtmp的元素移动到最后
                        if (nnk == 0)
                            // 如果res为空
                            for (int ii = 0, jj = 0; jj < ttk; ++ii, ++jj)
                                res[ii] = rtmp[jj];
                        // 将rtmp中的元素添加到res中，从头开始添加
                        else
                            for (int ii = 1, jj = 0; jj < ttk; ++ii, ++jj)
                                res[ii] = rtmp[jj];
                        // 将rtmp中的元素添加到res中，从第二个元素开始添加
                    }
                    if (!backadded.ContainsKey(itemid))
                        backadded.Add(itemid, new Dictionary<string, HashSet<string>>());
                    // 如果backadded中不包含itemid，就在backadded中添加一个新的键值对，键为itemid，值为一个新的字典
                    if (!backadded[itemid].ContainsKey(id1)) backadded[itemid].Add(id1, new HashSet<string>());
                    // 如果backadded[itemid]中不包含id1，就在backadded[itemid]中添加一个新的键值对，键为id1，值为一个新的哈希集合
                    backadded[itemid][id1].Add(res[0]);
                    // 在backadded[itemid][id1]中添加res的第一个元素
                }
                shareIDs[itemid].Remove(id1);
                // 从shareIDs的itemid对应的字典中移除键为id1的键值对
                wrt = "";
                // 初始化wrt为空字符串
                wrt = id1 + "\t" + itemid;
                // 为wrt赋值，值为id1和itemid，中间由制表符分隔
                foreach (var xxid in res)
                    // 遍历res列表
                    if (responsenum[id1].ContainsKey(xxid))
                        // 如果responsenum[id1]包含xxid
                        wrt = wrt + "\t" + responsenum[id1][xxid].Count.ToString();
                // 向wrt添加新的内容，为"\t"和responsenum[id1][xxid]的元素数量
                ppr.WriteLine(wrt);
                // 将wrt写入到ppr中
                diffnum[res.Count]++;
                // 将res数量对应的diffnum的值加1
                subnew.Add(testdata[0].ToString(), res);
                // 在subnew中添加一个新的键值对，键为testdata[0]的字符串形式，值为res
            }

            // 这段代码主要完成了模型预测结果的序列化和存储。它首先遍历subnew字典的所有键，这些键是各个待预测的triple_id。
            // 然后，对于每一个triple_id，代码创建一个SubmitInfo对象，设置其triple_id属性和candidate_voter_list属性，
            // 然后将该对象添加到submitres列表中。遍历结束后，代码将关闭文件写入器ppr，然后将submitres列表序列化为JSON字符串，
            // 并将该字符串写入到一个名为"submit.json"的文件中。这样，预测的候选voter_id列表就被保存为一个JSON文件。
            // for (int ii = 0; ii < 6; ++ii) ppr.WriteLine(ii.ToString() + "\t" + diffnum[ii].ToString());
            foreach (var id in subnew.Keys.WithProgressBar("Processing 结果转换..."))
            // 遍历subnew字典的所有键，这些键应该是一个triple_id的列表，"WithProgressBar"是一个扩展方法，它会显示一个进度条
            {
                subtemp = new SubmitInfo();
                // 创建一个新的SubmitInfo对象
                subtemp.triple_id = id;
                // 设置subtemp的triple_id属性为当前遍历的键
                subtemp.candidate_voter_list = subnew[id].ToArray();
                // 将subnew字典中与当前键对应的值转换为数组，并设置为subtemp的candidate_voter_list属性
                submitres.Add(subtemp);
                // 将subtemp对象添加到submitres列表中
            }
            ppr.Close();
            // 关闭文件写入器ppr
            var text = JsonSerializer.Serialize(submitres);
            // 将submitres列表序列化为JSON字符串
            System.IO.File.WriteAllText("submit.json", text);
            // 将序列化的JSON字符串写入到名为"submit.json"的文件中
        }
    }
}