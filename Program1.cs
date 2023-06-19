using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LitJson; // You'll need to add LitJson as a NuGet package

// 存储用户动态商品分享数据，item_share_train_info.json
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
    public double Level { get; set; }
    public double Gender { get; set; }

    public double Age { get; set; }

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
}

// 存储输出的提交文件数据
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
            program.newSubmittPlus();
        }

        // 比较两个浮点数是否相等
        public bool isEqual(double a, double b)
        {
            return Math.Abs(a - b) < 1e-5;
        }

        private void submittwoPlusTheSimilarityOfUserAttributes()
        {
            // 该代码段定义了一个名为submittwo3272的方法，用于加载和解析分享数据、商品数据、用户数据和测试数据。接下来，它创建了两个根据时间排序的字典以存储链接信息。最后，定义了一些变量，用于在后续处理中存储不同类型的信息。
            // 创建一个打开文件对话框实例
            var ofd = new OpenFileDialog();

            // 为打开文件对话框设置标题
            ofd.Title = "分享数据";
            // 显示打开文件对话框
            ofd.ShowDialog();
            // 读取选择的文件并将其内容解析为JsonData对象
            var jsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));

            // 重复上述步骤，加载商品数据
            ofd.Title = "商品数据";
            ofd.ShowDialog();
            var itemjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));

            // 重复上述步骤，加载用户数据
            ofd.Title = "用户数据";
            ofd.ShowDialog();
            var userjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));

            // 重复上述步骤，加载测试数据
            ofd.Title = "测试数据";
            ofd.ShowDialog();
            var testjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));

            // 输出信息
            Console.WriteLine("数据加载完成");

            // 创建两个根据时间排序的字典，用于存储链接信息
            SortedDictionary<DateTime, List<LinkInfo>> data = new SortedDictionary<DateTime, List<LinkInfo>>(),
                data2 = new SortedDictionary<DateTime, List<LinkInfo>>();

            // 定义一个DateTime变量，用于存储时间信息
            DateTime dt;
            // 定义一个LinkInfo变量，用于存储链接信息
            LinkInfo lk;

            // 定义两个整数变量，n和k
            int n = 0, k;
            // 定义两个字符串变量，itemid和id1、id2
            string itemid;
            string id1, id2;


            // 定义一个权重W，用于计算用户相似度时，基于用户性别年龄等级上的相似度与基于用户商品兴趣之间的相似度的比重
            var userGenderAgeClassSimilarityWeighting = 0.2;
            // 定义二层嵌套字典，用于储存并计算用户的平均性别独热编码
            var userGenderAverageOneHotCode = new int[2];
            // 定义两个变量，用于储存并计算用户的平均年龄段、等级
            double userAverageAge = 0, userAverageClass = 0;
            // 定义年龄段、等级的最大值
            int maxAge = 8, maxClass = 10;

            // 在这段代码中，您创建了许多嵌套字典，用于存储不同类型的信息，例如用户和商品的数据、网络关系数据、回流用户与邀请用户和商品的关系等。这些数据结构将在后续的数据处理和模型构建过程中发挥作用。
            // 创建一个存储UserIDInfo的字典，键为用户ID
            var users = new Dictionary<string, UserIDInfo>();

            // 创建一个存储ItemInfo的字典，键为商品ID
            var itemsinfo = new Dictionary<string, ItemInfo>();

            // 创建一个三层嵌套字典，用于存储每个商品被每个用户接收的信息
            var itemreceive =
                new Dictionary<string, Dictionary<string, HashSet<string>>>();

            // 创建一个四层嵌套字典，用于存储网络关系数据
            var
                netrelation = new Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<string, double>>>>();

            // 创建一个三层嵌套字典，用于存储回流用户与邀请用户和商品的关系
            var responseitems =
                new Dictionary<string, Dictionary<string, HashSet<string>>>(); //voteid, userid, itemid

            // 创建一个二层嵌套字典，用于存储每个用户分享的商品的排名
            var sharerank =
                new Dictionary<string, Dictionary<string, double>>(); //id, item, rank

            // 定义一个字符串变量item
            string item;

            // 创建一个三层嵌套字典，用于存储商品、用户和时间的排名数据，多层嵌套类似于树状结构
            var ranks =
                new Dictionary<string, Dictionary<string, SortedDictionary<DateTime, List<string>>>>();

            // 创建一个存储用户分享次数的字典，键为用户ID
            var sharenum = new Dictionary<string, HashSet<string>>();

            // 创建一个三层嵌套字典，用于存储回流用户与邀请用户和商品的关系
            var responsenum =
                new Dictionary<string, Dictionary<string, HashSet<string>>>();

            // 遍历了用户数据集，并从中提取了用户ID、等级、性别和年龄等信息。接着，您将这些信息添加到了之前创建的字典和数据结构中
            // 记录有效的平均用户年龄和等级的数量
            int userAverageAgeCount = 0, userAverageClassCount = 0;
            // 记录有效的平均用户年龄和等级的和
            double userAverageAgeSum = 0, userAverageClassSum = 0;
            // 记录有效的平均用户性别的数量
            var userGenderAverageCount = 0;
            // 记录有效的平均用户性别的独热编码的和
            var userGenderAverageSum = new int[2];
            //输出信息 变量初始化完成
            Console.WriteLine("变量初始化完成");
            // 遍历用户数据集
            foreach (JsonData temp in userjsonData)
            {
                var user_id = temp[0];
                // 获取用户ID
                //JsonData cate_id = temp[1];
                //JsonData level_id = temp[2];
                // 获取用户等级
                var level = temp[3];
                //JsonData shopid = temp[4];

                // 将用户ID转换为字符串
                id1 = user_id.ToString();

                // 将用户ID添加到users字典中，并为其分配一个新的UserIDInfo对象
                users.Add(id1, new UserIDInfo());
                // 设置用户的等级，并进行归一化处理
                users[id1].Level = int.Parse(level.ToString()) * 1.0 / maxClass;
                // 保留两位小数
                users[id1].Level = Math.Round(users[id1].Level, 3);
                // 设置用户的性别
                var intTypeGender = int.Parse(temp[1].ToString());
                users[id1].Gender = 1.0 * intTypeGender;
                // 设置用户的年龄，并进行归一化处理
                users[id1].Age = int.Parse(temp[2].ToString()) * 1.0 / maxAge;
                // 保留两位小数
                users[id1].Age = Math.Round(users[id1].Age, 3);
                // 初始化用户的性别独热编码
                users[id1].GenderOneHot = new double[2];
                // 设置用户的年龄独热编码
                users[id1].GenderOneHot = intTypeGender == 0 ? new double[2] { 1, 0 } : //女性
                    intTypeGender == 1 ? new double[2] { 0, 1 } : //男性
                    new double[2] { 0, 0 }; //未知

                // 判断年龄等级有效，是的话则进行累加
                if (users[id1].Age > 0)
                {
                    userAverageAgeCount++;
                    userAverageAgeSum += users[id1].Age;
                }

                // 判断用户等级有效，是的话则进行累加
                if (users[id1].Level > 0)
                {
                    userAverageClassCount++;
                    userAverageClassSum += users[id1].Level;
                }

                // 判断用户性别有效，是的话则对独热编码进行累加
                if (intTypeGender == 0 || intTypeGender == 1)
                {
                    userGenderAverageCount++;
                    // 累加独热编码
                    foreach (int value in users[id1].GenderOneHot)
                    {
                        var index = Array.IndexOf(users[id1].GenderOneHot, value);
                        userGenderAverageSum[index] += value;
                    }
                }

                // 初始化用户的邻居信息、新邻居信息、响应时间等数据结构
                users[id1].Neighbor = new Dictionary<string, List<DateTime>>();
                users[id1].NewNeighbor = new Dictionary<string, List<DateTime>>();
                users[id1].Ratio = new Dictionary<int, Dictionary<string, double>>();
                users[id1].ResponesTime = new List<DateTime>();
                users[id1].ItemID = new HashSet<string>();
                users[id1].responseTimeZone = new Dictionary<int, double>();
                users[id1].StaticSimUsers = new Dictionary<string, double>();

                // 为用户ID初始化网络关系、回流商品、分享排名等数据结构
                netrelation.Add(id1, new Dictionary<string, Dictionary<int, Dictionary<string, double>>>());
                responseitems.Add(id1, new Dictionary<string, HashSet<string>>());
                sharerank.Add(id1, new Dictionary<string, double>());
                sharenum.Add(id1, new HashSet<string>());
                responsenum.Add(id1, new Dictionary<string, HashSet<string>>());
            }

            // 计算平均用户年龄、等级、性别独热编码
            userAverageAge = userAverageAgeSum / userAverageAgeCount;
            userAverageClass = userAverageClassSum / userAverageClassCount;
            var userGenderAverage = new double[2];
            foreach (var value in userGenderAverageSum)
            {
                var index = Array.IndexOf(userGenderAverageSum, value);
                userGenderAverage[index] = userGenderAverageSum[index] * 1.0 / userGenderAverageCount;
            }

            // 通过性别独热编码计算平均性别,取三位小数
            var userAverageGender = Math.Round(userGenderAverage[1], 3);

            // 输出
            // 用户数据读取完毕
            Console.WriteLine("用户数据读取完毕，共有" + users.Count + "个用户");
            Console.WriteLine("用户平均年龄：" + userAverageAge);
            Console.WriteLine("用户平均等级：" + userAverageClass);
            Console.WriteLine("用户平均性别：" + userAverageGender);
            Console.WriteLine("用户平均性别独热编码：[ " + userGenderAverage[0] + " , " + userGenderAverage[1] + " ]");
            // 输出无效年龄、等级、性别数量
            Console.WriteLine("无效年龄数量：" + (users.Count - userAverageAgeCount) + "，无效等级数量：" +
                              (users.Count - userAverageClassCount) + "，无效性别数量：" +
                              (users.Count - userGenderAverageCount));
            // 重新遍历所有用户，将所有用户的未知年龄、等级、性别、性别独热编码设置为平均值
            foreach (var tempUser in users)
            {
                // 记录是否有修改
                var isChange = false;
                var userInfo = tempUser.Value;
                // 在这里处理 userInfo 对象
                // 判断用户年龄是否有效，无效则设置为平均值
                if (userInfo.Age <= 0)
                {
                    userInfo.Age = userAverageAge;
                    isChange = true;
                }

                // 判断用户等级是否有效，无效则设置为平均值
                if (userInfo.Level <= 0)
                {
                    userInfo.Level = userAverageClass;
                    isChange = true;
                }

                // 判断用户性别是否有效，无效则设置为平均值
                {
                    userInfo.Gender = userAverageGender;
                    userInfo.GenderOneHot = userGenderAverage;
                    isChange = true;
                }
                // 仅限debug使用
                // if (isChange)
                //     Console.WriteLine("用户" + tempUser.Key + "的年龄、等级、性别存在无效值，已设置为平均值:"+userInfo.Age+","+userInfo.Level+","+userInfo.Gender+","+userInfo.GenderOneHot);
            }

            // 输出阶段性提示
            Console.WriteLine("用户数据处理完毕，开始处理商品信息");
            // 遍历了商品数据集，并从中提取商品ID、类目ID、一级类目ID、品牌ID和店铺ID等信息，将这些信息添加到了之前创建的字典和数据结构中
            foreach (JsonData temp in itemjsonData)
            {
                // 获取商品ID、类目ID、一级类目ID、品牌ID和店铺ID
                var item_id = temp[0];
                var cate_id = temp[1];
                var level_id = temp[2];
                var brandid = temp[3];
                var shopid = temp[4];

                // 将商品ID转换为字符串
                itemid = item_id.ToString();

                // 将商品ID添加到itemsinfo字典中，并为其分配一个新的ItemInfo对象
                itemsinfo.Add(itemid, new ItemInfo());

                // 设置商品的店铺ID、品牌ID、类目ID和一级类目ID
                itemsinfo[itemid].ShopId = shopid.ToString();
                itemsinfo[itemid].BrandId = brandid.ToString();
                itemsinfo[itemid].CateId = cate_id.ToString();
                itemsinfo[itemid].CateLevelOneId = level_id.ToString();

                // 初始化每个商品ID对应的回流用户数据结构
                itemreceive.Add(itemid, new Dictionary<string, HashSet<string>>());
            }

            // 输出阶段性提示
            Console.WriteLine("商品数据处理完毕，开始处理回流数据");
            // 遍历了分享数据集，并从中提取了邀请用户ID、商品ID、回流用户ID和时间戳等信息，将这些信息添加到了之前创建的字典和数据结构中
            foreach (JsonData temp in jsonData)
            {
                // 获取邀请用户ID、商品ID、回流用户ID和时间戳
                var user_id = temp[0];
                var item_id = temp[1];
                var voter_id = temp[2];
                var timestamp = temp[3];

                // 创建一个新的LinkInfo对象并设置相应的属性
                lk = new LinkInfo();
                id1 = lk.UserID = user_id.ToString();
                item = lk.ItemID = item_id.ToString();
                id2 = lk.VoterID = voter_id.ToString();
                dt = DateTime.Parse(timestamp.ToString());

                // 将LinkInfo对象添加到按时间戳排序的字典中
                // data的定义：SortedDictionary<DateTime, List<LinkInfo>> data = new SortedDictionary<DateTime, List<LinkInfo>>()
                if (!data.ContainsKey(dt)) data.Add(dt, new List<LinkInfo>());
                data[dt].Add(lk);

                // 更新邀请者-商品-时间戳-受邀请者的排名信息，将用户动态商品分享数据存入了这样一个多层嵌套，类似于一个树状结构的字典中
                if (!ranks.ContainsKey(id1))
                    ranks.Add(id1, new Dictionary<string, SortedDictionary<DateTime, List<string>>>());
                if (!ranks[id1].ContainsKey(item)) ranks[id1].Add(item, new SortedDictionary<DateTime, List<string>>());
                if (!ranks[id1][item].ContainsKey(dt)) ranks[id1][item].Add(dt, new List<string>());
                ranks[id1][item][dt].Add(id2);

                // 更新用户的分享数量信息
                sharenum[id1].Add(item);

                // 更新用户间的回流数量信息
                if (!responsenum[id1].ContainsKey(id2)) responsenum[id1].Add(id2, new HashSet<string>());
                responsenum[id1][id2].Add(item);

                // 计数器递增
                ++n;
            }

            // 输出阶段性提示
            Console.WriteLine("回流数据处理完毕，开始计算回流用户的排名得分");
            // 遍历了之前填充的ranks字典，根据分享过的商品和回流用户信息计算了sharerank字典中的排名得分
            // 遍历ranks字典的键（用户ID）
            foreach (var id in ranks.Keys)
            {
                // 计算该用户分享的商品数量
                double tt = ranks[id].Count;

                // 遍历该用户分享过的每个商品
                foreach (var fid in ranks[id].Keys)
                {
                    double ii = 1;

                    // 遍历该用户分享过的商品的每个时间戳
                    foreach (var d in ranks[id][fid].Keys)
                    {
                        // 遍历该用户在特定时间戳下分享过的商品的每个回流用户
                        foreach (var xid in ranks[id][fid][d])
                            // 更新sharerank字典，计算回流用户的排名得分
                            // ii是当前时间戳下的回流用户数量，tt是该分享用户分享过的商品数量。
                            // 这个公式反映了回流用户在分享用户的分享行为中的权重。权重较高的回流用户在后续的推荐算法中会获得更高的优先级。
                            if (!sharerank[id].ContainsKey(xid)) sharerank[id].Add(xid, 1.0 / ii / tt);
                            else sharerank[id][xid] += 1.0 / ii / tt;

                        // 更新计数器，加上当前时间戳下的回流用户数量
                        ii += ranks[id][fid][d].Count;
                    }
                }
            }

            // 输出阶段性提示
            Console.WriteLine("用户排名计算完毕，开始计算商品排名");
            // 初始化了一些与回流用户和商品相关的字典以及商品的时间戳列表和用户集合
            // 初始化计数器
            k = 0;

            // 初始化与回流用户和商品相关的字典
            // dataLF 邀请者-回流者-商品
            // dataItemLF 邀请者-商品-回流者
            Dictionary<string, Dictionary<string, HashSet<string>>> dataLF =
                    new Dictionary<string, Dictionary<string, HashSet<string>>>(),
                dataItemLF = new Dictionary<string, Dictionary<string, HashSet<string>>>();

            // 初始化商品的时间戳列表和商品的用户集合
            var items = new Dictionary<string, List<DateTime>>();
            var itemusers = new Dictionary<string, HashSet<string>>();

            //
            // 遍历之前收集的所有时间戳
            foreach (var d in data.Keys)
            {
                // 遍历该时间戳下的所有链接信息
                foreach (var llk in data[d])
                {
                    itemid = llk.ItemID;
                    id1 = llk.UserID;
                    id2 = llk.VoterID;

                    // 更新dataLF字典，存储用户与回流用户之间的关系
                    if (!dataLF.ContainsKey(id1)) dataLF.Add(id1, new Dictionary<string, HashSet<string>>());
                    if (!dataLF[id1].ContainsKey(id2)) dataLF[id1].Add(id2, new HashSet<string>());
                    dataLF[id1][id2].Add(itemid);

                    // 更新dataItemLF字典，存储用户分享商品与回流用户之间的关系
                    if (!dataItemLF.ContainsKey(id1)) dataItemLF.Add(id1, new Dictionary<string, HashSet<string>>());
                    if (!dataItemLF[id1].ContainsKey(itemid)) dataItemLF[id1].Add(itemid, new HashSet<string>());
                    dataItemLF[id1][itemid].Add(id2);

                    // 更新users字典中的Neighbor字段，建立用户与回流用户之间的联系
                    if (!users[id1].Neighbor.ContainsKey(id2))
                    {
                        netrelation[id1].Add(id2, new Dictionary<int, Dictionary<string, double>>());
                        users[id1].Neighbor.Add(id2, new List<DateTime>());
                    }

                    // 初始化netrelation字典的各个层次
                    for (var ii = 0; ii < 4; ++ii)
                        if (!netrelation[id1][id2].ContainsKey(ii))
                            netrelation[id1][id2].Add(ii, new Dictionary<string, double>());

                    // 更新netrelation字典，存储用户与回流用户之间在不同维度上的联系强度,(交互次数)
                    // 将用户（id1）与回流用户（id2）之间的联系分为四个维度：一级类目（CateLevelOneId），二级类目（CateId），商铺（ShopId）和品牌（BrandId）
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

                    // 更新用户之间的联系时间戳
                    users[id1].Neighbor[id2].Add(d);
                    users[id2].ResponesTime.Add(d);
                    // 更新用户的商品ID集合
                    users[id1].ItemID.Add(llk.ItemID);
                    users[id2].ItemID.Add(llk.ItemID);

                    // 更新商品的时间戳信息
                    if (!items.ContainsKey(llk.ItemID))
                        items.Add(llk.ItemID, new List<DateTime>());
                    items[llk.ItemID].Add(d);

                    // 更新商品与用户的关系
                    if (!itemusers.ContainsKey(itemid)) itemusers.Add(itemid, new HashSet<string>());
                    itemusers[itemid].Add(id1);
                    itemusers[itemid].Add(id2);

                    // 更新商品接收者与回流用户的关系
                    if (!itemreceive[itemid].ContainsKey(id1)) itemreceive[itemid].Add(id1, new HashSet<string>());
                    itemreceive[itemid][id1].Add(id2);

                    // 更新回流用户与分享用户的关系
                    if (!responseitems[id2].ContainsKey(id1)) responseitems[id2].Add(id1, new HashSet<string>());
                    responseitems[id2][id1].Add(itemid);

                    ++k;
                }
            }

            // 遍历netrelation字典，键为分享者ID（iid）
            foreach (var iid in netrelation.Keys)
            {
                // 遍历netrelation[iid]字典，键为回流者ID（fid）
                foreach (var fid in netrelation[iid].Keys)
                {
                    // 遍历netrelation[iid][fid]字典，键为分类层级索引（xid）
                    foreach (var xid in netrelation[iid][fid].Keys)
                    {
                        // 计算当前分类层级索引下的所有值之和
                        var yy = netrelation[iid][fid][xid].Values.Sum();

                        // 用一个临时列表存储当前分类层级索引下的所有键
                        var tmparr = new List<string>(netrelation[iid][fid][xid].Keys);

                        // 遍历临时列表中的键（mid）
                        foreach (var mid in tmparr)
                            // 对当前键对应的值进行归一化处理，除以所有值之和（转化成联系强度的相对比例）
                            // 如果两个用户在某个维度上的联系强度值接近1，那么我们可以认为他们在这个维度上的联系非常紧密。
                            // 相反，如果联系强度值接近0，那么我们可以认为他们在这个维度上几乎没有联系
                            netrelation[iid][fid][xid][mid] = netrelation[iid][fid][xid][mid] * 1.0 / yy;
                    }
                }
            }

            // 定义两个整型变量k1和k2
            int k1, k2;

            // 定义一个双精度浮点型变量sim
            double sim;
            
            // 1 计算每个用户在不同分类层级（一级类目、二级类目、店铺ID、品牌ID）的比率，即用户在这些分类中分享物品的相对比例。这有助于了解用户的兴趣和偏好。
            // 2 计算每个用户在不同时间段的回流率。这有助于了解用户在哪些时间段更可能参与分享物品的行为。
            // 遍历users字典中的所有用户ID（iid）
            foreach (var iid in users.Keys)
            {
                // 创建一个嵌套的字典结构，用于存储不同分类层级的计数
                var catenum = new Dictionary<int, Dictionary<string, int>>();

                // 初始化字典结构和用户比率信息
                for (var ii = 0; ii < 4; ++ii)
                {
                    catenum.Add(ii, new Dictionary<string, int>());
                    users[iid].Ratio.Add(ii, new Dictionary<string, double>());
                }

                // 遍历用户分享过的所有物品ID（fid）
                foreach (var fid in users[iid].ItemID)
                {
                    // 对不同分类层级进行计数
                    // 一级类目
                    if (!catenum[0].ContainsKey(itemsinfo[fid].CateLevelOneId))
                        catenum[0].Add(itemsinfo[fid].CateLevelOneId, 1);
                    else catenum[0][itemsinfo[fid].CateLevelOneId]++;

                    // 二级类目
                    if (!catenum[1].ContainsKey(itemsinfo[fid].CateId))
                        catenum[1].Add(itemsinfo[fid].CateId, 1);
                    else catenum[1][itemsinfo[fid].CateId]++;

                    // 店铺ID
                    if (!catenum[2].ContainsKey(itemsinfo[fid].ShopId))
                        catenum[2].Add(itemsinfo[fid].ShopId, 1);
                    else catenum[2][itemsinfo[fid].ShopId]++;

                    // 品牌ID
                    if (!catenum[3].ContainsKey(itemsinfo[fid].BrandId))
                        catenum[3].Add(itemsinfo[fid].BrandId, 1);
                    else catenum[3][itemsinfo[fid].BrandId]++;
                }

                // 计算用户在不同分类层级的比率
                double tt;
                for (var ii = 0; ii < 4; ++ii)
                {
                    tt = catenum[ii].Values.Sum();
                    foreach (var xx in catenum[ii].Keys) users[iid].Ratio[ii].Add(xx, catenum[ii][xx] * 1.0 / tt);
                }

                // 计算用户在不同时间段的回流率
                var timetemp = new Dictionary<int, int>();
                foreach (var it in users[iid].ResponesTime)
                {
                    var timezone = it.TimeOfDay.Hours;
                    if (!timetemp.ContainsKey(timezone)) timetemp.Add(timezone, 1);
                    else timetemp[timezone]++;
                }

                // 计算回流率并存储在responseTimeZone字典中
                var timetotal = timetemp.Values.Sum();
                foreach (var it in timetemp.Keys) users[iid].responseTimeZone.Add(it, 1.0 * timetemp[it] / timetotal);
            }


            // 得到每个用户与其他用户的相似度
            // 初始化一个计数器 kx 用于追踪处理的用户数，并设置一个变量 kn 为用户总数。
            int kx = 0, kn = users.Count;
            // 遍历 users 字典中的所有用户（iid）
            foreach (var iid in users.Keys)
            {
                // 每当处理的用户数（kx）是 100 的倍数时，更新程序的文本显示以显示处理进度。
                if (++kx % 100 == 0) Text = kx.ToString() + "  " + kn.ToString();
                // 调用 Application.DoEvents() 以允许其他事件处理并保持程序响应。
                Application.DoEvents();
                // 为当前用户 iid 初始化一个新的字典 SimLusers 用于存储与其他用户的相似度。
                users[iid].SimLusers = new Dictionary<string, double>();
                // 创建一个有序字典 simUser 用于临时存储相似度和与之对应的用户列表。
                var simUser = new SortedDictionary<double, List<string>>();
                // 初始化一个 HashSet backusers 用于存储与当前用户 iid 分享过物品的所有用户。
                var backusers = new HashSet<string>();
                // 遍历当前用户 iid 分享过的所有物品（itemiid）
                foreach (var itemiid in users[iid].ItemID)
                    // 对于当前物品 itemiid，遍历与其相关的所有用户（zid），将它们添加到 backusers HashSet 中
                foreach (var zid in itemusers[itemiid])
                    backusers.Add(zid);

                // 遍历 backusers 中的所有用户（fid）
                foreach (var fid in backusers)
                {
                    // 如果当前用户 fid 与正在处理的用户 iid 相同，则跳过此次循环。
                    if (fid == iid) continue;
                    // 计算两个用户分享的物品的交集数量（k2）和并集数量（k1）
                    k2 = users[iid].ItemID.Intersect(users[fid].ItemID).Count();
                    k1 = users[iid].ItemID.Union(users[fid].ItemID).Count();
                    // 如果交集数量为零，则跳过此次循环。否则，根据公式计算用户间的相似度（sim）
                    if (k2 == 0) continue;
                    sim = -k2 * 1.0 / k1 * (1 - 1.0 / Math.Sqrt(k1));
                    // 如果 simUser 字典中尚未包含相似度 sim，则添加一个新条目，以相似度作为键，值为包含用户 fid 的列表
                    if (!simUser.ContainsKey(sim)) simUser.Add(sim, new List<string>());
                    // 将当前用户 fid 添加到 simUser 字典中相应相似度的用户列表中
                    simUser[sim].Add(fid);
                }

                // 遍历 simUser 字典中的所有相似度（dd）
                foreach (var dd in simUser.Keys)
                    // 对于每个相似度 dd，遍历与之对应的用户列表
                foreach (var fid in simUser[dd])
                    // 将用户 fid 和相似度 -dd 添加到当前用户 iid 的 SimLusers 字典中
                    users[iid].SimLusers.Add(fid, -dd);
            }

            // 从测试数据集中读取数据，并对其进行预处理以便于后续分析
            // 初始化一个名为 mmr 的变量以计算平均逆文档频率，以及一个名为 ktotal 的变量用于计算总数量
            double mmr = 0, ktotal = 0;
            // 初始化 tsp、rsim、ritem 和 rresponse 变量以计算临时评分、相似度、物品评分和响应评分
            double tsp, rsim, ritem, rresponse;
            // 初始化一个名为 receivedata 的字典，用于存储每个用户接收到的物品
            var receivedata = new Dictionary<string, HashSet<string>>();
            // 初始化一个名为 submitres 的 List，用于存储提交的结果
            var submitres = new List<SubmitInfo>();
            // 初始化一个名为 subtemp 的 SubmitInfo 对象
            SubmitInfo subtemp;
            // 创建一个 System.IO.StreamWriter 对象 ppr，用于将结果写入名为 "testres.txt" 的文件
            var ppr = new System.IO.StreamWriter("testres.txt");
            // 初始化一个名为 itemreceived 的字典，用于存储每个物品在特定日期被接收的信息
            var itemreceived =
                new Dictionary<string, Dictionary<DateTime, HashSet<string>>>();
            // 初始化一个名为 shareitems 的字典，用于存储每个用户分享过的物品及其分享时间
            var shareitems =
                new Dictionary<string, Dictionary<string, List<DateTime>>>();
            // 遍历测试数据集 testjsonData 中的每条数据
            foreach (JsonData testdata in testjsonData)
            {
                // 提取用户 ID（id1）
                id1 = testdata[1].ToString();
                // 提取物品 ID（itemid）
                itemid = testdata[2].ToString();
                // 将数据中的时间戳转换为 DateTime 对象（testdate）
                var testdate = DateTime.Parse(testdata[3].ToString());
                // 如果 shareitems 字典中尚未包含用户 id1，则添加一个新条目
                if (!shareitems.ContainsKey(id1)) shareitems.Add(id1, new Dictionary<string, List<DateTime>>());
                // 如果 shareitems 字典中的用户 id1 尚未包含物品 itemid，则添加一个新条目
                if (!shareitems[id1].ContainsKey(itemid)) shareitems[id1].Add(itemid, new List<DateTime>());
                // 将物品 itemid 的分享时间 testdate 添加到 shareitems 字典中相应的用户和物品条目下
                shareitems[id1][itemid].Add(testdate);
            }

            // 针对测试数据集中的每一条数据，计算相似用户对于物品的兴趣分数
            // 遍历测试数据集 testjsonData 中的每条数据
            foreach (JsonData testdata in testjsonData)
            {
                // 初始化一个名为 res 的 List，用于存储推荐结果
                var res = new List<string>();
                // 创建一个新的 LinkInfo 对象 linfo，用于存储链接信息
                var linfo = new LinkInfo();
                // 提取用户 ID（id1）并存储在 linfo.UserID 中
                id1 = linfo.UserID = testdata[1].ToString();
                // 提取物品 ID（itemid）并存储在 linfo.ItemID 中
                itemid = linfo.ItemID = testdata[2].ToString();
                // 将数据中的时间戳转换为 DateTime 对象（testdate）
                var testdate = DateTime.Parse(testdata[3].ToString());
                // 如果 receivedata 字典中尚未包含物品 itemid，则添加一个新条目
                if (!receivedata.ContainsKey(itemid)) receivedata.Add(itemid, new HashSet<string>());
                // 如果 users 字典中尚未包含用户 id1，则跳过此次循环
                if (!users.ContainsKey(id1)) continue;
                // 如果 itemreceived 字典中尚未包含物品 itemid，则添加一个新条目
                if (!itemreceived.ContainsKey(itemid))
                    itemreceived.Add(itemid, new Dictionary<DateTime, HashSet<string>>());
                // 如果 itemreceived[itemid] 字典中尚未包含 testdate.Date，则添加一个新条目
                if (!itemreceived[itemid].ContainsKey(testdate.Date))
                    itemreceived[itemid].Add(testdate.Date, new HashSet<string>());
                // 初始化一个名为 scor 的 SortedDictionary，用于存储评分
                var scor = new SortedDictionary<double, List<string>>();
                //  对每个相似用户 fid，计算兴趣分数（sir）、转发兴趣分数（kir）、响应相似度（rsim）和响应评分（rresponse），根据物品的一级类别、类别、店铺和品牌来计算兴趣分数
                // 遍历用户 id1 的相似用户（fid）
                foreach (var fid in users[id1].SimLusers.Keys)
                {
                    //对每一个相似用户
                    double r = 0, sir = 1, fir = 1, kir = 0;
                    if (dataLF.ContainsKey(id1) && dataLF[id1].ContainsKey(fid))
                        foreach (var ssitem in dataLF[id1][fid])
                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                                kir += dataItemLF[fid][ssitem].Count; //每一个项目，邻居的二次转发
                    //kir = kir / dataLF[id1][fid].Count;
                    kir = Math.Exp(0.001 * kir);
                    if (users[fid].Ratio[0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                        sir *= 1 + 5 * users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId];
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
                    // 计算与测试数据相关的各种评分，并将评分存储在 SortedDictionary 中
                    // 通过计算各种评分来衡量用户和物品之间的关联程度，进一步筛选出更符合用户兴趣的推荐物品
                    // 如果 netrelation[id1] 字典中包含键值 fid，则执行以下操作
                    if (netrelation[id1].ContainsKey(fid))
                    {
                        // 根据物品的一级类别、类别、店铺和品牌，计算转发兴趣分数（fir）
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

                    // 如果用户 id1 的邻居中包含用户 fid，则执行以下操作
                    if (users[id1].Neighbor.ContainsKey(fid))
                    {
                        // 遍历 users[id1].Neighbor[fid] 中的时间戳，计算响应相似度（rsim）
                        foreach (var it in users[id1].Neighbor[fid])
                        {
                            tsp = (testdate - it).TotalDays;
                            rsim += Math.Exp(-0.02 * tsp);
                        }

                        // 计算响应评分（rresponse）
                        rresponse = (testdate - users[id1].Neighbor[fid].Max()).TotalDays;
                        //if (users[id1].NewNeighbor.ContainsKey(fid))
                        //    rresponse = Math.Min(rresponse, (d - users[id1].NewNeighbor[fid].Max()).TotalDays);
                    }
                    // 否则，如果用户 fid 的邻居中包含用户 id1
                    else if (users[fid].Neighbor.ContainsKey(id1))
                    {
                        // 遍历 users[fid].Neighbor[id1] 中的时间戳，计算响应相似度（rsim）
                        foreach (var it in users[fid].Neighbor[id1])
                        {
                            tsp = (testdate - it).TotalDays;
                            rsim += Math.Exp(-0.1 * tsp);
                        }
                    }

                    // 初始化物品评分（ritem）为0
                    ritem = 0;
                    // 遍历用户 fid 的响应时间戳，计算物品评分（ritem）
                    foreach (var it in users[fid].ResponesTime)
                    {
                        tsp = (testdate - it).TotalDays;
                        ritem += Math.Exp(-0.05 * tsp);
                    }

                    // 计算总评分（r），这是将兴趣分数（sir）、响应相似度（rsim）、物品评分（ritem）、相似用户评分（users[id1].SimLusers[fid]）和用户等级（users[fid].Level）相乘后的结果
                    r = sir * rsim * ritem * users[id1].SimLusers[fid] * users[fid].Level * 0.1;
                    // *timeratio* (1 - Math.Exp(-0.1 * rresponse));
                    // 将总评分（r）与转发兴趣分数（fir）和转发兴趣比率（kir）相乘。
                    r = r * fir * kir;
                    // 如果 sharerank[id1] 字典中包含键值 fid，则将评分（r）与 sharerank[id1][fid] 相乘；否则，将评分（r）乘以一个很小的数（0.0000001）。
                    if (sharerank[id1].ContainsKey(fid)) r *= sharerank[id1][fid];
                    else r *= 0.0000001;
                    // 将评分（r）添加到 SortedDictionary scor 中。
                    //if (responsenum[id1].ContainsKey(fid)) r *= (1 + responsenum[id1][fid].Count * 1.0 / sharenum[id1].Count);
                    // if (r > 0)
                    {
                        if (!scor.ContainsKey(-r)) scor.Add(-r, new List<string>());
                        scor[-r].Add(fid);
                    }
                }

                // 对已计算出的评分进行筛选，并将前五名的评分和相关信息记录下来
                // 初始化变量 kx 为 -1
                kx = -1;
                // 定义一个字符串变量 wrt，包含用户 ID（id1）、物品 ID（itemid）、物品分享次数、用户的邻居数量、测试日期和相似用户的数量等信息
                var wrt = id1 + "\t" + itemid + "\t" + shareitems[id1][itemid].Count.ToString() + "\t" +
                          users[id1].Neighbor.Keys.Count.ToString() +
                          "\t" + testdate.ToString() +
                          "\t" + users[id1].SimLusers.Count.ToString();
                // 遍历 SortedDictionary scor 中的键（即评分）
                foreach (var xx in scor.Keys)
                {
                    // 遍历与当前评分关联的用户列表（yid），并进行一些筛选条件的检查
                    foreach (var yid in scor[xx])
                    {
                        if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                            continue; //同一item，同一分享用户，回流的用户只会回流一次（train）
                        if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                        //if (users[id1].NewNeighbor.ContainsKey(yid) && (d - users[id1].NewNeighbor[yid].Max()).TotalDays < 1) continue;
                        // 如果筛选后的用户列表 res 中的元素数量小于5，执行以下操作
                        if (res.Count < 5)
                        {
                            // 更新字符串变量 wrt，添加当前评分
                            wrt = wrt + "\t" + xx.ToString();
                            // if (itemreceive[itemid].Contains(yid)) continue;
                            // 添加当前用户 yid 到 res 列表中
                            res.Add(yid);
                            // 如果 res 列表中的元素数量等于1且物品的分享次数大于5
                            if (res.Count == 1 && shareitems[id1][itemid].Count > 5) //5比10好
                            {
                                //itemreceived[itemid][testdate.Date].Add(yid);
                                //receivedata[itemid].Add(yid);
                                // 将当前用户 yid 添加到 responseitems 字典中
                                if (!responseitems[yid].ContainsKey(id1))
                                    responseitems[yid].Add(id1, new HashSet<string>());
                                //if (users[id1].SimLusers[yid] > 0.6)
                                // 并将物品 ID（itemid）添加到该用户的响应物品列表中
                                responseitems[yid][id1].Add(itemid);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 如果 res 列表中的元素数量达到5，跳出当前循环
                    if (res.Count < 5) continue;
                }

                // 将推荐结果（即前五名用户）以及与之相关的信息记录下来，并将这些信息写入文件
                // 实例化一个 SubmitInfo 类对象 subtemp
                subtemp = new SubmitInfo();
                // 将当前测试数据的第一个元素（即 triple_id）赋值给 subtemp.triple_id
                subtemp.triple_id = testdata[0].ToString();
                // 将筛选后的前五名用户列表 res 转换为数组，并赋值给 subtemp.candidate_voter_list
                subtemp.candidate_voter_list = res.ToArray();
                // 将字符串变量 wrt 写入文本文件 testres.txt
                ppr.WriteLine(wrt);
                //if (subtemp.candidate_voter_list.Count() < 5)
                //{
                //    ppr.WriteLine(subtemp.triple_id + "\t" + subtemp.candidate_voter_list.Count().ToString());
                //}
                // 将 subtemp 对象添加到 submitres 列表中
                submitres.Add(subtemp);
            }

            // 关闭文本文件 testres.txt
            ppr.Close();
            // 将 submitres 列表转换为 JSON 格式的字符串 text
            var text = JsonMapper.ToJson(submitres);
            // 将字符串 text 写入文件 submit.json
            System.IO.File.WriteAllText("submit.json", text);
        }

        // 定义一个名为submittwo3272的私有方法
        private void submittwo3272()
        {
            // 该代码段定义了一个名为submittwo3272的方法，用于加载和解析分享数据、商品数据、用户数据和测试数据。接下来，它创建了两个根据时间排序的字典以存储链接信息。最后，定义了一些变量，用于在后续处理中存储不同类型的信息。
            // 创建一个打开文件对话框实例
            var ofd = new OpenFileDialog();

            // 为打开文件对话框设置标题
            ofd.Title = "分享数据";
            // 显示打开文件对话框
            ofd.ShowDialog();
            // 读取选择的文件并将其内容解析为JsonData对象
            var jsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));

            // 重复上述步骤，加载商品数据
            ofd.Title = "商品数据";
            ofd.ShowDialog();
            var itemjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));

            // 重复上述步骤，加载用户数据
            ofd.Title = "用户数据";
            ofd.ShowDialog();
            var userjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));

            // 重复上述步骤，加载测试数据
            ofd.Title = "测试数据";
            ofd.ShowDialog();
            var testjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));

            // 创建两个根据时间排序的字典，用于存储链接信息
            SortedDictionary<DateTime, List<LinkInfo>> data = new SortedDictionary<DateTime, List<LinkInfo>>(),
                data2 = new SortedDictionary<DateTime, List<LinkInfo>>();

            // 定义一个DateTime变量，用于存储时间信息
            DateTime dt;
            // 定义一个LinkInfo变量，用于存储链接信息
            LinkInfo lk;

            // 定义两个整数变量，n和k
            int n = 0, k;
            // 定义两个字符串变量，itemid和id1、id2
            string itemid;
            string id1, id2;

            // 在这段代码中，您创建了许多嵌套字典，用于存储不同类型的信息，例如用户和商品的数据、网络关系数据、回流用户与邀请用户和商品的关系等。这些数据结构将在后续的数据处理和模型构建过程中发挥作用。
            // 创建一个存储UserIDInfo的字典，键为用户ID
            var users = new Dictionary<string, UserIDInfo>();

            // 创建一个存储ItemInfo的字典，键为商品ID
            var itemsinfo = new Dictionary<string, ItemInfo>();

            // 创建一个三层嵌套字典，用于存储每个商品被每个用户接收的信息
            var itemreceive =
                new Dictionary<string, Dictionary<string, HashSet<string>>>();

            // 创建一个四层嵌套字典，用于存储网络关系数据
            var
                netrelation = new Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<string, double>>>>();

            // 创建一个三层嵌套字典，用于存储回流用户与邀请用户和商品的关系
            var responseitems =
                new Dictionary<string, Dictionary<string, HashSet<string>>>(); //voteid, userid, itemid

            // 创建一个二层嵌套字典，用于存储每个用户分享的商品的排名
            var sharerank =
                new Dictionary<string, Dictionary<string, double>>(); //id, item, rank

            // 定义一个字符串变量item
            string item;

            // 创建一个三层嵌套字典，用于存储商品、用户和时间的排名数据
            var ranks =
                new Dictionary<string, Dictionary<string, SortedDictionary<DateTime, List<string>>>>();

            // 创建一个存储用户分享次数的字典，键为用户ID
            var sharenum = new Dictionary<string, HashSet<string>>();

            // 创建一个三层嵌套字典，用于存储回流用户与邀请用户和商品的关系
            var responsenum =
                new Dictionary<string, Dictionary<string, HashSet<string>>>();

            // 遍历了用户数据集，并从中提取了用户ID、等级、性别和年龄等信息。接着，您将这些信息添加到了之前创建的字典和数据结构中
            // 遍历用户数据集
            foreach (JsonData temp in userjsonData)
            {
                var user_id = temp[0];
                // 获取用户ID
                //JsonData cate_id = temp[1];
                //JsonData level_id = temp[2];
                // 获取用户等级
                var level = temp[3];
                //JsonData shopid = temp[4];

                // 将用户ID转换为字符串
                id1 = user_id.ToString();

                // 将用户ID添加到users字典中，并为其分配一个新的UserIDInfo对象
                users.Add(id1, new UserIDInfo());
                // 设置用户的等级
                users[id1].Level = int.Parse(level.ToString());
                // 设置用户的性别
                users[id1].Gender = int.Parse(temp[1].ToString());
                // 设置用户的年龄
                users[id1].Age = int.Parse(temp[2].ToString());

                // 初始化用户的邻居信息、新邻居信息、响应时间等数据结构
                users[id1].Neighbor = new Dictionary<string, List<DateTime>>();
                users[id1].NewNeighbor = new Dictionary<string, List<DateTime>>();
                users[id1].Ratio = new Dictionary<int, Dictionary<string, double>>();
                users[id1].ResponesTime = new List<DateTime>();
                users[id1].ItemID = new HashSet<string>();
                users[id1].responseTimeZone = new Dictionary<int, double>();
                users[id1].StaticSimUsers = new Dictionary<string, double>();

                // 为用户ID初始化网络关系、回流商品、分享排名等数据结构
                netrelation.Add(id1, new Dictionary<string, Dictionary<int, Dictionary<string, double>>>());
                responseitems.Add(id1, new Dictionary<string, HashSet<string>>());
                sharerank.Add(id1, new Dictionary<string, double>());
                sharenum.Add(id1, new HashSet<string>());
                responsenum.Add(id1, new Dictionary<string, HashSet<string>>());
            }

            // 遍历了商品数据集，并从中提取商品ID、类目ID、一级类目ID、品牌ID和店铺ID等信息，将这些信息添加到了之前创建的字典和数据结构中
            foreach (JsonData temp in itemjsonData)
            {
                // 获取商品ID、类目ID、一级类目ID、品牌ID和店铺ID
                var item_id = temp[0];
                var cate_id = temp[1];
                var level_id = temp[2];
                var brandid = temp[3];
                var shopid = temp[4];

                // 将商品ID转换为字符串
                itemid = item_id.ToString();

                // 将商品ID添加到itemsinfo字典中，并为其分配一个新的ItemInfo对象
                itemsinfo.Add(itemid, new ItemInfo());

                // 设置商品的店铺ID、品牌ID、类目ID和一级类目ID
                itemsinfo[itemid].ShopId = shopid.ToString();
                itemsinfo[itemid].BrandId = brandid.ToString();
                itemsinfo[itemid].CateId = cate_id.ToString();
                itemsinfo[itemid].CateLevelOneId = level_id.ToString();

                // 初始化每个商品ID对应的回流用户数据结构
                itemreceive.Add(itemid, new Dictionary<string, HashSet<string>>());
            }


            // 遍历了分享数据集，并从中提取了邀请用户ID、商品ID、回流用户ID和时间戳等信息，将这些信息添加到了之前创建的字典和数据结构中
            foreach (JsonData temp in jsonData)
            {
                // 获取邀请用户ID、商品ID、回流用户ID和时间戳
                var user_id = temp[0];
                var item_id = temp[1];
                var voter_id = temp[2];
                var timestamp = temp[3];

                // 创建一个新的LinkInfo对象并设置相应的属性
                lk = new LinkInfo();
                id1 = lk.UserID = user_id.ToString();
                item = lk.ItemID = item_id.ToString();
                id2 = lk.VoterID = voter_id.ToString();
                dt = DateTime.Parse(timestamp.ToString());

                // 将LinkInfo对象添加到按时间戳排序的字典中
                if (!data.ContainsKey(dt)) data.Add(dt, new List<LinkInfo>());
                data[dt].Add(lk);

                // 更新用户-商品-时间戳的排名信息
                if (!ranks.ContainsKey(id1))
                    ranks.Add(id1, new Dictionary<string, SortedDictionary<DateTime, List<string>>>());
                if (!ranks[id1].ContainsKey(item)) ranks[id1].Add(item, new SortedDictionary<DateTime, List<string>>());
                if (!ranks[id1][item].ContainsKey(dt)) ranks[id1][item].Add(dt, new List<string>());
                ranks[id1][item][dt].Add(id2);

                // 更新用户的分享数量信息
                sharenum[id1].Add(item);

                // 更新用户间的回流数量信息
                if (!responsenum[id1].ContainsKey(id2)) responsenum[id1].Add(id2, new HashSet<string>());
                responsenum[id1][id2].Add(item);

                // 计数器递增
                ++n;
            }

            // 遍历了之前填充的ranks字典，根据分享过的商品和回流用户信息计算了sharerank字典中的排名得分
            // 遍历ranks字典的键（用户ID）
            foreach (var id in ranks.Keys)
            {
                // 计算该用户分享的商品数量
                double tt = ranks[id].Count;

                // 遍历该用户分享过的每个商品
                foreach (var fid in ranks[id].Keys)
                {
                    double ii = 1;

                    // 遍历该用户分享过的商品的每个时间戳
                    foreach (var d in ranks[id][fid].Keys)
                    {
                        // 遍历该用户在特定时间戳下分享过的商品的每个回流用户
                        foreach (var xid in ranks[id][fid][d])
                            // 更新sharerank字典，计算回流用户的排名得分
                            if (!sharerank[id].ContainsKey(xid)) sharerank[id].Add(xid, 1.0 / ii / tt);
                            else sharerank[id][xid] += 1.0 / ii / tt;

                        // 更新计数器，加上当前时间戳下的回流用户数量
                        ii += ranks[id][fid][d].Count;
                    }
                }
            }

            // 初始化了一些与回流用户和商品相关的字典以及商品的时间戳列表和用户集合
            // 初始化计数器
            k = 0;

            // 初始化与回流用户和商品相关的字典
            Dictionary<string, Dictionary<string, HashSet<string>>> dataLF =
                    new Dictionary<string, Dictionary<string, HashSet<string>>>(),
                dataItemLF = new Dictionary<string, Dictionary<string, HashSet<string>>>();

            // 初始化商品的时间戳列表和商品的用户集合
            var items = new Dictionary<string, List<DateTime>>();
            var itemusers = new Dictionary<string, HashSet<string>>();

            //
            // 遍历之前收集的所有时间戳
            foreach (var d in data.Keys)
                // 遍历该时间戳下的所有链接信息
            foreach (var llk in data[d])
            {
                itemid = llk.ItemID;
                id1 = llk.UserID;
                id2 = llk.VoterID;

                // 更新dataLF字典，存储用户与回流用户之间的关系
                if (!dataLF.ContainsKey(id1)) dataLF.Add(id1, new Dictionary<string, HashSet<string>>());
                if (!dataLF[id1].ContainsKey(id2)) dataLF[id1].Add(id2, new HashSet<string>());
                dataLF[id1][id2].Add(itemid);

                // 更新dataItemLF字典，存储用户分享商品与回流用户之间的关系
                if (!dataItemLF.ContainsKey(id1)) dataItemLF.Add(id1, new Dictionary<string, HashSet<string>>());
                if (!dataItemLF[id1].ContainsKey(itemid)) dataItemLF[id1].Add(itemid, new HashSet<string>());
                dataItemLF[id1][itemid].Add(id2);

                // 更新users字典中的Neighbor字段，建立用户与回流用户之间的联系
                if (!users[id1].Neighbor.ContainsKey(id2))
                {
                    netrelation[id1].Add(id2, new Dictionary<int, Dictionary<string, double>>());
                    users[id1].Neighbor.Add(id2, new List<DateTime>());
                }

                // 初始化netrelation字典的各个层次
                for (var ii = 0; ii < 4; ++ii)
                    if (!netrelation[id1][id2].ContainsKey(ii))
                        netrelation[id1][id2].Add(ii, new Dictionary<string, double>());

                // 更新netrelation字典，存储用户与回流用户之间在不同维度上的联系强度
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

                // 更新用户之间的联系时间戳
                users[id1].Neighbor[id2].Add(d);
                users[id2].ResponesTime.Add(d);
                // 更新用户的商品ID集合
                users[id1].ItemID.Add(llk.ItemID);
                users[id2].ItemID.Add(llk.ItemID);

                // 更新商品的时间戳信息
                if (!items.ContainsKey(llk.ItemID))
                    items.Add(llk.ItemID, new List<DateTime>());
                items[llk.ItemID].Add(d);

                // 更新商品与用户的关系
                if (!itemusers.ContainsKey(itemid)) itemusers.Add(itemid, new HashSet<string>());
                itemusers[itemid].Add(id1);
                itemusers[itemid].Add(id2);

                // 更新商品接收者与回流用户的关系
                if (!itemreceive[itemid].ContainsKey(id1)) itemreceive[itemid].Add(id1, new HashSet<string>());
                itemreceive[itemid][id1].Add(id2);

                // 更新回流用户与分享用户的关系
                if (!responseitems[id2].ContainsKey(id1)) responseitems[id2].Add(id1, new HashSet<string>());
                responseitems[id2][id1].Add(itemid);

                ++k;
            }

            // 遍历netrelation字典，键为分享者ID（iid）
            foreach (var iid in netrelation.Keys)
                // 遍历netrelation[iid]字典，键为回流者ID（fid）
            foreach (var fid in netrelation[iid].Keys)
                // 遍历netrelation[iid][fid]字典，键为分类层级索引（xid）
            foreach (var xid in netrelation[iid][fid].Keys)
            {
                // 计算当前分类层级索引下的所有值之和
                var yy = netrelation[iid][fid][xid].Values.Sum();

                // 用一个临时列表存储当前分类层级索引下的所有键
                var tmparr = new List<string>(netrelation[iid][fid][xid].Keys);

                // 遍历临时列表中的键（mid）
                foreach (var mid in tmparr)
                    // 对当前键对应的值进行归一化处理，除以所有值之和
                    netrelation[iid][fid][xid][mid] = netrelation[iid][fid][xid][mid] * 1.0 / yy;
            }

            // 定义两个整型变量k1和k2
            int k1, k2;

            // 定义一个双精度浮点型变量sim
            double sim;


            // 1 计算每个用户在不同分类层级（一级类目、二级类目、店铺ID、品牌ID）的比率，即用户在这些分类中分享物品的相对比例。这有助于了解用户的兴趣和偏好。
            // 2 计算每个用户在不同时间段的回流率。这有助于了解用户在哪些时间段更可能参与分享物品的行为。
            // 遍历users字典中的所有用户ID（iid）
            foreach (var iid in users.Keys)
            {
                // 创建一个嵌套的字典结构，用于存储不同分类层级的计数
                var catenum = new Dictionary<int, Dictionary<string, int>>();

                // 初始化字典结构和用户比率信息
                for (var ii = 0; ii < 4; ++ii)
                {
                    catenum.Add(ii, new Dictionary<string, int>());
                    users[iid].Ratio.Add(ii, new Dictionary<string, double>());
                }

                // 遍历用户分享过的所有物品ID（fid）
                foreach (var fid in users[iid].ItemID)
                {
                    // 对不同分类层级进行计数
                    // 一级类目
                    if (!catenum[0].ContainsKey(itemsinfo[fid].CateLevelOneId))
                        catenum[0].Add(itemsinfo[fid].CateLevelOneId, 1);
                    else catenum[0][itemsinfo[fid].CateLevelOneId]++;

                    // 二级类目
                    if (!catenum[1].ContainsKey(itemsinfo[fid].CateId))
                        catenum[1].Add(itemsinfo[fid].CateId, 1);
                    else catenum[1][itemsinfo[fid].CateId]++;

                    // 店铺ID
                    if (!catenum[2].ContainsKey(itemsinfo[fid].ShopId))
                        catenum[2].Add(itemsinfo[fid].ShopId, 1);
                    else catenum[2][itemsinfo[fid].ShopId]++;

                    // 品牌ID
                    if (!catenum[3].ContainsKey(itemsinfo[fid].BrandId))
                        catenum[3].Add(itemsinfo[fid].BrandId, 1);
                    else catenum[3][itemsinfo[fid].BrandId]++;
                }

                // 计算用户在不同分类层级的比率
                double tt;
                for (var ii = 0; ii < 4; ++ii)
                {
                    tt = catenum[ii].Values.Sum();
                    foreach (var xx in catenum[ii].Keys) users[iid].Ratio[ii].Add(xx, catenum[ii][xx] * 1.0 / tt);
                }

                // 计算用户在不同时间段的回流率
                var timetemp = new Dictionary<int, int>();
                foreach (var it in users[iid].ResponesTime)
                {
                    var timezone = it.TimeOfDay.Hours;
                    if (!timetemp.ContainsKey(timezone)) timetemp.Add(timezone, 1);
                    else timetemp[timezone]++;
                }

                // 计算回流率并存储在responseTimeZone字典中
                var timetotal = timetemp.Values.Sum();
                foreach (var it in timetemp.Keys) users[iid].responseTimeZone.Add(it, 1.0 * timetemp[it] / timetotal);
            }


            // 得到每个用户与其他用户的相似度
            // 初始化一个计数器 kx 用于追踪处理的用户数，并设置一个变量 kn 为用户总数。
            int kx = 0, kn = users.Count;
            // 遍历 users 字典中的所有用户（iid）
            foreach (var iid in users.Keys)
            {
                // 每当处理的用户数（kx）是 100 的倍数时，更新程序的文本显示以显示处理进度。
                if (++kx % 100 == 0) Text = kx.ToString() + "  " + kn.ToString();
                // 调用 Application.DoEvents() 以允许其他事件处理并保持程序响应。
                Application.DoEvents();
                // 为当前用户 iid 初始化一个新的字典 SimLusers 用于存储与其他用户的相似度。
                users[iid].SimLusers = new Dictionary<string, double>();
                // 创建一个有序字典 simUser 用于临时存储相似度和与之对应的用户列表。
                var simUser = new SortedDictionary<double, List<string>>();
                // 初始化一个 HashSet backusers 用于存储与当前用户 iid 分享过物品的所有用户。
                var backusers = new HashSet<string>();
                // 遍历当前用户 iid 分享过的所有物品（itemiid）
                foreach (var itemiid in users[iid].ItemID)
                    // 对于当前物品 itemiid，遍历与其相关的所有用户（zid），将它们添加到 backusers HashSet 中
                foreach (var zid in itemusers[itemiid])
                    backusers.Add(zid);

                // 遍历 backusers 中的所有用户（fid）
                foreach (var fid in backusers)
                {
                    // 如果当前用户 fid 与正在处理的用户 iid 相同，则跳过此次循环。
                    if (fid == iid) continue;
                    // 计算两个用户分享的物品的交集数量（k2）和并集数量（k1）
                    k2 = users[iid].ItemID.Intersect(users[fid].ItemID).Count();
                    k1 = users[iid].ItemID.Union(users[fid].ItemID).Count();
                    // 如果交集数量为零，则跳过此次循环。否则，根据公式计算用户间的相似度（sim）
                    if (k2 == 0) continue;
                    sim = -k2 * 1.0 / k1 * (1 - 1.0 / Math.Sqrt(k1));
                    // 如果 simUser 字典中尚未包含相似度 sim，则添加一个新条目，以相似度作为键，值为包含用户 fid 的列表
                    if (!simUser.ContainsKey(sim)) simUser.Add(sim, new List<string>());
                    // 将当前用户 fid 添加到 simUser 字典中相应相似度的用户列表中
                    simUser[sim].Add(fid);
                }

                // 遍历 simUser 字典中的所有相似度（dd）
                foreach (var dd in simUser.Keys)
                    // 对于每个相似度 dd，遍历与之对应的用户列表
                foreach (var fid in simUser[dd])
                    // 将用户 fid 和相似度 -dd 添加到当前用户 iid 的 SimLusers 字典中
                    users[iid].SimLusers.Add(fid, -dd);
            }

            // 从测试数据集中读取数据，并对其进行预处理以便于后续分析
            // 初始化一个名为 mmr 的变量以计算平均逆文档频率，以及一个名为 ktotal 的变量用于计算总数量
            double mmr = 0, ktotal = 0;
            // 初始化 tsp、rsim、ritem 和 rresponse 变量以计算临时评分、相似度、物品评分和响应评分
            double tsp, rsim, ritem, rresponse;
            // 初始化一个名为 receivedata 的字典，用于存储每个用户接收到的物品
            var receivedata = new Dictionary<string, HashSet<string>>();
            // 初始化一个名为 submitres 的 List，用于存储提交的结果
            var submitres = new List<SubmitInfo>();
            // 初始化一个名为 subtemp 的 SubmitInfo 对象
            SubmitInfo subtemp;
            // 创建一个 System.IO.StreamWriter 对象 ppr，用于将结果写入名为 "testres.txt" 的文件
            var ppr = new System.IO.StreamWriter("testres.txt");
            // 初始化一个名为 itemreceived 的字典，用于存储每个物品在特定日期被接收的信息
            var itemreceived =
                new Dictionary<string, Dictionary<DateTime, HashSet<string>>>();
            // 初始化一个名为 shareitems 的字典，用于存储每个用户分享过的物品及其分享时间
            var shareitems =
                new Dictionary<string, Dictionary<string, List<DateTime>>>();
            // 遍历测试数据集 testjsonData 中的每条数据
            foreach (JsonData testdata in testjsonData)
            {
                // 提取用户 ID（id1）
                id1 = testdata[1].ToString();
                // 提取物品 ID（itemid）
                itemid = testdata[2].ToString();
                // 将数据中的时间戳转换为 DateTime 对象（testdate）
                var testdate = DateTime.Parse(testdata[3].ToString());
                // 如果 shareitems 字典中尚未包含用户 id1，则添加一个新条目
                if (!shareitems.ContainsKey(id1)) shareitems.Add(id1, new Dictionary<string, List<DateTime>>());
                // 如果 shareitems 字典中的用户 id1 尚未包含物品 itemid，则添加一个新条目
                if (!shareitems[id1].ContainsKey(itemid)) shareitems[id1].Add(itemid, new List<DateTime>());
                // 将物品 itemid 的分享时间 testdate 添加到 shareitems 字典中相应的用户和物品条目下
                shareitems[id1][itemid].Add(testdate);
            }

            // 针对测试数据集中的每一条数据，计算相似用户对于物品的兴趣分数
            // 遍历测试数据集 testjsonData 中的每条数据
            foreach (JsonData testdata in testjsonData)
            {
                // 初始化一个名为 res 的 List，用于存储推荐结果
                var res = new List<string>();
                // 创建一个新的 LinkInfo 对象 linfo，用于存储链接信息
                var linfo = new LinkInfo();
                // 提取用户 ID（id1）并存储在 linfo.UserID 中
                id1 = linfo.UserID = testdata[1].ToString();
                // 提取物品 ID（itemid）并存储在 linfo.ItemID 中
                itemid = linfo.ItemID = testdata[2].ToString();
                // 将数据中的时间戳转换为 DateTime 对象（testdate）
                var testdate = DateTime.Parse(testdata[3].ToString());
                // 如果 receivedata 字典中尚未包含物品 itemid，则添加一个新条目
                if (!receivedata.ContainsKey(itemid)) receivedata.Add(itemid, new HashSet<string>());
                // 如果 users 字典中尚未包含用户 id1，则跳过此次循环
                if (!users.ContainsKey(id1)) continue;
                // 如果 itemreceived 字典中尚未包含物品 itemid，则添加一个新条目
                if (!itemreceived.ContainsKey(itemid))
                    itemreceived.Add(itemid, new Dictionary<DateTime, HashSet<string>>());
                // 如果 itemreceived[itemid] 字典中尚未包含 testdate.Date，则添加一个新条目
                if (!itemreceived[itemid].ContainsKey(testdate.Date))
                    itemreceived[itemid].Add(testdate.Date, new HashSet<string>());
                // 初始化一个名为 scor 的 SortedDictionary，用于存储评分
                var scor = new SortedDictionary<double, List<string>>();
                //  对每个相似用户 fid，计算兴趣分数（sir）、转发兴趣分数（kir）、响应相似度（rsim）和响应评分（rresponse），根据物品的一级类别、类别、店铺和品牌来计算兴趣分数
                // 遍历用户 id1 的相似用户（fid）
                foreach (var fid in users[id1].SimLusers.Keys)
                {
                    //对每一个相似用户
                    double r = 0, sir = 1, fir = 1, kir = 0;
                    if (dataLF.ContainsKey(id1) && dataLF[id1].ContainsKey(fid))
                        foreach (var ssitem in dataLF[id1][fid])
                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                                kir += dataItemLF[fid][ssitem].Count; //每一个项目，邻居的二次转发
                    //kir = kir / dataLF[id1][fid].Count;
                    kir = Math.Exp(0.001 * kir);
                    if (users[fid].Ratio[0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                        sir *= 1 + 5 * users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId];
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
                    // 计算与测试数据相关的各种评分，并将评分存储在 SortedDictionary 中
                    // 通过计算各种评分来衡量用户和物品之间的关联程度，进一步筛选出更符合用户兴趣的推荐物品
                    // 如果 netrelation[id1] 字典中包含键值 fid，则执行以下操作
                    if (netrelation[id1].ContainsKey(fid))
                    {
                        // 根据物品的一级类别、类别、店铺和品牌，计算转发兴趣分数（fir）
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

                    // 如果用户 id1 的邻居中包含用户 fid，则执行以下操作
                    if (users[id1].Neighbor.ContainsKey(fid))
                    {
                        // 遍历 users[id1].Neighbor[fid] 中的时间戳，计算响应相似度（rsim）
                        foreach (var it in users[id1].Neighbor[fid])
                        {
                            tsp = (testdate - it).TotalDays;
                            rsim += Math.Exp(-0.02 * tsp);
                        }

                        // 计算响应评分（rresponse）
                        rresponse = (testdate - users[id1].Neighbor[fid].Max()).TotalDays;
                        //if (users[id1].NewNeighbor.ContainsKey(fid))
                        //    rresponse = Math.Min(rresponse, (d - users[id1].NewNeighbor[fid].Max()).TotalDays);
                    }
                    // 否则，如果用户 fid 的邻居中包含用户 id1
                    else if (users[fid].Neighbor.ContainsKey(id1))
                    {
                        // 遍历 users[fid].Neighbor[id1] 中的时间戳，计算响应相似度（rsim）
                        foreach (var it in users[fid].Neighbor[id1])
                        {
                            tsp = (testdate - it).TotalDays;
                            rsim += Math.Exp(-0.1 * tsp);
                        }
                    }

                    // 初始化物品评分（ritem）为0
                    ritem = 0;
                    // 遍历用户 fid 的响应时间戳，计算物品评分（ritem）
                    foreach (var it in users[fid].ResponesTime)
                    {
                        tsp = (testdate - it).TotalDays;
                        ritem += Math.Exp(-0.05 * tsp);
                    }

                    // 计算总评分（r），这是将兴趣分数（sir）、响应相似度（rsim）、物品评分（ritem）、相似用户评分（users[id1].SimLusers[fid]）和用户等级（users[fid].Level）相乘后的结果
                    r = sir * rsim * ritem * users[id1].SimLusers[fid] * users[fid].Level * 0.1;
                    // *timeratio* (1 - Math.Exp(-0.1 * rresponse));
                    // 将总评分（r）与转发兴趣分数（fir）和转发兴趣比率（kir）相乘。
                    r = r * fir * kir;
                    // 如果 sharerank[id1] 字典中包含键值 fid，则将评分（r）与 sharerank[id1][fid] 相乘；否则，将评分（r）乘以一个很小的数（0.0000001）。
                    if (sharerank[id1].ContainsKey(fid)) r *= sharerank[id1][fid];
                    else r *= 0.0000001;
                    // 将评分（r）添加到 SortedDictionary scor 中。
                    //if (responsenum[id1].ContainsKey(fid)) r *= (1 + responsenum[id1][fid].Count * 1.0 / sharenum[id1].Count);
                    // if (r > 0)
                    {
                        if (!scor.ContainsKey(-r)) scor.Add(-r, new List<string>());
                        scor[-r].Add(fid);
                    }
                }

                // 对已计算出的评分进行筛选，并将前五名的评分和相关信息记录下来
                // 初始化变量 kx 为 -1
                kx = -1;
                // 定义一个字符串变量 wrt，包含用户 ID（id1）、物品 ID（itemid）、物品分享次数、用户的邻居数量、测试日期和相似用户的数量等信息
                var wrt = id1 + "\t" + itemid + "\t" + shareitems[id1][itemid].Count.ToString() + "\t" +
                          users[id1].Neighbor.Keys.Count.ToString() +
                          "\t" + testdate.ToString() +
                          "\t" + users[id1].SimLusers.Count.ToString();
                // 遍历 SortedDictionary scor 中的键（即评分）
                foreach (var xx in scor.Keys)
                {
                    // 遍历与当前评分关联的用户列表（yid），并进行一些筛选条件的检查
                    foreach (var yid in scor[xx])
                    {
                        if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid))
                            continue; //同一item，同一分享用户，回流的用户只会回流一次（train）
                        if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                        //if (users[id1].NewNeighbor.ContainsKey(yid) && (d - users[id1].NewNeighbor[yid].Max()).TotalDays < 1) continue;
                        // 如果筛选后的用户列表 res 中的元素数量小于5，执行以下操作
                        if (res.Count < 5)
                        {
                            // 更新字符串变量 wrt，添加当前评分
                            wrt = wrt + "\t" + xx.ToString();
                            // if (itemreceive[itemid].Contains(yid)) continue;
                            // 添加当前用户 yid 到 res 列表中
                            res.Add(yid);
                            // 如果 res 列表中的元素数量等于1且物品的分享次数大于5
                            if (res.Count == 1 && shareitems[id1][itemid].Count > 5) //5比10好
                            {
                                //itemreceived[itemid][testdate.Date].Add(yid);
                                //receivedata[itemid].Add(yid);
                                // 将当前用户 yid 添加到 responseitems 字典中
                                if (!responseitems[yid].ContainsKey(id1))
                                    responseitems[yid].Add(id1, new HashSet<string>());
                                //if (users[id1].SimLusers[yid] > 0.6)
                                // 并将物品 ID（itemid）添加到该用户的响应物品列表中
                                responseitems[yid][id1].Add(itemid);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 如果 res 列表中的元素数量达到5，跳出当前循环
                    if (res.Count < 5) continue;
                }

                // 将推荐结果（即前五名用户）以及与之相关的信息记录下来，并将这些信息写入文件
                // 实例化一个 SubmitInfo 类对象 subtemp
                subtemp = new SubmitInfo();
                // 将当前测试数据的第一个元素（即 triple_id）赋值给 subtemp.triple_id
                subtemp.triple_id = testdata[0].ToString();
                // 将筛选后的前五名用户列表 res 转换为数组，并赋值给 subtemp.candidate_voter_list
                subtemp.candidate_voter_list = res.ToArray();
                // 将字符串变量 wrt 写入文本文件 testres.txt
                ppr.WriteLine(wrt);
                //if (subtemp.candidate_voter_list.Count() < 5)
                //{
                //    ppr.WriteLine(subtemp.triple_id + "\t" + subtemp.candidate_voter_list.Count().ToString());
                //}
                // 将 subtemp 对象添加到 submitres 列表中
                submitres.Add(subtemp);
            }

            // 关闭文本文件 testres.txt
            ppr.Close();
            // 将 submitres 列表转换为 JSON 格式的字符串 text
            var text = JsonMapper.ToJson(submitres);
            // 将字符串 text 写入文件 submit.json
            System.IO.File.WriteAllText("submit.json", text);
        }

        // 新代码
        private void newSubmitt()
        {
            // 创建文件对话框来获取各类数据，然后把这些数据以json的形式解析后保存在相应的字典和列表中，方便后续处理和分析。
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
            Dictionary<string, Dictionary<string, List<double>>> shareresponse = new Dictionary<string, Dictionary<string, List<double>>>(); // 分享响应信息
            Dictionary<string, string> classtype = new Dictionary<string, string>(); // 分类类型信息
            Dictionary<string, double> classtypeweight = new Dictionary<string, double>(); // 分类类型权重信息
            List<string> classtypekey = new List<string>() { "Brand", "CateID", "CateOne", "Shop" }; // 分类类型键列表
            List<double> weight = new List<double>() { 0.1, 0.1, 0.5, 0.3 }; // 权重列表
            n = 0; // 重新设置n为0
                   //Dictionary<string, SortedDictionary<DateTime, List<Tuple<string, string>>>> ItemAll = 
                   //    new Dictionary<string, SortedDictionary<DateTime, List<Tuple<string, string>>>>();
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

                // 构建多个字典和列表，为每个用户存储相关的信息和行为数据。
                users.Add(id1, new UserIDInfo()); // 在用户信息字典中添加新的用户信息
                users[id1].SimLLusers = new Dictionary<string, double>(); // 记录每个用户分享回流item的次数（如果分享一次，折算为2）
                users[id1].SimFFusers = new Dictionary<string, double>(); // 记录是否已分享，已分享记为1
                responstimeAllitems.Add(id1, new Dictionary<string, SortedSet<DateTime>>()); // 添加用户的所有商品响应时间
                responseall.Add(id1, new HashSet<string>()); // 添加用户的所有响应
                shareresponse.Add(id1, new Dictionary<string, List<double>>()); // 添加用户的分享响应
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
            // 负责解析Json数据，以及在多种数据结构中创建空间来存储相关的信息。同时，它也处理了一些特定的业务逻辑，例如记录用户对投票者的响应数，增加投票者的活跃度，以及为每一个类型的每一个用户和商品创建空间等等。
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

            // 遍历数据集，然后基于这些数据构建一些特征，例如分享排名（sharerank）、数据连接（netrelation）、用户响应时间（userresptime）等等。可以根据这些特征来预测可能的voter_id
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
            // 进一步构造了一些基于用户和商品的特征，包括用户的响应时间，商品的访问时间，用户之间的关系网络以及一些基于类别、店铺和品牌的特征等。
            //遍历data中的每一个日期
            foreach (DateTime d in data.Keys)
            {
                //遍历data[d]中的每一个LinkInfo对象
                foreach (LinkInfo llk in data[d])
                {
                    itemid = llk.ItemID; // 获取当前数据项的ItemID
                    id1 = llk.UserID; id2 = llk.VoterID; // 获取用户ID和投票者ID

                    userresptime[id2].Add(d); // 将当前日期添加到用户的响应时间中
                    responseall[id2].Add(itemid); // 将itemid添加到用户的所有响应中

                    // 后续的代码都是根据读取到的数据进行特定业务逻辑的处理
                    // 比如，添加数据到相关的数据结构中，或者在某些条件满足的情况下，执行特定的操作，计算和更新一些特征等
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
            // 一是对网络关系的属性进行了归一化处理；二是寻找并构造了所有节点的二阶和三阶邻居，这将有助于从更广阔的视角分析和预测用户的行为。
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

            //double mmr = 0, ktotal = 0;
            // 针对一些测试数据进行了处理，通过字典的方式记录了用户对项目的共享信息，包括共享时间，共享次数等，用于后续的特征分析和预测
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
            // 进行了用户行为特征的提取，包括用户的响应频率，响应次数，以及用户对各级别分类的交互比例
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


            //foreach (string iid in IDTest)
            //{
            //    if (++kx % 100 == 0) this.Text = kx.ToString() + "  " + IDTest.Count.ToString();
            //    Application.DoEvents();
            //    foreach (string fid in users[iid].ItemPath.Keys)
            //    {
            //        int y0 = users[iid].ItemPath[fid], y1; //每一个item
            //        y1 = y0; id1 = iid;
            //        Queue<string> Lid = new Queue<string>(users[iid].Neighbor.Keys);
            //        int yk = Lid.Count;
            //        while (yk > 0)
            //        {
            //            for (int ix = 0; ix < yk; ++ix)
            //            {//BFS
            //                id2 = Lid.Dequeue();
            //                if (users[id2].ItemPath.ContainsKey(fid))
            //                {
            //                    if (!users[iid].StaticSimUsers.ContainsKey(id2))
            //                        users[iid].StaticSimUsers.Add(id2, Math.Pow(0.1, users[id2].ItemPath[fid] - y0));
            //                    else users[iid].StaticSimUsers[id2] += Math.Pow(0.1, users[id2].ItemPath[fid] - y0);
            //                    foreach (string mid in users[id2].Neighbor.Keys)
            //                    {
            //                        if (Lid.Contains(mid) || users[iid].StaticSimUsers.ContainsKey(mid)) continue;
            //                        Lid.Enqueue(mid);
            //                    }
            //                }

            //            }
            //            yk = Lid.Count;
            //        }
            //    }
            //}kx = 0;

            // 创建并填充几个关键的字典，以计算并存储关于用户相似度的信息，这些信息将在后续步骤中用于预测可能的voter_id
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
                    sim = -k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));
                    double edt = 0;
                    if (users[iid].Neighbor.ContainsKey(fid))
                    {
                        // 计算相似邻居的得分
                        k1 = sharenum[iid].Count; k2 = responsenum[iid][fid].Count;
                        if (!SimNeigbor.ContainsKey(iid)) SimNeigbor.Add(iid, new Dictionary<string, double>());
                        double sk = k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));
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

            // 对于类类型(classtype)中的每种类型，计算了userclassinfo中该类型所有id的值的总和并存储在usersClassNum字典中
            // 遍历classtype字典的键
            foreach (string xid in classtype.Keys)
            {
                // 在usersClassNum字典中为每个xid添加一个新的字典
                usersClassNum.Add(xid, new Dictionary<string, int>());

                // 遍历userclassinfo中对应xid的键
                foreach (string id in userclassinfo[xid].Keys)
                {
                    // 在对应的字典中添加一个元素，键是id，值是userclassinfo[xid][id]中的值的总和
                    usersClassNum[xid].Add(id, userclassinfo[xid][id].Values.Sum());
                }
            }

            int ch = 0;
            HashSet<string> boolusers = new HashSet<string>();

            // 清空数据，遍历测试数据，获取每个测试数据的UserID和ItemID，更新AddNum字典和receivedata字典，并创建了一系列的排序字典用于存储分数。
            // 清空数据
            data.Clear();

            // 遍历测试json数据
            foreach (JsonData testdata in testjsonData)
            {
                // 每遍历100个数据更新一次界面显示的字符串
                if (++ch % 100 == 0) this.Text = ch.ToString(); Application.DoEvents();

                List<string> res = new List<string>();
                LinkInfo linfo = new LinkInfo();

                // 获取测试数据的UserID和ItemID
                id1 = linfo.UserID = testdata[1].ToString();
                itemid = linfo.ItemID = testdata[2].ToString();

                // 获取测试日期
                DateTime testdate = DateTime.Parse(testdata[3].ToString());

                // 更新AddNum字典
                if (!AddNum.ContainsKey(id1)) AddNum.Add(id1, new Dictionary<string, int>());
                if (!AddNum[id1].ContainsKey(itemid)) AddNum[id1].Add(itemid, 1);
                else AddNum[id1][itemid]++;

                // 更新receivedata字典
                if (!receivedata.ContainsKey(itemid)) receivedata.Add(itemid, new HashSet<string>());
                if (!users.ContainsKey(id1)) continue;

                // 创建存储分数的排序字典
                SortedDictionary<double, List<string>> scor = new SortedDictionary<double, List<string>>(),
                    scorF = new SortedDictionary<double, List<string>>(),
                    scorL = new SortedDictionary<double, List<string>>(),
                    scorX = new SortedDictionary<double, List<string>>(),
                    scorXF = new SortedDictionary<double, List<string>>(),
                    scorT1 = new SortedDictionary<double, List<string>>(),
                    scorT2 = new SortedDictionary<double, List<string>>(),
                    scorT3 = new SortedDictionary<double, List<string>>();

                // 计算att的值
                int att = AddNum[id1][itemid] / 5;
                if (AddNum[id1][itemid] % 5 == 0) att = att * 5;
                else att = att * 5 + 5;

                // 如果特定用户的相似用户数量少于5或att，并且该用户还未在boolusers集合中，那么它将该用户添加到boolusers集合中。
                // 然后，它遍历所有的类别类型，并为每种类型添加所有与该类型相关的用户到backtmp集合中。
                // 然后，从backtmp中删除已经作为相似用户的所有用户和当前用户。
                // 接下来，对于backtmp中的每个用户，如果它还未在备份用户字典中，则将其添加到备份用户字典中，并计算用户的相似度。
                // 最后，对于备份用户字典中的每个用户，计算它们的权重，然后将它们添加到特定用户的相似用户集合中。

                // 如果特定用户的相似用户数少于5或att（之前计算得出的一个值），且boolusers集合中没有这个用户
                if (users[id1].SimLusers.Count < Math.Max(5, att) && !boolusers.Contains(id1))
                {
                    // 将此用户添加到boolusers集合中
                    boolusers.Add(id1);
                    {
                        // 更新界面显示
                        // if (++k % 100 == 0) this.Text = k.ToString() + " / " + IDTest.Count.ToString();
                        Application.DoEvents();

                        // 创建一个新的字典存储备份用户
                        Dictionary<string, Dictionary<string, double>> backuser = new Dictionary<string, Dictionary<string, double>>();

                        // 遍历类别类型
                        foreach (string mid in classtype.Keys)
                        {
                            HashSet<string> backtmp = new HashSet<string>();
                            // 遍历指定用户的每个类别信息
                            foreach (string xid in userclassinfo[mid][id1].Keys)
                            {
                                // 添加与当前类别信息相关的所有用户
                                foreach (string fid in itemuserclassinfo[mid][xid]) backtmp.Add(fid);
                            }
                            // 从backtmp中删除已经作为相似用户的所有用户
                            backtmp = new HashSet<string>(backtmp.Except(users[id1].SimLusers.Keys));

                            // 从backtmp中删除当前用户
                            backtmp.Remove(id1);

                            // 对于backtmp中的每个用户
                            foreach (string fid in backtmp)
                            {
                                // 如果备份用户字典中没有此用户，就在备份用户字典中添加此用户
                                if (!backuser.ContainsKey(fid)) backuser.Add(fid, new Dictionary<string, double>());

                                // 计算用户的相似度
                                k2 = (userclassinfo[mid][id1].Keys.Intersect(userclassinfo[mid][fid].Keys)).Count();
                                if (k2 == 0) continue;
                                k1 = (userclassinfo[mid][id1].Keys.Union(userclassinfo[mid][fid].Keys)).Count();
                                sim = k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));
                                backuser[fid].Add(mid, sim);
                            }
                        }

                        // 对于备份用户字典中的每个用户
                        foreach (string fid in backuser.Keys)
                        {
                            double w = 0;
                            // 计算权重
                            foreach (string mid in backuser[fid].Keys)
                            {
                                w += backuser[fid][mid] * classtypeweight[mid];
                            }
                            // 添加到特定用户的相似用户集合中
                            users[id1].SimFusers.Add(fid, w);
                        }
                    }
                }
                // 基于用户的各种属性，计算一个评分来衡量用户的相似度。评分是通过多个因子（如用户的类别，店铺，品牌等信息，以及用户的行为时间等）相乘得到的。
                // 这些因子都经过适当的缩放，以保证评分的数值在一个合理的范围内。最后，根据评分将用户添加到相应的集合中。
                // 对每个相似用户进行操作
                foreach (string fid in users[id1].SimLusers.Keys)
                {
                    // 初始化相关的变量
                    double r = 0, sir = 1, fir = 1, kir = 0;
                    // 检查是否存在指定的键值
                    if (dataLF.ContainsKey(id1) && dataLF[id1].ContainsKey(fid))
                    {
                        // 遍历特定用户的每个项
                        foreach (string ssitem in dataLF[id1][fid])
                        {
                            // 如果存在特定的键值，累计数量
                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                            {
                                kir += dataItemLF[fid][ssitem].Count;//每一个项目，邻居的二次转发
                            }
                        }
                    }
                    // 对kir值取自然对数，缩放其范围
                    kir = Math.Exp(0.01 * kir);

                    // 检查用户的特定属性，并相应地调整sir值
                    if (users[fid].Ratio[0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                        sir *= (1 + 6 * users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId]);
                    if (users[fid].Ratio[1].ContainsKey(itemsinfo[itemid].CateId))
                        sir *= (1 + 1 * users[fid].Ratio[1][itemsinfo[itemid].CateId]);
                    if (users[fid].Ratio[2].ContainsKey(itemsinfo[itemid].ShopId))
                        sir *= (1 + 2 * users[fid].Ratio[2][itemsinfo[itemid].ShopId]);
                    if (users[fid].Ratio[3].ContainsKey(itemsinfo[itemid].BrandId))
                        sir *= (1 + users[fid].Ratio[3][itemsinfo[itemid].BrandId]);

                    // 初始化相关变量
                    rsim = 0; rresponse = 1;
                    double rneighbor = 0, resneigbor = 0, rkdt = 0;

                    // 检查网络关系，并调整fir值
                    if (netrelation[id1].ContainsKey(fid))
                    {
                        if (netrelation[id1][fid][0].ContainsKey(itemsinfo[itemid].CateLevelOneId))
                            fir *= (1 + 5 * netrelation[id1][fid][0][itemsinfo[itemid].CateLevelOneId]);
                        if (netrelation[id1][fid][1].ContainsKey(itemsinfo[itemid].CateId))
                            fir *= (1 + 1 * netrelation[id1][fid][1][itemsinfo[itemid].CateId]);
                        if (netrelation[id1][fid][2].ContainsKey(itemsinfo[itemid].ShopId))
                            fir *= (1 + 2 * netrelation[id1][fid][2][itemsinfo[itemid].ShopId]);
                        if (netrelation[id1][fid][3].ContainsKey(itemsinfo[itemid].BrandId))
                            fir *= (1 + netrelation[id1][fid][3][itemsinfo[itemid].BrandId]);
                    }

                    // 如果用户的邻居中包含特定用户，计算相关变量
                    if (users[id1].Neighbor.ContainsKey(fid))
                    {
                        int kk1 = users[id1].Neighbor[fid].Count, kk2 = users[fid].ResponesTime.Count;
                        rneighbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));
                        kk1 = responsenum[id1][fid].Count; kk2 = responseall[fid].Count;
                        resneigbor = 1.0 * kk1 / kk2 * (1 - 0.5 / Math.Sqrt(kk2));
                        rkdt = Math.Exp((users[id1].Neighbor[fid].Count - kk1) / kk1);
                        // rneighbor = Math.Exp(kk1 * 1.0 / kk2);
                        // 计算日期差并进行指数衰减
                        foreach (DateTime it in users[id1].Neighbor[fid])
                        {
                            tsp = (testdate - it).TotalDays;
                            rsim += Math.Exp(-0.02 * tsp);
                        }
                        // 计算响应日期
                        rresponse = (testdate - users[id1].Neighbor[fid].Max()).TotalDays;
                    }
                    // 如果特定用户的邻居中包含用户，进行相似的计算
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
                    // 遍历特定用户的响应时间，计算日期差并进行指数衰减
                    ritem = 0;
                    foreach (DateTime it in users[fid].ResponesTime)
                    {
                        tsp = (testdate - it).TotalDays;
                        ritem += Math.Exp(-0.06 * tsp);
                    }

                    // 计算评分r的值，将各个因子相乘
                    r = sir * ritem * users[id1].SimLusers[fid] * users[fid].Level * 0.1;
                    r = r * fir * kir;
                    if (sharerank[id1].ContainsKey(fid)) r *= sharerank[id1][fid];
                    else r *= 0.000000001;

                    // 对评分进行指数转换，并乘以频率和时间因子
                    r *= Math.Exp(-2 * userfreq[fid]) * Math.Exp(2 * usertimes[fid]);

                    // 将评分添加到相应的字典中
                    if (allNeigbors[id1]["F"].Contains(fid))
                    {
                        r *= rsim * resneigbor;
                        if (!scorF.ContainsKey(-r)) scorF.Add(-r, new List<string>());
                        scorF[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["L"].Contains(fid))
                    {
                        r *= rsim * resneigbor;
                        if (!scorL.ContainsKey(-r)) scorL.Add(-r, new List<string>());
                        scorL[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["LF"].Contains(fid))
                    {//rsim=0
                        if (!scorX.ContainsKey(-r)) scorX.Add(-r, new List<string>());
                        scorX[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["XF"].Contains(fid))
                    {//rsim=0
                        if (!scorXF.ContainsKey(-r)) scorXF.Add(-r, new List<string>());
                        scorXF[-r].Add(fid);
                    }
                    else
                    {//rsim=0
                        if (!scor.ContainsKey(-r)) scor.Add(-r, new List<string>());
                        scor[-r].Add(fid);
                    }
                }

                // 每个相似用户都被赋予了一个评分，这个评分是根据多种因素（例如，该用户与其他用户的相似度、该用户的等级、用户频率和时间等）计算得出的。这些评分然后被添加到相应的集合中。
                // 对每个相似用户进行操作
                foreach (string fid in users[id1].SimFusers.Keys)
                {
                    // 初始化相关的变量
                    double r = 0, sir = 1, fir = 1, kir = 0;
                    // 检查是否存在指定的键值
                    if (dataLF.ContainsKey(id1) && dataLF[id1].ContainsKey(fid))
                    {
                        // 遍历特定用户的每个项
                        foreach (string ssitem in dataLF[id1][fid])
                        {
                            // 如果存在特定的键值，累计数量
                            if (dataItemLF.ContainsKey(fid) && dataItemLF[fid].ContainsKey(ssitem))
                            {
                                kir += dataItemLF[fid][ssitem].Count;//每一个项目，邻居的二次转发
                            }
                        }
                    }
                    // 对kir值取自然对数，缩放其范围

                    // 检查用户的特定属性，并相应地调整sir值
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

                    // 初始化相关变量
                    rsim = 0; rresponse = 1;

                    // 检查网络关系，并调整fir值
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


                    // 遍历特定用户的响应时间，计算日期差并进行指数衰减
                    ritem = 0;
                    foreach (DateTime it in users[fid].ResponesTime)
                    {
                        tsp = (testdate - it).TotalDays;
                        ritem += Math.Exp(-0.06 * tsp);
                    }

                    // 计算评分r的值，将各个因子相乘
                    r = sir * ritem * users[id1].SimFusers[fid] * users[fid].Level * 0.1;
                    r = r * fir * kir;

                    // 对评分进行指数转换，并乘以频率和时间因子
                    r *= Math.Exp(-2 * userfreq[fid]) * Math.Exp(2 * usertimes[fid]);

                    // 将评分添加到相应的字典中
                    if (allNeigbors[id1]["LF"].Contains(fid))
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

                // 通过从不同的评分字典中选择用户并添加到res中，以构建一个满足candNum大小要求的候选用户列表。在添加用户的过程中，会检查一些条件以避免添加已经对特定项目做出过反应的用户，或者已经从特定用户那里接收过特定项目的用户。
                // 初始化kx为-1
                kx = -1;

                // 构建输出字符串，包含id1、id1的所有邻居数量、id1与特定项目的共享数量
                string wrt = id1 + "\t" + allNeigbors[id1]["F"].Count.ToString() + "\t" + shareitems[id1][itemid].Count.ToString();

                // 如果itemid不在AddedNumber中，就在AddedNumber中为这个itemid添加一个新的字典
                if (!AddedNumber.ContainsKey(itemid)) AddedNumber.Add(itemid, new Dictionary<string, int>());

                // 如果id1不在itemid的字典中，就在这个字典中为id1添加一个新的条目，并初始化为0
                if (!AddedNumber[itemid].ContainsKey(id1)) AddedNumber[itemid].Add(id1, 0);

                // 对id1的条目加一
                AddedNumber[itemid][id1]++;

                // 对于scorF中的每个评分值
                foreach (double xx in scorF.Keys)
                {
                    // 对于该评分值对应的每个用户
                    foreach (string yid in scorF[xx])
                    {
                        // 如果这个用户已经从id1那里接收过该项目，或者用户已经对id1的该项目做出反应，或者用户已经被添加，那么跳过该用户
                        if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;
                        if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
                        if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1) && haveadded[itemid][id1].Contains(yid)) continue;

                        // 如果res的数量小于candNum，则将yid添加到res中
                        if (res.Count < candNum)
                            res.Add(yid);
                        else break; // 如果res的数量达到candNum，则跳出循环
                    }
                    // 如果res的数量达到candNum，则跳出循环
                    if (res.Count == candNum) break;   
                }
                // 如果res的数量仍然小于candNum，那么从scorL中找更多的用户添加进去
                // 同样的过程重复在scorX、scorT1、scorXF、scorT2、scor以及scorT3中
                // 每个循环都是从对应的评分字典中提取用户，检查这个用户是否满足添加到res的条件，如果满足就添加进去，直到res的数量达到candNum
                // 同时，如果某个用户已经被添加过，或者这个用户已经对id1的该项目做出反应，或者这个用户已经从id1那里接收过该项目，那么就跳过这个用户
                if (res.Count < candNum)
                {
                    foreach (double xx in scorL.Keys)
                    {

                        foreach (string yid in scorL[xx])
                        {
                            if (itemreceive[itemid].ContainsKey(id1) && itemreceive[itemid][id1].Contains(yid)) continue;//同一item，同一分享用户，回流的用户只会回流一次（train）
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                // 创建一个候选列表，这个列表包含了可能会对特定项目做出反应的用户。
                // 它会遍历所有共享特定项目的用户，并检查他们是否在测试日期的12小时内做出了反应。
                // 如果找到了这样的用户，就会将他们添加到候选列表中。如果候选列表的长度达到了预定的大小，或者所有的相似用户都已经被考虑过了，就会根据一些条件调整候选列表。
                // 最后，将测试数据和候选列表添加到最终的预测结果中。
                // 将与给定itemid共享的所有id转换为列表
                List<string> uid = shareIDs[itemid].Keys.ToList();

                // 初始化shflag为false，这个标志用于跟踪是否在12小时内找到了共享项目
                bool shflag = false;

                // 遍历uid中的每一个xid
                foreach (string xid in uid)
                {
                    // 如果xid等于id1，则跳过当前循环
                    if (xid == id1) continue;

                    // 将与xid共享的itemid的所有日期转换为列表
                    List<DateTime> ld = shareIDs[itemid][xid].ToList();

                    // 遍历ld中的每一个日期dx
                    foreach (DateTime dx in ld)
                    {
                        // 计算dx与testdate之间的时间差，单位是小时
                        double days = (dx - testdate).TotalHours;

                        // 如果时间差在0到12小时之间
                        if (days > 0 && days < 12)
                        {
                            // 如果res中包含xid
                            if (res.Contains(xid))
                            {
                                // 获取res的长度nnk
                                int nnk = res.Count;

                                // 遍历res
                                for (int ii = 0; ii < nnk; ++ii)
                                {
                                    // 如果找到res中的xid
                                    if (res[ii] == xid)
                                    {
                                        // 交换res中的第一个元素和xid
                                        res[ii] = res[0];
                                        res[0] = xid;

                                        // 从shareIDs[itemid]中移除xid
                                        shareIDs[itemid].Remove(xid);
                                        break;
                                    }
                                }
                            }
                            // 如果res不包含xid，且res的长度小于5，则将xid添加到res
                            else if (res.Count < 5) res.Add(xid);

                            // 如果res的长度等于或超过5，则将res的最后一个元素设置为xid
                            else res[4] = xid;

                            // 将shflag设置为true，并退出循环
                            shflag = true;
                            break;
                        }
                    }

                    // 如果shflag为true，退出循环
                    if (shflag) break;
                }

                // 如果res的长度等于candNum，或者id1的相似用户数量等于5并且res的长度等于candNum - 1
                if (res.Count == candNum || users[id1].SimLusers.Count == 5 && res.Count == candNum - 1)
                {
                    // 如果haveadded[itemid][id1]包含res[0]，则从res中移除res[0]
                    if (haveadded.ContainsKey(itemid) && haveadded[itemid].ContainsKey(id1)
                            && haveadded[itemid][id1].Contains(res[0]))
                    {
                        res.Remove(res[0]);
                    }
                    // 如果res的长度等于candNum，从res中移除最后一个元素
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
                        foreach (string xmid in backadded[itemid][id1])
                            haveadded[itemid][id1].Add(xmid);
                        backadded[itemid][id1].Clear();
                    }
                }
                // 如果res的长度大于0
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



                // 从shareIDs[itemid]中移除id1
                shareIDs[itemid].Remove(id1);

                // 如果itemid和id1共享的数量大于id1的相似用户数量
                if (shareNumber[itemid][id1] > users[id1].SimLusers.Count)
                {
                    // 组装输出字符串
                    wrt = id1 + "\t" + itemid + "\t" + (shareNumber[itemid][id1] - users[id1].SimLusers.Count).ToString() + "\t" + testdate.ToString();
                    foreach (string ss in res) wrt = wrt + "\t" + ss;

                    // 写入到输出文件
                    ppr.WriteLine(wrt);
                }

                // 计算各长度的结果数量
                diffnum[res.Count]++;

                // 将测试数据和结果添加到subnew
                subnew.Add(testdata[0].ToString(), res);
            }

            // for (int ii = 0; ii < 6; ++ii) ppr.WriteLine(ii.ToString() + "\t" + diffnum[ii].ToString());
            // 遍历subnew中的每个键（也就是id）
            foreach (string id in subnew.Keys)
            {
                // 创建一个新的SubmitInfo对象
                subtemp = new SubmitInfo();

                // 设置triple_id为当前的id
                subtemp.triple_id = id;

                // 将subnew[id]转换为数组，然后设置为candidate_voter_list
                subtemp.candidate_voter_list = subnew[id].ToArray();

                // 将subtemp添加到submitres列表
                submitres.Add(subtemp);
            }

            // 关闭ppr文件流
            ppr.Close();

            // 将submitres对象转换为JSON格式的文本
            string text = JsonMapper.ToJson(submitres);

            // 将这段文本写入到名为"submit.json"的文件中
            System.IO.File.WriteAllText("submit.json", text);
        }

        // 带注释的新代码
        private void newSubmittPlus()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "分享数据";
            ofd.ShowDialog();
            JsonData jsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));
            ofd.Title = "商品数据";
            ofd.ShowDialog();
            JsonData itemjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));
            ofd.Title = "用户数据";
            ofd.ShowDialog();
            JsonData userjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));
            ofd.Title = "测试数据";
            ofd.ShowDialog();
            JsonData testjsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(ofd.FileName));
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
            //Dictionary<string, SortedDictionary<DateTime, List<Tuple<string, string>>>> ItemAll = 
            //    new Dictionary<string, SortedDictionary<DateTime, List<Tuple<string, string>>>>();
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
                //JsonData cate_id = temp[1];
                //JsonData level_id = temp[2];
                JsonData level = temp[3];
                //JsonData shopid = temp[4];
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
                JsonData user_id = temp[0];
                JsonData item_id = temp[1];
                JsonData voter_id = temp[2];
                JsonData timestamp = temp[3];
                lk = new LinkInfo(); id1 = lk.UserID = user_id.ToString();
                item = lk.ItemID = item_id.ToString();
                id2 = lk.VoterID = voter_id.ToString();
                dt = DateTime.Parse(timestamp.ToString());
                if (!responstimeAllitems[id2].ContainsKey(item)) responstimeAllitems[id2].Add(item, new SortedSet<DateTime>());
                responstimeAllitems[id2][item].Add(dt);
                if (!data.ContainsKey(dt)) data.Add(dt, new List<LinkInfo>());
                data[dt].Add(lk);

                //if (!ItemAll[item].ContainsKey(dt)) ItemAll[item].Add(dt, new List<Tuple<string, string>>());
                //ItemAll[item][dt].Add(new Tuple<string, string>(id1, id2));
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
                    foreach (string xid in allNeigbors[fid]["F"]) allNeigbors[id]["LF"].Add(xid);
                    foreach (string xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
                }
                foreach (string fid in allNeigbors[id]["L"])
                {
                    foreach (string xid in allNeigbors[fid]["F"]) allNeigbors[id]["LF"].Add(xid);
                    foreach (string xid in allNeigbors[fid]["L"]) allNeigbors[id]["LF"].Add(xid);
                }
                foreach (string fid in allNeigbors[id]["F"])
                    allNeigbors[id]["LF"].Remove(fid);
                foreach (string fid in allNeigbors[id]["L"])
                    allNeigbors[id]["LF"].Remove(fid);
                //寻找3阶邻居
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

                id1 = testdata[1].ToString();
                itemid = testdata[2].ToString();
                DateTime testdate = DateTime.Parse(testdata[3].ToString());
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

            //foreach (string iid in IDTest)
            //{
            //    if (++kx % 100 == 0) this.Text = kx.ToString() + "  " + IDTest.Count.ToString();
            //    Application.DoEvents();
            //    foreach (string fid in users[iid].ItemPath.Keys)
            //    {
            //        int y0 = users[iid].ItemPath[fid], y1; //每一个item
            //        y1 = y0; id1 = iid;
            //        Queue<string> Lid = new Queue<string>(users[iid].Neighbor.Keys);
            //        int yk = Lid.Count;
            //        while (yk > 0)
            //        {
            //            for (int ix = 0; ix < yk; ++ix)
            //            {//BFS
            //                id2 = Lid.Dequeue();
            //                if (users[id2].ItemPath.ContainsKey(fid))
            //                {
            //                    if (!users[iid].StaticSimUsers.ContainsKey(id2))
            //                        users[iid].StaticSimUsers.Add(id2, Math.Pow(0.1, users[id2].ItemPath[fid] - y0));
            //                    else users[iid].StaticSimUsers[id2] += Math.Pow(0.1, users[id2].ItemPath[fid] - y0);
            //                    foreach (string mid in users[id2].Neighbor.Keys)
            //                    {
            //                        if (Lid.Contains(mid) || users[iid].StaticSimUsers.ContainsKey(mid)) continue;
            //                        Lid.Enqueue(mid);
            //                    }
            //                }

            //            }
            //            yk = Lid.Count;
            //        }
            //    }
            //}kx = 0;
            Dictionary<string, Dictionary<string, double>> SimNeigbor = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, double> SingleDt = new Dictionary<string, double>();
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

                    k2 = (users[iid].ItemID.Intersect(users[fid].ItemID)).Count();
                    k1 = (users[iid].ItemID.Union(users[fid].ItemID)).Count();
                    if (k2 == 0) continue;
                    sim = -k2 * 1.0 / k1 * (1 - 0.5 / Math.Sqrt(k1));
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
                backadded = new Dictionary<string, Dictionary<string, HashSet<string>>>();
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
            foreach (JsonData testdata in testjsonData)
            {
                if (++ch % 100 == 0) this.Text = ch.ToString(); Application.DoEvents();
                List<string> res = new List<string>();
                LinkInfo linfo = new LinkInfo();
                id1 = linfo.UserID = testdata[1].ToString();
                itemid = linfo.ItemID = testdata[2].ToString();
                DateTime testdate = DateTime.Parse(testdata[3].ToString());
                if (!AddNum.ContainsKey(id1)) AddNum.Add(id1, new Dictionary<string, int>());
                if (!AddNum[id1].ContainsKey(itemid)) AddNum[id1].Add(itemid, 1);
                else AddNum[id1][itemid]++;
                if (!receivedata.ContainsKey(itemid)) receivedata.Add(itemid, new HashSet<string>());
                if (!users.ContainsKey(id1)) continue;
                //if (!itemreceived.ContainsKey(itemid)) itemreceived.Add(itemid, new Dictionary<DateTime, HashSet<string>>());
                //if (!itemreceived[itemid].ContainsKey(testdate.Date)) itemreceived[itemid].Add(testdate.Date, new HashSet<string>());
                SortedDictionary<double, List<string>> scor = new SortedDictionary<double, List<string>>(),
                    scorF = new SortedDictionary<double, List<string>>(),
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
                    double r = 0, sir = 1, fir = 1, kir = 0;
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
                    //if (users[id1].StaticSimUsers.ContainsKey(fid)) r *= Math.Exp(0.1 * users[id1].StaticSimUsers[fid]);//按圈层加权
                    if (allNeigbors[id1]["F"].Contains(fid))
                    {
                        r *= rsim * resneigbor;// *shareresponse[fid][id1].Average();
                        if (!scorF.ContainsKey(-r)) scorF.Add(-r, new List<string>());
                        scorF[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["L"].Contains(fid))
                    {
                        r *= rsim * resneigbor;
                        if (!scorL.ContainsKey(-r)) scorL.Add(-r, new List<string>());
                        scorL[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["LF"].Contains(fid))
                    {//rsim=0
                        if (!scorX.ContainsKey(-r)) scorX.Add(-r, new List<string>());
                        scorX[-r].Add(fid);
                    }
                    else if (allNeigbors[id1]["XF"].Contains(fid))
                    {//rsim=0
                        if (!scorXF.ContainsKey(-r)) scorXF.Add(-r, new List<string>());
                        scorXF[-r].Add(fid);
                    }
                    else
                    {//rsim=0
                        if (!scor.ContainsKey(-r)) scor.Add(-r, new List<string>());
                        scor[-r].Add(fid);
                    }
                }
                foreach (string fid in users[id1].SimFusers.Keys)
                {
                    //对每一个相似用户
                    double r = 0, sir = 1, fir = 1, kir = 0;
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
                    if (allNeigbors[id1]["LF"].Contains(fid))
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
                        if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                            if (responseitems[yid].ContainsKey(id1) && responseitems[yid][id1].Contains(itemid)) continue;
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
                                        shareIDs[itemid].Remove(xid);
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
                        foreach (string xmid in backadded[itemid][id1])
                            haveadded[itemid][id1].Add(xmid);
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



                shareIDs[itemid].Remove(id1); wrt = "";
                if (shareNumber[itemid][id1] > users[id1].SimLusers.Count)
                {
                    wrt = id1 + "\t" + itemid + "\t" + (shareNumber[itemid][id1] - users[id1].SimLusers.Count).ToString() + "\t" + testdate.ToString();
                    foreach (string ss in res) wrt = wrt + "\t" + ss;
                    ppr.WriteLine(wrt);
                }


                diffnum[res.Count]++;
                subnew.Add(testdata[0].ToString(), res);
            }

            // for (int ii = 0; ii < 6; ++ii) ppr.WriteLine(ii.ToString() + "\t" + diffnum[ii].ToString());
            foreach (string id in subnew.Keys)
            {
                subtemp = new SubmitInfo();
                subtemp.triple_id = id;
                subtemp.candidate_voter_list = subnew[id].ToArray();
                submitres.Add(subtemp);
            }
            ppr.Close();
            string text = JsonMapper.ToJson(submitres);
            System.IO.File.WriteAllText("submit.json", text);
        }
    }
}