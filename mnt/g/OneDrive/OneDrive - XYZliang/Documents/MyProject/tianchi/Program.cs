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
            program.submittwoPlusTheSimilarityOfUserAttributes();
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
    }
}