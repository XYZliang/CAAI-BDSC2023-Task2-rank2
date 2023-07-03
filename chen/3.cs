OpenFileDialog ofd = new OpenFileDialog(); // 创建一个文件对话框对象
ofd.Title = "分享数据"; // 设置文件对话框的标题
ofd.ShowDialog(); // 显示文件对话框
JsonData jsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName)); // 读取选定文件的内容并解析为Json对象，这是分享数据

ofd.Title = "商品数据"; // 设置文件对话框的标题
ofd.ShowDialog(); // 显示文件对话框
JsonData itemjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName)); // 读取选定文件的内容并解析为Json对象，这是商品数据

ofd.Title = "用户数据"; // 设置文件对话框的标题
ofd.ShowDialog(); // 显示文件对话框
JsonData userjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName)); // 读取选定文件的内容并解析为Json对象，这是用户数据

ofd.Title = "测试数据"; // 设置文件对话框的标题
ofd.ShowDialog(); // 显示文件对话框
JsonData testjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName)); // 读取选定文件的内容并解析为Json对象，这是测试数据

SortedDictionary<DateTime, List<LinkInfo>> data = new SortedDictionary<DateTime, List<LinkInfo>>(),
    data2 = new SortedDictionary<DateTime, List<LinkInfo>>(); // 创建两个已排序的字典来存储链接信息，以日期为键，链接信息列表为值
DateTime dt; // 创建一个日期时间对象，用于后续操作
LinkInfo lk; // 创建一个链接信息对象，用于后续操作
int n = 0, k; // 创建两个整型变量，初始化n为0
string itemid; // 创建一个字符串变量，用于保存商品id
string id1, id2; // 创建两个字符串变量，用于后续操作

// 创建几个字典对象来存储各类信息
Dictionary<string, UserIDInfo> users = new Dictionary<string, UserIDInfo>(); // 用户id对应用户信息
Dictionary<string, ItemInfo> itemsinfo = new Dictionary<string, ItemInfo>(); // 商品id对应商品信息
Dictionary<string, Dictionary<string, HashSet<string>>> itemreceive = new Dictionary<string, Dictionary<string, HashSet<string>>>(); // 商品接收者信息
Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<string, double>>>> netrelation = new Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<string, double>>>>(); // 网络关系信息
Dictionary<string, Dictionary<string, HashSet<string>>> responseitems = new Dictionary<string, Dictionary<string, HashSet<string>>>(); // 响应商品信息
Dictionary<string, Dictionary<string, double>> sharerank = new Dictionary<string, Dictionary<string, double>>(); // 分享排名信息

string item; // 创建一个字符串变量，用于保存商品
Dictionary<string, Dictionary<string, SortedDictionary<DateTime, List<string>>>> ranks = new Dictionary<string, Dictionary<string, SortedDictionary<DateTime, List<string>>>>(); // 排名信息

// 创建几个字典和哈希集合来存储各类信息
Dictionary<string, HashSet<string>> sharenum = new Dictionary<string, HashSet<string>>(), responseall = new Dictionary<string, HashSet<string>>();
Dictionary<string, Dictionary<string, HashSet<string>>> responsenum = new Dictionary<string, Dictionary<string, HashSet<string>>>();
Dictionary<string, double> activenum = new Dictionary<string, double>();
Dictionary<string, Dictionary<string, HashSet<string>>> allNeigbors = new Dictionary<string, Dictionary<string, HashSet<string>>>();

Dictionary<string, int> sharetimes = new Dictionary<string, int>(); // 分享次数信息
Dictionary<string, double> activefrenq = new Dictionary<string, double>(); // 活跃频率信息
Dictionary<string, List<DateTime>> userresptime = new Dictionary<string, List<DateTime>>(); // 用户响应时间信息
Dictionary<string, Dictionary<string, SortedSet<DateTime>>> shareusers = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(),
    responstimeAllitems = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(),
    sharetrainitem = new Dictionary<string, Dictionary<string, SortedSet<DateTime>>>(); // 分享用户、所有商品响应时间和分享训练商品的信息
Dictionary<string, Dictionary<string, Dictionary<string, int>>> userclassinfo = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>(); // 用户分类信息
Dictionary<string, Dictionary<string, HashSet<string>>> itemuserclassinfo = new Dictionary<string, Dictionary<string, HashSet<string>>>(); // 商品用户分类信息
Dictionary<string, Dictionary<string,List< double>>> shareresponse = new Dictionary<string, Dictionary<string, List< double>>>(); // 分享响应信息
Dictionary<string, string> classtype = new Dictionary<string, string>(); // 分类类型信息
Dictionary<string, double> classtypeweight = new Dictionary<string, double>(); // 分类类型权重信息
List<string> classtypekey = new List<string>() { "Brand", "CateID", "CateOne", "Shop" }; // 分类类型键列表
List<double> weight = new List<double>() { 0.1, 0.1, 0.5, 0.3 }; // 权重列表
foreach (string xid in classtypekey) { classtype.Add(xid, ""); classtypeweight.Add(xid, weight[n++]); } // 遍历classtypekey，将其项作为键添加到classtype和classtypeweight字典中，值分别为""和weight的项
n = 0; // 将n重置为0

// 遍历classtype的键，为每一个键在userclassinfo和itemuserclassinfo字典中添加一个新的空字典
foreach (string xid in classtype.Keys)
{
    userclassinfo.Add(xid, new Dictionary<string, Dictionary<string, int>>());
    itemuserclassinfo.Add(xid, new Dictionary<string, HashSet<string>>());
}

// 遍历用户的Json数据，为每一个用户在各种数据结构中创建空间，用于存储用户的信息和用户的行为
foreach (JsonData temp in userjsonData)
{
    JsonData user_id = temp[0]; // 获取用户的id
    JsonData level = temp[3]; // 获取用户的level
    id1 = user_id.ToString(); // 将用户id转换为字符串

    users.Add(id1, new UserIDInfo()); // 在用户信息字典中添加新的用户信息
    users[id1].SimLLusers = new Dictionary<string, double>(); // 记录每个用户分享回流item的次数（如果分享一次，折算为2）
    users[id1].SimFFusers = new Dictionary<string, double>(); // 记录是否已分享，已分享记为1
    responstimeAllitems.Add(id1, new Dictionary<string, SortedSet<DateTime>>()); // 添加用户的所有商品响应时间
    responseall.Add(id1, new HashSet<string>()); // 添加用户的所有响应
    shareresponse.Add(id1, new Dictionary<string,List< double>>()); // 添加用户的分享响应
    sharetrainitem.Add(id1, new Dictionary<string, SortedSet<DateTime>>()); // 添加用户的分享训练商品
    users[id1].ItemPath = new Dictionary<string, int>(); // 添加用户的商品路径
    users[id1].Level = int.Parse(level.ToString()); // 解析并设置用户的级别
    users[id1].Gender = int.Parse(temp[1].ToString()); // 解析并设置用户的性别
    users[id1].Age = int.Parse(temp[2].ToString()); // 解析并设置用户的年龄
    users[id1].Neighbor = new Dictionary<string, List<DateTime>>(); // 添加用户的邻居
    users[id1].NewNeighbor = new Dictionary<string, List<DateTime>>(); // 添加用户的新邻居
    users[id1].Ratio = new Dictionary<int, Dictionary<string, double>>(); // 添加用户的比率
    users[id1].ResponesTime = new List<DateTime>(); // 添加用户的响应时间
    users[id1].ItemID = new HashSet<string>(); // 添加用户的商品ID
    users[id1].responseTimeZone = new Dictionary<int, double>(); // 添加用户的响应时间区
    users[id1].StaticSimUsers = new Dictionary<string, double>(); // 添加用户的静态相似用户
    netrelation.Add(id1, new Dictionary<string, Dictionary<int, Dictionary<string, double>>>()); // 添加用户的网络关系
    responseitems.Add(id1, new Dictionary<string, HashSet<string>>()); // 添加用户的响应商品
    sharerank.Add(id1, new Dictionary<string, double>()); // 添加用户的分享排名
    sharenum.Add(id1, new HashSet<string>()); // 添加用户的分享数量
    responsenum.Add(id1, new Dictionary<string, HashSet<string>>()); // 添加用户的响应数量
    activenum.Add(id1, 0); // 设置用户的活跃度为0
    allNeigbors.Add(id1, new Dictionary<string, HashSet<string>>()); // 添加用户的所有邻居
    allNeigbors[id1].Add("F", new HashSet<string>()); // 添加一阶邻居
    allNeigbors[id1].Add("L", new HashSet<string>()); // 添加一阶邻居
    allNeigbors[id1].Add("LF", new HashSet<string>()); // 添加二阶邻居
    allNeigbors[id1].Add("XF", new HashSet<string>()); // 添加三阶邻居
    activefrenq.Add(id1, 0); // 设置用户的活跃频率为0
    userresptime.Add(id1, new List<DateTime>()); // 添加用户的响应时间
}
// 遍历商品的Json数据，为每一个商品在各种数据结构中创建空间，并存储相关信息
foreach (JsonData temp in itemjsonData)
{
    JsonData item_id = temp[0]; // 获取商品id
    JsonData cate_id = temp[1]; // 获取商品分类id
    JsonData level_id = temp[2]; // 获取等级id
    JsonData brandid = temp[3]; // 获取品牌id
    JsonData shopid = temp[4]; // 获取店铺id
    itemid = item_id.ToString(); // 将商品id转换为字符串
    itemsinfo.Add(itemid, new ItemInfo()); // 在商品信息字典中添加新的商品信息
    itemsinfo[itemid].ShopId = shopid.ToString(); // 设置商品的店铺id
    itemsinfo[itemid].BrandId = brandid.ToString(); // 设置商品的品牌id
    itemsinfo[itemid].CateId = cate_id.ToString(); // 设置商品的分类id
    itemsinfo[itemid].CateLevelOneId = level_id.ToString(); // 设置商品的一级分类id
    itemreceive.Add(itemid, new Dictionary<string, HashSet<string>>()); // 为每一个商品添加一个新的接收字典
}

// 遍历整体的Json数据，为每一个用户在各种数据结构中创建空间，存储相关信息，以及处理特定的业务逻辑
foreach (JsonData temp in jsonData)
{
    JsonData user_id = temp[0]; // 获取用户id
    JsonData item_id = temp[1]; // 获取商品id
    JsonData voter_id = temp[2]; // 获取投票者id
    JsonData timestamp = temp[3]; // 获取时间戳
    lk = new LinkInfo();
    id1 = lk.UserID = user_id.ToString(); // 将用户id转换为字符串并设置为LinkInfo的用户id
    item = lk.ItemID = item_id.ToString(); // 将商品id转换为字符串并设置为LinkInfo的商品id
    id2 = lk.VoterID = voter_id.ToString(); // 将投票者id转换为字符串并设置为LinkInfo的投票者id
    dt = DateTime.Parse(timestamp.ToString()); // 将时间戳转换为DateTime对象

    if (!responstimeAllitems[id2].ContainsKey(item)) responstimeAllitems[id2].Add(item, new SortedSet<DateTime>()); // 如果在所有商品的响应时间中没有该商品，则添加
    responstimeAllitems[id2][item].Add(dt); // 添加商品的响应时间
    if (!data.ContainsKey(dt)) data.Add(dt, new List<LinkInfo>()); // 如果数据中没有该时间，则添加
    data[dt].Add(lk); // 向特定时间的数据中添加链接信息

    // 管理ranks数据结构，记录特定用户对特定商品在特定时间的投票行为
    if (!ranks.ContainsKey(id1)) ranks.Add(id1, new Dictionary<string, SortedDictionary<DateTime, List<string>>>());
    if (!ranks[id1].ContainsKey(item)) ranks[id1].Add(item, new SortedDictionary<DateTime, List<string>>());
    if (!ranks[id1][item].ContainsKey(dt)) ranks[id1][item].Add(dt, new List<string>());
    ranks[id1][item][dt].Add(id2);

    sharenum[id1].Add(item); // 添加用户的分享数

    // 管理responsenum数据结构，记录用户对投票者的响应数
    if (!responsenum[id1].ContainsKey(id2)) responsenum[id1].Add(id2, new HashSet<string>());
    responsenum[id1][id2].Add(item);

    activenum[id2] += 1; // 增加投票者的活跃度

    // 设置商品的各种类型
    classtype["Brand"] = itemsinfo[item].BrandId;
    classtype["CateID"] = itemsinfo[item].CateId;
    classtype["CateOne"] = itemsinfo[item].CateLevelOneId;
    classtype["Shop"] = itemsinfo[item].ShopId;

    // 遍历所有的类型，为每一个类型的每一个用户和商品创建空间，以及处理相关的业务逻辑
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
        itemuserclassinfo[xid][classtype[xid]].Add(id1);
        itemuserclassinfo[xid][classtype[xid]].Add(id2);
    }

    ++n; // 增加计数器
}

// 清空所有的Json数据
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

//遍历每个在ranks中的id
foreach (string id in ranks.Keys)
{
    // 初始化两个变量tt和ii
    double tt = 1;
    foreach (string fid in ranks[id].Keys)
    {
        double ii = 1;

        //遍历ranks[id][fid]中的每一个日期
        foreach (DateTime d in ranks[id][fid].Keys)
        {
            //遍历ranks[id][fid][d]中的每一个xid
            foreach (string xid in ranks[id][fid][d])
            {
                //如果sharerank[id]中不包含xid，那么将xid加入sharerank[id]并赋值为1.0 / ii / tt，否则就将sharerank[id][xid]的值增加1.0 / ii / tt
                if (!sharerank[id].ContainsKey(xid)) sharerank[id].Add(xid, 1.0 / ii / tt);
                else sharerank[id][xid] += 1.0 / ii / tt;
            }
            //ii增加ranks[id][fid][d].Count的值
            ii += ranks[id][fid][d].Count;
        }
    }
}

//初始化k值
k = 0;
//初始化一些新的字典对象
Dictionary<string, Dictionary<string, HashSet<string>>> dataLF = new Dictionary<string, Dictionary<string, HashSet<string>>>(),
    dataItemLF = new Dictionary<string, Dictionary<string, HashSet<string>>>();
Dictionary<string, List<DateTime>> items = new Dictionary<string, List<DateTime>>();
Dictionary<string, HashSet<string>> itemusers = new Dictionary<string, HashSet<string>>();
DateTime dtmax = data.Keys.Max(), dtmin = data.Keys.Min();
// 遍历数据中的每一个日期
foreach (DateTime d in data.Keys)
{
    // 遍历每个日期对应的数据项
    foreach (LinkInfo llk in data[d])
    {
        itemid = llk.ItemID; // 获取当前数据项的ItemID
        id1 = llk.UserID; id2 = llk.VoterID; // 获取用户ID和投票者ID

        userresptime[id2].Add(d); // 将当前日期添加到用户的响应时间中
        responseall[id2].Add(itemid); // 将itemid添加到用户的所有响应中

        // 如果sharetrainitem对应用户不包含当前的itemid，则新建一组数据项
        if (!sharetrainitem[id1].ContainsKey(itemid)) sharetrainitem[id1].Add(itemid, new SortedSet<DateTime>());
        sharetrainitem[id1][itemid].Add(d); // 在对应的itemid下添加当前日期

        // 如果用户的邻居中没有包含当前投票者id2，则在网络关系和用户邻居中添加相关的数据结构
        if (!users[id1].Neighbor.ContainsKey(id2))
        {
            netrelation[id1].Add(id2, new Dictionary<int, Dictionary<string, double>>());
            users[id1].Neighbor.Add(id2, new List<DateTime>());
        }

        // 为网络关系的二级字典添加新的空字典
        for (int ii = 0; ii < 4; ++ii)
        {
            if (!netrelation[id1][id2].ContainsKey(ii)) netrelation[id1][id2].Add(ii, new Dictionary<string, double>());
        }

        // 下面的四个if-else结构类似，根据商品信息更新网络关系中的各级类别、店铺和品牌的权重
        // 如果对应的键不存在，则新建并设置值为1，否则增加权重
        if (!netrelation[id1][id2][0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
            netrelation[id1][id2][0].Add(itemsinfo[itemid].CateLevelOneId, 1);
        else netrelation[id1][id2][0][itemsinfo[itemid].CateLevelOneId]++;
        // 同样操作对二级类别、店铺ID和品牌ID
        // ...

        users[id1].Neighbor[id2].Add(d); // 将当前日期添加到用户的邻居对应的时间列表中
        users[id2].ResponesTime.Add(d); // 将当前日期添加到投票者的响应时间列表中
        users[id1].ItemID.Add(llk.ItemID); // 将商品ID添加到用户的商品列表中
        users[id2].ItemID.Add(llk.ItemID); // 将商品ID添加到投票者的商品列表中

        // 如果商品的时间列表不存在，则新建一个时间列表
        if (!items.ContainsKey(llk.ItemID))
            items.Add(llk.ItemID, new List<DateTime>());

        items[llk.ItemID].Add(d); // 将当前日期添加到商品的时间列表中
        // 同样的操作对用户和接收者进行添加和更新
        // ...

        // 计算投票者的活跃频率，如果当前日期距离最大日期不超过30天，则活跃频率加1
        if ((dtmax - d).TotalDays < 30) activefrenq[id2]++;

        // ...

        // 更新邻居关系和计数器k
        allNeigbors[id1]["F"].Add(id2); allNeigbors[id2]["L"].Add(id1);
        ++k;
    }
}

// 对活跃频率进行归一化处理
List<string> strlist = activefrenq.Keys.ToList();
double actmax = activefrenq.Values.Max();
foreach (string iid in strlist) activefrenq[iid] = (activefrenq[iid] + 0.001) / actmax;
// 遍历网络关系中的所有ID
foreach (string iid in netrelation.Keys)
{
    // 遍历每个ID关联的所有ID
    foreach (string fid in netrelation[iid].Keys)
    {
        // 遍历每个关联ID的所有属性
        foreach (int xid in netrelation[iid][fid].Keys)
        {
            // 计算属性值的总和
            double yy = netrelation[iid][fid][xid].Values.Sum();
            // 生成属性键列表
            List<string> tmparr = new List<string>(netrelation[iid][fid][xid].Keys);

            // 遍历属性键列表，对每个属性进行归一化处理
            foreach (string mid in tmparr)
            {
                netrelation[iid][fid][xid][mid] = netrelation[iid][fid][xid][mid] * 1.0 / yy;
            }
        }
    }
}

int k1, k2;
double sim;

// 遍历所有邻居列表
foreach (string id in allNeigbors.Keys)
{
    // 寻找二阶邻居，遍历每个邻居的邻居，并将其添加到二阶邻居列表中
    foreach (string fid in allNeigbors[id]["F"])
    {
        foreach (string xid in allNeigbors[fid]["F"]) allNeigbors[id]["LF"].Add(xid);
        foreach (string xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
    }
    foreach (string fid in allNeigbors[id]["L"])
    {
        foreach (string xid in allNeigbors[fid]["F"]) allNeigbors[id]["LF"].Add(xid);
        foreach (string xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
    }

    // 从二阶邻居列表中移除一阶邻居，避免重复
    foreach (string fid in allNeigbors[id]["F"])
        allNeigbors[id]["LF"].Remove(fid);
    foreach (string fid in allNeigbors[id]["L"])
        allNeigbors[id]["LF"].Remove(fid);

    // 寻找三阶邻居，遍历二阶邻居的邻居，并将其添加到三阶邻居列表中
    foreach (string fid in allNeigbors[id]["LF"])
    {
        foreach (string xid in allNeigbors[fid]["F"]) allNeigbors[id]["XF"].Add(xid);
        foreach (string xid in allNeigbors[fid]["L"]) allNeigbors[id]["XF"].Add(xid);
    }

    // 从三阶邻居列表中移除一阶邻居和二阶邻居，避免重复
    foreach (string fid in allNeigbors[id]["F"])
        allNeigbors[id]["XF"].Remove(fid);
    foreach (string fid in allNeigbors[id]["L"])
        allNeigbors[id]["XF"].Remove(fid);
    foreach (string fid in allNeigbors[id]["LF"])
        allNeigbors[id]["XF"].Remove(fid);
}
// 定义一些用于接收和处理数据的变量
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

// 遍历测试数据
foreach (JsonData testdata in testjsonData)
{
    // 解析JSON数据
    id1 = testdata[1].ToString();
    itemid = testdata[2].ToString();
    DateTime testdate = DateTime.Parse(testdata[3].ToString());

    // 初始化并记录每个ID共享的项目和时间
    if (!shareitems.ContainsKey(id1)) shareitems.Add(id1, new Dictionary<string, List<DateTime>>());
    if (!shareitems[id1].ContainsKey(itemid)) shareitems[id1].Add(itemid, new List<DateTime>());
    shareitems[id1][itemid].Add(testdate);
    IDTest.Add(id1);

    // 初始化并记录每个项目的共享信息
    if (!shareIDs.ContainsKey(itemid))
    {
        shareIDs.Add(itemid, new Dictionary<string, List<DateTime>>());
        shareNumber.Add(itemid, new Dictionary<string, int>());
    }
    if (!shareIDs[itemid].ContainsKey(id1)) { shareIDs[itemid].Add(id1, new List<DateTime>()); shareNumber[itemid].Add(id1, 0); }
    shareIDs[itemid][id1].Add(testdate);
    shareNumber[itemid][id1]++;

    // 记录项目的最后共享时间
    if (!lastshare.ContainsKey(itemid)) lastshare.Add(itemid, testdate);
    else lastshare[itemid] = testdate;

    // 记录项目的共享次数
    if (!sharetimes.ContainsKey(itemid)) sharetimes.Add(itemid, 1);
    else sharetimes[itemid]++;

    // 记录项目的共享用户和时间
    if (!shareusers.ContainsKey(itemid)) shareusers.Add(itemid, new Dictionary<string, SortedSet<DateTime>>());
    if (!shareusers[itemid].ContainsKey(id1))
        shareusers[itemid].Add(id1, new SortedSet<DateTime>()); shareusers[itemid][id1].Add(testdate);

    // 记录详细的共享信息
    if (!sharedetails.ContainsKey(itemid)) sharedetails.Add(itemid, new Dictionary<string, List<DateTime>>());
    if (!sharedetails[itemid].ContainsKey(id1)) sharedetails[itemid].Add(id1, new List<DateTime>());
    sharedetails[itemid][id1].Add(testdate);
}

// 定义用户频率和时间的字典
Dictionary<string, double> userfreq = new Dictionary<string, double>(), usertimes = new Dictionary<string, double>();
// 遍历所有用户
foreach (string iid in users.Keys)
{
    // 如果用户的响应时间记录不为空，则计算用户的频率（最后一次响应距离现在的时间），否则设置为从开始到现在的时间
    if (userresptime[iid].Count > 0)
        userfreq.Add(iid, (dtmax - userresptime[iid].Max()).TotalDays);
    else userfreq.Add(iid, (dtmax - dtmin).TotalDays);

    // 初始化用户的响应次数
    usertimes.Add(iid, 0);
    foreach (DateTime dx in userresptime[iid])
    {
        // 如果用户在过去30天内有响应，则增加响应次数
        if ((dtmax - dx).TotalDays < 30) usertimes[iid]++;
    }

    // 初始化分类数量字典
    Dictionary<int, Dictionary<string, int>> catenum = new Dictionary<int, Dictionary<string, int>>();
    for (int ii = 0; ii < 4; ++ii)
    {
        catenum.Add(ii, new Dictionary<string, int>());
        users[iid].Ratio.Add(ii, new Dictionary<string, double>());
    }

    // 遍历用户的所有项目
    foreach (string fid in users[iid].ItemID)
    {
        // 计算各级别的分类数量
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

    double tt;
    for (int ii = 0; ii < 4; ++ii)
    {
        // 计算每一级别的分类占比
        tt = catenum[ii].Values.Sum();
        foreach (string xx in catenum[ii].Keys)
        {
            users[iid].Ratio[ii].Add(xx, catenum[ii][xx] * 1.0 / tt);
        }
    }
}

// 获取用户频率和响应次数的最大值
double freqmax = userfreq.Values.Max(), timesmax = usertimes.Values.Max();

// 归一化用户频率和响应次数
foreach (string iid in users.Keys) { userfreq[iid] /= freqmax; usertimes[iid] /= timesmax; }

// 初始化用户数量的变量
int kx = 0, kn = users.Count;
// 创建相似邻居和单一日期字典
Dictionary<string, Dictionary<string, double>> SimNeigbor = new Dictionary<string, Dictionary<string, double>>();
Dictionary<string, double> SingleDt = new Dictionary<string, double>();

// 遍历测试ID集合
foreach (string iid in IDTest)
{
    // 计数，每100次更新一次界面显示的字符串
    if (++kx % 100 == 0) this.Text = kx.ToString() + "  " + kn.ToString();
    Application.DoEvents();

    // 初始化相似的后置用户和前置用户字典
    users[iid].SimLusers = new Dictionary<string, double>();
    users[iid].SimFusers = new Dictionary<string, double>();

    // 创建排序字典用于存储相似的用户
    SortedDictionary<double, List<string>> simUser = new SortedDictionary<double, List<string>>();

    // 创建集合用于存储用户项目ID相同的用户
    HashSet<string> backusers = new HashSet<string>();
    foreach (string itemiid in users[iid].ItemID)
    {
        foreach (string zid in itemusers[itemiid]) backusers.Add(zid);
    }

    // 遍历backusers
    foreach (string fid in backusers)
    {
        if (fid == iid) continue; // 如果fid等于当前测试ID，跳过本次循环

        // 计算交集和并集的大小
        k2 = (users[iid].ItemID.Intersect(users[fid].ItemID)).Count();
        k1 = (users[iid].ItemID.Union(users[fid].ItemID)).Count();
        if (k2 == 0) continue; // 如果交集大小为0，跳过本次循环

        // 计算相似度
        sim = -k2 * 1.0 / k1 * (1 - 0.5/ Math.Sqrt(k1));
        double edt = 0;
        if (users[iid].Neighbor.ContainsKey(fid))
        {
            // 计算相似邻居的得分
            k1 = sharenum[iid].Count; k2 = responsenum[iid][fid].Count;
            if (!SimNeigbor.ContainsKey(iid)) SimNeigbor.Add(iid, new Dictionary<string, double>());
            double sk= k2 * 1.0 / k1 * (1 - 0.5/ Math.Sqrt(k1));
            SimNeigbor[iid].Add(fid, sk);
        }

        // 向simUser添加相似用户
        if (!simUser.ContainsKey(sim)) simUser.Add(sim, new List<string>());
        simUser[sim].Add(fid);
    }

    // 向SimLusers添加相似用户
    foreach (double dd in simUser.Keys)
    {
        foreach (string fid in simUser[dd])
        {
            users[iid].SimLusers.Add(fid, -dd);
        }
    }
}

// 创建其他相关字典
Dictionary<int, int> diffnum = new Dictionary<int, int>();
int candNum = 6;
Dictionary<string, Dictionary<string, int>> AddNum = new Dictionary<string, Dictionary<string, int>>();
for (int ii = 0; ii < candNum; ++ii) diffnum.Add(ii, 0);
Dictionary<string, List<string>> subnew = new Dictionary<string, List<string>>();
Dictionary<string, Dictionary<string, HashSet<string>>> haveadded = new Dictionary<string, Dictionary<string, HashSet<string>>>(),
    backadded = new Dictionary<string, Dictionary<string, HashSet<string>>>();
Dictionary<string, Dictionary<string, int>> AddedNumber = new Dictionary<string, Dictionary<string, int>>();
k = 0;
Dictionary<string, Dictionary<string, int>> usersClassNum = new Dictionary<string, Dictionary<string, int>>();
