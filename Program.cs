using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;
using LitJson;
using System.Diagnostics;
using ShellProgressBar;

// 模拟python tqdm进度条
public static class ProgressBarExtensions
{
    public static IEnumerable<T> WithProgressBar<T>(this IEnumerable<T> enumerable, string message = "Processing...")
    {
        var totalCount = enumerable.Count();
        var processedItems = 0;

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
                progressBar.Message =
                    $"{message} ({processedItems}/{totalCount}) Estimated time remaining: {estimatedTimeRemaining}";
            }
        }

        stopwatch.Stop();
        progressBar.Message = $"{message}已完成，总用时：{stopwatch.Elapsed}";
        progressBar.Dispose();
    }

    // 读取文件版本
    public static IEnumerable<JsonData> LoadFilesWithProgressBar(string[] filePaths,
        string message = "Loading files...")
    {
        var fileDataList = new List<JsonData>();

        foreach (var filePath in filePaths.WithProgressBar(message))
        {
            var fileData = JsonMapper.ToObject(System.IO.File.ReadAllText(filePath));
            fileDataList.Add(fileData);
            yield return fileData;
        }
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

    // 分享次数
    public int ShareNum { get; set; }

    // 被分享次数
    public int BeShareNum { get; set; }

    // 临时储存一阶下游
    public List<string> tempNeighborList { get; set; }
    public List<string> tempItemList { get; set; }
    public int OneNeighborNum { get; set; }
    public int OneShareNum { get; set; }
    public int itemNum { get; set; }
    public bool bigV { get; set; }

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
            var ver = false;
            var shareDataPath = "";
            var testDataPath = "";
            var itemDataPath = @"./data/item_info.json"; //定义商品信息数据文件的路径
            var userDataPath = @"./data/user_info.json"; //定义用户信息数据文件的路径
            if (ver)
            {
                shareDataPath = @"./data/item_share_train_info_09.json"; //定义商品分享数据文件的路径
                testDataPath = @"./data/item_share_train_info_test_01.json"; //定义测试数据文件的路径
            }
            else
            {
                shareDataPath = @"./data/item_share_train_info_AB.json"; //定义商品分享数据文件的路径
                testDataPath = @"./data/item_share_test_info_B.json"; //定义测试数据文件的路径
            }
            // var jsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(shareDataPath)); //读取并解析商品分享数据文件
            // var itemjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(itemDataPath)); //读取并解析商品信息数据文件
            // var userjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(userDataPath)); //读取并解析用户信息数据文件
            // var testjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(testDataPath)); //读取并解析测试数据文件
            // 读取文件并显示进度条
            var filePaths = new[] { shareDataPath, itemDataPath, userDataPath, testDataPath };
            var jsonDataList = ProgressBarExtensions.LoadFilesWithProgressBar(filePaths).ToList();
            var (jsonData, itemjsonData, userjsonData, testjsonData) = (jsonDataList[0], jsonDataList[1], jsonDataList[2], jsonDataList[3]);
            SortedDictionary<DateTime, List<LinkInfo>> data = new SortedDictionary<DateTime, List<LinkInfo>>(),
                            data2 = new SortedDictionary<DateTime, List<LinkInfo>>();
            DateTime dt;
            LinkInfo lk;
            int n = 0, k;
            string itemid;
            string id1, id2;
            Dictionary<string, UserIDInfo> users = new Dictionary<string, UserIDInfo>();
            Dictionary<string, ItemInfo> itemsinfo = new Dictionary<string, ItemInfo>();
            Dictionary<string, Dictionary<string, HashSet<string>>> itemreceive = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<string, double>>>>
                netrelation = new Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<string, double>>>>();
            Dictionary<string, Dictionary<string, HashSet<string>>> responseitems =
                new Dictionary<string, Dictionary<string, HashSet<string>>>();//voteid,userid,itemid
            Dictionary<string, Dictionary<string, double>> sharerank = new Dictionary<string, Dictionary<string, double>>();//id,item,rank
            string item;
            Dictionary<string, Dictionary<string, SortedDictionary<DateTime, List<string>>>> ranks =
                new Dictionary<string, Dictionary<string, SortedDictionary<DateTime, List<string>>>>();
            Dictionary<string, HashSet<string>> sharenum = new Dictionary<string, HashSet<string>>(),
                responseall = new Dictionary<string, HashSet<string>>();
            Dictionary<string, Dictionary<string, HashSet<string>>> responsenum = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            Dictionary<string, double> activenum = new Dictionary<string, double>();
            Dictionary<string, Dictionary<string, HashSet<string>>> allNeigbors = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            //List<string> itemclass = new List<string>() { "BrandID", "CateID", "OneID", "ShopID" };
            Dictionary<string, int> sharetimes = new Dictionary<string, int>();
            Dictionary<string, double> activefrenq = new Dictionary<string, double>();
            Dictionary<string, List<DateTime>> userresptime = new Dictionary<string, List<DateTime>>();
            Dictionary<string, Dictionary<string, SortedSet<DateTime>>> shareusers = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(),
                responstimeAllitems = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(),
                sharetrainitem = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>();
            Dictionary<string, Dictionary<string, Dictionary<string, int>>> userclassinfo = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();//class
            Dictionary<string, Dictionary<string, HashSet<string>>> itemuserclassinfo = new Dictionary<string, Dictionary<string, HashSet<string>>>();//class
            Dictionary<string, Dictionary<string, List<double>>> shareresponse = new Dictionary<string, Dictionary<string, List<double>>>();
            Dictionary<string, string> classtype = new Dictionary<string, string>();
            Dictionary<string, double> classtypeweight = new Dictionary<string, double>();
            List<string> classtypekey = new List<string>() { "Brand", "CateID", "CateOne", "Shop" };
            List<double> weight = new List<double>() { 0.1, 0.1, 0.5, 0.3 }; n = 0;
            Dictionary<string, Dictionary<string, Dictionary<DateTime, List<string>>>> shallAll =
                new Dictionary<string, Dictionary<string, Dictionary<DateTime, List<string>>>>();
            //Dictionary<string, SortedDictionary<DateTime, List<Tuple<string, string>>>> ItemAll = 
            //    new Dictionary<string, SortedDictionary<DateTime, List<Tuple<string, string>>>>();
            Dictionary<string, Dictionary<string, List<DateTime>>> details = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            Dictionary<string, HashSet<string>> netw = new Dictionary<string, HashSet<string>>();
            foreach (string xid in classtypekey) { classtype.Add(xid, ""); classtypeweight.Add(xid, weight[n++]); }
            n = 0;
            foreach (string xid in classtype.Keys)
            {
                userclassinfo.Add(xid, new Dictionary<string, Dictionary<string, int>>());
                itemuserclassinfo.Add(xid, new Dictionary<string, HashSet<string>>());
            }
            foreach (JsonData temp in userjsonData)
            {
                JsonData user_id = temp[0];
                JsonData level = temp[3];
                id1 = user_id.ToString();
                users.Add(id1, new UserIDInfo());
                users[id1].SimLLusers = new Dictionary<string, double>(); //记录每个用户分享回流item的次数（如果分享一次，折算为2）
                users[id1].SimFFusers = new Dictionary<string, double>();//记录是否已分享，已分享记为1；
                responstimeAllitems.Add(id1, new Dictionary<string, SortedSet<DateTime>>());
                responseall.Add(id1, new HashSet<string>());
                shareresponse.Add(id1, new Dictionary<string, List<double>>());
                sharetrainitem.Add(id1, new Dictionary<string, SortedSet<DateTime>>());
                //responseclass.Add(id1, new Dictionary<string, double>());
                users[id1].ItemPath = new Dictionary<string, int>();
                users[id1].Level = int.Parse(level.ToString());
                users[id1].Gender = int.Parse(temp[1].ToString());
                users[id1].Age = int.Parse(temp[2].ToString());
                users[id1].Neighbor = new Dictionary<string, List<DateTime>>();
                users[id1].NewNeighbor = new Dictionary<string, List<DateTime>>();
                users[id1].Ratio = new Dictionary<int, Dictionary<string, double>>();
                users[id1].ResponesTime = new List<DateTime>();
                users[id1].ItemID = new HashSet<string>();
                users[id1].responseTimeZone = new Dictionary<int, double>();
                users[id1].StaticSimUsers = new Dictionary<string, double>();
                netrelation.Add(id1, new Dictionary<string, Dictionary<int, Dictionary<string, double>>>());
                responseitems.Add(id1, new Dictionary<string, HashSet<string>>());
                sharerank.Add(id1, new Dictionary<string, double>());
                sharenum.Add(id1, new HashSet<string>());
                responsenum.Add(id1, new Dictionary<string, HashSet<string>>());
                activenum.Add(id1, 0);
                allNeigbors.Add(id1, new Dictionary<string, HashSet<string>>());
                allNeigbors[id1].Add("F", new HashSet<string>());//一阶邻居
                allNeigbors[id1].Add("L", new HashSet<string>());//一阶邻居
                allNeigbors[id1].Add("FF", new HashSet<string>());//一阶邻居
                allNeigbors[id1].Add("LF", new HashSet<string>());//二阶邻居
                allNeigbors[id1].Add("XF", new HashSet<string>()); //三阶邻居
                activefrenq.Add(id1, 0);
                userresptime.Add(id1, new List<DateTime>());

            }
            foreach (JsonData temp in itemjsonData)
            {
                JsonData item_id = temp[0];
                JsonData cate_id = temp[1];
                JsonData level_id = temp[2];
                JsonData brandid = temp[3];
                JsonData shopid = temp[4];
                itemid = item_id.ToString();
                itemsinfo.Add(itemid, new ItemInfo());
                itemsinfo[itemid].ShopId = shopid.ToString();
                itemsinfo[itemid].BrandId = brandid.ToString();
                itemsinfo[itemid].CateId = cate_id.ToString();
                itemsinfo[itemid].CateLevelOneId = level_id.ToString();
                itemreceive.Add(itemid, new Dictionary<string, HashSet<string>>());
                //ItemAll.Add(itemid,new SortedDictionary<DateTime, List<Tuple<string, string>>>());

            }

            foreach (JsonData temp in jsonData)
            {
                var user_id = temp["inviter_id"]; //提取用户id
                var item_id = temp["item_id"]; //提取物品id
                var voter_id = temp["voter_id"]; //提取投票者id
                var timestamp = temp["timestamp"]; //提取时间戳
                lk = new LinkInfo(); id1 = lk.UserID = user_id.ToString();
                item = lk.ItemID = item_id.ToString();
                id2 = lk.VoterID = voter_id.ToString();
                dt = DateTime.Parse(timestamp.ToString());
                if (!responstimeAllitems[id2].ContainsKey(item)) responstimeAllitems[id2].Add(item, new SortedSet<DateTime>());
                responstimeAllitems[id2][item].Add(dt);
                if (!data.ContainsKey(dt)) data.Add(dt, new List<LinkInfo>());
                data[dt].Add(lk);
                if (!details.ContainsKey(id1)) details.Add(id1, new Dictionary<string, List<DateTime>>());
                if (!details[id1].ContainsKey(item)) details[id1].Add(item, new List<DateTime>());
                details[id1][item].Add(dt);

                if (!ranks.ContainsKey(id1)) ranks.Add(id1, new Dictionary<string, SortedDictionary<DateTime, List<string>>>());
                if (!ranks[id1].ContainsKey(item)) ranks[id1].Add(item, new SortedDictionary<DateTime, List<string>>());
                if (!ranks[id1][item].ContainsKey(dt)) ranks[id1][item].Add(dt, new List<string>());
                ranks[id1][item][dt].Add(id2);
                sharenum[id1].Add(item);
                if (!responsenum[id1].ContainsKey(id2)) responsenum[id1].Add(id2, new HashSet<string>());
                responsenum[id1][id2].Add(item);
                activenum[id2] += 1;

                classtype["Brand"] = itemsinfo[item].BrandId;
                classtype["CateID"] = itemsinfo[item].CateId;
                classtype["CateOne"] = itemsinfo[item].CateLevelOneId;
                classtype["Shop"] = itemsinfo[item].ShopId;
                foreach (string xid in classtype.Keys)
                {
                    if (!userclassinfo[xid].ContainsKey(id1))
                        userclassinfo[xid].Add(id1, new Dictionary<string, int>());
                    if (!userclassinfo[xid][id1].ContainsKey(classtype[xid])) userclassinfo[xid][id1].Add(classtype[xid], 0);
                    userclassinfo[xid][id1][classtype[xid]]++;

                    if (!userclassinfo[xid].ContainsKey(id2))
                        userclassinfo[xid].Add(id2, new Dictionary<string, int>());
                    if (!userclassinfo[xid][id2].ContainsKey(classtype[xid])) userclassinfo[xid][id2].Add(classtype[xid], 0);
                    userclassinfo[xid][id2][classtype[xid]]++;

                    if (!itemuserclassinfo[xid].ContainsKey(classtype[xid]))
                        itemuserclassinfo[xid].Add(classtype[xid], new HashSet<string>());
                    itemuserclassinfo[xid][classtype[xid]].Add(id1); itemuserclassinfo[xid][classtype[xid]].Add(id2);
                }

                ++n;
            }

            jsonData.Clear();
            userjsonData.Clear();
            itemjsonData.Clear();



            foreach (string id in ranks.Keys)
            {
                double tt = 1;// ranks[id].Count;
                foreach (string fid in ranks[id].Keys)
                {
                    double ii = 1;


                    foreach (DateTime d in ranks[id][fid].Keys)
                    {

                        foreach (string xid in ranks[id][fid][d])
                        {
                            if (!sharerank[id].ContainsKey(xid)) sharerank[id].Add(xid, 1.0 / ii / tt);
                            else sharerank[id][xid] += 1.0 / ii / tt;
                        }
                        ii += ranks[id][fid][d].Count;
                    }
                }
            }

            k = 0;
            Dictionary<string, Dictionary<string, HashSet<string>>> dataLF = new Dictionary<string, Dictionary<string, HashSet<string>>>(),
                dataItemLF = new Dictionary<string, Dictionary<string, HashSet<string>>>();

            Dictionary<string, List<DateTime>> items = new Dictionary<string, List<DateTime>>();
            Dictionary<string, HashSet<string>> itemusers = new Dictionary<string, HashSet<string>>();
            DateTime dtmax = data.Keys.Max(), dtmin = data.Keys.Min();

            Dictionary<string, int> SingleShare = new Dictionary<string, int>();
            Dictionary<string, Dictionary<string, double>> sharereceipIndex = new Dictionary<string, Dictionary<string, double>>();
            foreach (DateTime d in data.Keys)
            {


                foreach (LinkInfo llk in data[d])
                {
                    itemid = llk.ItemID;
                    id1 = llk.UserID; id2 = llk.VoterID;
                    userresptime[id2].Add(d);

                    responseall[id2].Add(itemid);
                    if (!sharetrainitem[id1].ContainsKey(itemid)) sharetrainitem[id1].Add(itemid, new SortedSet<DateTime>());
                    sharetrainitem[id1][itemid].Add(d);
                    if (!users[id1].Neighbor.ContainsKey(id2))
                    {
                        netrelation[id1].Add(id2, new Dictionary<int, Dictionary<string, double>>());
                        users[id1].Neighbor.Add(id2, new List<DateTime>());
                    }
                    for (int ii = 0; ii < 4; ++ii)
                    {
                        if (!netrelation[id1][id2].ContainsKey(ii)) netrelation[id1][id2].Add(ii, new Dictionary<string, double>());

                    }
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
                    users[id1].Neighbor[id2].Add(d);
                    users[id2].ResponesTime.Add(d);
                    users[id1].ItemID.Add(llk.ItemID);
                    users[id2].ItemID.Add(llk.ItemID);
                    if (!items.ContainsKey(llk.ItemID))

                        items.Add(llk.ItemID, new List<DateTime>());

                    items[llk.ItemID].Add(d);
                    if (!itemusers.ContainsKey(itemid)) itemusers.Add(itemid, new HashSet<string>());
                    itemusers[itemid].Add(id1); itemusers[itemid].Add(id2);
                    if (!itemreceive[itemid].ContainsKey(id1)) itemreceive[itemid].Add(id1, new HashSet<string>());
                    if ((dtmax - d).TotalDays < 30) activefrenq[id2]++;
                    itemreceive[itemid][id1].Add(id2);

                    if (!responseitems[id2].ContainsKey(id1)) responseitems[id2].Add(id1, new HashSet<string>());
                    if (!shareresponse[id2].ContainsKey(id1))
                    {
                        shareresponse[id2].Add(id1, new List<double>());
                    }
                    shareresponse[id2][id1].Add(Math.Exp(-0.1 * (d - sharetrainitem[id1][itemid].Min()).TotalHours));
                    responseitems[id2][id1].Add(itemid);
                    allNeigbors[id1]["F"].Add(id2); allNeigbors[id2]["L"].Add(id1);
                    ++k;
                }
            }
            //sharetrainitem[id1][itemid].Add(d);
            foreach (string iid in sharetrainitem.Keys)
            {
                SingleShare.Add(iid, 0);
                foreach (string fid in sharetrainitem[iid].Keys)
                {
                    SingleShare[iid] += sharetrainitem[iid][fid].Count;
                }
                if (sharetrainitem[iid].Count > 0) SingleShare[iid] /= sharetrainitem[iid].Count;
            }
            sharetrainitem.Clear();
            List<string> strlist = activefrenq.Keys.ToList();
            double actmax = activefrenq.Values.Max();
            foreach (string iid in strlist) activefrenq[iid] = (activefrenq[iid] + 0.001) / actmax;
            foreach (string iid in netrelation.Keys)
            {
                foreach (string fid in netrelation[iid].Keys)
                {
                    foreach (int xid in netrelation[iid][fid].Keys)
                    {
                        double yy = netrelation[iid][fid][xid].Values.Sum();
                        List<string> tmparr = new List<string>(netrelation[iid][fid][xid].Keys);
                        foreach (string mid in tmparr)
                        {
                            netrelation[iid][fid][xid][mid] = netrelation[iid][fid][xid][mid] * 1.0 / yy;
                        }
                    }
                }
            }
            int k1, k2;
            double sim;
            foreach (string id in allNeigbors.Keys)
            {//寻找二阶邻居
                foreach (string fid in allNeigbors[id]["F"])
                {
                    allNeigbors[id]["L"].Remove(fid);
                    foreach (string xid in allNeigbors[fid]["F"]) allNeigbors[id]["FF"].Add(xid);
                    foreach (string xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
                }
                foreach (string fid in allNeigbors[id]["L"])
                {
                    foreach (string xid in allNeigbors[fid]["F"]) allNeigbors[id]["LF"].Add(xid);
                    foreach (string xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
                }
                foreach (string fid in allNeigbors[id]["F"])
                { allNeigbors[id]["LF"].Remove(fid); allNeigbors[id]["FF"].Remove(fid); }
                foreach (string fid in allNeigbors[id]["L"])
                    allNeigbors[id]["LF"].Remove(fid);
                //寻找3阶邻居
                foreach (string fid in allNeigbors[id]["FF"])
                {
                    foreach (string xid in allNeigbors[fid]["F"]) allNeigbors[id]["XF"].Add(xid);
                    foreach (string xid in allNeigbors[fid]["L"]) allNeigbors[id]["XF"].Add(xid);
                }
                foreach (string fid in allNeigbors[id]["LF"])
                {
                    foreach (string xid in allNeigbors[fid]["F"]) allNeigbors[id]["XF"].Add(xid);
                    foreach (string xid in allNeigbors[fid]["L"]) allNeigbors[id]["XF"].Add(xid);
                }
                foreach (string fid in allNeigbors[id]["F"])
                    allNeigbors[id]["XF"].Remove(fid);
                foreach (string fid in allNeigbors[id]["L"])
                    allNeigbors[id]["XF"].Remove(fid);
                foreach (string fid in allNeigbors[id]["LF"])
                    allNeigbors[id]["XF"].Remove(fid);
                foreach (string fid in allNeigbors[id]["FF"])
                    allNeigbors[id]["XF"].Remove(fid);
            }
            //double mmr = 0, ktotal = 0;
            double tsp, rsim, ritem, rresponse;
            Dictionary<string, HashSet<string>> receivedata = new Dictionary<string, HashSet<string>>();
            List<SubmitInfo> submitres = new List<SubmitInfo>();
            SubmitInfo subtemp;
            System.IO.StreamWriter ppr = new System.IO.StreamWriter("testres.txt");
            Dictionary<string, Dictionary<DateTime, HashSet<string>>> itemreceived = new Dictionary<string, Dictionary<DateTime, HashSet<string>>>();
            Dictionary<string, Dictionary<string, List<DateTime>>> shareitems = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            Dictionary<string, Dictionary<string, List<DateTime>>> sharedetails = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            Dictionary<string, Dictionary<string, List<DateTime>>> shareIDs = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            Dictionary<string, DateTime> lastshare = new Dictionary<string, DateTime>();
            Dictionary<string, Dictionary<string, int>> shareNumber = new Dictionary<string, Dictionary<string, int>>();

            HashSet<string> IDTest = new HashSet<string>();

            foreach (JsonData testdata in testjsonData)
            {

                id1 = testdata["inviter_id"].ToString();
                itemid = testdata["item_id"].ToString();
                DateTime testdate = DateTime.Parse(testdata["timestamp"].ToString());
                if (!shareitems.ContainsKey(id1)) shareitems.Add(id1, new Dictionary<string, List<DateTime>>());
                if (!shareitems[id1].ContainsKey(itemid)) shareitems[id1].Add(itemid, new List<DateTime>());
                shareitems[id1][itemid].Add(testdate);
                IDTest.Add(id1);

                if (!shareIDs.ContainsKey(itemid))
                {
                    shareIDs.Add(itemid, new Dictionary<string, List<DateTime>>());
                    shareNumber.Add(itemid, new Dictionary<string, int>());
                }
                if (!shareIDs[itemid].ContainsKey(id1)) { shareIDs[itemid].Add(id1, new List<DateTime>()); shareNumber[itemid].Add(id1, 0); }
                shareIDs[itemid][id1].Add(testdate);
                shareNumber[itemid][id1]++;
                if (!lastshare.ContainsKey(itemid)) lastshare.Add(itemid, testdate);
                else lastshare[itemid] = testdate;
                if (!sharetimes.ContainsKey(itemid)) sharetimes.Add(itemid, 1);
                else sharetimes[itemid]++;
                if (!shareusers.ContainsKey(itemid)) shareusers.Add(itemid, new Dictionary<string, SortedSet<DateTime>>());
                if (!shareusers[itemid].ContainsKey(id1))
                    shareusers[itemid].Add(id1, new SortedSet<DateTime>()); shareusers[itemid][id1].Add(testdate);

                if (!sharedetails.ContainsKey(itemid)) sharedetails.Add(itemid, new Dictionary<string, List<DateTime>>());
                if (!sharedetails[itemid].ContainsKey(id1)) sharedetails[itemid].Add(id1, new List<DateTime>());
                sharedetails[itemid][id1].Add(testdate);
            }
            //shareitems[id1][itemid].Add(testdate);
            Dictionary<string, double> userfreq = new Dictionary<string, double>(), usertimes = new Dictionary<string, double>();
            foreach (string iid in users.Keys)
            {
                if (userresptime[iid].Count > 0)
                    userfreq.Add(iid, (dtmax - userresptime[iid].Max()).TotalDays);
                else userfreq.Add(iid, (dtmax - dtmin).TotalDays);
                usertimes.Add(iid, 0);
                foreach (DateTime dx in userresptime[iid])
                {
                    if ((dtmax - dx).TotalDays < 30) usertimes[iid]++;
                }
                Dictionary<int, Dictionary<string, int>> catenum = new Dictionary<int, Dictionary<string, int>>();
                for (int ii = 0; ii < 4; ++ii)
                {
                    catenum.Add(ii, new Dictionary<string, int>());
                    users[iid].Ratio.Add(ii, new Dictionary<string, double>());
                }
                foreach (string fid in users[iid].ItemID)
                {

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
                double tt;// = catenum.Values.Sum();
                for (int ii = 0; ii < 4; ++ii)
                {
                    tt = catenum[ii].Values.Sum();
                    foreach (string xx in catenum[ii].Keys)
                    {
                        users[iid].Ratio[ii].Add(xx, catenum[ii][xx] * 1.0 / tt);
                    }
                }


            }
            double freqmax = userfreq.Values.Max(), timesmax = usertimes.Values.Max();
            foreach (string iid in users.Keys) { userfreq[iid] /= freqmax; usertimes[iid] /= timesmax; }
            int kx = 0, kn = users.Count;


            Dictionary<string, Dictionary<string, double>> SimNeigbor = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, double> SingleDt = new Dictionary<string, double>();
            HashSet<string> NTemp = new HashSet<string>();
            Dictionary<string, Dictionary<string, int>> shareresptimes = new Dictionary<string, Dictionary<string, int>>();
            foreach (string ite in itemreceive.Keys)
            {//itemid
                foreach (string xid in itemreceive[ite].Keys)
                {//id1
                    //  if (!IDTest.Contains(xid)) continue;
                    foreach (string yid in itemreceive[ite][xid])
                    {//id2
                        if (itemreceive[ite].ContainsKey(yid) && itemreceive[ite][yid].Contains(xid))
                        {
                            if (!shareresptimes.ContainsKey(xid)) shareresptimes.Add(xid, new Dictionary<string, int>());
                            if (!shareresptimes[xid].ContainsKey(yid)) shareresptimes[xid].Add(yid, 0);
                            shareresptimes[xid][yid]++;
                        }
                    }
                }
            }
            //foreach (string iid in shareresptimes.Keys)
            //{
            //    sharereceipIndex.Add(iid, shareresptimes[iid].Count * 1.0 / allNeigbors[iid]["F"].Count());
            //}
            System.IO.StreamWriter ppt = new System.IO.StreamWriter("ppt.txt");




            foreach (string id in details.Keys)
            {
                foreach (string xid in details[id].Keys)
                {
                    if (details[id][xid].Count * 1.0 / (details[id][xid].ToHashSet()).Count() > 1.5 && details[id][xid].Count > 4)
                    {
                        if (!netw.ContainsKey(id)) netw.Add(id, new HashSet<string>());
                        foreach (DateTime dx in ranks[id][xid].Keys)
                        {

                            foreach (string mid in ranks[id][xid][dx])
                            {
                                netw[id].Add(mid);
                            }
                        }

                    }
                }
            }
            //  System.IO.StreamWriter ppn = new System.IO.StreamWriter("node.txt");
            //  ppn.WriteLine("idx\tva");
            ppt.WriteLine("id1\tid2");

            foreach (string id in netw.Keys)
            {
                double xy = 0;
                foreach (string fid in netw[id])
                {
                    ppt.WriteLine(id + "\t" + fid);
                    if (netw.ContainsKey(fid) && netw[fid].Contains(id)) ++xy;
                }
                SingleDt.Add(id, xy / netw[id].Count);
                //   ppn.WriteLine(id + "\t" + (xy / netw[id].Count).ToString());
            }
            // ppn.Close();
            ppt.Close();
            //MessageBox.Show("OK");
            // return;
            foreach (string iid in IDTest)
            {
                if (++kx % 100 == 0) this.Text = kx.ToString() + "  " + kn.ToString();
                Application.DoEvents();
                users[iid].SimLusers = new Dictionary<string, double>();
                users[iid].SimFusers = new Dictionary<string, double>();

                SortedDictionary<double, List<string>> simUser = new SortedDictionary<double, List<string>>();
                //找用户
                HashSet<string> backusers = new HashSet<string>();
                foreach (string itemiid in users[iid].ItemID)
                {
                    foreach (string zid in itemusers[itemiid]) backusers.Add(zid);
                }
                //double ky1 = users[iid].SimLLusers.Values.Sum();

                foreach (string fid in backusers)
                {
                    if (fid == iid) continue;
                    NTemp = users[iid].ItemID.Intersect(users[fid].ItemID).ToHashSet();
                    k2 = NTemp.Count();
                    if (k2 == 0) continue;
                    int k21 = 0, k22 = 0;
                    if (allNeigbors[iid]["F"].Contains(fid))
                    {
                        k21 = responsenum[iid][fid].Count();
                        NTemp = (NTemp.Except(responsenum[iid][fid])).ToHashSet();
                    }
                    if (allNeigbors[iid]["L"].Contains(fid))
                        k22 = NTemp.Intersect(responsenum[fid][iid]).Count();
                    k1 = (users[iid].ItemID.Union(users[fid].ItemID)).Count();
                    if (SingleDt.ContainsKey(iid) && SingleDt[iid] > 0.2
                        || !SingleDt.ContainsKey(iid))
                        sim = -(k2 * 1.15) / k1 * (1 - 0.5 / Math.Sqrt(k1));
                    else
                        sim = -(k21 + k22 * 0.1 + (k2 - k21 - k22) * 0.01) / k1 * (1 - 0.5 / Math.Sqrt(k1));

                    double edt = 0;
                    if (users[iid].Neighbor.ContainsKey(fid))
                    {
                        //  foreach (DateTime dx in users[iid].Neighbor[fid]) edt += Math.Exp(-0.05*(dtmax - dx).TotalDays);
                        //if (!SingleDt.ContainsKey(fid))
                        //{
                        //    foreach (DateTime dx in userresptime[fid])
                        //    {
                        //        mdt += Math.Exp(-0.05 * (dtmax - dx).TotalDays);
                        //    }
                        //}
                        //else mdt = SingleDt[fid];
                        k1 = sharenum[iid].Count; k2 = responsenum[iid][fid].Count;
                        if (!SimNeigbor.ContainsKey(iid)) SimNeigbor.Add(iid, new Dictionary<string, double>());
                        double sk = k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));
                        SimNeigbor[iid].Add(fid, sk);

                    }
                    if (!simUser.ContainsKey(sim)) simUser.Add(sim, new List<string>());
                    simUser[sim].Add(fid);
                }
                foreach (double dd in simUser.Keys)
                {
                    foreach (string fid in simUser[dd])
                    {
                        users[iid].SimLusers.Add(fid, -dd);
                    }
                }
            }

            Dictionary<int, int> diffnum = new Dictionary<int, int>();
            int candNum = 6;
            Dictionary<string, Dictionary<string, int>> AddNum = new Dictionary<string, Dictionary<string, int>>();
            for (int ii = 0; ii < candNum; ++ii) diffnum.Add(ii, 0);
            Dictionary<string, List<string>> subnew = new Dictionary<string, List<string>>();
            Dictionary<string, Dictionary<string, HashSet<string>>> haveadded = new Dictionary<string, Dictionary<string, HashSet<string>>>(),
                backadded = new Dictionary<string, Dictionary<string, HashSet<string>>>(),
                haveaddedTmp = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            Dictionary<string, Dictionary<string, int>> AddedNumber = new Dictionary<string, Dictionary<string, int>>();
            k = 0;
            Dictionary<string, Dictionary<string, int>> usersClassNum = new Dictionary<string, Dictionary<string, int>>();
            foreach (string xid in classtype.Keys)
            {
                usersClassNum.Add(xid, new Dictionary<string, int>());
                foreach (string id in userclassinfo[xid].Keys)
                {
                    usersClassNum[xid].Add(id, userclassinfo[xid][id].Values.Sum());
                }
            }
            int ch = 0;
            HashSet<string> boolusers = new HashSet<string>();

            data.Clear();
            double MRR = 0;
            Dictionary<string, Dictionary<string, int>> loop = new Dictionary<string, Dictionary<string, int>>();
            foreach (string xid in IDTest) loop.Add(xid, new Dictionary<string, int>());
            foreach (JsonData testdata in testjsonData.Cast<JsonData>().ToList().WithProgressBar("Processing 结果输出..."))
            {
                if (++ch % 100 == 0) this.Text = ch.ToString(); Application.DoEvents();
                List<string> res = new List<string>();
                LinkInfo linfo = new LinkInfo();
                id1 = linfo.UserID = testdata["inviter_id"].ToString();
                id2 = testdata["voter_id"].ToString();
                itemid = linfo.ItemID = testdata["item_id"].ToString();
                DateTime testdate = DateTime.Parse(testdata["timestamp"].ToString());
                //if (shareNumber[itemid][id1] < 40) continue;
                if (!AddNum.ContainsKey(id1)) AddNum.Add(id1, new Dictionary<string, int>());
                if (!AddNum[id1].ContainsKey(itemid)) AddNum[id1].Add(itemid, 1);
                else AddNum[id1][itemid]++;
                if (!receivedata.ContainsKey(itemid)) receivedata.Add(itemid, new HashSet<string>());

                //if (!users.ContainsKey(id1)) continue;
                //if (!itemreceived.ContainsKey(itemid)) itemreceived.Add(itemid, new Dictionary<DateTime, HashSet<string>>());
                //if (!itemreceived[itemid].ContainsKey(testdate.Date)) itemreceived[itemid].Add(testdate.Date, new HashSet<string>());
                SortedDictionary<double, List<string>> scor = new SortedDictionary<double, List<string>>(),
                    scorF = new SortedDictionary<double, List<string>>(),
                    scorFF = new SortedDictionary<double, List<string>>(),
                    scorL = new SortedDictionary<double, List<string>>(),
                    scorX = new SortedDictionary<double, List<string>>(),
                    scorXF = new SortedDictionary<double, List<string>>(),
                    scorT1 = new SortedDictionary<double, List<string>>(),
                    scorT2 = new SortedDictionary<double, List<string>>(),
                    scorT3 = new SortedDictionary<double, List<string>>();
                int att = AddNum[id1][itemid] / 5;
                if (AddNum[id1][itemid] % 5 == 0) att = att * 5;
                else att = att * 5 + 5;
                if (users[id1].SimLusers.Count < Math.Max(5, att) && !boolusers.Contains(id1))
                {
                    boolusers.Add(id1);
                    {
                        //if (++k % 100 == 0) this.Text = k.ToString() + " / " + IDTest.Count.ToString();
                        Application.DoEvents();
                        Dictionary<string, Dictionary<string, double>> backuser = new Dictionary<string, Dictionary<string, double>>();
                        foreach (string mid in classtype.Keys)
                        {
                            HashSet<string> backtmp = new HashSet<string>();
                            //backuser.Add(mid, new Dictionary<string, double>());
                            foreach (string xid in userclassinfo[mid][id1].Keys)
                            {
                                foreach (string fid in itemuserclassinfo[mid][xid]) backtmp.Add(fid);
                            }
                            backtmp = new HashSet<string>(backtmp.Except(users[id1].SimLusers.Keys));
                            backtmp.Remove(id1);
                            foreach (string fid in backtmp)
                            {

                                if (!backuser.ContainsKey(fid)) backuser.Add(fid, new Dictionary<string, double>());
                                k2 = (userclassinfo[mid][id1].Keys.Intersect(userclassinfo[mid][fid].Keys)).Count();
                                if (k2 == 0) continue;
                                k1 = (userclassinfo[mid][id1].Keys.Union(userclassinfo[mid][fid].Keys)).Count();
                                sim = k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));
                                backuser[fid].Add(mid, sim);
                            }

                        }

                        foreach (string fid in backuser.Keys)
                        {
                            double w = 0;
                            foreach (string mid in backuser[fid].Keys)
                            {
                                w += backuser[fid][mid] * classtypeweight[mid];
                            }
                            users[id1].SimFusers.Add(fid, w);
                        }
                    }
                }
                foreach (string fid in users[id1].SimLusers.Keys)
                {
                    //对每一个相似用户
                    double r = 0, sir = 1, fir = 1, kir = 0, expN = 0.1;
                    if (dataLF.ContainsKey(id1) && dataLF[id1].ContainsKey(fid))
                    {
                        foreach (string ssitem in dataLF[id1][fid])
                        {
                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                            {
                                kir += dataItemLF[fid][ssitem].Count;//每一个项目，邻居的二次转发
                            }
                        }
                        //kir = kir / dataLF[id1][fid].Count;
                    }
                    kir = Math.Exp(0.01 * kir);
                    if (users[fid].Ratio[0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                        sir *= (1 + 6 * users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId]);
                    // else sir *= 0.000001;
                    if (users[fid].Ratio[1].ContainsKey(itemsinfo[itemid].CateId))
                        sir *= (1 + 1 * users[fid].Ratio[1][itemsinfo[itemid].CateId]);
                    // else sir *= 0.000001;
                    if (users[fid].Ratio[2].ContainsKey(itemsinfo[itemid].ShopId))
                        sir *= (1 + 2 * users[fid].Ratio[2][itemsinfo[itemid].ShopId]);
                    //  else sir *= 0.000001;
                    if (users[fid].Ratio[3].ContainsKey(itemsinfo[itemid].BrandId))
                        sir *= (1 + users[fid].Ratio[3][itemsinfo[itemid].BrandId]);
                    // else sir *= 0.000001;
                    rsim = 0; rresponse = 1;
                    double rneighbor = 0, resneigbor = 0, rkdt = 0;
                    if (netrelation[id1].ContainsKey(fid))
                    {
                        if (netrelation[id1][fid][0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                            fir *= (1 + 5 * netrelation[id1][fid][0][itemsinfo[itemid].CateLevelOneId]);
                        // else sir *= 0.000001;
                        if (netrelation[id1][fid][1].ContainsKey(itemsinfo[itemid].CateId))
                            fir *= (1 + 1 * netrelation[id1][fid][1][itemsinfo[itemid].CateId]);
                        // else sir *= 0.000001;
                        if (netrelation[id1][fid][2].ContainsKey(itemsinfo[itemid].ShopId))
                            fir *= (1 + 2 * netrelation[id1][fid][2][itemsinfo[itemid].ShopId]);
                        //  else sir *= 0.000001;
                        if (netrelation[id1][fid][3].ContainsKey(itemsinfo[itemid].BrandId))
                            fir *= (1 + netrelation[id1][fid][3][itemsinfo[itemid].BrandId]);
                    }
                    if (users[id1].Neighbor.ContainsKey(fid))
                    {
                        int kk1 = users[id1].Neighbor[fid].Count, kk2 = users[fid].ResponesTime.Count;
                        rneighbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));
                        kk1 = responsenum[id1][fid].Count; kk2 = responseall[fid].Count;
                        resneigbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));
                        rkdt = Math.Exp((users[id1].Neighbor[fid].Count - kk1) / kk1);
                        // rneighbor = Math.Exp(kk1 * 1.0 / kk2);
                        foreach (DateTime it in users[id1].Neighbor[fid])
                        {
                            tsp = (testdate - it).TotalDays;
                            rsim += Math.Exp(-0.02 * tsp);
                        }
                        rresponse = (testdate - users[id1].Neighbor[fid].Max()).TotalDays;
                    }
                    else if (users[fid].Neighbor.ContainsKey(id1))
                    {
                        int kk1 = users[fid].Neighbor[id1].Count, kk2 = users[id1].ResponesTime.Count;
                        rneighbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));
                        kk1 = responsenum[fid][id1].Count; kk2 = responseall[id1].Count;
                        resneigbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));
                        foreach (DateTime it in users[fid].Neighbor[id1])
                        {
                            tsp = (testdate - it).TotalDays;
                            rsim += Math.Exp(-0.10 * tsp);
                        }
                    }
                    ritem = 0;
                    foreach (DateTime it in users[fid].ResponesTime)
                    {
                        tsp = (testdate - it).TotalDays;
                        ritem += Math.Exp(-0.06 * tsp);
                    }


                    r = sir * ritem * users[id1].SimLusers[fid] * users[fid].Level * 0.1;// *timeratio* (1 - Math.Exp(-0.1 * rresponse));
                    r = r * fir * kir;
                    if (sharerank[id1].ContainsKey(fid)) r *= sharerank[id1][fid];
                    else r *= 0.000000001;

                    r *= Math.Exp(-2 * userfreq[fid]) * Math.Exp(2 * usertimes[fid]);
                    //r *= Math.Exp(1.0 / responseitems[fid].Count) * Math.Log(users[fid].ResponesTime.Count + 1);
                    //if (users[id1].StaticSimUsers.ContainsKey(fid)) r *= Math.Exp(0.1 * users[id1].StaticSimUsers[fid]);//按圈层加权
                    if (allNeigbors[id1]["F"].Contains(fid))
                    {
                        double u1 = responsenum[id1][fid].Count, u2 = 0;
                        //if (responsenum[fid].ContainsKey(id1)) u2 = responsenum[fid][id1].Count;
                        r *= rsim * resneigbor * rkdt;// *shareresponse[fid][id1].Average();
                        r *= Math.Exp(1.0 / responseitems[fid].Count);
                        if (!scorF.ContainsKey(-r)) scorF.Add(-r, new List<string>());
                        scorF[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["L"].Contains(fid))
                    {
                        r *= rsim * resneigbor;
                        r *= Math.Exp(expN * responseitems[fid].Count);
                        if (!scorL.ContainsKey(-r)) scorL.Add(-r, new List<string>());
                        scorL[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["FF"].Contains(fid) || allNeigbors[id1]["LF"].Contains(fid))

                    {//rsim=0
                        r *= Math.Exp(expN * responseitems[fid].Count);
                        if (!scorX.ContainsKey(-r)) scorX.Add(-r, new List<string>());
                        scorX[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["XF"].Contains(fid))
                    {//rsim=0
                        r *= Math.Exp(expN * responseitems[fid].Count);
                        if (!scorXF.ContainsKey(-r)) scorXF.Add(-r, new List<string>());
                        scorXF[-r].Add(fid);
                    }
                    else
                    {//rsim=0
                        r *= Math.Exp(expN * responseitems[fid].Count);
                        if (!scor.ContainsKey(-r)) scor.Add(-r, new List<string>());
                        scor[-r].Add(fid);
                    }
                }
                foreach (string fid in users[id1].SimFusers.Keys)
                {
                    //对每一个相似用户
                    double r = 0, sir = 1, fir = 1, kir = 0, expN = 0.1;
                    if (dataLF.ContainsKey(id1) && dataLF[id1].ContainsKey(fid))
                    {
                        foreach (string ssitem in dataLF[id1][fid])
                        {
                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                            {
                                kir += dataItemLF[fid][ssitem].Count;//每一个项目，邻居的二次转发
                            }
                        }
                        //kir = kir / dataLF[id1][fid].Count;
                    }
                    kir = Math.Exp(0.001 * kir);
                    if (users[fid].Ratio[0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                        sir *= (1 + 6 * users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId]);
                    // else sir *= 0.000001;
                    if (users[fid].Ratio[1].ContainsKey(itemsinfo[itemid].CateId))
                        sir *= (1 + users[fid].Ratio[1][itemsinfo[itemid].CateId]);
                    // else sir *= 0.000001;
                    if (users[fid].Ratio[2].ContainsKey(itemsinfo[itemid].ShopId))
                        sir *= (1 + 2 * users[fid].Ratio[2][itemsinfo[itemid].ShopId]);
                    //  else sir *= 0.000001;
                    if (users[fid].Ratio[3].ContainsKey(itemsinfo[itemid].BrandId))
                        sir *= (1 + users[fid].Ratio[3][itemsinfo[itemid].BrandId]);
                    // else sir *= 0.000001;
                    rsim = 0; rresponse = 1;

                    if (netrelation[id1].ContainsKey(fid))
                    {
                        if (netrelation[id1][fid][0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                            fir *= (1 + 5 * netrelation[id1][fid][0][itemsinfo[itemid].CateLevelOneId]);
                        // else sir *= 0.000001;
                        if (netrelation[id1][fid][1].ContainsKey(itemsinfo[itemid].CateId))
                            fir *= (1 + netrelation[id1][fid][1][itemsinfo[itemid].CateId]);
                        // else sir *= 0.000001;
                        if (netrelation[id1][fid][2].ContainsKey(itemsinfo[itemid].ShopId))
                            fir *= (1 + 2 * netrelation[id1][fid][2][itemsinfo[itemid].ShopId]);
                        //  else sir *= 0.000001;
                        if (netrelation[id1][fid][3].ContainsKey(itemsinfo[itemid].BrandId))
                            fir *= (1 + netrelation[id1][fid][3][itemsinfo[itemid].BrandId]);
                    }

                    ritem = 0;
                    foreach (DateTime it in users[fid].ResponesTime)
                    {
                        tsp = (testdate - it).TotalDays;
                        ritem += Math.Exp(-0.06 * tsp);
                    }

                    r = sir * ritem * users[id1].SimFusers[fid] * users[fid].Level * 0.1;// *timeratio* (1 - Math.Exp(-0.1 * rresponse));
                    r = r * fir * kir;



                    r *= Math.Exp(-2 * userfreq[fid]) * Math.Exp(2 * usertimes[fid]);
                    r *= Math.Exp(expN * responseitems[fid].Count);
                    if (allNeigbors[id1]["LF"].Contains(fid) || allNeigbors[id1]["FF"].Contains(fid))
                    {//rsim=0
                        if (!scorT1.ContainsKey(-r)) scorT1.Add(-r, new List<string>());
                        scorT1[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["XF"].Contains(fid))
                    {//rsim=0
                        if (!scorT2.ContainsKey(-r)) scorT2.Add(-r, new List<string>());
                        scorT2[-r].Add(fid);
                    }
                    else

                    {//rsim=0
                        if (!scorT3.ContainsKey(-r)) scorT3.Add(-r, new List<string>());
                        scorT3[-r].Add(fid);
                    }
                }

                kx = -1;
                string wrt = id1 + "\t" + allNeigbors[id1]["F"].Count.ToString() + "\t" + shareitems[id1][itemid].Count.ToString();// itemid + "\t" +(++rec).ToString()+"\t"+ shareitems[id1][itemid].Count.ToString() ;
                if (!AddedNumber.ContainsKey(itemid)) AddedNumber.Add(itemid, new Dictionary<string, int>());
                if (!AddedNumber[itemid].ContainsKey(id1)) AddedNumber[itemid].Add(id1, 0);
                AddedNumber[itemid][id1]++;

                foreach (double xx in scorF.Keys)
                {

                    foreach (string yid in scorF[xx])
                    {
                        if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;//同一item，同一分享用户，回流的用户只会回流一次（train）
                                                                                                                     //if (receivedata[itemid].Contains(yid)) continue;
                                                                                                                     //if (!(shareresptimes.ContainsKey(id1) && shareresptimes[id1].ContainsKey(yid) && shareresptimes[id1][yid] > 1)
                                                                                                                     //    && receivedata[itemid].Contains(yid)) continue;
                                                                                                                     //&& responsenum.ContainsKey(yid) && responsenum[yid].ContainsKey(id1) && responsenum[yid][id1].Contains(itemid)) continue;
                                                                                                                     //if (responsenum.ContainsKey(id1) && responsenum[id1].ContainsKey(yid) && responsenum[id1][yid].Contains(itemid)) continue;
                                                                                                                     //if (responsenum.ContainsKey(yid) && responsenum[yid].ContainsKey(id1) && responsenum[yid][id1].Contains(itemid)) continue;
                        if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) && haveadded[itemid][id1].Contains(yid)) continue;
                        //if (users[id1].NewNeighbor.ContainsKey(yid) && (d - users[id1].NewNeighbor[yid].Max()).TotalDays < 1) continue;

                        if (res.Count < candNum)
                            res.Add(yid);
                        else break;
                    }
                    if (res.Count == candNum) break;
                }

                if (res.Count < candNum)
                {
                    foreach (double xx in scorL.Keys)
                    {

                        foreach (string yid in scorL[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;//同一item，同一分享用户，回流的用户只会回流一次（train）
                                                                                                                         //if (responsenum.ContainsKey(id1) && responsenum[id1].ContainsKey(yid) && responsenum[id1][yid].Contains(itemid)) continue;

                            //if (responsenum.ContainsKey(yid) && responsenum[yid].ContainsKey(id1) && responsenum[yid][id1].Contains(itemid)) continue;
                            //if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                            //if (!(shareresptimes.ContainsKey(id1) && shareresptimes[id1].ContainsKey(yid) && shareresptimes[id1][yid] > 1)
                            //&& receivedata[itemid].Contains(yid)) continue;

                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) && haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)

                                res.Add(yid);

                            else break;
                        }
                        if (res.Count == candNum) break;
                    }
                }
                if (res.Count < candNum)
                {//二阶
                    foreach (double xx in scorX.Keys)
                    {
                        foreach (string yid in scorX[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;//同一item，同一分享用户，回流的用户只会回流一次（train）
                                                                                                                         //if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                                                                                                                         //if (!(shareresptimes.ContainsKey(id1) && shareresptimes[id1].ContainsKey(yid) && shareresptimes[id1][yid] > 1)
                                                                                                                         //&& receivedata[itemid].Contains(yid)) continue;

                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) && haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)

                                res.Add(yid);

                            else break;
                        }
                        if (res.Count == candNum) break;
                    }
                }
                if (res.Count < candNum)
                {//二阶
                    foreach (double xx in scorT1.Keys)
                    {
                        foreach (string yid in scorT1[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;//同一item，同一分享用户，回流的用户只会回流一次（train）
                                                                                                                         //if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                                                                                                                         //if (!(shareresptimes.ContainsKey(id1) && shareresptimes[id1].ContainsKey(yid) && shareresptimes[id1][yid] > 1)
                                                                                                                         //&& receivedata[itemid].Contains(yid)) continue;

                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) && haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)
                            {
                                res.Add(yid);

                            }

                            else break;
                        }
                        if (res.Count == candNum) break;
                    }
                }
                if (res.Count < candNum)
                {//三阶
                    foreach (double xx in scorXF.Keys)
                    {
                        foreach (string yid in scorXF[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;//同一item，同一分享用户，回流的用户只会回流一次（train）
                                                                                                                         //if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                                                                                                                         //if (!(shareresptimes.ContainsKey(id1) && shareresptimes[id1].ContainsKey(yid) && shareresptimes[id1][yid] > 1)
                                                                                                                         //&& receivedata[itemid].Contains(yid)) continue;

                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) && haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)

                                res.Add(yid);

                            else break;
                        }
                        if (res.Count == candNum) break;
                    }
                }

                if (res.Count < candNum)
                {//三阶
                    foreach (double xx in scorT2.Keys)
                    {
                        foreach (string yid in scorT2[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;//同一item，同一分享用户，回流的用户只会回流一次（train）
                                                                                                                         //if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                                                                                                                         //if (!(shareresptimes.ContainsKey(id1) && shareresptimes[id1].ContainsKey(yid) && shareresptimes[id1][yid] > 1)
                                                                                                                         //&& receivedata[itemid].Contains(yid)) continue;

                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) && haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)
                            {
                                res.Add(yid);

                            }

                            else break;
                        }
                        if (res.Count == candNum) break;
                    }
                }
                if (res.Count < candNum)
                {//其他
                    foreach (double xx in scor.Keys)
                    {
                        foreach (string yid in scor[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;//同一item，同一分享用户，回流的用户只会回流一次（train）
                                                                                                                         //if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                                                                                                                         //if (!(shareresptimes.ContainsKey(id1) && shareresptimes[id1].ContainsKey(yid) && shareresptimes[id1][yid] > 1)
                                                                                                                         //&& receivedata[itemid].Contains(yid)) continue;

                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) && haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)
                            {
                                res.Add(yid);

                            }

                            else break;
                        }
                        if (res.Count == candNum) break;
                    }
                }

                if (res.Count < candNum)
                {//其他
                    foreach (double xx in scorT3.Keys)
                    {
                        foreach (string yid in scorT3[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;//同一item，同一分享用户，回流的用户只会回流一次（train）
                                                                                                                         //if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                                                                                                                         //if (!(shareresptimes.ContainsKey(id1) && shareresptimes[id1].ContainsKey(yid) && shareresptimes[id1][yid] > 1)
                                                                                                                         //&& receivedata[itemid].Contains(yid)) continue;

                            if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) && haveadded[itemid][id1].Contains(yid)) continue;
                            if (res.Count < candNum)
                            {
                                res.Add(yid);

                            }

                            else break;
                        }
                        if (res.Count == candNum) break;
                    }
                }

                List<string> uid = shareIDs[itemid].Keys.ToList();
                bool shflag = false;
                //string preid = "";
                foreach (string xid in uid)
                {
                    if (xid == id1) continue;
                    List<DateTime> ld = shareIDs[itemid][xid].ToList();
                    foreach (DateTime dx in ld)
                    {
                        double days = (dx - testdate).TotalHours;
                        if (days > 0 && days < 12)
                        {
                            if (res.Contains(xid))
                            {
                                int nnk = res.Count;
                                for (int ii = 0; ii < nnk; ++ii)
                                {
                                    if (res[ii] == xid)
                                    {
                                        res[ii] = res[0];
                                        res[0] = xid;
                                        //shareIDs[itemid].Remove(xid);
                                        break;
                                    }
                                }
                            }
                            else if (res.Count < 5) res.Add(xid);
                            else res[4] = xid;
                            shflag = true;
                            break;
                        }
                    }
                    if (shflag) break;
                }

                if (res.Count == candNum || users[id1].SimLusers.Count == 5 && res.Count == candNum - 1)
                {
                    if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1)
                            && haveadded[itemid][id1].Contains(res[0]))
                    {
                        res.Remove(res[0]);
                    }
                    else if (res.Count == candNum) res.Remove(res[candNum - 1]);
                    if (backadded.ContainsKey(itemid) && backadded[itemid].ContainsKey(id1))
                    {
                        string stmp = res[0];
                        int nnk = res.Count;
                        List<string> rtmp = new List<string>();
                        for (int ii = nnk - 1; ii >= 0; --ii)
                            if (backadded[itemid][id1].Contains(res[ii]))
                            { rtmp.Add(res[ii]); res.Remove(res[ii]); }
                        nnk = res.Count;
                        if (nnk < candNum - 1)
                        {

                            int ttk = rtmp.Count;
                            for (int ii = 0; ii < ttk; ++ii) res.Add("");
                            // 有数据进入rtmp
                            for (int ii = candNum - 2; ii >= ttk + 1; --ii) res[ii] = res[ii - ttk];//剩余没有进入rtmp的数据移到最后
                            if (nnk == 0)
                            {
                                for (int ii = 0, jj = 0; jj < ttk; ++ii, ++jj) res[ii] = rtmp[jj];//移到back中的数据加入到第一个数据后
                            }
                            else
                            {
                                for (int ii = 1, jj = 0; jj < ttk; ++ii, ++jj) res[ii] = rtmp[jj];//移到back中的数据加入到第一个数据后
                            }
                        }

                    }
                    if (!backadded.ContainsKey(itemid)) backadded.Add(itemid, new Dictionary<string, HashSet<string>>());
                    if (!backadded[itemid].ContainsKey(id1)) backadded[itemid].Add(id1, new HashSet<string>());
                    backadded[itemid][id1].Add(res[0]);
                    //if(id1== "c4ae226760403027b6b4ad2f36663fb4" && itemid== "5e3952e42fd8021ef07f479698c26d84"
                    //    && testdate==new DateTime(2022, 11, 29, 13, 33, 25))
                    //{
                    //    int yux = -1;
                    //}
                    if (AddedNumber[itemid][id1] % (candNum - 1) == 0)
                    {
                        if (!haveadded.ContainsKey(itemid)) haveadded.Add(itemid, new Dictionary<string, HashSet<string>>());
                        if (!haveadded[itemid].ContainsKey(id1)) haveadded[itemid].Add(id1, new HashSet<string>());
                        if (SingleDt.ContainsKey(id1) && SingleDt[id1] > 0.2)
                        {
                            if (!loop[id1].ContainsKey(itemid)) loop[id1].Add(itemid, 0);
                            if (loop[id1][itemid] == 1)
                            {
                                foreach (string xmid in backadded[itemid][id1])
                                    haveadded[itemid][id1].Add(xmid);
                                loop[id1][itemid] = 0;
                            }
                            else
                            {
                                if (!loop[id1].ContainsKey(itemid)) loop[id1].Add(itemid, 0);
                                loop[id1][itemid]++;
                            }
                        }
                        else
                        {
                            foreach (string xmid in backadded[itemid][id1])
                                haveadded[itemid][id1].Add(xmid);
                        }
                        backadded[itemid][id1].Clear();
                    }
                }

                else if (res.Count > 0)
                {
                    int mk = res.Count;
                    if (backadded.ContainsKey(itemid) && backadded[itemid].ContainsKey(id1))
                    {
                        string stmp = res[0];
                        int nnk = mk;
                        List<string> rtmp = new List<string>();
                        for (int ii = nnk - 1; ii >= 0; --ii)
                            if (backadded[itemid][id1].Contains(res[ii]))
                            { rtmp.Add(res[ii]); res.Remove(res[ii]); }
                        nnk = res.Count;
                        int ttk = rtmp.Count;
                        for (int ii = 0; ii < ttk; ++ii) res.Add("");


                        // 有数据进入rtmp
                        for (int ii = mk - 1; ii >= ttk + 1; --ii) res[ii] = res[ii - ttk];//剩余没有进入rtmp的数据移到最后
                        if (nnk == 0)
                        {
                            for (int ii = 0, jj = 0; jj < ttk; ++ii, ++jj) res[ii] = rtmp[jj];//移到back中的数据加入到第一个数据后
                        }
                        else
                        {
                            for (int ii = 1, jj = 0; jj < ttk; ++ii, ++jj) res[ii] = rtmp[jj];//移到back中的数据加入到第一个数据后
                        }

                    }
                    if (!backadded.ContainsKey(itemid)) backadded.Add(itemid, new Dictionary<string, HashSet<string>>());
                    if (!backadded[itemid].ContainsKey(id1)) backadded[itemid].Add(id1, new HashSet<string>());
                    backadded[itemid][id1].Add(res[0]);
                }


                receivedata[itemid].Add(id1);
                subnew.Add(testdata[0].ToString(), res);

            }
            foreach (var id in subnew.Keys)
            {
                subtemp = new SubmitInfo();
                subtemp.triple_id = id;
                subtemp.candidate_voter_list = subnew[id].ToArray();
                submitres.Add(subtemp);
            }
            //ppr.Close();
            //ppr = new System.IO.StreamWriter("corr.txt");
            //foreach (string id in Sharecorr.Keys)
            //{
            //    foreach (string fid in Sharecorr[id].Keys)
            //    {
            //        ppr.WriteLine(id + "\t" + fid + "\t" + Sharecorr[id][fid][0].ToString() + "\t" + Sharecorr[id][fid][1].ToString());
            //    }
            //}
            //ppr.Close();
            var text = JsonMapper.ToJson(submitres);
            System.IO.File.WriteAllText("submit.json", text);
            // MessageBox.Show(MRR.ToString());
        }
    }
}