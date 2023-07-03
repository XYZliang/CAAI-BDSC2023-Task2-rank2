import multiprocessing
import traceback
from collections import defaultdict
from datetime import datetime
from typing import List, Dict, Set
import numpy as np
import math
import multiprocess as multiprocess
import orjson
import psutil
from sortedcontainers import SortedDict
from tqdm import tqdm

from tools import *


def task(index, testdatas, users, dataLF, dataItemLF, itemsinfo, netrelation, sharerank,
         userSimilarityMatrix, shareitems, itemreceive, responseitems, submitres, submitresAll):
    try:
        for testdata in tqdm(testdatas):
            print("我是第" + str(index) + "个进程,正在处理的用户是" + testdata['inviter_id'])
            # 初始化一个名为 res 的 List，用于存储推荐结果
            res = []
            # 提取用户 ID（id1）
            id1 = testdata['inviter_id']
            # 提取物品 ID（itemid）
            itemid = testdata['item_id']
            # 将数据中的时间戳转换为 DateTime 对象（testdate）
            testdate = datetime.strptime(
                testdata['timestamp'], "%Y-%m-%d %H:%M:%S")
            # 如果 users 字典中尚未包含用户 id1，则跳过此次循环
            if id1 not in users:
                return
            # 初始化一个名为 scor 的 SortedDict，用于存储评分
            scor = {}
            # 遍历用户 id1 的相似用户（fid）
            for fid in users[id1].SimLusers.keys():
                # 初始化评分
                r = 0
                sir = 1
                fir = 1
                kir = 0
                rsim = 0
                rresponse = 1
                ritem = 0
                # 计算kir
                if id1 in dataLF and fid in dataLF[id1]:
                    for ssitem in dataLF[id1][fid]:
                        if fid in dataItemLF and ssitem in dataItemLF[fid]:
                            kir += len(dataItemLF[fid][ssitem])
                kir = math.exp(0.001 * kir)
                # 计算sir
                if itemsinfo[itemid].CateLevelOneId in users[fid].Ratio[0]:
                    sir *= 1 + 5 * \
                           users[fid].Ratio[0][itemsinfo[itemid].CateLevelOneId]
                if itemsinfo[itemid].CateId in users[fid].Ratio[1]:
                    sir *= 1 + users[fid].Ratio[1][itemsinfo[itemid].CateId]
                if itemsinfo[itemid].ShopId in users[fid].Ratio[2]:
                    sir *= 1 + 2 * users[fid].Ratio[2][itemsinfo[itemid].ShopId]
                if itemsinfo[itemid].BrandId in users[fid].Ratio[3]:
                    sir *= 1 + users[fid].Ratio[3][itemsinfo[itemid].BrandId]
                # 计算fir
                if id1 in netrelation and fid in netrelation[id1]:
                    if itemsinfo[itemid].CateLevelOneId in netrelation[id1][fid][0]:
                        fir *= 1 + 5 * \
                               netrelation[id1][fid][0][itemsinfo[itemid].CateLevelOneId]
                    if itemsinfo[itemid].CateId in netrelation[id1][fid][1]:
                        fir *= 1 + \
                               netrelation[id1][fid][1][itemsinfo[itemid].CateId]
                    if itemsinfo[itemid].ShopId in netrelation[id1][fid][2]:
                        fir *= 1 + 2 * \
                               netrelation[id1][fid][2][itemsinfo[itemid].ShopId]
                    if itemsinfo[itemid].BrandId in netrelation[id1][fid][3]:
                        fir *= 1 + \
                               netrelation[id1][fid][3][itemsinfo[itemid].BrandId]
                # 计算rsim
                if fid in users[id1].Neighbor:
                    for it in users[id1].Neighbor[fid]:
                        tsp = (testdate - it).total_seconds() / \
                              86400  # convert seconds to days
                        rsim += math.exp(-0.02 * tsp)
                    rresponse = (
                                        testdate - max(users[id1].Neighbor[fid])).total_seconds() / 86400
                elif id1 in users[fid].Neighbor:
                    for it in users[fid].Neighbor[id1]:
                        tsp = (testdate - it).total_seconds() / \
                              86400  # convert seconds to days
                        rsim += math.exp(-0.1 * tsp)
                # 计算ritem
                for it in users[fid].ResponesTime:
                    tsp = (testdate - it).total_seconds() / \
                          86400  # convert seconds to days
                    ritem += math.exp(-0.05 * tsp)
                # 计算总评分（r）
                r = sir * rsim * ritem * \
                    users[id1].SimLusers[fid] * users[fid].Level * 0.1
                r *= fir * kir
                if fid in sharerank[id1]:
                    r *= sharerank[id1][fid]
                else:
                    r *= 0.0000001
                # 将评分（r）添加到 SortedDict scor 中
                if -r not in scor:
                    scor[-r] = []
                scor[-r].append(fid)
                if id1 not in userSimilarityMatrix:
                    userSimilarityMatrix[id1] = {}
                userSimilarityMatrix[id1][fid] = r
            scor = dict(sorted(scor.items()))
            # 对已计算出的评分进行筛选，并将前五名的评分和相关信息记录下来
            kx = -1
            # wrt = f"{id1}\t{itemid}\t{len(shareitems[id1][itemid])}\t{len(users[id1]['Neighbor'].keys())}\t{testdate}\t{len(users[id1]['SimLusers'])}"
            wrt = f"{id1}\t{itemid}\t{len(shareitems[id1][itemid])}\t{len(users[id1].Neighbor.keys())}\t{testdate}\t{len(users[id1].SimLusers)}"
            for xx in scor.keys():
                for yid in scor[xx]:
                    if itemid in itemreceive and id1 in itemreceive[itemid] and yid in itemreceive[itemid][id1]:
                        continue
                    if yid in responseitems and id1 in responseitems[yid] and itemid in responseitems[yid][id1]:
                        continue
                    if len(res) < 5:
                        wrt = f"{wrt}\t{xx}"
                        res.append(yid)
                        if len(res) == 1 and len(shareitems[id1][itemid]) > 5:
                            if yid not in responseitems:
                                responseitems[yid] = {}
                            if id1 not in responseitems[yid]:
                                responseitems[yid][id1] = {}
                            responseitems[yid][id1].add(itemid)
                    else:
                        break
                if len(res) >= 5:
                    break

            # 将推荐结果（即前五名用户）以及与之相关的信息记录下来，并将这些信息写入文件
            subtemp = {
                "triple_id": str(testdata['triple_id']),
                "candidate_voter_list": res,
            }
            subdata = {
                "inviter_id": str(testdata['inviter_id']),
                "triple_id": str(testdata['triple_id']),
                "candidate_voter_list": res,
            }
            submitres.append(subtemp)
            submitresAll.append(subdata)
    except Exception:
        print(traceback.format_exc())


# 存储用户动态商品分享数据，item_share_train_info.json
class LinkInfo:
    def __init__(self):
        self.UserID: str = ""
        self.ItemID: str = ""
        self.VoterID: str = ""
        # 其他字段...


class UserIDInfo:
    # 设为double形是为了后面更好的取平均值
    def __init__(self):
        self.Level: float = 0.0
        self.Gender: float = 0.0
        self.Age: float = 0.0

        # 性别的独热编码
        self.GenderOneHot: List[float] = []

        # 该用户分享过的用户
        self.Neighbor: Dict[str, List[datetime]] = {}
        self.NewNeighbor: Dict[str, List[datetime]] = {}
        self.Ratio: Dict[int, Dict[str, float]] = {}
        self.ResponesTime: List[datetime] = []
        self.ItemID: Set[str] = set()
        self.responseTimeZone: Dict[int, float] = {}
        self.StaticSimUsers: Dict[str, float] = {}
        self.SimLusers: Dict[str, float] = {}


class ItemIDInfoMini:
    def __init__(self):
        self.Level: float = 0.0
        # 该用户分享过的用户
        self.Neighbor: Dict[str, List[datetime]] = {}
        self.Ratio: Dict[int, Dict[str, float]] = {}
        self.SimLusers: Dict[str, float] = {}


# 存储输出的提交文件数据
class SubmitInfo:
    def __init__(self):
        self.triple_id: str = ""
        self.candidate_voter_list: List[str] = []
        # 其他字段...


class ItemInfo:
    def __init__(self):
        self.ShopId: str = ""
        self.BrandId: str = ""
        self.CateId: str = ""
        self.CateLevelOneId: str = ""


# 主函数
def main(verification=False, percentage=0.1, savaPath="./output/result/", Hyperthreading=True, MaxThreading=0):
    # 文件路径
    file_paths = {
        "分享数据": "./data/item_share_train_info.json",
        "商品数据": "./data/item_info.json",
        "用户数据": "./data/user_info.json",
        "测试数据": "./data/item_share_preliminary_test_info.json",
    }

    # 慢速加载数据，但有序（python>3.7）
    # with open(file_paths["分享数据"], "r") as f:
    #     jsonData = json.loads(f.read(), object_pairs_hook=collections.OrderedDict)
    # print("分享数据加载完成，共有数据："+str(len(jsonData)))
    #
    # with open(file_paths["商品数据"], "r") as f:
    #     itemjsonData = json.loads(f.read(), object_pairs_hook=collections.OrderedDict)
    # print("商品数据加载完成，共有数据："+str(len(itemjsonData)))
    #
    # with open(file_paths["用户数据"], "r") as f:
    #     userjsonData = json.loads(f.read(), object_pairs_hook=collections.OrderedDict)
    # print("用户数据加载完成，共有数据："+str(len(userjsonData)))
    #
    # with open(file_paths["测试数据"], "r") as f:
    #     testjsonData = json.loads(f.read(), object_pairs_hook=collections.OrderedDict)
    # print("测试数据加载完成，共有数据："+str(len(testjsonData)))

    # 使用orjson加快读取速度，但无序
    with open(file_paths["分享数据"], "rb") as f:
        jsonData = orjson.loads(f.read())
    print("分享数据加载完成，共有数据：" + str(len(jsonData)))

    with open(file_paths["商品数据"], "rb") as f:
        itemjsonData = orjson.loads(f.read())
    print("商品数据加载完成，共有数据：" + str(len(itemjsonData)))

    with open(file_paths["用户数据"], "rb") as f:
        userjsonData = orjson.loads(f.read())
    print("用户数据加载完成，共有数据：" + str(len(userjsonData)))

    if verification:
        jsonData, testjsonData = splitValidationFromTraining(jsonData, percentage)
    else:
        with open(file_paths["测试数据"], "rb") as f:
            testjsonData = orjson.loads(f.read())
    print("测试数据加载完成，共有数据：" + str(len(testjsonData)))

    # 创建两个根据时间排序的字典，用于存储链接信息
    data = SortedDict()
    data2 = SortedDict()

    # 定义一个DateTime变量，用于存储时间信息
    dt = datetime.now()

    # 定义一个LinkInfo变量，用于存储链接信息
    lk = LinkInfo()

    # 定义两个整数变量，n和k
    n = 0
    k = 0

    # 定义两个字符串变量，itemid和id1、id2
    itemid = ""
    id1 = ""
    id2 = ""

    # 定义一个权重W，用于计算用户相似度时，基于用户性别年龄等级上的相似度与基于用户商品兴趣之间的相似度的比重
    userGenderAgeClassSimilarityWeighting = 0.2

    # 定义二层嵌套字典，用于储存并计算用户的平均性别独热编码
    userGenderAverageOneHotCode = [0, 0]

    # 定义两个变量，用于储存并计算用户的平均年龄段、等级
    userAverageAge = 0.0
    userAverageClass = 0.0

    # 定义年龄段、等级的最大值
    maxAge = 8
    maxClass = 10

    # 创建许多嵌套字典，用于存储不同类型的信息
    users = {}  # 存储UserIDInfo的字典，键为用户ID
    itemsinfo = {}  # 存储ItemInfo的字典，键为商品ID
    itemreceive = {}  # 存储每个商品被每个用户接收的信息
    netrelation = {}  # 存储网络关系数据
    responseitems = {}  # 存储回流用户与邀请用户和商品的关系
    sharerank = {}  # 存储每个用户分享的商品的排名
    item = ""  # 字符串变量item
    ranks = {}  # 存储商品、用户和时间的排名数据
    sharenum = {}  # 存储用户分享次数的字典
    responsenum = {}  # 存储回流用户与邀请用户和商品的关系

    def default2_value():
        return -1

    def default_value():
        return defaultdict(default2_value)

    userSimilarityMatrix = defaultdict(default_value)  # 存储用户相似度矩阵的字典

    # 记录有效的平均用户年龄和等级的数量
    userAverageAgeCount = 0
    userAverageClassCount = 0

    # 记录有效的平均用户年龄和等级的和
    userAverageAgeSum = 0.0
    userAverageClassSum = 0.0

    # 记录有效的平均用户性别的数量
    userGenderAverageCount = 0

    # 记录有效的平均用户性别的独热编码的和
    userGenderAverageSum = [0, 0]

    print("变量初始化完成")
    # 遍历用户数据集
    for temp in tqdm(userjsonData, desc="正在处理用户数据"):
        user_id = temp['user_id']  # 获取用户ID
        level = temp['user_level']  # 获取用户等级

        id1 = str(user_id)  # 将用户ID转换为字符串

        # 将用户ID添加到users字典中，并为其分配一个新的UserIDInfo对象
        users[id1] = UserIDInfo()

        # 设置用户的等级，并进行归一化处理，保留三位小数
        users[id1].level = round(int(level) * 1.0 / maxClass, 3)

        # 设置用户的性别
        int_type_gender = int(temp['user_gender'])
        users[id1].gender = 1.0 * int_type_gender

        # 设置用户的年龄，并进行归一化处理，保留三位小数
        users[id1].age = round(int(temp['user_age']) * 1.0 / maxAge, 3)

        # 初始化用户的性别独热编码
        users[id1].gender_one_hot = [0.0, 0.0]

        # 设置用户的性别独热编码
        if int_type_gender == 0:  # 女性
            users[id1].gender_one_hot = [1.0, 0.0]
        elif int_type_gender == 1:  # 男性
            users[id1].gender_one_hot = [0.0, 1.0]
        # 判断年龄等级有效，是的话则进行累加
        if users[id1].age > 0:
            userAverageAgeCount += 1
            userAverageAgeSum += users[id1].age

        # 判断用户等级有效，是的话则进行累加
        if users[id1].level > 0:
            userAverageClassCount += 1
            userAverageClassSum += users[id1].level

        # 判断用户性别有效，是的话则对独热编码进行累加
        if int_type_gender == 0 or int_type_gender == 1:
            userGenderAverageCount += 1
            # 累加独热编码
            for value in users[id1].gender_one_hot:
                index = users[id1].gender_one_hot.index(value)
                userGenderAverageSum[index] += value

        # 初始化用户的邻居信息、新邻居信息、响应时间等数据结构
        users[id1].neighbor = SortedDict()
        users[id1].new_neighbor = SortedDict()
        users[id1].ratio = {}
        users[id1].response_time = []
        users[id1].item_id = set()
        users[id1].response_time_zone = {}
        users[id1].static_sim_users = {}

        # 为用户ID初始化网络关系、回流商品、分享排名等数据结构
        netrelation[id1] = {}
        responseitems[id1] = {}
        sharerank[id1] = {}
        sharenum[id1] = set()
        responsenum[id1] = {}

    # 计算平均用户年龄、等级、性别独热编码
    userAverageAge = userAverageAgeSum / userAverageAgeCount
    userAverageClass = userAverageClassSum / userAverageClassCount
    userGenderAverage = [
        val / userGenderAverageCount for val in userGenderAverageSum]

    # 通过性别独热编码计算平均性别, 取三位小数
    userAverageGender = round(userGenderAverage[1], 3)

    # 输出
    # 用户数据读取完毕
    print("用户数据读取完毕，共有{}个用户".format(len(users)))
    print("用户平均年龄：{}".format(userAverageAge))
    print("用户平均等级：{}".format(userAverageClass))
    print("用户平均性别：{}".format(userAverageGender))
    print("用户平均性别独热编码：[{}, {}]".format(
        userGenderAverage[0], userGenderAverage[1]))
    # 输出无效年龄、等级、性别数量
    print("无效年龄数量：{}，无效等级数量：{}，无效性别数量：{}".format(
        len(users) - userAverageAgeCount,
        len(users) - userAverageClassCount,
        len(users) - userGenderAverageCount))

    # 重新遍历所有用户，将所有用户的未知年龄、等级、性别、性别独热编码设置为平均值
    for id1, userInfo in tqdm(users.items()):
        # 记录是否有修改
        isChange = False
        # 在这里处理 userInfo 对象
        # 判断用户年龄是否有效，无效则设置为平均值
        if userInfo.Age <= 0:
            userInfo.Age = userAverageAge
            users[id1].Age = userAverageAge
            isChange = True

        # 判断用户等级是否有效，无效则设置为平均值
        if userInfo.Level <= 0:
            userInfo.Level = userAverageClass
            users[id1].Level = userAverageClass
            isChange = True

        # 判断用户性别是否有效，无效则设置为平均值
        if userInfo.Gender <= 0:
            userInfo.Gender = userAverageGender
            users[id1].Gender = userAverageGender
            userInfo.GenderOneHot = userGenderAverage
            users[id1].GenderOneHot = userGenderAverage
            isChange = True

        # 仅限debug使用
        # if isChange:
        #     print(f"用户{id1}的年龄、等级、性别存在无效值，已设置为平均值:{userInfo['Age']},{userInfo['Level']},{userInfo['Gender']},{userInfo['GenderOneHot']}")

    # 输出阶段性提示
    print("用户数据处理完毕，开始处理商品信息")

    # 遍历了商品数据集，并从中提取商品ID、类目ID、一级类目ID、品牌ID和店铺ID等信息，将这些信息添加到了之前创建的字典和数据结构中
    for temp in tqdm(itemjsonData):
        # 获取商品ID、类目ID、一级类目ID、品牌ID和店铺ID
        item_id = temp['item_id']
        cate_id = temp['cate_id']
        level_id = temp['cate_level1_id']
        brandid = temp['brand_id']
        shopid = temp['shop_id']

        # 将商品ID转换为字符串
        itemid = str(item_id)

        # 将商品ID添加到itemsinfo字典中，并为其分配一个新的ItemInfo对象
        itemsinfo[itemid] = ItemInfo()

        # 设置商品的店铺ID、品牌ID、类目ID和一级类目ID
        itemsinfo[itemid].ShopId = str(shopid)
        itemsinfo[itemid].BrandId = str(brandid)
        itemsinfo[itemid].CateId = str(cate_id)
        itemsinfo[itemid].CateLevelOneId = str(level_id)

        # 初始化每个商品ID对应的回流用户数据结构
        itemreceive[itemid] = {}

    # 输出阶段性提示
    print("商品数据处理完毕，开始处理回流数据")

    # 遍历了分享数据集，并从中提取了邀请用户ID、商品ID、回流用户ID和时间戳等信息，将这些信息添加到了之前创建的字典和数据结构中
    for temp in tqdm(jsonData):
        temp["test"] = 1
        # 获取邀请用户ID、商品ID、回流用户ID和时间戳
        user_id = temp['inviter_id']
        item_id = temp['item_id']
        voter_id = temp['voter_id']
        timestamp = temp['timestamp']

        # 创建一个新的LinkInfo对象并设置相应的属性
        lk = LinkInfo()
        id1 = lk.UserID = str(user_id)
        item = lk.ItemID = str(item_id)
        id2 = lk.VoterID = str(voter_id)
        dt = datetime.strptime(str(timestamp), '%Y-%m-%d %H:%M:%S')

        # 将LinkInfo对象添加到按时间戳排序的字典中
        if dt not in data:
            data[dt] = []
        data[dt].append(lk)

        # 更新邀请者-商品-时间戳-受邀请者的排名信息，将用户动态商品分享数据存入了这样一个多层嵌套，类似于一个树状结构的字典中
        if id1 not in ranks:
            ranks[id1] = {}
        if item not in ranks[id1]:
            ranks[id1][item] = {}
        if dt not in ranks[id1][item]:
            ranks[id1][item][dt] = []
        ranks[id1][item][dt].append(id2)

        # 更新用户的分享数量信息
        sharenum[id1].add(item)

        # 更新用户间的回流数量信息
        if id2 not in responsenum[id1]:
            responsenum[id1][id2] = set()
        responsenum[id1][id2].add(item)

        # 计数器递增
        n += 1
    # 输出阶段性提示
    print("回流数据处理完毕，开始计算回流用户的排名得分")

    # 遍历了之前填充的ranks字典，根据分享过的商品和回流用户信息计算了sharerank字典中的排名得分
    # 遍历ranks字典的键（用户ID）
    for id in tqdm(ranks.keys()):
        # 计算该用户分享的商品数量
        tt = len(ranks[id])
        # 遍历该用户分享过的每个商品
        for fid in ranks[id].keys():
            ii = 1
            # 遍历该用户分享过的商品的每个时间戳
            for d in ranks[id][fid].keys():
                # 遍历该用户在特定时间戳下分享过的商品的每个回流用户
                for xid in ranks[id][fid][d]:
                    # 更新sharerank字典，计算回流用户的排名得分
                    if xid not in sharerank[id]:
                        sharerank[id][xid] = 1.0 / ii / tt
                    else:
                        sharerank[id][xid] += 1.0 / ii / tt

                # 更新计数器，加上当前时间戳下的回流用户数量
                ii += len(ranks[id][fid][d])

    # 输出阶段性提示
    print("用户排名计算完毕，开始计算商品排名")

    # 初始化了一些与回流用户和商品相关的字典以及商品的时间戳列表和用户集合
    # 初始化计数器
    k = 0

    # 初始化与回流用户和商品相关的字典
    dataLF = {}
    dataItemLF = {}

    # 初始化商品的时间戳列表和商品的用户集合
    items = {}
    itemusers = {}

    # 遍历之前收集的所有时间戳
    for d in tqdm(data.keys()):
        # 遍历该时间戳下的所有链接信息
        for llk in data[d]:
            itemid = llk.ItemID
            id1 = llk.UserID
            id2 = llk.VoterID

            # 更新dataLF字典，存储用户与回流用户之间的关系
            dataLF.setdefault(id1, {}).setdefault(id2, set()).add(itemid)

            # 更新dataItemLF字典，存储用户分享商品与回流用户之间的关系
            dataItemLF.setdefault(id1, {}).setdefault(itemid, set()).add(id2)

            # 更新users字典中的Neighbor字段，建立用户与回流用户之间的联系
            if id2 not in users[id1].Neighbor:
                netrelation.setdefault(id1, {}).setdefault(id2, {})
                users[id1].Neighbor[id2] = []

            # 初始化netrelation字典的各个层次
            for ii in range(4):
                netrelation[id1][id2].setdefault(ii, {})

            # 更新netrelation字典，存储用户与回流用户之间在不同维度上的联系强度,(交互次数)
            # for index, field in enumerate(['CateLevelOneId', 'CateId', 'ShopId', 'BrandId']):
            #     netrelation[id1][id2][index][itemsinfo[itemid][field]] = netrelation[id1][id2][index].get(
            #         itemsinfo[itemid][field], 0) + 1
            for index, field in enumerate(['CateLevelOneId', 'CateId', 'ShopId', 'BrandId']):
                netrelation[id1][id2][index][getattr(itemsinfo[itemid], field)] = netrelation[id1][id2][index].get(
                    getattr(itemsinfo[itemid], field), 0) + 1

            # 更新用户之间的联系时间戳
            users[id1].Neighbor[id2].append(d)
            users[id2].ResponesTime.append(d)

            # 更新用户的商品ID集合
            users[id1].ItemID.add(llk.ItemID)
            users[id2].ItemID.add(llk.ItemID)

            # 更新商品的时间戳信息
            items.setdefault(llk.ItemID, []).append(d)

            # 更新商品与用户的关系
            itemusers.setdefault(itemid, set()).add(id1)
            itemusers[itemid].add(id2)

            # 更新商品接收者与回流用户的关系
            itemreceive.setdefault(itemid, {}).setdefault(id1, set()).add(id2)

            # 更新回流用户与分享用户的关系
            responseitems.setdefault(id2, {}).setdefault(
                id1, set()).add(itemid)

            k += 1

    # 遍历netrelation字典，键为分享者ID（iid）
    for iid in tqdm(netrelation.keys()):
        # 遍历netrelation[iid]字典，键为回流者ID（fid）
        for fid in netrelation[iid].keys():
            # 遍历netrelation[iid][fid]字典，键为分类层级索引（xid）
            for xid in netrelation[iid][fid].keys():
                # 计算当前分类层级索引下的所有值之和
                yy = sum(netrelation[iid][fid][xid].values())

                # 用一个临时列表存储当前分类层级索引下的所有键
                tmparr = list(netrelation[iid][fid][xid].keys())

                # 遍历临时列表中的键（mid）
                for mid in tmparr:
                    # 对当前键对应的值进行归一化处理，除以所有值之和（转化成联系强度的相对比例）
                    netrelation[iid][fid][xid][mid] /= yy

    # 定义两个整型变量k1和k2
    k1, k2 = 0, 0

    # 定义一个双精度浮点型变量sim
    sim = 0.0

    # 遍历users字典中的所有用户ID（iid）
    for iid in tqdm(users.keys()):
        # 创建一个嵌套的字典结构，用于存储不同分类层级的计数
        catenum = {ii: {} for ii in range(4)}

        # 初始化用户比率信息
        # users[iid]['Ratio'] = {ii: {} for ii in range(4)}
        users[iid].Ratio = {ii: {} for ii in range(4)}

        # 遍历用户分享过的所有物品ID（fid）
        for fid in users[iid].ItemID:
            # 对不同分类层级进行计数
            for index, field in enumerate(['CateLevelOneId', 'CateId', 'ShopId', 'BrandId']):
                # catenum[index][itemsinfo[fid][field]] = catenum[index].get(itemsinfo[fid][field], 0) + 1
                catenum[index][getattr(itemsinfo[fid], field)] = catenum[index].get(getattr(itemsinfo[fid], field),
                                                                                    0) + 1
        # 计算用户在不同分类层级的比率
        for ii in range(4):
            tt = sum(catenum[ii].values())
            users[iid].Ratio[ii] = {xx: catenum[ii]
                                        [xx] / tt for xx in catenum[ii].keys()}

        # 计算用户在不同时间段的回流率
        timetemp = {}
        for it in users[iid].ResponesTime:
            timezone = it.hour
            timetemp[timezone] = timetemp.get(timezone, 0) + 1

        # 计算回流率并存储在responseTimeZone字典中
        timetotal = sum(timetemp.values())
        users[iid].responseTimeZone = {
            it: timetemp[it] / timetotal for it in timetemp.keys()}

    # 得到每个用户与其他用户的相似度
    # 初始化一个计数器 kx 用于追踪处理的用户数，并设置一个变量 kn 为用户总数。
    kx = 0
    kn = len(users)

    # 遍历 users 字典中的所有用户（iid）
    for iid in tqdm(users.keys()):
        # 每当处理的用户数（kx）是 100 的倍数时，打印处理进度。
        # kx += 1
        # if kx % 100 == 0:
        #     print(f"Processed: {kx}/{kn}")

        # 为当前用户 iid 初始化一个新的字典 SimLusers 用于存储与其他用户的相似度。
        users[iid].SimLusers = {}

        # 创建一个有序字典 simUser 用于临时存储相似度和与之对应的用户列表。
        simUser = {}

        # 初始化一个 set backusers 用于存储与当前用户 iid 分享过物品的所有用户。
        backusers = set()

        # 遍历当前用户 iid 分享过的所有物品（itemiid）
        for itemiid in users[iid].ItemID:
            # 对于当前物品 itemiid，遍历与其相关的所有用户（zid），将它们添加到 backusers set 中
            backusers.update(itemusers[itemiid])

        # 遍历 backusers 中的所有用户（fid）
        for fid in backusers:
            # 如果当前用户 fid 与正在处理的用户 iid 相同，则跳过此次循环。
            if fid == iid:
                continue

            # 计算两个用户分享的物品的交集数量（k2）和并集数量（k1）
            k2 = len(set(users[iid].ItemID) & set(users[fid].ItemID))
            k1 = len(set(users[iid].ItemID) | set(users[fid].ItemID))

            # 如果交集数量为零，则跳过此次循环。否则，根据公式计算用户间的相似度（sim）
            if k2 == 0:
                continue

            sim = -k2 * 1.0 / k1 * (1 - 1.0 / math.sqrt(k1))

            # 如果 simUser 字典中尚未包含相似度 sim，则添加一个新条目，以相似度作为键，值为包含用户 fid 的列表
            if sim not in simUser:
                simUser[sim] = []

            # 将当前用户 fid 添加到 simUser 字典中相应相似度的用户列表中
            simUser[sim].append(fid)

        # 遍历 simUser 字典中的所有相似度（dd）
        for dd in simUser.keys():
            # 对于每个相似度 dd，遍历与之对应的用户列表
            for fid in simUser[dd]:
                # 将用户 fid 和相似度 -dd 添加到当前用户 iid 的 SimLusers 字典中
                users[iid].SimLusers[fid] = -dd

    # 从测试数据集中读取数据，并对其进行预处理以便于后续分析
    # 初始化一个名为 mmr 的变量以计算平均逆文档频率，以及一个名为 ktotal 的变量用于计算总数量
    mmr = 0
    ktotal = 0

    # 初始化 tsp、rsim、ritem 和 rresponse 变量以计算临时评分、相似度、物品评分和响应评分
    tsp = 0
    rsim = 0
    ritem = 0
    rresponse = 0

    # 初始化一个名为 submitres 的 List，用于存储提交的结果
    submitres = []
    submitresAll = []

    # 初始化一个名为 shareitems 的字典，用于存储每个用户分享过的物品及其分享时间
    shareitems = {}

    # 遍历测试数据集 testjsonData 中的每条数据
    for testdata in tqdm(testjsonData):
        # 提取用户 ID（id1）
        id1 = testdata['inviter_id']

        # 提取物品 ID（itemid）
        itemid = testdata['item_id']

        # 将数据中的时间戳转换为 datetime 对象（testdate）
        testdate = datetime.strptime(
            testdata['timestamp'], '%Y-%m-%d %H:%M:%S')

        # 如果 shareitems 字典中尚未包含用户 id1，则添加一个新条目
        if id1 not in shareitems:
            shareitems[id1] = {}

        # 如果 shareitems 字典中的用户 id1 尚未包含物品 itemid，则添加一个新条目
        if itemid not in shareitems[id1]:
            shareitems[id1][itemid] = []

        # 将物品 itemid 的分享时间 testdate 添加到 shareitems 字典中相应的用户和物品条目下
        shareitems[id1][itemid].append(testdate)

    # 获取用户单CPU的核心数、线程数、内存数
    cpu_count = psutil.cpu_count(False)
    cpu_count_logical = multiprocessing.cpu_count()
    free_memory = psutil.virtual_memory().free / 1024 / 1024 / 1024
    # 保留两位小数
    free_memory = round(free_memory, 2)

    # 初始化进程
    if Hyperthreading:
        threadingNum = round(cpu_count_logical)
    else:
        threadingNum = round(cpu_count)
    threadingNum = round(threadingNum * 3 / 4)
    if MaxThreading != 0:
        if threadingNum > MaxThreading:
            threadingNum = MaxThreading
    print(str(cpu_count) + "核心" + str(cpu_count_logical) + "线程" + str(
        free_memory) + "G内存" + "，开启任务进程数" + str(
        threadingNum))
    if free_memory < 12:
        print("多进程需要至少12G可用物理内存，请确保虚拟内存足够")
    # 数据集进行threadingNum数量的切分
    testjsonDataSplits = np.array_split(testjsonData, threadingNum)  # 切分数据

    m = multiprocess.Manager()
    p = multiprocess.Pool(threadingNum)

    # 初始化进程所需要的共享数据
    usersMini = {}
    for userId, user in tqdm(users.items()):
        usersMini[userId] = user
        usersMini[userId].Level = user.Level
        usersMini[userId].SimLusers = user.SimLusers
        usersMini[userId].Neighbor = user.Neighbor
        usersMini[userId].Ratio = user.Ratio
    # users,dataLF,dataItemLF,itemsinfo,netrelation,sharerank,userSimilarityMatrix,shareitems,itemreceive,responseitems,submitres,submitresAll
    variables = {
        'users': usersMini,  # read
        'dataLF': dataLF,  # read
        'dataItemLF': dataItemLF,  # read
        'itemsinfo': itemsinfo,  # read
        'netrelation': netrelation,  # read
        'sharerank': sharerank,  # read
        'userSimilarityMatrix': userSimilarityMatrix,  # write
        'shareitems': shareitems,  # read
        'itemreceive': itemreceive,  # read
        'responseitems': responseitems,  # change
        'submitres': submitres,  # out
        'submitresAll': submitresAll  # out
    }
    need_teans = ['userSimilarityMatrix', 'responseitems', 'submitres', 'submitresAll']
    # 初始化进程所需要的共享数据

    manager_dict_variables = {}
    for name, var in tqdm(variables.items(), desc="初始化进程所需要的共享数据"):
        if name in need_teans:
            if isinstance(var, dict):
                manager_dict_variables[name] = m.dict(var)
            else:
                manager_dict_variables[name] = m.list(var)
        else:
            manager_dict_variables[name] = var

    # 开始多进程
    for i in range(len(testjsonDataSplits)):
        p.apply_async(task, args=(i, testjsonDataSplits[i], *manager_dict_variables.values()))
    p.close()
    p.join()

    # 将多进程中的共享数据赋值给原始变量
    for name, var in tqdm(manager_dict_variables.items(), desc="将多进程中的共享数据赋值给原始变量"):
        if isinstance(var, m.dict):
            variables[name] = dict(var)
        else:
            variables[name] = list(var)

    #####################陈老师的原始方案 0.3256####################
    # 将 submitres 列表转换为 JSON 格式的字符串 text
    text = orjson.dumps(submitres)
    # 将字符串 text 写入文件 submit.json
    with open(savaPath + "submit.json", "wb") as f:  # 注意这里是"wb"，因为orjson.dumps返回的是bytes而不是str
        f.write(text)
    # 结果包含邀请者id的json，方便后续调试或者把模块单独拿出来测试
    text = orjson.dumps(submitresAll)
    with open(savaPath + "submitAll.json", "wb") as f:  # 注意这里是"wb"，因为orjson.dumps返回的是bytes而不是str
        f.write(text)
    statisticList = []
    statisticList.append(statistics(submitresAll, "陈老师原方案0.3256"))

    #####################张亮的方案二 提升到0.3267####################
    # 推荐结果不足5人的，尝试反向推理补全
    print("推理完毕，开始对于推荐结果不足5人的，尝试反向推理补全：")
    for res in tqdm(submitresAll):
        userID = res["inviter_id"]
        voters = res["candidate_voter_list"]
        scor = []
        # for voter in voters:
        #     print(userID+"-"+voter+":"+str(userSimilarityMatrix[userID][voter])+"/"+str(userSimilarityMatrix[voter][userID]))
        if len(voters) >= 5:
            continue
        for preSimilarUser in userSimilarityMatrix:
            score = userSimilarityMatrix[preSimilarUser][userID]
            if score != -1:
                # 以 (score, preSimilarUser) 的形式存储
                scor.append((-score, preSimilarUser))
        scor.sort()
        # 输出scor
        print(userID + "的scor：")
        for score, preSimilarUser in scor:
            if preSimilarUser != userID and preSimilarUser not in voters:
                voters.append(preSimilarUser)
                if len(voters) >= 5:
                    break
            if len(voters) >= 5:
                break
        # if len(voters) != len(res["candidate_voter_list"]):
        #     print(userID+"补全后人数从" + str(len(res["candidate_voter_list"]))+"变为"+str(len(voters)))
        # if len(voters) < 5:
        #     print(userID+"补全后依旧不足五人")
        res["candidate_voter_list"] = voters[:5]
    # 结果包含邀请者id的json，方便后续调试或者把模块单独拿出来测试
    with open(savaPath + "submitCompleteWithInviter.json", "wb") as f:
        f.write(orjson.dumps(submitresAll))
    with open(savaPath + "submitComplete.json", "wb") as f:
        f.write(orjson.dumps(del_inviter_id(submitresAll)))
    statisticList.append(statistics(submitresAll, "张亮方案二反向推理0.3267"))

    ####################张亮的方案三 提升到0.3276 效果不是很好####################
    # 补足5人的继续推理：对于用户A的相似用户们，对于每一个相似用户，将A的相似用户们的相似用户补全到A中
    print("推理完毕，开始对于推荐结果不足5人的，继续尝试递归补全：")
    submitDict = {}
    for obj in tqdm(submitresAll):
        inviter_id = obj["inviter_id"]
        candidate_voter_list = obj["candidate_voter_list"]
        # 将 "inviter_id" 和 "candidate_voter_list" 组合成键值对，并添加到结果字典
        submitDict[inviter_id] = candidate_voter_list
    for res in tqdm(submitresAll):
        userID = res["inviter_id"]
        voters = res["candidate_voter_list"]
        if len(voters) == 0:
            # print(userID + "的voters为空，没救了")
            continue
        if len(voters) > 5:
            continue
        if len(voters) < 5:
            newVoters = voters
            for voter in voters:
                if len(submitDict[voter]) == 0:
                    continue
                for voter2 in submitDict[voter]:
                    if voter2 not in voters:
                        newVoters.append(voter2)
                        if len(newVoters) >= 5:
                            break
                if len(newVoters) >= 5:
                    break
            # if len(newVoters) != len(voters):
            #     print(userID+"补全后人数从" + str(len(voters))+"变为"+str(len(newVoters)))
            # if len(newVoters) < 5:
            #     print(userID+"补全后依旧不足五人")
            res["candidate_voter_list"] = newVoters[:5]

    with open(savaPath + "submitCompleteStep2WithInviter.json", "wb") as f:
        f.write(orjson.dumps(submitresAll))
    with open(savaPath + "submitCompleteStep2.json", "wb") as f:
        f.write(orjson.dumps(del_inviter_id(submitresAll)))
    statisticList.append(statistics(submitresAll, "张亮方案三递归推理0.3276"))

    ####################结束####################
    makeStatistics(statisticList, savaPath)

    return jsonData, testjsonData, submitresAll, statisticList


if __name__ == "__main__":
    main(savaPath="./output/mresult/")
