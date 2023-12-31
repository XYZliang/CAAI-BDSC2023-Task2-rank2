﻿using System;
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
            var ofd = new OpenFileDialog();
            ofd.Title = "分享数据";
            ofd.ShowDialog();
            var jsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));
            ofd.Title = "商品数据";
            ofd.ShowDialog();
            var itemjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));
            ofd.Title = "用户数据";
            ofd.ShowDialog();
            var userjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));
            ofd.Title = "测试数据";
            ofd.ShowDialog();
            var testjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));
            SortedDictionary<DateTime, List<LinkInfo>> data = new SortedDictionary<DateTime, List<LinkInfo>>(),
                data2 = new SortedDictionary<DateTime, List<LinkInfo>>();
            DateTime dt;
            LinkInfo lk;
            int n = 0, k;
            string itemid;
            string id1, id2;
            Dictionary<string, UserIDInfo> users = new Dictionary<string, UserIDInfo>();
            Dictionary<string, ItemInfo> itemsinfo = new Dictionary<string, ItemInfo>();
            var itemreceive = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            var
                netrelation = new Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<string, double>>>>();
            var responseitems =
                new Dictionary<string, Dictionary<string, HashSet<string>>>(); //voteid,userid,itemid
            var sharerank = new Dictionary<string, Dictionary<string, double>>(); //id,item,rank
            string item;
            var ranks =
                new Dictionary<string, Dictionary<string, SortedDictionary<DateTime, List<string>>>>();
            Dictionary<string, HashSet<string>> sharenum = new Dictionary<string, HashSet<string>>(),
                responseall = new Dictionary<string, HashSet<string>>();
            var responsenum = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            var activenum = new Dictionary<string, double>();
            var allNeigbors = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            //List<string> itemclass = new List<string>() { "BrandID", "CateID", "OneID", "ShopID" };
            var sharetimes = new Dictionary<string, int>();
            var activefrenq = new Dictionary<string, double>();
            var userresptime = new Dictionary<string, List<DateTime>>();
            Dictionary<string, Dictionary<string, SortedSet<DateTime>>> shareusers =
                    new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(),
                responstimeAllitems = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(),
                sharetrainitem = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>();
            var userclassinfo = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>(); //class
            var itemuserclassinfo = new Dictionary<string, Dictionary<string, HashSet<string>>>(); //class
            var shareresponse = new Dictionary<string, Dictionary<string, List<double>>>();
            var classtype = new Dictionary<string, string>();
            var classtypeweight = new Dictionary<string, double>();
            var classtypekey = new List<string>() { "Brand", "CateID", "CateOne", "Shop" };
            var weight = new List<double>() { 0.1, 0.1, 0.5, 0.3 };
            n = 0;
            //Dictionary<string, SortedDictionary<DateTime, List<Tuple<string, string>>>> ItemAll = 
            //    new Dictionary<string, SortedDictionary<DateTime, List<Tuple<string, string>>>>();
            foreach (var xid in classtypekey)
            {
                classtype.Add(xid, "");
                classtypeweight.Add(xid, weight[n++]);
            }

            n = 0;
            foreach (var xid in classtype.Keys)
            {
                userclassinfo.Add(xid, new Dictionary<string, Dictionary<string, int>>());
                itemuserclassinfo.Add(xid, new Dictionary<string, HashSet<string>>());
            }

            foreach (JsonData temp in userjsonData)
            {
                var user_id = temp[0];
                //JsonData cate_id = temp[1];
                //JsonData level_id = temp[2];
                var level = temp[3];
                //JsonData shopid = temp[4];
                id1 = user_id.ToString();
                users.Add(id1, new UserIDInfo());
                users[id1].SimLLusers = new Dictionary<string, double>(); //记录每个用户分享回流item的次数（如果分享一次，折算为2）
                users[id1].SimFFusers = new Dictionary<string, double>(); //记录是否已分享，已分享记为1；
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
                allNeigbors[id1].Add("F", new HashSet<string>()); //一阶邻居
                allNeigbors[id1].Add("L", new HashSet<string>()); //一阶邻居
                allNeigbors[id1].Add("FF", new HashSet<string>()); //一阶邻居
                allNeigbors[id1].Add("LF", new HashSet<string>()); //二阶邻居
                allNeigbors[id1].Add("XF", new HashSet<string>()); //三阶邻居
                activefrenq.Add(id1, 0);
                userresptime.Add(id1, new List<DateTime>());
            }

            foreach (JsonData temp in itemjsonData)
            {
                var item_id = temp[0];
                var cate_id = temp[1];
                var level_id = temp[2];
                var brandid = temp[3];
                var shopid = temp[4];
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
                var user_id = temp[0];
                var item_id = temp[1];
                var voter_id = temp[2];
                var timestamp = temp[3];
                lk = new LinkInfo();
                id1 = lk.UserID = user_id.ToString();
                item = lk.ItemID = item_id.ToString();
                id2 = lk.VoterID = voter_id.ToString();
                dt = DateTime.Parse(timestamp.ToString());
                if (!responstimeAllitems[id2].ContainsKey(item))
                    responstimeAllitems[id2].Add(item, new SortedSet<DateTime>());
                responstimeAllitems[id2][item].Add(dt);
                if (!data.ContainsKey(dt)) data.Add(dt, new List<LinkInfo>());
                data[dt].Add(lk);

                //if (!ItemAll[item].ContainsKey(dt)) ItemAll[item].Add(dt, new List<Tuple<string, string>>());
                //ItemAll[item][dt].Add(new Tuple<string, string>(id1, id2));
                if (!ranks.ContainsKey(id1))
                    ranks.Add(id1, new Dictionary<string, SortedDictionary<DateTime, List<string>>>());
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
                foreach (var xid in classtype.Keys)
                {
                    if (!userclassinfo[xid].ContainsKey(id1))
                        userclassinfo[xid].Add(id1, new Dictionary<string, int>());
                    if (!userclassinfo[xid][id1].ContainsKey(classtype[xid]))
                        userclassinfo[xid][id1].Add(classtype[xid], 0);
                    userclassinfo[xid][id1][classtype[xid]]++;

                    if (!userclassinfo[xid].ContainsKey(id2))
                        userclassinfo[xid].Add(id2, new Dictionary<string, int>());
                    if (!userclassinfo[xid][id2].ContainsKey(classtype[xid]))
                        userclassinfo[xid][id2].Add(classtype[xid], 0);
                    userclassinfo[xid][id2][classtype[xid]]++;

                    if (!itemuserclassinfo[xid].ContainsKey(classtype[xid]))
                        itemuserclassinfo[xid].Add(classtype[xid], new HashSet<string>());
                    itemuserclassinfo[xid][classtype[xid]].Add(id1);
                    itemuserclassinfo[xid][classtype[xid]].Add(id2);
                }

                ++n;
            }

            jsonData.Clear();
            userjsonData.Clear();
            itemjsonData.Clear();

            //foreach(string id in ItemAll.Keys)
            //{
            //    foreach(DateTime dx in ItemAll[id].Keys)
            //    {
            //        foreach(Tuple<string,string > tp in ItemAll[id][dx])
            //        {
            //            id1 = tp.Item1;id2 = tp.Item2;
            //            if (!users[id1].ItemPath.ContainsKey(id)) users[id1].ItemPath.Add(id, 0);                        
            //                if (!users[id2].ItemPath.ContainsKey(id)) 
            //                users[id2].ItemPath.Add(id, users[id1].ItemPath[id] + 1);

            //        }
            //    }
            //}
            //ItemAll.Clear();

            foreach (var id in ranks.Keys)
            {
                double tt = 1; // ranks[id].Count;
                foreach (var fid in ranks[id].Keys)
                {
                    double ii = 1;


                    foreach (var d in ranks[id][fid].Keys)
                    {
                        foreach (var xid in ranks[id][fid][d])
                            if (!sharerank[id].ContainsKey(xid)) sharerank[id].Add(xid, 1.0 / ii / tt);
                            else sharerank[id][xid] += 1.0 / ii / tt;
                        ii += ranks[id][fid][d].Count;
                    }
                }
            }

            k = 0;
            Dictionary<string, Dictionary<string, HashSet<string>>> dataLF =
                    new Dictionary<string, Dictionary<string, HashSet<string>>>(),
                dataItemLF = new Dictionary<string, Dictionary<string, HashSet<string>>>();

            var items = new Dictionary<string, List<DateTime>>();
            var itemusers = new Dictionary<string, HashSet<string>>();
            DateTime dtmax = data.Keys.Max(), dtmin = data.Keys.Min();

            foreach (var d in data.Keys)
            foreach (LinkInfo llk in data[d])
            {
                itemid = llk.ItemID;
                id1 = llk.UserID;
                id2 = llk.VoterID;
                userresptime[id2].Add(d);

                responseall[id2].Add(itemid);
                if (!sharetrainitem[id1].ContainsKey(itemid))
                    sharetrainitem[id1].Add(itemid, new SortedSet<DateTime>());
                sharetrainitem[id1][itemid].Add(d);
                if (!users[id1].Neighbor.ContainsKey(id2))
                {
                    netrelation[id1].Add(id2, new Dictionary<int, Dictionary<string, double>>());
                    users[id1].Neighbor.Add(id2, new List<DateTime>());
                }

                for (var ii = 0; ii < 4; ++ii)
                    if (!netrelation[id1][id2].ContainsKey(ii))
                        netrelation[id1][id2].Add(ii, new Dictionary<string, double>());
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
                ++k;
            }

            var strlist = activefrenq.Keys.ToList();
            var actmax = activefrenq.Values.Max();
            foreach (var iid in strlist) activefrenq[iid] = (activefrenq[iid] + 0.001) / actmax;
            foreach (var iid in netrelation.Keys)
            foreach (var fid in netrelation[iid].Keys)
            foreach (var xid in netrelation[iid][fid].Keys)
            {
                var yy = netrelation[iid][fid][xid].Values.Sum();
                var tmparr = new List<string>(netrelation[iid][fid][xid].Keys);
                foreach (var mid in tmparr)
                    netrelation[iid][fid][xid][mid] = netrelation[iid][fid][xid][mid] * 1.0 / yy;
            }

            int k1, k2;
            double sim;
            foreach (var id in allNeigbors.Keys)
            {
                //寻找二阶邻居
                foreach (var fid in allNeigbors[id]["F"])
                {
                    allNeigbors[id]["L"].Remove(fid);
                    foreach (var xid in allNeigbors[fid]["F"]) allNeigbors[id]["FF"].Add(xid);
                    foreach (var xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
                }

                foreach (var fid in allNeigbors[id]["L"])
                {
                    foreach (var xid in allNeigbors[fid]["F"]) allNeigbors[id]["LF"].Add(xid);
                    foreach (var xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
                }

                foreach (var fid in allNeigbors[id]["F"])
                {
                    allNeigbors[id]["LF"].Remove(fid);
                    allNeigbors[id]["FF"].Remove(fid);
                }

                foreach (var fid in allNeigbors[id]["L"])
                    allNeigbors[id]["LF"].Remove(fid);
                //寻找3阶邻居
                foreach (var fid in allNeigbors[id]["FF"])
                {
                    foreach (var xid in allNeigbors[fid]["F"]) allNeigbors[id]["XF"].Add(xid);
                    foreach (var xid in allNeigbors[fid]["L"]) allNeigbors[id]["XF"].Add(xid);
                }

                foreach (var fid in allNeigbors[id]["LF"])
                {
                    foreach (var xid in allNeigbors[fid]["F"]) allNeigbors[id]["XF"].Add(xid);
                    foreach (var xid in allNeigbors[fid]["L"]) allNeigbors[id]["XF"].Add(xid);
                }

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
            double tsp, rsim, ritem, rresponse;
            var receivedata = new Dictionary<string, HashSet<string>>();
            List<SubmitInfo> submitres = new List<SubmitInfo>();
            SubmitInfo subtemp;
            var ppr = new System.IO.StreamWriter("testres.txt");
            var itemreceived = new Dictionary<string, Dictionary<DateTime, HashSet<string>>>();
            var shareitems = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            var sharedetails = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            var shareIDs = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            var lastshare = new Dictionary<string, DateTime>();
            var shareNumber = new Dictionary<string, Dictionary<string, int>>();

            var IDTest = new HashSet<string>();

            foreach (JsonData testdata in testjsonData)
            {
                id1 = testdata[1].ToString();
                itemid = testdata[2].ToString();
                var testdate = DateTime.Parse(testdata[3].ToString());
                if (!shareitems.ContainsKey(id1)) shareitems.Add(id1, new Dictionary<string, List<DateTime>>());
                if (!shareitems[id1].ContainsKey(itemid)) shareitems[id1].Add(itemid, new List<DateTime>());
                shareitems[id1][itemid].Add(testdate);
                IDTest.Add(id1);
                if (!shareIDs.ContainsKey(itemid))
                {
                    shareIDs.Add(itemid, new Dictionary<string, List<DateTime>>());
                    shareNumber.Add(itemid, new Dictionary<string, int>());
                }

                if (!shareIDs[itemid].ContainsKey(id1))
                {
                    shareIDs[itemid].Add(id1, new List<DateTime>());
                    shareNumber[itemid].Add(id1, 0);
                }

                shareIDs[itemid][id1].Add(testdate);
                shareNumber[itemid][id1]++;
                if (!lastshare.ContainsKey(itemid)) lastshare.Add(itemid, testdate);
                else lastshare[itemid] = testdate;
                if (!sharetimes.ContainsKey(itemid)) sharetimes.Add(itemid, 1);
                else sharetimes[itemid]++;
                if (!shareusers.ContainsKey(itemid))
                    shareusers.Add(itemid, new Dictionary<string, SortedSet<DateTime>>());
                if (!shareusers[itemid].ContainsKey(id1))
                    shareusers[itemid].Add(id1, new SortedSet<DateTime>());
                shareusers[itemid][id1].Add(testdate);

                if (!sharedetails.ContainsKey(itemid))
                    sharedetails.Add(itemid, new Dictionary<string, List<DateTime>>());
                if (!sharedetails[itemid].ContainsKey(id1)) sharedetails[itemid].Add(id1, new List<DateTime>());
                sharedetails[itemid][id1].Add(testdate);
            }

            Dictionary<string, double> userfreq = new Dictionary<string, double>(),
                usertimes = new Dictionary<string, double>();
            foreach (var iid in users.Keys)
            {
                if (userresptime[iid].Count > 0)
                    userfreq.Add(iid, (dtmax - userresptime[iid].Max()).TotalDays);
                else userfreq.Add(iid, (dtmax - dtmin).TotalDays);
                usertimes.Add(iid, 0);
                foreach (var dx in userresptime[iid])
                    if ((dtmax - dx).TotalDays < 30)
                        usertimes[iid]++;
                var catenum = new Dictionary<int, Dictionary<string, int>>();
                for (var ii = 0; ii < 4; ++ii)
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

                double tt; // = catenum.Values.Sum();
                for (var ii = 0; ii < 4; ++ii)
                {
                    tt = catenum[ii].Values.Sum();
                    foreach (var xx in catenum[ii].Keys) users[iid].Ratio[ii].Add(xx, catenum[ii][xx] * 1.0 / tt);
                }
            }

            double freqmax = userfreq.Values.Max(), timesmax = usertimes.Values.Max();
            foreach (var iid in users.Keys)
            {
                userfreq[iid] /= freqmax;
                usertimes[iid] /= timesmax;
            }

            int kx = 0, kn = users.Count;


            var SimNeigbor = new Dictionary<string, Dictionary<string, double>>();
            var SingleDt = new Dictionary<string, double>();
            var NTemp = new HashSet<string>();
            foreach (var iid in IDTest)
            {
                if (++kx % 100 == 0) Text = kx.ToString() + "  " + kn.ToString();
                Application.DoEvents();
                users[iid].SimLusers = new Dictionary<string, double>();
                users[iid].SimFusers = new Dictionary<string, double>();

                var simUser = new SortedDictionary<double, List<string>>();
                //找用户
                var backusers = new HashSet<string>();
                foreach (string itemiid in users[iid].ItemID)
                foreach (var zid in itemusers[itemiid])
                    backusers.Add(zid);
                //double ky1 = users[iid].SimLLusers.Values.Sum();

                foreach (var fid in backusers)
                {
                    if (fid == iid) continue;
                    NTemp = users[iid].ItemID.Intersect(users[fid].ItemID).ToHashSet();
                    k2 = NTemp.Count();
                    if (k2 == 0) continue;
                    int k21 = 0, k22 = 0;
                    if (allNeigbors[iid].ContainsKey("F"))
                        k21 = NTemp.Intersect(allNeigbors[iid]["F"]).Count();
                    if (allNeigbors[iid].ContainsKey("L"))
                        k22 = NTemp.Intersect(allNeigbors[iid]["L"]).Count();
                    k1 = users[iid].ItemID.Union(users[fid].ItemID).Count();

                    sim = -(k21 * 1.2 + k22 * 0.2 + (k2 - k21 - k22) * 0.1) / k1 * (1 - 0.5 / Math.Sqrt(k1));
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
                        k1 = sharenum[iid].Count;
                        k2 = responsenum[iid][fid].Count;
                        if (!SimNeigbor.ContainsKey(iid)) SimNeigbor.Add(iid, new Dictionary<string, double>());
                        var sk = k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));
                        SimNeigbor[iid].Add(fid, sk);
                    }

                    if (!simUser.ContainsKey(sim)) simUser.Add(sim, new List<string>());
                    simUser[sim].Add(fid);
                }

                foreach (var dd in simUser.Keys)
                foreach (var fid in simUser[dd])
                    users[iid].SimLusers.Add(fid, -dd);
            }

            var diffnum = new Dictionary<int, int>();
            var candNum = 6;
            var AddNum = new Dictionary<string, Dictionary<string, int>>();
            for (var ii = 0; ii < candNum; ++ii) diffnum.Add(ii, 0);
            var subnew = new Dictionary<string, List<string>>();
            Dictionary<string, Dictionary<string, HashSet<string>>> haveadded =
                    new Dictionary<string, Dictionary<string, HashSet<string>>>(),
                backadded = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            var AddedNumber = new Dictionary<string, Dictionary<string, int>>();
            k = 0;
            var usersClassNum = new Dictionary<string, Dictionary<string, int>>();
            foreach (var xid in classtype.Keys)
            {
                usersClassNum.Add(xid, new Dictionary<string, int>());
                foreach (var id in userclassinfo[xid].Keys)
                    usersClassNum[xid].Add(id, userclassinfo[xid][id].Values.Sum());
            }

            var ch = 0;
            var boolusers = new HashSet<string>();

            data.Clear();
            foreach (JsonData testdata in testjsonData)
            {
                if (++ch % 100 == 0) Text = ch.ToString();
                Application.DoEvents();
                var res = new List<string>();
                LinkInfo linfo = new LinkInfo();
                id1 = linfo.UserID = testdata[1].ToString();
                itemid = linfo.ItemID = testdata[2].ToString();
                var testdate = DateTime.Parse(testdata[3].ToString());
                if (!AddNum.ContainsKey(id1)) AddNum.Add(id1, new Dictionary<string, int>());
                if (!AddNum[id1].ContainsKey(itemid)) AddNum[id1].Add(itemid, 1);
                else AddNum[id1][itemid]++;
                if (!receivedata.ContainsKey(itemid)) receivedata.Add(itemid, new HashSet<string>());
                if (!users.ContainsKey(id1)) continue;
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
                var att = AddNum[id1][itemid] / 5;
                if (AddNum[id1][itemid] % 5 == 0) att = att * 5;
                else att = att * 5 + 5;
                if (users[id1].SimLusers.Count < Math.Max(5, att) && !boolusers.Contains(id1))
                {
                    boolusers.Add(id1);
                    {
                        //if (++k % 100 == 0) this.Text = k.ToString() + " / " + IDTest.Count.ToString();
                        Application.DoEvents();
                        var backuser = new Dictionary<string, Dictionary<string, double>>();
                        foreach (var mid in classtype.Keys)
                        {
                            var backtmp = new HashSet<string>();
                            //backuser.Add(mid, new Dictionary<string, double>());
                            foreach (var xid in userclassinfo[mid][id1].Keys)
                            foreach (var fid in itemuserclassinfo[mid][xid])
                                backtmp.Add(fid);
                            backtmp = new HashSet<string>(backtmp.Except(users[id1].SimLusers.Keys));
                            backtmp.Remove(id1);
                            foreach (var fid in backtmp)
                            {
                                if (!backuser.ContainsKey(fid)) backuser.Add(fid, new Dictionary<string, double>());
                                k2 = userclassinfo[mid][id1].Keys.Intersect(userclassinfo[mid][fid].Keys).Count();
                                if (k2 == 0) continue;
                                k1 = userclassinfo[mid][id1].Keys.Union(userclassinfo[mid][fid].Keys).Count();
                                sim = k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));
                                backuser[fid].Add(mid, sim);
                            }
                        }

                        foreach (var fid in backuser.Keys)
                        {
                            double w = 0;
                            foreach (var mid in backuser[fid].Keys) w += backuser[fid][mid] * classtypeweight[mid];
                            users[id1].SimFusers.Add(fid, w);
                        }
                    }
                }

                foreach (string fid in users[id1].SimLusers.Keys)
                {
                    //对每一个相似用户
                    double r = 0, sir = 1, fir = 1, kir = 0, expN = 0.1;
                    if (dataLF.ContainsKey(id1) && dataLF[id1].ContainsKey(fid))
                        foreach (var ssitem in dataLF[id1][fid])
                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                                kir += dataItemLF[fid][ssitem].Count; //每一个项目，邻居的二次转发
                    //kir = kir / dataLF[id1][fid].Count;
                    kir = Math.Exp(0.01 * kir);
                    if (users[fid].Ratio[0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                        sir *= 1 + 6 * users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId];
                    // else sir *= 0.000001;
                    if (users[fid].Ratio[1].ContainsKey(itemsinfo[itemid].CateId))
                        sir *= 1 + 1 * users[fid].Ratio[1][itemsinfo[itemid].CateId];
                    // else sir *= 0.000001;
                    if (users[fid].Ratio[2].ContainsKey(itemsinfo[itemid].ShopId))
                        sir *= 1 + 2 * users[fid].Ratio[2][itemsinfo[itemid].ShopId];
                    //  else sir *= 0.000001;
                    if (users[fid].Ratio[3].ContainsKey(itemsinfo[itemid].BrandId))
                        sir *= 1 + users[fid].Ratio[3][itemsinfo[itemid].BrandId];
                    // else sir *= 0.000001;
                    rsim = 0;
                    rresponse = 1;
                    double rneighbor = 0, resneigbor = 0, rkdt = 0;
                    if (netrelation[id1].ContainsKey(fid))
                    {
                        if (netrelation[id1][fid][0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                            fir *= 1 + 5 * netrelation[id1][fid][0][itemsinfo[itemid].CateLevelOneId];
                        // else sir *= 0.000001;
                        if (netrelation[id1][fid][1].ContainsKey(itemsinfo[itemid].CateId))
                            fir *= 1 + 1 * netrelation[id1][fid][1][itemsinfo[itemid].CateId];
                        // else sir *= 0.000001;
                        if (netrelation[id1][fid][2].ContainsKey(itemsinfo[itemid].ShopId))
                            fir *= 1 + 2 * netrelation[id1][fid][2][itemsinfo[itemid].ShopId];
                        //  else sir *= 0.000001;
                        if (netrelation[id1][fid][3].ContainsKey(itemsinfo[itemid].BrandId))
                            fir *= 1 + netrelation[id1][fid][3][itemsinfo[itemid].BrandId];
                    }

                    if (users[id1].Neighbor.ContainsKey(fid))
                    {
                        int kk1 = users[id1].Neighbor[fid].Count, kk2 = users[fid].ResponesTime.Count;
                        rneighbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));
                        kk1 = responsenum[id1][fid].Count;
                        kk2 = responseall[fid].Count;
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
                        kk1 = responsenum[fid][id1].Count;
                        kk2 = responseall[id1].Count;
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


                    r = sir * ritem * users[id1].SimLusers[fid] * users[fid].Level *
                        0.1; // *timeratio* (1 - Math.Exp(-0.1 * rresponse));
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
                        r *= rsim * resneigbor * rkdt; // *shareresponse[fid][id1].Average();
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

                    {
                        //rsim=0
                        r *= Math.Exp(expN * responseitems[fid].Count);
                        if (!scorX.ContainsKey(-r)) scorX.Add(-r, new List<string>());
                        scorX[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["XF"].Contains(fid))
                    {
                        //rsim=0
                        r *= Math.Exp(expN * responseitems[fid].Count);
                        if (!scorXF.ContainsKey(-r)) scorXF.Add(-r, new List<string>());
                        scorXF[-r].Add(fid);
                    }
                    else
                    {
                        //rsim=0
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
                        foreach (var ssitem in dataLF[id1][fid])
                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                                kir += dataItemLF[fid][ssitem].Count; //每一个项目，邻居的二次转发
                    //kir = kir / dataLF[id1][fid].Count;
                    kir = Math.Exp(0.001 * kir);
                    if (users[fid].Ratio[0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                        sir *= 1 + 6 * users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId];
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
                    rresponse = 1;

                    if (netrelation[id1].ContainsKey(fid))
                    {
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
                    foreach (DateTime it in users[fid].ResponesTime)
                    {
                        tsp = (testdate - it).TotalDays;
                        ritem += Math.Exp(-0.06 * tsp);
                    }

                    r = sir * ritem * users[id1].SimFusers[fid] * users[fid].Level *
                        0.1; // *timeratio* (1 - Math.Exp(-0.1 * rresponse));
                    r = r * fir * kir;


                    r *= Math.Exp(-2 * userfreq[fid]) * Math.Exp(2 * usertimes[fid]);
                    r *= Math.Exp(expN * responseitems[fid].Count);
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

                kx = -1;
                var wrt = id1 + "\t" + allNeigbors[id1]["F"].Count.ToString() + "\t" +
                          shareitems[id1][itemid].Count
                              .ToString(); // itemid + "\t" +(++rec).ToString()+"\t"+ shareitems[id1][itemid].Count.ToString() ;
                if (!AddedNumber.ContainsKey(itemid)) AddedNumber.Add(itemid, new Dictionary<string, int>());
                if (!AddedNumber[itemid].ContainsKey(id1)) AddedNumber[itemid].Add(id1, 0);
                AddedNumber[itemid][id1]++;


                foreach (var xx in scorF.Keys)
                {
                    foreach (var yid in scorF[xx])
                    {
                        if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                            continue; //同一item，同一分享用户，回流的用户只会回流一次（train）
                        if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                        //if (responsenum.ContainsKey(id1) && responsenum[id1].ContainsKey(yid) && responsenum[id1][yid].Contains(itemid)) continue;
                        //if (responsenum.ContainsKey(yid) && responsenum[yid].ContainsKey(id1) && responsenum[yid][id1].Contains(itemid)) continue;
                        if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) &&
                            haveadded[itemid][id1].Contains(yid)) continue;
                        //if (users[id1].NewNeighbor.ContainsKey(yid) && (d - users[id1].NewNeighbor[yid].Max()).TotalDays < 1) continue;


                        if (res.Count < candNum)
                            res.Add(yid);
                        else break;
                    }

                    if (res.Count == candNum) break;
                }

                if (res.Count < candNum)
                    foreach (var xx in scorL.Keys)
                    {
                        foreach (var yid in scorL[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                                continue; //同一item，同一分享用户，回流的用户只会回流一次（train）

                            //if (responsenum.ContainsKey(id1) && responsenum[id1].ContainsKey(yid) && responsenum[id1][yid].Contains(itemid)) continue;
                            //if (responsenum.ContainsKey(yid) && responsenum[yid].ContainsKey(id1) && responsenum[yid][id1].Contains(itemid)) continue;
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

                if (res.Count < candNum)
                    //三阶
                    foreach (var xx in scorXF.Keys)
                    {
                        foreach (var yid in scorXF[xx])
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
                var uid = shareIDs[itemid].Keys.ToList();
                var shflag = false;
                //string preid = "";
                foreach (var xid in uid)
                {
                    if (xid == id1) continue;
                    var ld = shareIDs[itemid][xid].ToList();
                    foreach (var dx in ld)
                    {
                        var days = (dx - testdate).TotalHours;
                        if (days > 0 && days < 12)
                        {
                            if (res.Contains(xid))
                            {
                                var nnk = res.Count;
                                for (var ii = 0; ii < nnk; ++ii)
                                    if (res[ii] == xid)
                                    {
                                        res[ii] = res[0];
                                        res[0] = xid;
                                        shareIDs[itemid].Remove(xid);
                                        break;
                                    }
                            }
                            else if (res.Count < 5)
                            {
                                res.Add(xid);
                            }
                            else
                            {
                                res[4] = xid;
                            }

                            shflag = true;
                            break;
                        }
                    }

                    if (shflag) break;
                }

                if (res.Count == candNum || (users[id1].SimLusers.Count == 5 && res.Count == candNum - 1))
                {
                    if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1)
                                                      && haveadded[itemid][id1].Contains(res[0]))
                        res.Remove(res[0]);
                    else if (res.Count == candNum) res.Remove(res[candNum - 1]);
                    if (backadded.ContainsKey(itemid) && backadded[itemid].ContainsKey(id1))
                    {
                        var stmp = res[0];
                        var nnk = res.Count;
                        var rtmp = new List<string>();
                        for (var ii = nnk - 1; ii >= 0; --ii)
                            if (backadded[itemid][id1].Contains(res[ii]))
                            {
                                rtmp.Add(res[ii]);
                                res.Remove(res[ii]);
                            }

                        nnk = res.Count;
                        if (nnk < candNum - 1)
                        {
                            var ttk = rtmp.Count;
                            for (var ii = 0; ii < ttk; ++ii) res.Add("");
                            // 有数据进入rtmp
                            for (var ii = candNum - 2; ii >= ttk + 1; --ii) res[ii] = res[ii - ttk]; //剩余没有进入rtmp的数据移到最后
                            if (nnk == 0)
                                for (int ii = 0, jj = 0; jj < ttk; ++ii, ++jj)
                                    res[ii] = rtmp[jj]; //移到back中的数据加入到第一个数据后
                            else
                                for (int ii = 1, jj = 0; jj < ttk; ++ii, ++jj)
                                    res[ii] = rtmp[jj]; //移到back中的数据加入到第一个数据后
                        }
                    }

                    if (!backadded.ContainsKey(itemid))
                        backadded.Add(itemid, new Dictionary<string, HashSet<string>>());
                    if (!backadded[itemid].ContainsKey(id1)) backadded[itemid].Add(id1, new HashSet<string>());
                    backadded[itemid][id1].Add(res[0]);
                    //if(id1== "c4ae226760403027b6b4ad2f36663fb4" && itemid== "5e3952e42fd8021ef07f479698c26d84"
                    //    && testdate==new DateTime(2022, 11, 29, 13, 33, 25))
                    //{
                    //    int yux = -1;
                    //}
                    if (AddedNumber[itemid][id1] % (candNum - 1) == 0)
                    {
                        if (!haveadded.ContainsKey(itemid))
                            haveadded.Add(itemid, new Dictionary<string, HashSet<string>>());
                        if (!haveadded[itemid].ContainsKey(id1)) haveadded[itemid].Add(id1, new HashSet<string>());
                        foreach (var xmid in backadded[itemid][id1])
                            haveadded[itemid][id1].Add(xmid);
                        backadded[itemid][id1].Clear();
                    }
                }

                else if (res.Count > 0)
                {
                    var mk = res.Count;
                    if (backadded.ContainsKey(itemid) && backadded[itemid].ContainsKey(id1))
                    {
                        var stmp = res[0];
                        var nnk = mk;
                        var rtmp = new List<string>();
                        for (var ii = nnk - 1; ii >= 0; --ii)
                            if (backadded[itemid][id1].Contains(res[ii]))
                            {
                                rtmp.Add(res[ii]);
                                res.Remove(res[ii]);
                            }

                        nnk = res.Count;
                        var ttk = rtmp.Count;
                        for (var ii = 0; ii < ttk; ++ii) res.Add("");


                        // 有数据进入rtmp
                        for (var ii = mk - 1; ii >= ttk + 1; --ii) res[ii] = res[ii - ttk]; //剩余没有进入rtmp的数据移到最后
                        if (nnk == 0)
                            for (int ii = 0, jj = 0; jj < ttk; ++ii, ++jj)
                                res[ii] = rtmp[jj]; //移到back中的数据加入到第一个数据后
                        else
                            for (int ii = 1, jj = 0; jj < ttk; ++ii, ++jj)
                                res[ii] = rtmp[jj]; //移到back中的数据加入到第一个数据后
                    }

                    if (!backadded.ContainsKey(itemid))
                        backadded.Add(itemid, new Dictionary<string, HashSet<string>>());
                    if (!backadded[itemid].ContainsKey(id1)) backadded[itemid].Add(id1, new HashSet<string>());
                    backadded[itemid][id1].Add(res[0]);
                }


                shareIDs[itemid].Remove(id1);
                wrt = "";
                wrt = id1 + "\t" + itemid;
                foreach (var xxid in res)
                    if (responsenum[id1].ContainsKey(xxid))
                        wrt = wrt + "\t" + responsenum[id1][xxid].Count.ToString();
                ppr.WriteLine(wrt);
                //if (shareNumber[itemid][id1] > users[id1].SimLusers.Count)
                //{
                //    wrt = id1 + "\t" + itemid + "\t" + (shareNumber[itemid][id1] - users[id1].SimLusers.Count).ToString() + "\t" + testdate.ToString();
                //    foreach (string ss in res) wrt = wrt + "\t" + ss;
                //    ppr.WriteLine(wrt);
                //}


                diffnum[res.Count]++;
                subnew.Add(testdata[0].ToString(), res);
            }

            // for (int ii = 0; ii < 6; ++ii) ppr.WriteLine(ii.ToString() + "\t" + diffnum[ii].ToString());
            foreach (var id in subnew.Keys)
            {
                subtemp = new SubmitInfo();
                subtemp.triple_id = id;
                subtemp.candidate_voter_list = subnew[id].ToArray();
                submitres.Add(subtemp);
            }

            ppr.Close();
            var text = JsonMapper.ToJson(submitres);
            System.IO.File.WriteAllText("submit.json", text);
        }
    }
}